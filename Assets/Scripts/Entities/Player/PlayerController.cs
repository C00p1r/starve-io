//using StarveIO.Data;
//using StarveIO.Input; // 確保引用了你的 Input 命名空間
//using System;
//using System.Drawing;
//using UnityEngine;
//using UnityEngine.EventSystems;
//public class PlayerController : MonoBehaviour
//{

//    // new add
//    private Vector2 _mouseScreenPos;
//    private Camera _mainCamera;
//    private InventoryManager _inventoryManager;

//    [Header("輸入設定")]
//    [SerializeField] private InputReader _inputReader; // 拖入你的 InputReader 資源檔
//    //[SerializeField] private InventoryManager _inventoryManager;

//    [Header("移動設定")]
//    public float baseSpeed = 5f;             // 基礎速度
//    public float speedReductionRate = 0.5f;  // 進入葉子要減去的速度比率
//    private float currentSpeed;              // 當前實際速度

//    [Header("攻擊與採集設定(暫時沒用)")]
//    [SerializeField] private float attackRange = 1.0f;  // 圓圈距離玩家中心的距離
//    [SerializeField] private float attackRadius = 0.5f; // 攻擊圓圈的半徑
//    [SerializeField] private int attackDamage = 10;     // 攻擊力
//    [Header("資源層")]
//    [SerializeField] private LayerMask resourceLayer;   // 設定哪些 Layer 是可採集的 (如 Resource)


//    [Header("膠囊攻擊設定")]
//    [SerializeField] private Vector2 capsuleSize = new Vector2(1.2f, 2.0f); // 寬度與長度
//    [SerializeField] private float capsuleOffset = 1.0f;                 // 距離玩家中心的距離
//    [SerializeField] private CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical;


//    private int leavesCount = 0;             // 目前重疊的葉子數量
//    private Rigidbody2D rb;
//    private Vector2 _moveInput;              // 從 InputReader 接收到的數值
//    private float targetRotation;


//    private void OnEnable()
//    {
//        // 訂閱 InputReader 的移動事件
//        _inputReader.MoveEvent += OnMove;
//         _inputReader.AttackEvent += OnAttack; // new added
//        _inputReader.LookEvent += OnLook; // 訂閱滑鼠位置 new added
//    }

//    private void OnDisable()
//    {
//        // 取消訂閱，防止物件銷毀後發生記憶體錯誤
//        _inputReader.MoveEvent -= OnMove;
//        _inputReader.AttackEvent -= OnAttack;
//        _inputReader.LookEvent -= OnLook; // new added
//    }

//    void Start()
//    {
//        _inventoryManager = InventoryManager.Instance;
//        _mainCamera = Camera.main; // new added

//        rb = GetComponent<Rigidbody2D>();
//        currentSpeed = baseSpeed;

//        rb.gravityScale = 0;
//        rb.freezeRotation = true;
//    }

//    // 事件回調函數：當 InputReader 偵測到移動時會呼叫此處
//    public void OnMove(Vector2 direction)
//    {
//        _moveInput = direction;
//    }
//    private void OnLook(Vector2 mousePos)
//    {
//        _mouseScreenPos = mousePos;
//    }

//    // new added attack
//    private void OnAttack()
//    {
//        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//        {
//            // 如果在 UI 上，這裡「不做任何事」
//            // 因為 UI Toolkit 的 RegisterCallback<PointerDownEvent> 會「自己」處理掉後續
//            Debug.Log("決策：交給 UI 處理");
//            return;
//        }

//        Debug.Log("perform attack animation");
//        OnAttackHitFrame();
//    }
//    public void OnAttackHitFrame()
//    {
//        Debug.Log("執行採集動作！");

//        // 1. 計算膠囊的中心點 (玩家位置 + 正前方偏移)
//        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;

//        // 2. 獲取玩家目前的旋轉角度 (Z軸)
//        float angle = transform.eulerAngles.z;

//        // 3. 執行膠囊偵測
//        Collider2D[] hitObjects = Physics2D.OverlapCapsuleAll(center, capsuleSize, capsuleDirection, angle, resourceLayer);
//        //Physics2D.OverlapCapsuleAll
//        foreach (Collider2D hit in hitObjects)
//        {
//            // exclude collision box for triggering sth
//            if (hit.TryGetComponent<ResourceNode>(out ResourceNode node))
//            {
//                ItemData gatheredItem = node.GetItemData();
//                int gatheredAmount = node.GatherResource();

