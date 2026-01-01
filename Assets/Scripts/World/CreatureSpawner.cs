// 2025/12/30 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEditor;
using UnityEngine;
// 2025/12/30 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.


public class CreatureSpawner : MonoBehaviour
{

    [Header("層級管理")]
    public Transform resourceParent;

    public GameObject wolvesPrefab; // Reference to the wolves prefab
    public GameObject spidersPrefab; // Reference to the spiders prefab
    public int maxCreatures = 20; // Maximum number of creatures on the map
    public int mapWidth = 50; // Width of the map
    public int mapHeight = 50; // Height of the map
    public float frontZPosition = 0f; // Z position to bring creatures to the front
    public bool spawnOnlyAtNight = true;
    public bool despawnAtDay = true;

    private int currentCreatureCount;
    private TimeManager timeManager;

    void Start()
    {
        timeManager = TimeManager.Instance ?? FindObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            timeManager.OnDayStateChanged += HandleDayStateChanged;
        }

        if (!spawnOnlyAtNight || IsNight())
            SpawnCreaturesToMax();
    }

    void Update()
    {
        if (!spawnOnlyAtNight || IsNight())
            CheckAndSpawnCreatures();
    }

    void OnDestroy()
    {
        if (timeManager != null)
            timeManager.OnDayStateChanged -= HandleDayStateChanged;
    }

    void SpawnCreaturesToMax()
    {
        currentCreatureCount = GameObject.FindObjectsOfType<MonsterBehavior>().Length;
        while (currentCreatureCount < maxCreatures)
        {
            SpawnRandomCreature();
            currentCreatureCount++;
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

    void HandleDayStateChanged(bool isDay)
    {
        if (!spawnOnlyAtNight)
            return;

        if (isDay && despawnAtDay)
        {
            DespawnAllCreatures();
            return;
        }

        if (!isDay)
            SpawnCreaturesToMax();
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

    void DespawnAllCreatures()
    {
        foreach (var monster in FindObjectsOfType<MonsterBehavior>())
        {
            monster.Despawn();
        }
    }

    bool IsNight()
    {
        return timeManager == null || timeManager.IsNight;
    }
}
