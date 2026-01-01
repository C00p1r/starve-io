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
    public float attackRange = 1.2f;
    public float attackCoolDown = 1f; // 攻擊冷卻時間
    public Color alertColor = new Color(1f, 0f, 0f, 1f); // 受傷閃紅
    public float alertDuration = 0.3f;
    private Color originalColor = Color.white;
    private float contactTimer = 0f;

    [Header("Health Settings")]
    public int maxHealth = 100; // Maximum health of the wolves
    private int currentHealth; // Current health of the wolves
    
    [Header("移動與旋轉")]
    public float randomMoveInterval = 2f;
    public float randomMoveDistance = 1f;
    public float hitboxScale = 1.3f;
    public float facingOffsetDegrees = 180f;
    [Header("受傷回饋 (新增)")]
    public Color monsterFlashColor = Color.red;
    public float monsterFlashDuration = 0.2f;
    private SpriteRenderer monsterSR;
    private Color monsterOriginalColor;
    private Coroutine flashCoroutine;

    private Vector3 randomTarget;
    private float randomMoveTimer;
    private BoxCollider2D hitbox;
    private Rigidbody2D rb;
    private Collider2D monsterCollider;
    private Collider2D playerCollider;
    private Vector2 moveDirection;
    private PlayerStats playerStats;
    private PlayerFeedback playerFeedback;

    void Awake()
    {
        // --- 新增：初始化顏色回饋 ---
        monsterSR = GetComponent<SpriteRenderer>();
        if (monsterSR != null)
        {
            monsterOriginalColor = monsterSR.color; // 存下最初的顏色 (重要！)
        }

        // --- 原有的 Rigidbody 設定 ---
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

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

        currentHealth = maxHealth;
    }

    void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerCollider = playerObject.GetComponent<Collider2D>();
            playerStats = playerObject.GetComponent<PlayerStats>();
            playerFeedback = playerObject.GetComponent<PlayerFeedback>();
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

        HandleAttack(distanceToPlayer);
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

    void HandleAttack(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            contactTimer = 0f;
            return;
        }

        contactTimer -= Time.deltaTime;
        if (contactTimer > 0f)
            return;

        if (playerFeedback != null)
            playerFeedback.TriggerDamageFlash(alertColor, alertDuration);

        if (playerStats != null)
            playerStats.TakeDamage(damagePerHit);

        contactTimer = attackCoolDown;
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

    // --- 修改：TakeDamage 增加閃紅光邏輯 ---
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! HP: {currentHealth}");

        // 觸發閃紅光
        if (monsterSR != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine); // 如果還在閃，先停止舊的
            flashCoroutine = StartCoroutine(MonsterFlashRoutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- 新增：怪物閃爍協程 ---
    IEnumerator MonsterFlashRoutine()
    {
        monsterSR.color = monsterFlashColor;
        yield return new WaitForSeconds(monsterFlashDuration);
        monsterSR.color = monsterOriginalColor; // 變回 Awake 存下的原始顏色
        flashCoroutine = null;
    }

    void Die()
    {
        // 確保死掉時不會因為正在協程中而導致報錯
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);

        Debug.Log($"{gameObject.name} has died!");

        // 掉落物邏輯
        if (gameObject.name.Contains("wolves") && meetPrefab != null)
            Instantiate(meetPrefab, transform.position, Quaternion.identity);
        else if (gameObject.name.Contains("spider") && threadPrefab != null)
            Instantiate(threadPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
