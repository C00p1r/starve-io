using Unity.Cinemachine; // 必須引用，才能控制 Confiner
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("地圖與資源")]
    public Tilemap groundTilemap;
    public TileBase grassTile;
    public TileBase snowTile;
    public GameObject treePrefab;
    public GameObject rockPrefab;

    [Header("大小設定")]
    public int width = 100;
    public int height = 100;

    [Header("邊界引用")]
    public PolygonCollider2D mapBoundsCollider; // 拖入你的 MapBounds 物件
    public EdgeCollider2D physicalBoundaryCollider; // 專門擋玩家的
    public CinemachineConfiner2D confiner; // 拖入你的 CM vcam1 (包含 Confiner 組件的那個)

    [Header("機率")]
    [Range(0, 1)] public float treeDensity = 0.05f;
    [Range(0, 1)] public float rockDensity = 0.02f;


    void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        groundTilemap.ClearAllTiles();

        // 1. 生成地圖與資源
        int midY = height / 2;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase selectedTile = (y >= midY) ? snowTile : grassTile;
                Vector3Int tilePos = new Vector3Int(x - width / 2, y - height / 2, 0);
                groundTilemap.SetTile(tilePos, selectedTile);

                Vector3 worldPos = groundTilemap.GetCellCenterWorld(tilePos);
                SpawnRandomResource(worldPos);
            }
        }

        // 2. 自動更新邊界
        UpdateMapBounds();
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

    void SpawnRandomResource(Vector3 pos)
    {
        float rand = Random.value;
        if (rand < treeDensity)
        {
            Instantiate(treePrefab, pos, Quaternion.identity, transform);
        }
        else if (rand < treeDensity + rockDensity)
        {
            Instantiate(rockPrefab, pos, Quaternion.identity, transform);
        }
    }
}