//using StarveIO.Input;
//using StarveIO.Data;
//using UnityEngine;
//using System;
//public class PlayerController : MonoBehaviour
//{

//    private Vector2 _mouseScreenPos;
//    private Camera _mainCamera;
//    private InventoryManager _inventoryManager;
//    private Animator _animator;

//    [Header("輸入設定")]
//    [SerializeField] private InputReader _inputReader;

//    [Header("移動設定")]
//    public float baseSpeed = 5f;
//    public float speedReductionRate = 0.5f;
//    private float currentSpeed;

//    [Header("攻擊與採集設定(暫時沒用)")]
//    [SerializeField] private float attackRange = 1.0f;
//    [SerializeField] private float attackRadius = 0.5f;
//    [SerializeField] private int attackDamage = 10;
//    [Header("資源層")]
//    [SerializeField] private LayerMask resourceLayer;

//    [Header("膠囊攻擊設定")]
//    [SerializeField] private Vector2 capsuleSize = new Vector2(1.2f, 2.0f);
//    [SerializeField] private float capsuleOffset = 1.0f;
//    [SerializeField] private CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical;

//    private int leavesCount = 0;
//    private Rigidbody2D rb;
//    private Vector2 _moveInput;
//    private float targetRotation;

//    private void OnEnable()
//    {
//        _inputReader.MoveEvent += OnMove;
//        _inputReader.AttackEvent += OnAttack;
//        _inputReader.LookEvent += OnLook;
//    }

//    private void OnDisable()
//    {
//        _inputReader.MoveEvent -= OnMove;
//        _inputReader.AttackEvent -= OnAttack;
//        _inputReader.LookEvent -= OnLook;
//    }

//    void Start()
//    {
//        _animator = GetComponent<Animator>();
//        _inventoryManager = InventoryManager.Instance;
//        _mainCamera = Camera.main;

//        rb = GetComponent<Rigidbody2D>();
//        currentSpeed = baseSpeed;

//        rb.gravityScale = 0;
//        rb.freezeRotation = true;
//    }

//    public void OnMove(Vector2 direction)
//    {
//        _moveInput = direction;
//    }
//    private void OnLook(Vector2 mousePos)
//    {
//        _mouseScreenPos = mousePos;
//    }

//    private void OnAttack()
//    {

//        if (_animator != null)
//        {
//            Debug.Log("perform attack animation");
//            _animator.SetTrigger("Attack");
//        }
//        OnAttackHitFrame();
//    }
//    public void OnAttackHitFrame()
//    {
//        Debug.Log("執行採集動作！");
//        if (_inventoryManager == null)
//        {
//            Debug.LogWarning("InventoryManager instance not found.");
//            return;
//        }

//        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
//        float angle = transform.eulerAngles.z;
//        Collider2D[] hitObjects = Physics2D.OverlapCapsuleAll(center, capsuleSize, capsuleDirection, angle, resourceLayer);
//        bool notifiedMissingTool = false;
//        foreach (Collider2D hit in hitObjects)
//        {
//            if (hit.TryGetComponent<ResourceNode>(out ResourceNode node))
//            {
//                ResourceData resourceData = node.GetResourceData();
//                ItemData selectedItem = _inventoryManager.GetSelectedItem();
//                if (!CanGatherResource(resourceData, selectedItem, _inventoryManager, out string denyMessage))
//                {
//                    if (!notifiedMissingTool)
//                    {
//                        UIEventManager.TriggerNotify(denyMessage);
//                        notifiedMissingTool = true;
//                    }
//                    continue;
//                }

//                ItemData gatheredItem = node.GetItemData();
//                int gatheredAmount = node.GatherResource();

//                if (gatheredAmount > 0)
//                {
//                    bool success = _inventoryManager.AddItem(gatheredItem, gatheredAmount);
//                    if (!success) UIEventManager.TriggerNotify("The Inventory is Full!");
//                    else Debug.Log($"成功將 {gatheredItem.name}x{gatheredAmount} 放入背包!");
//                }
//            }
//        }
//    }

