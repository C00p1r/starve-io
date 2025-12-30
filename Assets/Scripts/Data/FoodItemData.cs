using UnityEngine;
using StarveIO.Data; // 確保指向你的數據空間

[CreateAssetMenu(fileName = "New Food", menuName = "Inventory/Items/Food")]
public class FoodItemData : ItemData, IUsable
{
    [Header("食物效果")]
    public int hungerRestoration = 20; // 回復飢餓度
    //public int healthRestoration = 5;  // 回復血量

    // 實作 IUsable 介面
    public void Use(GameObject user)
    {
        // 找到玩家身上的屬性系統
        if (user.TryGetComponent<PlayerStats>(out var stats))
        {
            stats.RestoreHunger(hungerRestoration);
            //stats.RestoreHealth(healthRestoration);
            Debug.Log($"[食物] 吃了 {itemName}，恢復了 {hungerRestoration} 點飢餓。");
        }
    }

    // 告訴 Manager 這是食物類
    public ItemCategory GetCategory() => ItemCategory.Food;
}