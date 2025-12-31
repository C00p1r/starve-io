// 2025/12/30 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEditor;
using UnityEngine;
// 2025/12/30 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.


public class CreatureSpawner : MonoBehaviour
{

    [Header("¼h¯ÅºÞ²z")]
    public Transform resourceParent;

    public GameObject wolvesPrefab; // Reference to the wolves prefab
    public GameObject spidersPrefab; // Reference to the spiders prefab
    public int maxCreatures = 20; // Maximum number of creatures on the map
    public int mapWidth = 50; // Width of the map
    public int mapHeight = 50; // Height of the map
    public float frontZPosition = -1f; // Z position to bring creatures to the front

    private int currentCreatureCount;

    void Start()
    {
        SpawnInitialCreatures();
    }

    void Update()
    {
        CheckAndSpawnCreatures();
    }

    void SpawnInitialCreatures()
    {
        for (int i = 0; i < maxCreatures; i++)
        {
            SpawnRandomCreature();
        }
    }

    void CheckAndSpawnCreatures()
    {
        currentCreatureCount = GameObject.FindObjectsOfType<MonsterBehavior>().Length;

        if (currentCreatureCount < maxCreatures)
        {
            SpawnRandomCreature();
        }
    }

    void SpawnRandomCreature()
    {
        Vector3 randomPosition = new Vector3(
            Random.Range(-mapWidth / 2f, mapWidth / 2f),
            Random.Range(-mapHeight / 2f, mapHeight / 2f),
            frontZPosition // Set Z position to bring the creature to the front
        );

        GameObject creaturePrefab = Random.value < 0.5f ? wolvesPrefab : spidersPrefab;
        Instantiate(creaturePrefab, randomPosition, Quaternion.identity, resourceParent);
    }
}
