using UnityEngine;

namespace StarveIO.Data
{
    public enum ToolType
    {
        None = 0,
        Pickaxe = 1,
        Sword = 2,
    }

    [CreateAssetMenu(fileName = "NewItem", menuName = "StarveIO/Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        public string itemName;      // 物品顯示名稱 (例如：木頭)
        public Sprite icon;          // 物品在 UI 顯示的圖片
        public string description;   // 物品描述 (例如：基礎的建築材料)
        public int maxStack = 99;    // 持有上限

        [Header("Tool Settings")]
        public ToolType toolType = ToolType.None;
        public int toolTier = 0;
        public int damage = 0;
    }
}
