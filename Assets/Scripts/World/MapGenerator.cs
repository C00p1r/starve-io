using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("層級管理")]
    public Transform resourceParent; // 在 Inspector 建立一個空物件並拖入

    [Header("地圖底層")]
    public Tilemap groundTilemap;
    public TileBase grassTile;
    public TileBase snowTile;

    [Header("基礎資源 Prefabs")]
    public GameObject treePrefab;
    public GameObject tree2Prefab;
    public GameObject rockPrefab;
    public GameObject goldPrefab;
    public GameObject diamondPrefab;
    public GameObject fruitPrefab;
    [Header("生成物縮放範圍")]
    public float minScale;
    public float maxScale;
    [Header("地圖大小設定")]
    public int width = 100;
    public int height = 100;

    [Header("邊界引用")]
    public PolygonCollider2D mapBoundsCollider;
    public EdgeCollider2D physicalBoundaryCollider;
    public CinemachineConfiner2D confiner;

    [Header("出現機率 (0~1)")]
    [Range(0, 1)] public float treeDensity = 0.04f;
    [Range(0, 1)] public float tree2Density = 0.01f;
    [Range(0, 1)] public float rockDensity = 0.02f;
    [Range(0, 1)] public float goldDensity = 0.01f;
    [Range(0, 1)] public float diamondDensity = 0.002f;
    [Range(0, 1)] public float fruitDensity = 0.03f;

    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        groundTilemap.ClearAllTiles();

        // 清除舊的資源物件 (防止重複生成)
        if(resourceParent != null)
        {
            foreach (Transform child in resourceParent)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("未設定 Resource Parent，資源將生成在根目錄且不會被自動清除。");
        }

        int midY = height / 2;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 1. 決定地塊類型
                bool isSnow = (y >= midY);
                TileBase selectedTile = isSnow ? snowTile : grassTile;
                Vector3Int tilePos = new Vector3Int(x - width / 2, y - height / 2, 0);
                groundTilemap.SetTile(tilePos, selectedTile);

                // 2. 獲取世界座標
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(tilePos);

                // 3. 嘗試生成資源 (傳入地塊類型資訊)
                SpawnRandomResource(worldPos, isSnow);
            }
        }

        UpdateMapBounds();
    }

    void SpawnRandomResource(Vector3 pos, bool isSnow)
    {
        float rand = Random.value;

        // 使用累積機率判斷
        float cumulative = 0f;

        // 決定要生成的物件類型
        GameObject prefabToSpawn = null;

        if (rand < (cumulative += treeDensity)) prefabToSpawn = treePrefab;
        else if (rand < (cumulative += tree2Density)) prefabToSpawn = tree2Prefab;
        else if (rand < (cumulative += rockDensity)) prefabToSpawn = rockPrefab;
        else if (rand < (cumulative += goldDensity)) prefabToSpawn = goldPrefab;
        else if (rand < (cumulative += diamondDensity)) prefabToSpawn = diamondPrefab;
        else if (rand < (cumulative += (isSnow ? fruitDensity * 0.2f : fruitDensity))) prefabToSpawn = fruitPrefab;

        if (prefabToSpawn != null)
        {
            // 1. 確保 Z 軸座標為 0，解決你之前提到的消失問題
            Vector3 spawnPos = new Vector3(pos.x, pos.y, 0f);

            // 2. 實例化物件
            GameObject instance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, resourceParent);

            // 3. 執行隨機縮放
            float randomScale = Random.Range(minScale, maxScale);
            instance.transform.localScale = Vector3.one * randomScale;

            // 4. (選配) 加入一點隨機旋轉讓地圖更亂一點 (僅限 2D)
            // instance.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

    }

    void UpdateMapBounds()
    {
        float xMin = -width / 2f;
        float xMax = width / 2f;
        float yMin = -height / 2f;
        float yMax = height / 2f;

        if (mapBoundsCollider != null)
        {
            Vector2[] path = new Vector2[4];
            path[0] = new Vector2(xMin, yMin);
            path[1] = new Vector2(xMax, yMin);
            path[2] = new Vector2(xMax, yMax);
            path[3] = new Vector2(xMin, yMax);
            mapBoundsCollider.SetPath(0, path);

            if (confiner != null)
            {
                confiner.InvalidateBoundingShapeCache();
            }
        }

        if (physicalBoundaryCollider != null)
        {
            Vector2[] points = new Vector2[5];
            points[0] = new Vector2(xMin, yMin);
            points[1] = new Vector2(xMax, yMin);
            points[2] = new Vector2(xMax, yMax);
            points[3] = new Vector2(xMin, yMax);
            points[4] = new Vector2(xMin, yMin);
            physicalBoundaryCollider.points = points;
        }
    }
}