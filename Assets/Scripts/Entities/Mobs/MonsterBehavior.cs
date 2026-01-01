using UnityEngine;
using System.Collections;

public class MonsterBehavior : MonoBehaviour
{
    [Header("基礎設定")]
    public Transform player;
    public float moveSpeed = 2f;
    public float followDistance = 5f;
    public float stopDistance = 1f;

    [Header("掉落物")]
    public GameObject meetPrefab;
    public GameObject threadPrefab;

    [Header("攻擊與變色設定 (新增)")]
    public float damagePerHit = 10f;
    public float attackCoolDown = 1f; // 攻擊冷卻時間
    public Color alertColor = new Color(1f, 0f, 0f, 1f); // 受傷閃紅
    public float alertDuration = 0.3f;
    private Color originalColor = Color.white;
    private float contactTimer = 0f;

    [Header("移動與旋轉")]
    public float randomMoveInterval = 2f;
    public float randomMoveDistance = 1f;
    public float hitboxScale = 1.3f;
    public float facingOffsetDegrees = 180f;

    private Vector3 randomTarget;
    private float randomMoveTimer;
    private BoxCollider2D hitbox;
    private Rigidbody2D rb;

    void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>(); // 確保抓到 Rigidbody

        if (hitbox != null && hitboxScale > 0f && !Mathf.Approximately(hitboxScale, 1f))
        {
            hitbox.size = hitbox.size * hitboxScale;
        }
    }

    void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        randomMoveTimer = randomMoveInterval;
        SetRandomTarget();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= followDistance)
        {
            FollowPlayer();
        }
        else
        {
            RandomMove();
        }
    }

    // --- 修改：使用 rb.MovePosition 以支援物理碰撞 ---
    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            // 推薦做法：直接給予速度，物理引擎會自動處理碰撞阻擋
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // 停下
        }

        FaceDirection(direction);
    }

    void RandomMove()
    {
        randomMoveTimer -= Time.deltaTime;

        if (randomMoveTimer <= 0)
        {
            SetRandomTarget();
            randomMoveTimer = randomMoveInterval;
        }

        Vector3 direction = (randomTarget - transform.position).normalized;
        rb.MovePosition(rb.position + (Vector2)direction * moveSpeed * Time.deltaTime);
        FaceDirection(direction);

        if (Vector3.Distance(transform.position, randomTarget) < 0.1f)
        {
            SetRandomTarget();
        }
    }

    // --- 新增：處理與玩家的碰撞傷害 ---
    // 在 MonsterBehavior.cs 裡的 OnTriggerStay2D 中修改
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            contactTimer -= Time.deltaTime;

            if (contactTimer <= 0)
            {
                // 1. 呼叫玩家的變色回饋
                PlayerFeedback feedback = collision.GetComponent<PlayerFeedback>();
                if (feedback != null)
                {
                    feedback.TriggerDamageFlash(alertColor, alertDuration);
                }

                // 2. 扣血邏輯
                PlayerStats stats = collision.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(damagePerHit);
                }

                contactTimer = attackCoolDown;
            }
        }
    }
    void SetRandomTarget()
    {
        float randomX = Random.Range(-randomMoveDistance, randomMoveDistance);
        float randomY = Random.Range(-randomMoveDistance, randomMoveDistance);
        randomTarget = transform.position + new Vector3(randomX, randomY, 0);
    }

    void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + facingOffsetDegrees;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnDestroy()
    {
        // 只有在遊戲運行中且物件被摧毀（死亡）時才掉落
        if (!gameObject.scene.isLoaded) return;

        if (gameObject.name.Contains("wolves") && meetPrefab != null)
        {
            Instantiate(meetPrefab, transform.position, Quaternion.identity);
        }
        else if (gameObject.name.Contains("spider") && threadPrefab != null)
        {
            Instantiate(threadPrefab, transform.position, Quaternion.identity);
        }
    }
}
