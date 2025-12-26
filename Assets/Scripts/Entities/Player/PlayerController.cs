using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float baseSpeed = 5f;       // 基礎速度
    public float speedReductionRate = 0.5f;  // 進入葉子要減去的速度比率
    private float currentSpeed;        // 當前實際速度

    private int leavesCount = 0;       // 目前重疊的葉子數量
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float targetRotation;

    public PlayerInput playerInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed; // 初始速度等於基礎速度

        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        // 旋轉邏輯 (修正後的 WASD 轉向)
        if (moveInput.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            targetRotation = angle + 90f;
        }
    }

    void FixedUpdate()
    {
        // 使用動態變化的 currentSpeed 移動
        rb.MovePosition(rb.position + moveInput.normalized * currentSpeed * Time.fixedDeltaTime);

        // 平滑轉向
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetRotation, 720f * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }

    // --- 核心：進入/離開 Trigger 的速度增減 ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Leaves"))
        {
            // 如果是進入第一片葉子，才執行減速
            if (leavesCount == 0)
            {
                currentSpeed *= 1-speedReductionRate;
                Debug.Log("進入葉子範圍，速度減至: " + currentSpeed);
            }
            leavesCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Leaves"))
        {
            leavesCount--;
            // 只有離開最後一片葉子時，才加回速度
            if (leavesCount <= 0)
            {
                leavesCount = 0;
                currentSpeed = baseSpeed;
                Debug.Log("離開葉子範圍，恢復速度: " + currentSpeed);
            }
        }
    }
}