// 2025/12/30 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEditor;
using UnityEngine;

public class MonsterBehavior : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform
    public GameObject meetPrefab; // Wolves drop this object
    public GameObject threadPrefab; // Spiders drop this object
    public float followDistance = 5f; // Distance within which the monster follows the player
    public float stopDistance = 1f; // Distance to keep so the monster doesn't overlap the player
    public float moveSpeed = 2f; // Movement speed
    public float randomMoveInterval = 2f; // Interval for random movement
    public float randomMoveDistance = 1f; // Distance for random movement
    public float hitboxScale = 1.3f; // Scales the trigger hitbox to land hits before contact
    public float facingOffsetDegrees = 180f; // Sprite faces down by default, offset to face target

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
        // Automatically find the Player GameObject and assign its Transform
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
        else
        {
            Debug.LogError("Player GameObject not found in the scene!");
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

        if (distanceToPlayer <= followDistance)
        {
            moveDirection = GetFollowDirection(distanceToPlayer);
        }
        else
        {
            moveDirection = GetRandomDirection();
        }
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

    void SetRandomTarget()
    {
        float randomX = Random.Range(-randomMoveDistance, randomMoveDistance);
        float randomY = Random.Range(-randomMoveDistance, randomMoveDistance);
        Vector2 origin = rb != null ? rb.position : (Vector2)transform.position;
        randomTarget = origin + new Vector2(randomX, randomY);
    }

    void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + facingOffsetDegrees;
        if (rb != null)
            rb.MoveRotation(angle);
        else
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnDestroy()
    {
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