//                if (gatheredAmount > 0)
//                {
//                    // 將資源存入背包
//                    bool success = _inventoryManager.AddItem(gatheredItem, gatheredAmount);
//                    if (!success) Debug.Log("背包已滿！顯示");
//                    else Debug.Log($"成功將 {gatheredItem.name}x{gatheredAmount} 放入背包!");
//                }
//            }
//        }
//    }

//    // 在編輯器中畫出攻擊範圍，方便除錯
//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = UnityEngine.Color.red;

//        // 計算與上面相同的中心點與旋轉
//        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
//        float angle = transform.eulerAngles.z;

//        // 使用矩陣來繪製旋轉過的膠囊 (這比較進階，但能畫出正確的旋轉)
//        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angle), Vector3.one);
//        Gizmos.matrix = rotationMatrix;

//        // 畫出一個代表膠囊的 WireCube 或自定義形狀
//        // 雖然 Gizmos 沒有直接畫 Capsule 的方法，但畫一個帶圓角的長方形效果一致
//        Gizmos.DrawWireCube(Vector3.zero, new Vector3(capsuleSize.x, capsuleSize.y, 0));

//        // 重置矩陣，避免影響其他 Gizmos
//        Gizmos.matrix = Matrix4x4.identity;
//    }
//    void Update()
//    {
//        RotateTowardsMouse();
//    }


//    private void RotateTowardsMouse()
//    {
//        // 轉換滑鼠座標
//        Vector3 mousePos = _mouseScreenPos;
//        mousePos.z = -_mainCamera.transform.position.z;
//        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

//        // 計算方向向量
//        Vector2 lookDir = (Vector2)worldPos - rb.position;

//        // 計算角度並統一使用 -90f 偏移量
//        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;

//        targetRotation = angle;
//    }

//    void FixedUpdate()
//    {
//        // 移動邏輯
//        if (_moveInput.sqrMagnitude > 0.01f)
//        {
//            rb.MovePosition(rb.position + currentSpeed * Time.fixedDeltaTime * _moveInput.normalized);
//        }
//        else
//        {
//            rb.linearVelocity = new Vector2(0, 0);
//        }

//            // 平滑轉向
//            float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetRotation, 720f * Time.fixedDeltaTime);
//        rb.MoveRotation(newAngle);
//    }

//    // --- 葉子速度邏輯 (保持不變) ---
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Leaves"))
//        {
//            if (leavesCount == 0)
//            {
//                currentSpeed = baseSpeed * (1 - speedReductionRate);
//                //Debug.Log("進入葉子範圍，速度減至: " + currentSpeed);
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
//                //Debug.Log("離開葉子範圍，恢復速度: " + currentSpeed);
//            }
//        }
//    }
//}

