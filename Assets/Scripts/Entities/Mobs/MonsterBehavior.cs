// 2025/12/31 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.


using UnityEditor;
using UnityEngine;
// 2025/12/30 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.


public class MonsterBehavior : MonoBehaviour
{
    public Transform player; // Reference to the player's Transform
    public GameObject meetPrefab; // Wolves drop this object
    public GameObject threadPrefab; // Spiders drop this object
    public float followDistance = 5f; // Distance within which the monster follows the player
    public float moveSpeed = 2f; // Movement speed
    public float randomMoveInterval = 2f; // Interval for random movement
    public float randomMoveDistance = 1f; // Distance for random movement
    public float colorChangeInterval = 1.5f; // Interval for changing the background color
    public Color alertColor = new Color(1f, 0f, 0f, 1f); // Red color
    public float alertDuration = 0.3f; // Duration of the red color

    private Vector3 randomTarget;
    private float randomMoveTimer;
    private float colorChangeTimer;
    private Camera mainCamera;
    private Color originalColor = new Color(1f, 1f, 1f, 1f);

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

        // Get the main camera and store its original background color
        mainCamera = Camera.main;

        randomMoveTimer = randomMoveInterval;
        colorChangeTimer = colorChangeInterval;
        SetRandomTarget();
    }

    void Update()
    {
        if (player == null || mainCamera == null) return;

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
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
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
    private float contactTimer = 0f; // 記錄接觸的時間

    void OnTriggerStay2D(Collider2D collision)
    {
        // 確保玩家的 SpriteRenderer 存在
        SpriteRenderer playerSpriteRenderer = player.GetComponent<SpriteRenderer>();

        // 檢查碰撞的物件是否是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            contactTimer -= Time.deltaTime;

            if (contactTimer <= 0)
            {
                if (playerSpriteRenderer != null)
                {
                    StartCoroutine(ChangeColorCoroutine());
                    contactTimer = colorChangeInterval; // 重置計時器
                }
            }
        }
    }

    System.Collections.IEnumerator ChangeColorCoroutine()
    {
        // 確保玩家的 SpriteRenderer 存在
        SpriteRenderer playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
        if (playerSpriteRenderer != null)
        {
            // 將玩家的顏色改為紅色
            playerSpriteRenderer.color = alertColor;
            yield return new WaitForSeconds(alertDuration);
            // 恢復玩家的原始顏色
            playerSpriteRenderer.color = originalColor;
        }
        else
        {
            Debug.LogError("Player does not具有SpriteRenderer元件！");
        }
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
