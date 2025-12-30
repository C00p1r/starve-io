using UnityEngine;
using StarveIO.Data;

public class CraftingManager : MonoBehaviour
{
    [Header("Recipe")]
    [SerializeField] private ItemData woodItem;
    [SerializeField] private ItemData woodenPickaxeItem;
    [SerializeField] private int woodCost = 5;
    [SerializeField] private int pickaxeAmount = 1;
    [SerializeField] private ItemData stoneItem;
    [SerializeField] private ItemData stonePickaxeItem;
    [SerializeField] private int stoneCost = 5;
    [SerializeField] private int stonePickaxeAmount = 1;
    [SerializeField] private ItemData goldItem;
    [SerializeField] private ItemData goldenPickaxeItem;
    [SerializeField] private int goldCost = 5;
    [SerializeField] private int goldenPickaxeAmount = 1;
    [SerializeField] private ItemData diamondItem;
    [SerializeField] private ItemData diamondPickaxeItem;
    [SerializeField] private int diamondCost = 5;
    [SerializeField] private int diamondPickaxeAmount = 1;

    public bool CraftPickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (woodItem == null || woodenPickaxeItem == null)
        {
            Debug.LogWarning("CraftingManager missing item references.");
            return false;
        }

        if (inventory.GetItemCount(woodItem) < woodCost)
        {
            UIEventManager.TriggerNotify("Not enough wood to craft a pickaxe.");
            return false;
        }

        if (!inventory.TryRemoveItem(woodItem, woodCost))
            return false;

        if (!inventory.AddItem(woodenPickaxeItem, pickaxeAmount))
        {
            inventory.AddItem(woodItem, woodCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a wooden pickaxe.");
        return true;
    }

    public bool CanCraftPickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || woodItem == null)
            return false;

        return inventory.GetItemCount(woodItem) >= woodCost;
    }

    public bool CraftStonePickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (stoneItem == null || stonePickaxeItem == null)
        {
            Debug.LogWarning("CraftingManager missing stone item references.");
            return false;
        }

        if (inventory.GetItemCount(stoneItem) < stoneCost)
        {
            UIEventManager.TriggerNotify("Not enough stone to craft a stone pickaxe.");
            return false;
        }

        if (!inventory.TryRemoveItem(stoneItem, stoneCost))
            return false;

        if (!inventory.AddItem(stonePickaxeItem, stonePickaxeAmount))
        {
            inventory.AddItem(stoneItem, stoneCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a stone pickaxe.");
        return true;
    }

    public bool CanCraftStonePickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || stoneItem == null)
            return false;

        return inventory.GetItemCount(stoneItem) >= stoneCost;
    }

    public bool CraftGoldenPickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (goldItem == null || goldenPickaxeItem == null)
        {
            Debug.LogWarning("CraftingManager missing gold item references.");
            return false;
        }

        if (inventory.GetItemCount(goldItem) < goldCost)
        {
            UIEventManager.TriggerNotify("Not enough gold to craft a golden pickaxe.");
            return false;
        }

        if (!inventory.TryRemoveItem(goldItem, goldCost))
            return false;

        if (!inventory.AddItem(goldenPickaxeItem, goldenPickaxeAmount))
        {
            inventory.AddItem(goldItem, goldCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a golden pickaxe.");
        return true;
    }

    public bool CanCraftGoldenPickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || goldItem == null)
            return false;

        return inventory.GetItemCount(goldItem) >= goldCost;
    }

    public bool CraftDiamondPickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (diamondItem == null || diamondPickaxeItem == null)
        {
            Debug.LogWarning("CraftingManager missing diamond item references.");
            return false;
        }

        if (inventory.GetItemCount(diamondItem) < diamondCost)
        {
            UIEventManager.TriggerNotify("Not enough diamond to craft a diamond pickaxe.");
            return false;
        }

        if (!inventory.TryRemoveItem(diamondItem, diamondCost))
            return false;

        if (!inventory.AddItem(diamondPickaxeItem, diamondPickaxeAmount))
        {
            inventory.AddItem(diamondItem, diamondCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a diamond pickaxe.");
        return true;
    }

    public bool CanCraftDiamondPickaxe()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || diamondItem == null)
            return false;

        return inventory.GetItemCount(diamondItem) >= diamondCost;
    }
}