using StarveIO.Data;
using StarveIO.Input;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    private Vector2 _mouseScreenPos;
    private Camera _mainCamera;
    private InventoryManager _inventoryManager;
    private WeaponHandler _weaponHandler;
    private Rigidbody2D rb;
    private int leavesCount = 0;
    private Vector2 _moveInput;
    private float targetRotation;
    //public ItemData testSword;
    [Header("輸入設定")]
    [SerializeField] private InputReader _inputReader;

    [Header("移動設定")]
    public float baseSpeed = 5f;
    public float speedReductionRate = 0.5f;
    private float currentSpeed;

    [Header("固定攻擊/採集範圍")]
    [SerializeField] private Vector2 capsuleSize = new Vector2(1.2f, 2.0f);
    [SerializeField] private float capsuleOffset = 1.0f;
    [SerializeField] private CapsuleDirection2D capsuleDirection = CapsuleDirection2D.Vertical;
    [SerializeField] public LayerMask interactableLayers; // 包含 Resource 與 Monster

    [Header("基礎戰鬥參數 (空手)")]
    [SerializeField] private int _handDamage = 5;
    [SerializeField] private float _handCooldown = 0.6f;

    [Header("當前裝備狀態")]
    private int _currentEquippedDamage;
    private float _currentEquippedCooldown;
    private float _nextAttackTime = 0f;

    [Header("自動切換工具")]
    public bool useAutoToolSelection = true;


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

        _weaponHandler = GetComponent<WeaponHandler>();
    }

    // --- 外部介面：讓武器或裝備管理器更新玩家數值 ---
    public void UpdateWeaponStats(int dmg, float cd)
    {
        _currentEquippedDamage = dmg;
        _currentEquippedCooldown = cd;
        Debug.Log($"武器已更新：傷害 {dmg}, 冷卻 {cd}s");
    }

    public void OnMove(Vector2 direction) => _moveInput = direction;
    private void OnLook(Vector2 mousePos) => _mouseScreenPos = mousePos;

    private void OnAttack()
    {
        // ui inteaction penetration
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 決定當前冷卻時間
        float cooldown = (_weaponHandler.currentWeapon != null) ? _currentEquippedCooldown : _handCooldown;

        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + cooldown;

        ExecuteInteraction();
    }

    public void ExecuteAttackDetection()
    {
        var currentCategory = (_weaponHandler.currentWeapon != null) ?
                                _weaponHandler.currentWeapon.category : ItemCategory.None;

        // 1. 計算膠囊中心點 (使用動態 offset)
        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
        float angle = transform.eulerAngles.z;

        // 2. 物理偵測 (使用動態 size)
        Collider2D[] hitObjects = Physics2D.OverlapCapsuleAll(center, capsuleSize, capsuleDirection, angle, interactableLayers);

        foreach (Collider2D hit in hitObjects)
        {
            if (hit.TryGetComponent<ResourceNode>(out ResourceNode node))
            {
                // 這裡我們假設 ResourceNode 有一個接收傷害的方法 TakeDamage
                // node.TakeDamage(_currentDamage); 

                // 目前暫時維持你原本的採集邏輯，但使用 _currentDamage (如果採集量與傷害掛鉤的話)
                ItemData gatheredItem = node.GetItemData();
                int gatheredAmount = node.GatherResource(); // 你可以在這裡把傷害傳進去

                if (gatheredAmount > 0)
                {
                    bool success = _inventoryManager.AddItem(gatheredItem, gatheredAmount);
                    if (!success) Debug.Log("背包已滿！");
                }
            }
        }
    }

    public void ExecuteInteraction()
    {
        ItemCategory currentCategory = (_weaponHandler.currentWeapon != null)
                                       ? _weaponHandler.currentWeapon.category
                                       : ItemCategory.None;

        // 使用固定的中心點與大小
        Vector2 center = (Vector2)transform.position + (Vector2)transform.up * capsuleOffset;
        Collider2D[] hits = Physics2D.OverlapCapsuleAll(center, capsuleSize, capsuleDirection, transform.eulerAngles.z, interactableLayers);

        foreach (Collider2D hit in hits)
        {
            // --- 資源採集 ---
            if (hit.TryGetComponent<ResourceNode>(out ResourceNode node))
            {
                //if (currentCategory == ItemCategory.Weapon) continue; // 武器不准採集
                Debug.Log("採集中!");
                ItemData gatheredItem = node.GetItemData();
                int gatheredAmount = node.GatherResource(); // 你可以在這裡把傷害傳進去

                if (gatheredAmount > 0)
                {
                    bool success = _inventoryManager.AddItem(gatheredItem, gatheredAmount);
                    if (!success) UIEventManager.TriggerNotify("Inventory is Full!");
                }
            }

            // --- 怪物戰鬥 (預留) ---
            //if (hit.TryGetComponent<MonsterBase>(out var monster))
            //{
            //    if (currentCategory == ItemCategory.Weapon || currentCategory == ItemCategory.None)
            //    {
            //        int dmg = (currentCategory == ItemCategory.Weapon) ? _currentEquippedDamage : _handDamage;
            //        // monster.TakeDamage(dmg);
            //    }
            //}
        }
    }

    private void Update() => RotateTowardsMouse();

    private void RotateTowardsMouse()
    {
        Vector3 mousePos = _mouseScreenPos;
        mousePos.z = -_mainCamera.transform.position.z;
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
        Vector2 lookDir = (Vector2)worldPos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        targetRotation = angle;
    }

    void FixedUpdate()
    {
        if (_moveInput.sqrMagnitude > 0.01f)
            rb.MovePosition(rb.position + currentSpeed * Time.fixedDeltaTime * _moveInput.normalized);
        else
            rb.linearVelocity = Vector2.zero;

        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetRotation, 720f * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }

    // Gizmos 預覽也改成使用動態變數，方便你在換武器時看到範圍變化
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

    // --- 葉子邏輯 ---
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