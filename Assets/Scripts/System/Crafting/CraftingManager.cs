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

    [Header("Sword Recipe")]
    [SerializeField] private ItemData woodenSwordItem;
    [SerializeField] private int woodSwordCost = 5;
    [SerializeField] private int woodenSwordAmount = 1;
    [SerializeField] private ItemData stoneSwordItem;
    [SerializeField] private int stoneSwordCost = 5;
    [SerializeField] private int stoneSwordAmount = 1;
    [SerializeField] private ItemData goldenSwordItem;
    [SerializeField] private int goldSwordCost = 5;
    [SerializeField] private int goldenSwordAmount = 1;
    [SerializeField] private ItemData diamondSwordItem;
    [SerializeField] private int diamondSwordCost = 5;
    [SerializeField] private int diamondSwordAmount = 1;

    [Header("Bonfire Recipe")]
    [SerializeField] private ItemData bonfireItem;
    [SerializeField] private int bonfireWoodCost = 10;
    [SerializeField] private int bonfireAmount = 1;

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

        if (inventory.GetItemCount(stoneItem) < stoneCost || inventory.GetItemCount(woodenPickaxeItem) < 1)
        {
            UIEventManager.TriggerNotify("Can't craft a stone pickaxe.\nNeed a wooden pickaxe first.");
            return false;
        }

        if (!inventory.TryRemoveItem(stoneItem, stoneCost) || !inventory.TryRemoveItem(woodenPickaxeItem, 1))
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

        if (inventory.GetItemCount(goldItem) < goldCost || inventory.GetItemCount(stonePickaxeItem) < 1)
        {
            UIEventManager.TriggerNotify("Can't craft a golden pickaxe.\nNeed a stone pickaxe first.");
            return false;
        }

        if (!inventory.TryRemoveItem(goldItem, goldCost) || !inventory.TryRemoveItem(stonePickaxeItem, 1))
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

        if (inventory.GetItemCount(diamondItem) < diamondCost || inventory.GetItemCount(goldenPickaxeItem) < 1)
        {
            UIEventManager.TriggerNotify("Can't craft a diamond pickaxe.\nNeed a golden pickaxe first.");
            return false;
        }

        if (!inventory.TryRemoveItem(diamondItem, diamondCost) || !inventory.TryRemoveItem(goldenPickaxeItem, 1))
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

    public bool CraftWoodenSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (woodItem == null || woodenSwordItem == null)
        {
            Debug.LogWarning("CraftingManager missing sword item references.");
            return false;
        }

        if (inventory.GetItemCount(woodItem) < woodSwordCost)
        {
            UIEventManager.TriggerNotify("Not enough wood to craft a sword.");
            return false;
        }

        if (!inventory.TryRemoveItem(woodItem, woodSwordCost))
            return false;

        if (!inventory.AddItem(woodenSwordItem, woodenSwordAmount))
        {
            inventory.AddItem(woodItem, woodSwordCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a wooden sword.");
        return true;
    }

    public bool CanCraftWoodenSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || woodItem == null)
            return false;

        return inventory.GetItemCount(woodItem) >= woodSwordCost;
    }

    public bool CraftStoneSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (stoneItem == null || stoneSwordItem == null)
        {
            Debug.LogWarning("CraftingManager missing sword item references.");
            return false;
        }

        if (inventory.GetItemCount(stoneItem) < stoneSwordCost || inventory.GetItemCount(woodenSwordItem) < 1)
        {
            UIEventManager.TriggerNotify("Can't craft a stone sword.\nNeed a wooden sword first.");
            return false;
        }

        if (!inventory.TryRemoveItem(stoneItem, stoneSwordCost) || !inventory.TryRemoveItem(woodenSwordItem, 1))
            return false;

        if (!inventory.AddItem(stoneSwordItem, stoneSwordAmount))
        {
            inventory.AddItem(stoneItem, stoneSwordCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a stone sword.");
        return true;
    }

    public bool CanCraftStoneSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || stoneItem == null)
            return false;

        return inventory.GetItemCount(stoneItem) >= stoneSwordCost;
    }

    public bool CraftGoldenSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (goldItem == null || goldenSwordItem == null)
        {
            Debug.LogWarning("CraftingManager missing sword item references.");
            return false;
        }

        if (inventory.GetItemCount(goldItem) < goldSwordCost || inventory.GetItemCount(stoneSwordItem) < 1)
        {
            UIEventManager.TriggerNotify("Can't craft a golden sword.\nNeed a stone sword first.");
            return false;
        }

        if (!inventory.TryRemoveItem(goldItem, goldSwordCost) || !inventory.TryRemoveItem(stoneSwordItem, 1))
            return false;

        if (!inventory.AddItem(goldenSwordItem, goldenSwordAmount))
        {
            inventory.AddItem(goldItem, goldSwordCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a golden sword.");
        return true;
    }

    public bool CanCraftGoldenSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || goldItem == null)
            return false;

        return inventory.GetItemCount(goldItem) >= goldSwordCost;
    }

    public bool CraftDiamondSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (diamondItem == null || diamondSwordItem == null)
        {
            Debug.LogWarning("CraftingManager missing sword item references.");
            return false;
        }

        if (inventory.GetItemCount(diamondItem) < diamondSwordCost || inventory.GetItemCount(goldenSwordItem) < 1)
        {
            UIEventManager.TriggerNotify("Can't craft a diamond sword.\nNeed a golden sword first.");
            return false;
        }

        if (!inventory.TryRemoveItem(diamondItem, diamondSwordCost) || !inventory.TryRemoveItem(goldenSwordItem, 1))
            return false;

        if (!inventory.AddItem(diamondSwordItem, diamondSwordAmount))
        {
            inventory.AddItem(diamondItem, diamondSwordCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a diamond sword.");
        return true;
    }

    public bool CanCraftDiamondSword()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || diamondItem == null)
            return false;

        return inventory.GetItemCount(diamondItem) >= diamondSwordCost;
    }

    public bool CraftBonfire()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return false;
        }

        if (woodItem == null || bonfireItem == null)
        {
            Debug.LogWarning("CraftingManager missing bonfire item references.");
            return false;
        }

        if (inventory.GetItemCount(woodItem) < bonfireWoodCost)
        {
            UIEventManager.TriggerNotify("Not enough wood to craft a bonfire.");
            return false;
        }

        if (!inventory.TryRemoveItem(woodItem, bonfireWoodCost))
            return false;

        if (!inventory.AddItem(bonfireItem, bonfireAmount))
        {
            inventory.AddItem(woodItem, bonfireWoodCost);
            return false;
        }

        UIEventManager.TriggerNotify("Crafted a bonfire.");
        return true;
    }

    public bool CanCraftBonfire()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null || woodItem == null)
            return false;

        return inventory.GetItemCount(woodItem) >= bonfireWoodCost;
    }

    public ItemData WoodenPickaxeItem => woodenPickaxeItem;
    public ItemData StonePickaxeItem => stonePickaxeItem;
    public ItemData GoldenPickaxeItem => goldenPickaxeItem;
    public ItemData DiamondPickaxeItem => diamondPickaxeItem;
    public ItemData WoodenSwordItem => woodenSwordItem;
    public ItemData StoneSwordItem => stoneSwordItem;
    public ItemData GoldenSwordItem => goldenSwordItem;
    public ItemData DiamondSwordItem => diamondSwordItem;
    public ItemData BonfireItem => bonfireItem;
}
