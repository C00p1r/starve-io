using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("層級管理")]
    public Transform resourceParent;

    [Header("地圖與資源")]
    public Tilemap groundTilemap;
    public TileBase grassTile;
    public TileBase snowTile;

    [Header("單一水池 Rule Tile")]
    public TileBase waterRuleTile; // 這裡拖入你唯一的 Rule Tile 檔案

    [Header("資源 Prefabs")]
    public GameObject treePrefab;
    public GameObject tree2Prefab;
    public GameObject fruitTreePrefab;
    public GameObject rockPrefab;
    public GameObject goldPrefab;
    public GameObject diamondPrefab;


    //[Header("湖泊生成設定")]
    //[Range(0, 1)] public float lakeProbability = 0.3f;
    //public float lakeAreaScale = 2f;
    //[Range(0, 1)] public float waterThreshold = 0.6f;
    //public float noiseScale = 10f;
    private float offsetX;
    private float offsetY;

    //[Tooltip("如果一格水周圍的水格少於這個數量，它就會變成陸地。建議設為 3 或 4")]
    //[Range(0, 8)] public int waterSmoothThreshold = 4;

    [Header("生成物縮放範圍")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [Header("地圖大小")]
    public int width = 100;
    public int height = 100;

    [Header("攝影機與邊界")]
    public PolygonCollider2D mapBoundsCollider;
    public EdgeCollider2D physicalBoundaryCollider;
    public CinemachineConfiner2D confiner;

    [Header("生成機率")]
    [Range(0, 1)] public float treeDensity = 0.04f;
    [Range(0, 1)] public float tree2Density = 0.01f;
    [Range(0, 1)] public float fruitTreeDensity = 0.02f;
    [Range(0, 1)] public float rockDensity = 0.02f;
    [Range(0, 1)] public float goldDensity = 0.005f;
    [Range(0, 1)] public float diamondDensity = 0.002f;

    void Start()
    {
        offsetX = Random.Range(0, 9999f);
        offsetY = Random.Range(0, 9999f);
        GenerateMap();
    }

    [Header("矩形湖泊複合設定")]
    [Tooltip("地圖上嘗試生成湖泊區域的次數")]
    public int lakeSpawnAttempts = 10;
    [Tooltip("每次嘗試成功生成的機率 (0~1)")]
    [Range(0, 1)] public float lakeSpawnProbability = 0.6f;
    [Tooltip("一個湖泊區域由多少個隨機矩形組成 (複合效果)")]
    public int rectsPerLake = 3;

    [Header("矩形大小設定")]
    public int minRectSize = 3;
    public int maxRectSize = 8;
    [Tooltip("複合矩形之間的偏移範圍 (數值越大，湖泊形狀越散)")]
    public int compoundOffset = 4;

    public void GenerateMap()
    {
        groundTilemap.ClearAllTiles();
        int midY = height / 2;

        // 1. 初始化地圖數據
        int[,] mapData = new int[width, height];

        // 2. 複合矩形生成邏輯
        for (int i = 0; i < lakeSpawnAttempts; i++)
        {
            // 判定本次嘗試是否成功生成
            if (Random.value > lakeSpawnProbability) continue;

            // 隨機決定這個「湖泊群」的中心點 (限制在草地 midY 以下)
            int centerX = Random.Range(10, width - 10);
            int centerY = Random.Range(10, midY - 10);

            // 在中心點附近生成數個重疊的矩形，形成複合形狀
            for (int j = 0; j < rectsPerLake; j++)
            {
                int lakeWidth = Random.Range(minRectSize, maxRectSize);
                int lakeHeight = Random.Range(minRectSize, maxRectSize);

                // 讓每個矩形在中心點附近隨機偏移
                int startX = centerX + Random.Range(-compoundOffset, compoundOffset);
                int startY = centerY + Random.Range(-compoundOffset, compoundOffset);

                // 填充矩形數據
                for (int x = startX; x < startX + lakeWidth; x++)
                {
                    for (int y = startY; y < startY + lakeHeight; y++)
                    {
                        // 邊界檢查，防止超出地圖數組
                        if (x >= 0 && x < width && y >= 0 && y < height && y < midY)
                        {
                            mapData[x, y] = 1;
                        }
                    }
                }
            }
        }

        // 3. 平滑處理 (選擇性使用)
        // 如果你想要完全死板的矩形，可以註解掉這行。
        // 如果希望複合矩形的連接處更自然，可以執行 1 次平滑化。
        // mapData = SmoothMap(mapData); 

        // 4. 正式繪製
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x - width / 2, y - height / 2, 0);
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(tilePos);

                if (mapData[x, y] == 1)
                {
                    groundTilemap.SetTile(tilePos, waterRuleTile);
                }
                else
                {
                    groundTilemap.SetTile(tilePos, (y >= midY) ? snowTile : grassTile);
                    SpawnRandomResource(worldPos);
                }
            }
        }

        groundTilemap.RefreshAllTiles();
        UpdateMapBounds();
    }


    void SpawnRandomResource(Vector3 pos)
    {
        float rand = Random.value;
        float cumulative = 0f;
        GameObject prefabToSpawn = null;

        if (rand < (cumulative += treeDensity)) prefabToSpawn = treePrefab;
        else if (rand < (cumulative += tree2Density)) prefabToSpawn = tree2Prefab;
        else if (rand < (cumulative += rockDensity)) prefabToSpawn = rockPrefab;
        else if (rand < (cumulative += goldDensity)) prefabToSpawn = goldPrefab;
        else if (rand < (cumulative += diamondDensity)) prefabToSpawn = diamondPrefab;
        else if (rand < (cumulative += fruitTreeDensity)) prefabToSpawn = fruitTreePrefab;

        if (prefabToSpawn != null)
        {
            SpawnObject(prefabToSpawn, pos);
        }
    }

    void SpawnObject(GameObject prefab, Vector3 pos)
    {
        // 確保 Z 軸為 0，避免碰撞偵測失效
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0f);
        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, resourceParent);

        float randomScale = Random.Range(minScale, maxScale);
        instance.transform.localScale = Vector3.one * randomScale;
    }

    void UpdateMapBounds()
    {
        float xMin = -width / 2f;
        float xMax = width / 2f;
        float yMin = -height / 2f;
        float yMax = height / 2f;

        // --- 1. 更新攝影機邊界 (PolygonCollider2D) ---
        if (mapBoundsCollider != null)
        {
            Vector2[] path = new Vector2[4];
            path[0] = new Vector2(xMin, yMin);
            path[1] = new Vector2(xMax, yMin);
            path[2] = new Vector2(xMax, yMax);
            path[3] = new Vector2(xMin, yMax);

            mapBoundsCollider.SetPath(0, path);

            // 關鍵：通知 Cinemachine 邊界形狀已改變，請刷新緩存
            if (confiner != null)
            {
                confiner.BoundingShape2D = null;

                // 2. 重新設定頂點 (確保 Collider 已經更新)
                mapBoundsCollider.SetPath(0, path);

                // 3. 重新接回連結
                confiner.BoundingShape2D = mapBoundsCollider;

                // 4. 最後補一發刷新 (確保萬無一失)
                confiner.InvalidateBoundingShapeCache();
            }
        }

        // --- 2. 更新玩家物理邊界 (EdgeCollider2D) ---
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

    //void SpawnRandomResource(Vector3 pos)
    //{
    //    float rand = Random.value;
        
    //    // 使用累積機率判斷
    //    float cumulative = 0f;

    //    // 決定要生成的物件類型
    //    GameObject prefabToSpawn = null;

    //    if (rand < (cumulative += treeDensity)) prefabToSpawn = treePrefab;
    //    else if (rand < (cumulative += tree2Density)) prefabToSpawn = tree2Prefab;
    //    else if (rand < (cumulative += rockDensity)) prefabToSpawn = rockPrefab;
    //    else if (rand < (cumulative += goldDensity)) prefabToSpawn = goldPrefab;
    //    else if (rand < (cumulative += diamondDensity)) prefabToSpawn = diamondPrefab;
    //    else if (rand < (cumulative += fruitTreeDensity)) prefabToSpawn = fruitTreePrefab;

    //    if (prefabToSpawn != null)
    //    {
    //        // 1. 確保 Z 軸座標為 0，解決你之前提到的消失問題
    //        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0f);

    //        // 2. 實例化物件
    //        GameObject instance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, resourceParent);

    //        // 3. 執行隨機縮放
    //        float randomScale = Random.Range(minScale, maxScale);
    //        instance.transform.localScale = Vector3.one * randomScale;

    //        // 4. (選配) 加入一點隨機旋轉讓地圖更亂一點 (僅限 2D)
    //        // instance.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
    //    }
    //}
}
