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


    [Header("湖泊生成設定")]
    [Range(0, 1)] public float lakeProbability = 0.3f;
    public float lakeAreaScale = 2f;
    [Range(0, 1)] public float waterThreshold = 0.6f;
    public float noiseScale = 10f;
    private float offsetX;
    private float offsetY;

    [Tooltip("如果一格水周圍的水格少於這個數量，它就會變成陸地。建議設為 3 或 4")]
    [Range(0, 8)] public int waterSmoothThreshold = 4;

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
    public void GenerateMap()
    {
        groundTilemap.ClearAllTiles();
        int midY = height / 2;

        // 第一階段：原始雜訊生成
        int[,] mapData = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float maskX = (float)x / width * lakeAreaScale + offsetX;
                float maskY = (float)y / height * lakeAreaScale + offsetY;
                float lakeMask = Mathf.PerlinNoise(maskX, maskY);

                float xCoord = (float)x / width * noiseScale + offsetX;
                float yCoord = (float)y / height * noiseScale + offsetY;
                float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);

                bool isSnowRegion = y >= midY;
                mapData[x, y] = (!isSnowRegion && lakeMask < lakeProbability && noiseValue > waterThreshold) ? 1 : 0;
            }
        }

        // --- 關鍵修正：執行多次平滑化以確保連貫性 ---
        // 執行 3 次平滑化會讓水池邊緣變得非常圓滑且連貫
        for (int i = 0; i < 3; i++)
        {
            mapData = SmoothMap(mapData);
        }

        // 第三階段：繪製與生成資源
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

    // 獨立出來的平滑化函數
    int[,] SmoothMap(int[,] oldMap)
    {
        int[,] newMap = (int[,])oldMap.Clone();
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int neighbors = CountWaterNeighbors(oldMap, x, y);

                // 核心規則：
                // 如果周圍水很多(>4)，這格就變成水 (填補空洞)
                // 如果周圍水太少(<4)，這格就變成草地 (消除孤立點)
                if (neighbors > 4) newMap[x, y] = 1;
                else if (neighbors < 4) newMap[x, y] = 0;
            }
        }
        return newMap;
    }

    // 計算周圍 8 格水的數量 (輔助函數保持不變)
    int CountWaterNeighbors(int[,] map, int centerX, int centerY)
    {
        if (centerX <= 0 || centerX >= width - 1 || centerY <= 0 || centerY >= height - 1) return 0;
        int count = 0;
        for (int x = centerX - 1; x <= centerX + 1; x++)
        {
            for (int y = centerY - 1; y <= centerY + 1; y++)
            {
                if (x == centerX && y == centerY) continue;
                if (map[x, y] == 1) count++;
            }
        }
        return count;
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
