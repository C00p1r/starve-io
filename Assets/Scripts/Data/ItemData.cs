using UnityEngine;
public enum ItemCategory
{
    None,           // 空手
    Resource,   // 基礎資源（木頭、石頭、金礦）- 不可使用
    Food,       // 食物（水果、肉）- 使用後增加屬性並消耗
    Weapon,     // 武器（劍）- 使用後裝備，不消耗
    Tool,       // 工具（斧頭、鎬子）- 使用後裝備，不消耗
    Consumable  // 其他消耗品（藥水、繃帶）
}

namespace StarveIO.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "StarveIO/Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemName;      // 物品顯示名稱 (例如：木頭)
        public Sprite icon;          // 物品在 UI 顯示的圖片
        public string description;   // 物品描述 (例如：基礎的建築材料)
        public ItemCategory category; // <--- 新增分類標籤
        public bool IsConsumable => category == ItemCategory.Food || category == ItemCategory.Consumable;
        public int maxStack = 99;    // 持有上限
    }
}