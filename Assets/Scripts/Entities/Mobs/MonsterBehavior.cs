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
    public float colorChangeInterval = 1.5f; // 攻擊冷卻時間
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
    private Collider2D monsterCollider;
    private Collider2D playerCollider;
    private Vector2 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        monsterCollider = GetComponent<Collider2D>();
        hitbox = GetComponent<BoxCollider2D>();
        if (monsterCollider != null)
            monsterCollider.isTrigger = false;

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
            playerCollider = playerObject.GetComponent<Collider2D>();
            if (monsterCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(monsterCollider, playerCollider, true);
            }
        }

        randomMoveTimer = randomMoveInterval;
        SetRandomTarget();
    }

    void Update()
    {
        if (player == null)
        {
            moveDirection = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        moveDirection = distanceToPlayer <= followDistance
            ? GetFollowDirection(distanceToPlayer)
            : GetRandomDirection();
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Vector2 step = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + step);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    Vector2 GetFollowDirection(float distanceToPlayer)
    {
        if (distanceToPlayer <= stopDistance)
            return Vector2.zero;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        FaceDirection(direction);
        return direction;
    }

    Vector2 GetRandomDirection()
    {
        randomMoveTimer -= Time.deltaTime;

        if (randomMoveTimer <= 0)
        {
            SetRandomTarget();
            randomMoveTimer = randomMoveInterval;
        }

        Vector2 toTarget = (Vector2)randomTarget - rb.position;
        if (toTarget.sqrMagnitude < 0.01f)
        {
            SetRandomTarget();
            toTarget = (Vector2)randomTarget - rb.position;
        }

        Vector2 direction = toTarget.normalized;
        FaceDirection(direction);
        return direction;
    }

    // --- 新增：處理與玩家的碰撞傷害 ---
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            contactTimer -= Time.deltaTime;

            if (contactTimer <= 0)
            {
                // 1. 取得玩家的 Sprite 並變色
                SpriteRenderer playerSR = collision.GetComponent<SpriteRenderer>();
                if (playerSR != null)
                {
                    StartCoroutine(ChangeColorCoroutine(playerSR));
                }

                // 2. 核心：讓玩家扣血
                PlayerStats stats = collision.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.TakeDamage(damagePerHit);
                }

                contactTimer = colorChangeInterval; // 進入攻擊冷卻
            }
        }
    }

    IEnumerator ChangeColorCoroutine(SpriteRenderer sr)
    {
        Color oldColor = sr.color; // 存下原本顏色
        sr.color = alertColor;
        yield return new WaitForSeconds(alertDuration);
        sr.color = oldColor; // 變回原本顏色
    }

    void SetRandomTarget()
    {
        float randomX = Random.Range(-randomMoveDistance, randomMoveDistance);
        float randomY = Random.Range(-randomMoveDistance, randomMoveDistance);
        Vector2 origin = rb != null ? rb.position : (Vector2)transform.position;
        randomTarget = origin + new Vector2(randomX, randomY);
    }

    void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + facingOffsetDegrees;
        if (rb != null)
            rb.MoveRotation(angle);
        else
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
