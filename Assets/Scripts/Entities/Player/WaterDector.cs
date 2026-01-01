using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterDetector : MonoBehaviour
{
    private PlayerStats _playerStats;
    public Tilemap groundTilemap;

    [Header("Tile 素材引用")]
    public TileBase waterTile;
    public TileBase snowTile;

    void Start()
    {
        _playerStats = GetComponent<PlayerStats>();
        if (groundTilemap == null) groundTilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();
    }

    void Update()
    {
        if (groundTilemap == null || _playerStats == null) return;

        Vector3Int cellPosition = groundTilemap.WorldToCell(transform.position);
        TileBase currentTile = groundTilemap.GetTile(cellPosition);

        // 判斷水池 (恢復口渴，快速失溫)
        _playerStats.isStandingInWater = (currentTile == waterTile);

        // 判斷雪地 (普通失溫)
        _playerStats.isInSnow = (currentTile == snowTile);
    }
}