//    private bool CanGatherResource(ResourceData resourceData, ItemData selectedItem, InventoryManager inventory, out string denyMessage)
//    {
//        denyMessage = null;
//        if (resourceData == null)
//            return true;

//        ToolType requiredType = resourceData.requiredToolType;
//        int requiredTier = resourceData.requiredToolTier;

//        if (requiredType == ToolType.None)
//            return true;

//        int minTier = Mathf.Max(1, requiredTier);
//        if (selectedItem != null &&
//            selectedItem.toolType == requiredType &&
//            selectedItem.toolTier >= minTier)
//            return true;

//        bool hasRequiredTool = HasRequiredToolInInventory(inventory, requiredType, minTier);
//        string tierName = GetTierName(minTier);
//        string toolName = requiredType.ToString().ToLowerInvariant();
//        denyMessage = hasRequiredTool
//            ? $"Need to hold a {tierName} {toolName} to mine this."
//            : $"Need a {tierName} {toolName} to mine this.";
//        return false;

//        return true;
//    }

//    private string GetTierName(int tier)
//    {
//        switch (tier)
//        {
//            case 1:
//                return "wooden";
//            case 2:
//                return "stone";
//            case 3:
//                return "golden";
//            case 4:
//                return "diamond";
//            default:
//                return $"tier {tier}";
//        }
//    }

//    private bool HasRequiredToolInInventory(InventoryManager inventory, ToolType requiredType, int minTier)
//    {
//        if (inventory == null)
//            return false;

//        foreach (var slot in inventory.GetSlots())
//        {
//            if (slot.item == null || slot.count <= 0)
//                continue;

//            if (slot.item.toolType == requiredType && slot.item.toolTier >= minTier)
//                return true;
//        }

//        return false;
//    }

//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = UnityEngine.Color.red;

//        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
//        float angle = transform.eulerAngles.z;

//        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
//        Gizmos.matrix = rotationMatrix;

//        Gizmos.DrawWireCube(Vector3.zero, new Vector3(capsuleSize.x, capsuleSize.y, 0));

//        Gizmos.matrix = Matrix4x4.identity;
//    }
//    void Update()
//    {
//        RotateTowardsMouse();
//        HandleHotbarScroll();
//        HandleHotbarNumberSelection();
//    }

//    private void RotateTowardsMouse()
//    {
//        Vector3 mousePos = _mouseScreenPos;
//        mousePos.z = -_mainCamera.transform.position.z;
//        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

//        Vector2 lookDir = (Vector2)worldPos - rb.position;

//        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

//        targetRotation = angle;
//    }

//    private void HandleHotbarScroll()
//    {
//        if (_inventoryManager == null)
//            return;

//        float scroll = UnityEngine.InputSystem.Mouse.current != null
//            ? UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y
//            : 0f;
//        if (Mathf.Abs(scroll) > 0.01f)
//        {
//            int direction = scroll > 0f ? -1 : 1;
//            _inventoryManager.CycleSelection(direction);
//        }
//    }

//    private void HandleHotbarNumberSelection()
//    {
//        if (_inventoryManager == null || UnityEngine.InputSystem.Keyboard.current == null)
//            return;

