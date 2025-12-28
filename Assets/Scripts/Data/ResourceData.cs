using NUnit.Framework.Interfaces;
using StarveIO.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceData", menuName = "StarveIO/World/Resource Data")]
public class ResourceData : ScriptableObject
{
    [Header("基礎設定")]
    public ItemData item;             // 產出的物品 (如：木頭 ItemData)
    public int maxStock = 20;         // 資源最大容量
    public int yieldPerHit = 2;       // 每次敲擊產量

    [Header("恢復設定")]
    public bool canRegenerate = true; // 是否自動恢復
    public float regenInterval = 5f;  // 恢復間隔 (秒)
    public int regenAmount = 1;       // 每次恢復量
}