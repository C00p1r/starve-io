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

    void Awake()
    {
        hitbox = GetComponent<BoxCollider2D>();
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

    void FollowPlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer <= 0.001f)
        {
            return;
        }

        Vector3 direction = toPlayer / distanceToPlayer;
        if (distanceToPlayer > stopDistance)
        {
            float step = moveSpeed * Time.deltaTime;
            float maxStep = distanceToPlayer - stopDistance;
            transform.position += direction * Mathf.Min(step, maxStep);
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
        transform.position += direction * moveSpeed * Time.deltaTime;
        FaceDirection(direction);

        if (Vector3.Distance(transform.position, randomTarget) < 0.1f)
        {
            SetRandomTarget();
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
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + facingOffsetDegrees;
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