//        var keyboard = UnityEngine.InputSystem.Keyboard.current;
//        if (keyboard.digit1Key.wasPressedThisFrame) _inventoryManager.SelectIndex(0);
//        else if (keyboard.digit2Key.wasPressedThisFrame) _inventoryManager.SelectIndex(1);
//        else if (keyboard.digit3Key.wasPressedThisFrame) _inventoryManager.SelectIndex(2);
//        else if (keyboard.digit4Key.wasPressedThisFrame) _inventoryManager.SelectIndex(3);
//        else if (keyboard.digit5Key.wasPressedThisFrame) _inventoryManager.SelectIndex(4);
//        else if (keyboard.digit6Key.wasPressedThisFrame) _inventoryManager.SelectIndex(5);
//        else if (keyboard.digit7Key.wasPressedThisFrame) _inventoryManager.SelectIndex(6);
//        else if (keyboard.digit8Key.wasPressedThisFrame) _inventoryManager.SelectIndex(7);
//        else if (keyboard.digit9Key.wasPressedThisFrame) _inventoryManager.SelectIndex(8);
//        else if (keyboard.digit0Key.wasPressedThisFrame) _inventoryManager.SelectIndex(9);
//    }
//    void FixedUpdate()
//    {
//        if (_moveInput.sqrMagnitude > 0.01f)
//        {
//            rb.MovePosition(rb.position + currentSpeed * Time.fixedDeltaTime * _moveInput.normalized);
//        }
//        else
//        {
//            rb.linearVelocity = new Vector2(0, 0);
//        }

//        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetRotation, 720f * Time.fixedDeltaTime);
//        rb.MoveRotation(newAngle);
//    }

//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Leaves"))
//        {
//            if (leavesCount == 0)
//            {
//                currentSpeed = baseSpeed * (1 - speedReductionRate);
//            }
//            leavesCount++;
//        }
//    }

//    private void OnTriggerExit2D(Collider2D other)
//    {
//        if (other.CompareTag("Leaves"))
//        {
//            leavesCount--;
//            if (leavesCount <= 0)
//            {
//                leavesCount = 0;
//                currentSpeed = baseSpeed;
//            }
//        }
//    }
//}

