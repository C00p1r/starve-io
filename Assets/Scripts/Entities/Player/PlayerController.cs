using StarveIO.Input;
using StarveIO.Data;
using UnityEngine;
using System;
public class PlayerController : MonoBehaviour
{
    private Vector2 _mouseScreenPos;
    private Camera _mainCamera;
    private InventoryManager _inventoryManager;

    [Header("輸入設定")]
    [SerializeField] private InputReader _inputReader;

    [Header("移動設定")]
    public float baseSpeed = 5f;
    public float speedReductionRate = 0.5f;
    private float currentSpeed;

    [Header("攻擊與採集設定(暫時沒用)")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private int attackDamage = 10;
    [Header("資源層")]
    [SerializeField] private LayerMask resourceLayer;

    [Header("膠囊攻擊設定")]
    [SerializeField] private Vector2 capsuleSize = new Vector2(1.2f, 2.0f);
    [SerializeField] private float capsuleOffset = 1.0f;
    [SerializeField] private CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical;

    private int leavesCount = 0;
    private Rigidbody2D rb;
    private Vector2 _moveInput;
    private float targetRotation;

    private void OnEnable()
    {
        _inputReader.MoveEvent += OnMove;
        _inputReader.AttackEvent += OnAttack;
        _inputReader.LookEvent += OnLook;
    }

    private void OnDisable()
    {
        _inputReader.MoveEvent -= OnMove;
        _inputReader.AttackEvent -= OnAttack;
        _inputReader.LookEvent -= OnLook;
    }

    void Start()
    {
        _inventoryManager = InventoryManager.Instance;
        _mainCamera = Camera.main;

        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;

        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    public void OnMove(Vector2 direction)
    {
        _moveInput = direction;
    }
    private void OnLook(Vector2 mousePos)
    {
        _mouseScreenPos = mousePos;
    }

    private void OnAttack()
    {
        Debug.Log("perform attack animation");
        OnAttackHitFrame();
    }
    public void OnAttackHitFrame()
    {
        Debug.Log("執行採集動作！");
        if (_inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return;
        }

        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
        float angle = transform.eulerAngles.z;
        Collider2D[] hitObjects = Physics2D.OverlapCapsuleAll(center, capsuleSize, capsuleDirection, angle, resourceLayer);
        bool notifiedMissingTool = false;
        foreach (Collider2D hit in hitObjects)
        {
            if (hit.TryGetComponent<ResourceNode>(out ResourceNode node))
            {
                ResourceData resourceData = node.GetResourceData();
                ItemData selectedItem = _inventoryManager.GetSelectedItem();
                if (!CanGatherResource(resourceData, selectedItem, _inventoryManager, out string denyMessage))
                {
                    if (!notifiedMissingTool)
                    {
                        UIEventManager.TriggerNotify(denyMessage);
                        notifiedMissingTool = true;
                    }
                    continue;
                }

                ItemData gatheredItem = node.GetItemData();
                int gatheredAmount = node.GatherResource();

                if (gatheredAmount > 0)
                {
                    bool success = _inventoryManager.AddItem(gatheredItem, gatheredAmount);
                    if (!success) Debug.Log("背包已滿！顯示");
                    else Debug.Log($"成功將 {gatheredItem.name}x{gatheredAmount} 放入背包!");
                }
            }
        }
    }

    private bool CanGatherResource(ResourceData resourceData, ItemData selectedItem, InventoryManager inventory, out string denyMessage)
    {
        denyMessage = null;
        if (resourceData == null)
            return true;

        ToolType requiredType = resourceData.requiredToolType;
        int requiredTier = resourceData.requiredToolTier;

        if (requiredType == ToolType.None)
            return true;

        int minTier = Mathf.Max(1, requiredTier);
        if (selectedItem != null &&
            selectedItem.toolType == requiredType &&
            selectedItem.toolTier >= minTier)
            return true;

        bool hasRequiredTool = HasRequiredToolInInventory(inventory, requiredType, minTier);
        string tierName = GetTierName(minTier);
        string toolName = requiredType.ToString().ToLowerInvariant();
        denyMessage = hasRequiredTool
            ? $"Need to hold a {tierName} {toolName} to mine this."
            : $"Need a {tierName} {toolName} to mine this.";
        return false;

        return true;
    }

    private string GetTierName(int tier)
    {
        switch (tier)
        {
            case 1:
                return "wooden";
            case 2:
                return "stone";
            case 3:
                return "golden";
            case 4:
                return "diamond";
            default:
                return $"tier {tier}";
        }
    }

    private bool HasRequiredToolInInventory(InventoryManager inventory, ToolType requiredType, int minTier)
    {
        if (inventory == null)
            return false;

        foreach (var slot in inventory.GetSlots())
        {
            if (slot.item == null || slot.count <= 0)
                continue;

            if (slot.item.toolType == requiredType && slot.item.toolTier >= minTier)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = UnityEngine.Color.red;

        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
        float angle = transform.eulerAngles.z;

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Gizmos.DrawWireCube(Vector3.zero, new Vector3(capsuleSize.x, capsuleSize.y, 0));

        Gizmos.matrix = Matrix4x4.identity;
    }
    void Update()
    {
        RotateTowardsMouse();
        HandleHotbarScroll();
        HandleHotbarNumberSelection();
    }

    private void RotateTowardsMouse()
    {
        Vector3 mousePos = _mouseScreenPos;
        mousePos.z = -_mainCamera.transform.position.z;
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

        Vector2 lookDir = (Vector2)worldPos - rb.position;

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

        targetRotation = angle;
    }

    private void HandleHotbarScroll()
    {
        if (_inventoryManager == null)
            return;

        float scroll = UnityEngine.InputSystem.Mouse.current != null
            ? UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y
            : 0f;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            int direction = scroll > 0f ? -1 : 1;
            _inventoryManager.CycleSelection(direction);
        }
    }

    private void HandleHotbarNumberSelection()
    {
        if (_inventoryManager == null || UnityEngine.InputSystem.Keyboard.current == null)
            return;

        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard.digit1Key.wasPressedThisFrame) _inventoryManager.SelectIndex(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) _inventoryManager.SelectIndex(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) _inventoryManager.SelectIndex(2);
        else if (keyboard.digit4Key.wasPressedThisFrame) _inventoryManager.SelectIndex(3);
        else if (keyboard.digit5Key.wasPressedThisFrame) _inventoryManager.SelectIndex(4);
        else if (keyboard.digit6Key.wasPressedThisFrame) _inventoryManager.SelectIndex(5);
        else if (keyboard.digit7Key.wasPressedThisFrame) _inventoryManager.SelectIndex(6);
        else if (keyboard.digit8Key.wasPressedThisFrame) _inventoryManager.SelectIndex(7);
        else if (keyboard.digit9Key.wasPressedThisFrame) _inventoryManager.SelectIndex(8);
        else if (keyboard.digit0Key.wasPressedThisFrame) _inventoryManager.SelectIndex(9);
    }
    void FixedUpdate()
    {
        if (_moveInput.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + currentSpeed * Time.fixedDeltaTime * _moveInput.normalized);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, 0);
        }

        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetRotation, 720f * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Leaves"))
        {
            if (leavesCount == 0)
            {
                currentSpeed = baseSpeed * (1 - speedReductionRate);
            }
            leavesCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Leaves"))
        {
            leavesCount--;
            if (leavesCount <= 0)
            {
                leavesCount = 0;
                currentSpeed = baseSpeed;
            }
        }
    }
}
