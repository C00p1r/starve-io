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
    public float moveSpeed = 2f; // Movement speed
    public float randomMoveInterval = 2f; // Interval for random movement
    public float randomMoveDistance = 1f; // Distance for random movement

    private Vector3 randomTarget;
    private float randomMoveTimer;

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