using StarveIO.Data;
using StarveIO.Input;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private Vector2 _mouseScreenPos;
    private Camera _mainCamera;
    private Animator _animator;
    [SerializeField] public InventoryManager _inventoryManager;
    [Header("輸入設定")]
    [SerializeField] private InputReader _inputReader;

    [Header("移動設定")]
    public float baseSpeed = 5f;
    public float speedReductionRate = 0.5f;
    private float currentSpeed;

    [Header("基礎戰鬥/採集參數")]
    [SerializeField] private int _handDamage = 10;
    [SerializeField] private LayerMask resourceLayer;

    [Header("固定攻擊偵測範圍")]
    [SerializeField] private Vector2 capsuleSize = new Vector2(1.2f, 2.0f);
    [SerializeField] private float capsuleOffset = 1.0f;
    [SerializeField] private CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical;

    [Header("Placement")]
    [SerializeField] private GameObject bonfirePrefab;
    [SerializeField] private float bonfirePlaceOffset = 0.6f;
    [SerializeField] private float bonfirePlaceRadius = 0.2f;
    [SerializeField] private LayerMask bonfireBlockerLayers;
    [SerializeField] private Color bonfirePreviewValidColor = new Color(0.2f, 1f, 0.2f, 0.55f);
    [SerializeField] private Color bonfirePreviewInvalidColor = new Color(1f, 0.2f, 0.2f, 0.55f);
    private GameObject _bonfirePreview;
    private SpriteRenderer _bonfirePreviewRenderer;
    private bool _skipNextAttackHit;

    [Header("狀態監控")]
    private bool _isAttacking = false; // 新增：用來鎖定攻擊狀態
    private bool _isAttackingHeld = false;
    private int leavesCount = 0;
    private Rigidbody2D rb;
    private Vector2 _moveInput;
    private float targetRotation;
    [Header("Sound Effect Settings")]
    [SerializeField] private AudioSource die_effect;
    [SerializeField] private AudioSource eat_berry;
    [SerializeField] private AudioSource eat_meat;
    [SerializeField] private AudioSource get_hit;
    [SerializeField] private AudioSource hit_sound_diamond;
    [SerializeField] private AudioSource hit_sound_gold; 
    [SerializeField] private AudioSource hit_sound_stone;
    [SerializeField] private AudioSource hit_sound_tree;
    [SerializeField] private AudioSource mine_berry;
    [SerializeField] private AudioSource mine_fail;
    [SerializeField] private AudioSource use_bandage;
    private void OnEnable()
    {
        _inputReader.MoveEvent += OnMove;
        // 假設 InputReader 已經更新，能傳遞 Started 與 Canceled 狀態
        // 如果你的 InputReader 只有單次點擊事件，請參考下方的 Update 邏輯
        _inputReader.AttackEvent += HandleAttackInput;
        _inputReader.LookEvent += OnLook;
    }

    private void OnDisable()
    {
        _inputReader.MoveEvent -= OnMove;
        _inputReader.AttackEvent -= HandleAttackInput;
        _inputReader.LookEvent -= OnLook;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;
        if (_inventoryManager == null)
            _inventoryManager = InventoryManager.Instance;

        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        CacheAudioSources();
    }

    // --- 輸入處理 ---
    public void OnMove(Vector2 direction) => _moveInput = direction;
    private void OnLook(Vector2 mousePos) => _mouseScreenPos = mousePos;

    // 處理長按：如果你的 InputReader 是 Action.triggered，這裡需要邏輯判斷
    private void HandleAttackInput()
    {
        // 這裡如果是單次點擊事件，可以直接執行。
        // 如果需要長按，建議在 InputReader 裡區分 Started 和 Canceled。
        // 暫時以單次點擊作為觸發，或使用 UnityEngine.InputSystem.Mouse.current.leftButton.isPressed
        if (TryPlaceBonfire())
        {
            _skipNextAttackHit = true;
            return;
        }

        _isAttackingHeld = true;

    }

    void Update()
    {
        RotateTowardsMouse();
        HandleHotbarScroll();
        HandleHotbarNumberSelection();

        // --- 核心控制：根據按鍵狀態設定 Animator Bool ---
        bool isMousePressed = false;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            // 如果沒點在 UI 上，則讀取按鍵狀態
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                isMousePressed = UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
            }
        }
        bool moving = _moveInput.sqrMagnitude > 0.01f;
        bool holdingTool = false;
        if (_inventoryManager != null)
        {
            var item = _inventoryManager.GetSelectedItem();
            if (item != null && item.toolType > 0)
                holdingTool = true;
        }
        if (_animator != null)
        {
            _animator.SetBool("isAttacking", isMousePressed);
            _animator.SetBool("isMoving", moving);
            _animator.SetBool("holdingTool", holdingTool);
        }

        UpdateBonfirePreview();
    }


    // used by animation
    public void OnAttackHitFrame()
    {
        if (_skipNextAttackHit)
        {
            _skipNextAttackHit = false;
            return;
        }

        Debug.Log("動畫偵測到打擊點！執行判定");
        if (_inventoryManager == null)
        {
            Debug.Log("ERR: inventory manager not found");
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
                
                // 檢查工具階級
                
                if (!CanGatherResource(resourceData, selectedItem, _inventoryManager, out string denyMessage))
                {
                    if (!notifiedMissingTool)
                    {
                        PlayAudioOnce(mine_fail);
                        UIEventManager.TriggerNotify(denyMessage);
                        notifiedMissingTool = true;
                    }
                    continue;
                }
                int gatherMultiplier = GetGatherMultiplier(resourceData, selectedItem);
                switch(resourceData.name)
                {
                    case "Diamond":
                        PlayAudioOnce(hit_sound_diamond);
                        break;
                    case "Stone":
                        PlayAudioOnce(hit_sound_stone);
                        break;
                    case "Gold":
                        PlayAudioOnce(hit_sound_gold);
                        break;
                    case "Wood":
                        PlayAudioOnce(hit_sound_tree);
                        break;
                    case "FruitTree":
                        PlayAudioOnce(mine_berry);
                        break;
                    default:
                        Debug.LogWarning("未找到相符物件");
                        break;
                }
                // --- 新增：計算攻擊方向並觸發震動 ---
                // 方向 = 資源位置 - 玩家位置
                Vector2 hitDirection = (hit.transform.position - this.transform.position).normalized;
                Debug.Log($"Hit Direction ({hitDirection.x}, {hitDirection.y})");
                node.TriggerHitEffect(hitDirection);

                ItemData gatheredItem = node.GetItemData();
                int gatheredAmount = node.GatherResource(gatherMultiplier);

                if (gatheredAmount > 0)
                {
                    bool success = _inventoryManager.AddItem(gatheredItem, gatheredAmount);
                    if (!success) UIEventManager.TriggerNotify("The Inventory is Full!");
                }
            }
            if (hit.TryGetComponent<BonfireInteractable>(out BonfireInteractable bonfire))
            {
                Debug.Log("Bonfire hit.");
                bonfire.RequestDestroy();
                return;
            }
            if (hit.TryGetComponent<MonsterBehavior>(out MonsterBehavior monster))
            {
                if (!(hit is CircleCollider2D))
                {
                    continue;
                }
                ItemData selectedItem = _inventoryManager.GetSelectedItem();
                int damage = selectedItem != null ? selectedItem.damage : _handDamage; // Use item damage or hand damage
                monster.TakeDamage(damage); // Apply damage to the monster
            }
            
        }
        if (hitObjects.Length == 0)
        {
            if (!TryPlaceBonfire())
                _inventoryManager.UseSelectedItem();
        }
    }

    // --- 採集邏輯檢查 ---
    private bool CanGatherResource(ResourceData resourceData, ItemData selectedItem, InventoryManager inventory, out string denyMessage)
    {
        denyMessage = null;
        if (resourceData == null) return true;

        ToolType requiredType = resourceData.requiredToolType;
        int requiredTier = resourceData.requiredToolTier;

        if (requiredType == ToolType.None) return true;

        int minTier = Mathf.Max(1, requiredTier);

        // A. 檢查當前手持是否符合
        if (selectedItem != null && selectedItem.toolType == requiredType && selectedItem.toolTier >= minTier)
            return true;

        // B. 檢查背包是否有但未裝備 (自動切換提示)
        bool hasRequiredTool = HasRequiredToolInInventory(inventory, requiredType, minTier);
        string tierName = GetTierName(minTier);
        string toolName = requiredType.ToString().ToLowerInvariant();

        denyMessage = hasRequiredTool
            ? $"Need to hold a {tierName} {toolName} to mine this."
            : $"Need a {tierName} {toolName} to mine this.";

        return false;
    }

    private string GetTierName(int tier)
    {
        switch (tier)
        {
            case 1: return "wooden";
            case 2: return "stone";
            case 3: return "golden";
            case 4: return "diamond";
            default: return $"tier {tier}";
        }
    }

    private bool HasRequiredToolInInventory(InventoryManager inventory, ToolType requiredType, int minTier)
    {
        if (inventory == null) return false;
        foreach (var slot in inventory.GetSlots())
        {
            if (slot.item != null && slot.item.toolType == requiredType && slot.item.toolTier >= minTier)
                return true;
        }
        return false;
    }

    private int GetGatherMultiplier(ResourceData resourceData, ItemData selectedItem)
    {
        if (resourceData == null || selectedItem == null)
            return 1;

        int resourceTier = GetResourceTier(resourceData);
        if (resourceTier <= 0)
            return 1;

        if (selectedItem.toolType != ToolType.Pickaxe)
            return 1;

        int toolTier = selectedItem.toolTier;
        if (toolTier < resourceTier)
            return 1;
        if (toolTier == resourceTier)
            return 2;
        return 3;
    }

    private int GetResourceTier(ResourceData resourceData)
    {
        if (resourceData == null)
            return 0;

        if (string.Equals(resourceData.name, "Wood", StringComparison.OrdinalIgnoreCase))
            return 1;
        if (string.Equals(resourceData.name, "Stone", StringComparison.OrdinalIgnoreCase))
            return 2;
        if (string.Equals(resourceData.name, "Gold", StringComparison.OrdinalIgnoreCase))
            return 3;
        if (string.Equals(resourceData.name, "Diamond", StringComparison.OrdinalIgnoreCase))
            return 4;
        return 0;
    }

    private void PlayAudioOnce(AudioSource source)
    {
        if (source == null)
            return;

        source.time = 0f;
        source.Play();
    }

    private void CacheAudioSources()
    {
        var sources = GetComponentsInChildren<AudioSource>(true);
        foreach (var source in sources)
        {
            if (source == null)
                continue;

            string name = source.gameObject.name;
            switch (name)
            {
                case "chracter_die":
                    if (die_effect == null) die_effect = source;
                    break;
                case "eat_berry":
                    if (eat_berry == null) eat_berry = source;
                    break;
                case "eat_meat":
                    if (eat_meat == null) eat_meat = source;
                    break;
                case "player_GetHit":
                    if (get_hit == null) get_hit = source;
                    break;
                case "mine_diamond":
                    if (hit_sound_diamond == null) hit_sound_diamond = source;
                    break;
                case "mine_gold":
                    if (hit_sound_gold == null) hit_sound_gold = source;
                    break;
                case "mine_stone":
                    if (hit_sound_stone == null) hit_sound_stone = source;
                    break;
                case "mine_wood":
                    if (hit_sound_tree == null) hit_sound_tree = source;
                    break;
                case "use_thread//bandage":
                    if (use_bandage == null) use_bandage = source;
                    break;
                case "mine_fail":
                    if (mine_fail == null) mine_fail = source;
                    break;
                case "mine_berry":
                    if (mine_berry == null) mine_berry = source;
                    break;
            }
        }
    }

    private bool TryPlaceBonfire()
    {
        if (_inventoryManager == null)
            return false;

        var selectedItem = _inventoryManager.GetSelectedItem();
        if (selectedItem == null || !string.Equals(selectedItem.itemName, "Bonfire", StringComparison.OrdinalIgnoreCase))
            return false;

        if (bonfirePrefab == null)
        {
            Debug.LogWarning("Bonfire prefab not assigned on PlayerController.");
            return true;
        }

        Vector3 placePosition = transform.position + (Vector3)(Vector2)transform.up * bonfirePlaceOffset;
        placePosition.z = 0f;

        if (!IsBonfirePlacementValid(placePosition))
        {
            UIEventManager.TriggerNotify("Can't place bonfire here.");
            return true;
        }

        if (!_inventoryManager.TryRemoveItem(selectedItem, 1))
            return true;

        Instantiate(bonfirePrefab, placePosition, Quaternion.identity);
        return true;
    }

    private void UpdateBonfirePreview()
    {
        if (_inventoryManager == null)
        {
            SetBonfirePreviewVisible(false);
            return;
        }

        var selectedItem = _inventoryManager.GetSelectedItem();
        bool shouldShow = selectedItem != null &&
            string.Equals(selectedItem.itemName, "Bonfire", StringComparison.OrdinalIgnoreCase);
        if (!shouldShow)
        {
            SetBonfirePreviewVisible(false);
            return;
        }

        EnsureBonfirePreview(selectedItem);
        Vector3 placePosition = transform.position + (Vector3)(Vector2)transform.up * bonfirePlaceOffset;
        placePosition.z = 0f;
        _bonfirePreview.transform.position = placePosition;

        bool isValid = IsBonfirePlacementValid(placePosition);
        if (_bonfirePreviewRenderer != null)
            _bonfirePreviewRenderer.color = isValid ? bonfirePreviewValidColor : bonfirePreviewInvalidColor;

        SetBonfirePreviewVisible(true);
    }

    private void EnsureBonfirePreview(ItemData selectedItem)
    {
        if (_bonfirePreview != null)
            return;

        _bonfirePreview = new GameObject("BonfirePreview");
        _bonfirePreviewRenderer = _bonfirePreview.AddComponent<SpriteRenderer>();

        SpriteRenderer sourceRenderer = bonfirePrefab != null ? bonfirePrefab.GetComponent<SpriteRenderer>() : null;
        if (sourceRenderer != null)
        {
            _bonfirePreviewRenderer.sprite = sourceRenderer.sprite;
            _bonfirePreviewRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            _bonfirePreviewRenderer.sortingOrder = sourceRenderer.sortingOrder;
        }
        else if (selectedItem != null && selectedItem.icon != null)
        {
            _bonfirePreviewRenderer.sprite = selectedItem.icon;
        }

        _bonfirePreview.transform.localScale = Vector3.one * 2f;
    }

    private void SetBonfirePreviewVisible(bool visible)
    {
        if (_bonfirePreview != null && _bonfirePreview.activeSelf != visible)
            _bonfirePreview.SetActive(visible);
    }

    private bool IsBonfirePlacementValid(Vector3 position)
    {
        if (bonfireBlockerLayers.value == 0)
            return true;

        return Physics2D.OverlapCircle(position, bonfirePlaceRadius, bonfireBlockerLayers) == null;
    }

    // --- 移動與基礎邏輯 ---
    private void RotateTowardsMouse()
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(_mouseScreenPos.x, _mouseScreenPos.y, -_mainCamera.transform.position.z));
        Vector2 lookDir = (Vector2)worldPos - rb.position;
        targetRotation = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
    }

    private void HandleHotbarScroll()
    {
        if (_inventoryManager == null) return;
        float scroll = UnityEngine.InputSystem.Mouse.current?.scroll.ReadValue().y ?? 0f;
        if (Mathf.Abs(scroll) > 0.01f) _inventoryManager.CycleSelection(scroll > 0f ? -1 : 1);
    }

    private void HandleHotbarNumberSelection()
    {
        if (_inventoryManager == null || UnityEngine.InputSystem.Keyboard.current == null) return;
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb.digit1Key.wasPressedThisFrame) _inventoryManager.SelectIndex(0);
        else if (kb.digit2Key.wasPressedThisFrame) _inventoryManager.SelectIndex(1);
        else if (kb.digit3Key.wasPressedThisFrame) _inventoryManager.SelectIndex(2);
        else if (kb.digit4Key.wasPressedThisFrame) _inventoryManager.SelectIndex(3);
        else if (kb.digit5Key.wasPressedThisFrame) _inventoryManager.SelectIndex(4);
        else if (kb.digit6Key.wasPressedThisFrame) _inventoryManager.SelectIndex(5);
        else if (kb.digit7Key.wasPressedThisFrame) _inventoryManager.SelectIndex(6);
        else if (kb.digit8Key.wasPressedThisFrame) _inventoryManager.SelectIndex(7);
        else if (kb.digit9Key.wasPressedThisFrame) _inventoryManager.SelectIndex(8);
        else if (kb.digit0Key.wasPressedThisFrame) _inventoryManager.SelectIndex(9);
    }

    void FixedUpdate()
    {
        if (_moveInput.sqrMagnitude > 0.01f)
            rb.MovePosition(rb.position + currentSpeed * Time.fixedDeltaTime * _moveInput.normalized);
        else
            rb.linearVelocity = Vector2.zero;

        rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, targetRotation, 720f * Time.fixedDeltaTime));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, transform.eulerAngles.z), Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(capsuleSize.x, capsuleSize.y, 0));
        Gizmos.matrix = Matrix4x4.identity;
    }

    // --- 葉子減速邏輯 ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Leaves"))
        {
            if (leavesCount == 0) currentSpeed = baseSpeed * (1 - speedReductionRate);
            leavesCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Leaves"))
        {
            leavesCount--;
            if (leavesCount <= 0) { leavesCount = 0; currentSpeed = baseSpeed; }
        }
    }
}
