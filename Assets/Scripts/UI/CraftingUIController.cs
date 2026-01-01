using UnityEngine;
using UnityEngine.UIElements;
using StarveIO.Data;

public class CraftingUIController : MonoBehaviour
{
    [SerializeField] private CraftingManager craftingManager;

    private Button _craftButton;
    private Button _stoneCraftButton;
    private Button _goldCraftButton;
    private Button _diamondCraftButton;
    private Button _woodSwordCraftButton;
    private Button _stoneSwordCraftButton;
    private Button _goldSwordCraftButton;
    private Button _diamondSwordCraftButton;
    private InventoryManager _inventoryManager;
    private bool _subscribed;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogWarning("CraftingUIController requires a UIDocument on the same GameObject.");
            return;
        }

        _craftButton = uiDocument.rootVisualElement.Q<Button>("CraftPickaxeButton");
        _stoneCraftButton = uiDocument.rootVisualElement.Q<Button>("CraftStonePickaxeButton");
        _goldCraftButton = uiDocument.rootVisualElement.Q<Button>("CraftGoldenPickaxeButton");
        _diamondCraftButton = uiDocument.rootVisualElement.Q<Button>("CraftDiamondPickaxeButton");
        _woodSwordCraftButton = uiDocument.rootVisualElement.Q<Button>("CraftWoodenSwordButton");
        _stoneSwordCraftButton = uiDocument.rootVisualElement.Q<Button>("CraftStoneSwordButton");
        _goldSwordCraftButton = uiDocument.rootVisualElement.Q<Button>("CraftGoldenSwordButton");
        _diamondSwordCraftButton = uiDocument.rootVisualElement.Q<Button>("CraftDiamondSwordButton");
        if (_craftButton == null)
        {
            Debug.LogWarning("CraftPickaxeButton not found in UI.");
            return;
        }
        if (_stoneCraftButton == null)
        {
            Debug.LogWarning("CraftStonePickaxeButton not found in UI.");
            return;
        }
        if (_goldCraftButton == null)
        {
            Debug.LogWarning("CraftGoldenPickaxeButton not found in UI.");
            return;
        }
        if (_diamondCraftButton == null)
        {
            Debug.LogWarning("CraftDiamondPickaxeButton not found in UI.");
            return;
        }
        if (_woodSwordCraftButton == null)
        {
            Debug.LogWarning("CraftWoodenSwordButton not found in UI.");
            return;
        }
        if (_stoneSwordCraftButton == null)
        {
            Debug.LogWarning("CraftStoneSwordButton not found in UI.");
            return;
        }
        if (_goldSwordCraftButton == null)
        {
            Debug.LogWarning("CraftGoldenSwordButton not found in UI.");
            return;
        }
        if (_diamondSwordCraftButton == null)
        {
            Debug.LogWarning("CraftDiamondSwordButton not found in UI.");
            return;
        }

        _craftButton.clicked += HandleCraftClicked;
        _stoneCraftButton.clicked += HandleStoneCraftClicked;
        _goldCraftButton.clicked += HandleGoldCraftClicked;
        _diamondCraftButton.clicked += HandleDiamondCraftClicked;
        _woodSwordCraftButton.clicked += HandleWoodSwordCraftClicked;
        _stoneSwordCraftButton.clicked += HandleStoneSwordCraftClicked;
        _goldSwordCraftButton.clicked += HandleGoldSwordCraftClicked;
        _diamondSwordCraftButton.clicked += HandleDiamondSwordCraftClicked;
        TryHookInventory();

        ApplyButtonIcons();
        UpdateButtonVisibility();
    }

    private void OnDisable()
    {
        if (_craftButton != null)
            _craftButton.clicked -= HandleCraftClicked;
        if (_stoneCraftButton != null)
            _stoneCraftButton.clicked -= HandleStoneCraftClicked;
        if (_goldCraftButton != null)
            _goldCraftButton.clicked -= HandleGoldCraftClicked;
        if (_diamondCraftButton != null)
            _diamondCraftButton.clicked -= HandleDiamondCraftClicked;
        if (_woodSwordCraftButton != null)
            _woodSwordCraftButton.clicked -= HandleWoodSwordCraftClicked;
        if (_stoneSwordCraftButton != null)
            _stoneSwordCraftButton.clicked -= HandleStoneSwordCraftClicked;
        if (_goldSwordCraftButton != null)
            _goldSwordCraftButton.clicked -= HandleGoldSwordCraftClicked;
        if (_diamondSwordCraftButton != null)
            _diamondSwordCraftButton.clicked -= HandleDiamondSwordCraftClicked;
        UnhookInventory();
    }

    private void HandleCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftPickaxe();
    }

    private void HandleStoneCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftStonePickaxe();
    }

    private void HandleGoldCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftGoldenPickaxe();
    }

    private void HandleDiamondCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftDiamondPickaxe();
    }

    private void HandleWoodSwordCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftWoodenSword();
    }

    private void HandleStoneSwordCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftStoneSword();
    }

    private void HandleGoldSwordCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftGoldenSword();
    }

    private void HandleDiamondSwordCraftClicked()
    {
        if (craftingManager == null)
        {
            Debug.LogWarning("CraftingManager reference not set.");
            return;
        }

        craftingManager.CraftDiamondSword();
    }

    private void UpdateButtonVisibility()
    {
        if (_craftButton == null || _stoneCraftButton == null || _goldCraftButton == null ||
            _diamondCraftButton == null || _woodSwordCraftButton == null || _stoneSwordCraftButton == null ||
            _goldSwordCraftButton == null || _diamondSwordCraftButton == null || craftingManager == null)
            return;

        bool canCraftWood = craftingManager.CanCraftPickaxe();
        bool canCraftStone = craftingManager.CanCraftStonePickaxe();
        bool canCraftGold = craftingManager.CanCraftGoldenPickaxe();
        bool canCraftDiamond = craftingManager.CanCraftDiamondPickaxe();
        bool canCraftWoodSword = craftingManager.CanCraftWoodenSword();
        bool canCraftStoneSword = craftingManager.CanCraftStoneSword();
        bool canCraftGoldSword = craftingManager.CanCraftGoldenSword();
        bool canCraftDiamondSword = craftingManager.CanCraftDiamondSword();

        _craftButton.style.display = canCraftWood ? DisplayStyle.Flex : DisplayStyle.None;
        _stoneCraftButton.style.display = canCraftStone ? DisplayStyle.Flex : DisplayStyle.None;
        _goldCraftButton.style.display = canCraftGold ? DisplayStyle.Flex : DisplayStyle.None;
        _diamondCraftButton.style.display = canCraftDiamond ? DisplayStyle.Flex : DisplayStyle.None;
        _woodSwordCraftButton.style.display = canCraftWoodSword ? DisplayStyle.Flex : DisplayStyle.None;
        _stoneSwordCraftButton.style.display = canCraftStoneSword ? DisplayStyle.Flex : DisplayStyle.None;
        _goldSwordCraftButton.style.display = canCraftGoldSword ? DisplayStyle.Flex : DisplayStyle.None;
        _diamondSwordCraftButton.style.display = canCraftDiamondSword ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void ApplyButtonIcons()
    {
        if (craftingManager == null)
            return;

        SetButtonIcon(_craftButton, craftingManager.WoodenPickaxeItem);
        SetButtonIcon(_stoneCraftButton, craftingManager.StonePickaxeItem);
        SetButtonIcon(_goldCraftButton, craftingManager.GoldenPickaxeItem);
        SetButtonIcon(_diamondCraftButton, craftingManager.DiamondPickaxeItem);
        SetButtonIcon(_woodSwordCraftButton, craftingManager.WoodenSwordItem);
        SetButtonIcon(_stoneSwordCraftButton, craftingManager.StoneSwordItem);
        SetButtonIcon(_goldSwordCraftButton, craftingManager.GoldenSwordItem);
        SetButtonIcon(_diamondSwordCraftButton, craftingManager.DiamondSwordItem);
    }

    private static void SetButtonIcon(Button button, ItemData item)
    {
        if (button == null || item == null || item.icon == null)
            return;

        button.style.backgroundImage = new StyleBackground(item.icon);
        button.text = string.Empty;
    }

    private void Update()
    {
        if (!_subscribed)
            TryHookInventory();
    }

    private void TryHookInventory()
    {
        if (_subscribed)
            return;

        _inventoryManager = InventoryManager.Instance;
        if (_inventoryManager == null)
            return;

        _inventoryManager.OnInventoryChanged += UpdateButtonVisibility;
        _subscribed = true;
        UpdateButtonVisibility();
    }

    private void UnhookInventory()
    {
        if (_inventoryManager != null && _subscribed)
            _inventoryManager.OnInventoryChanged -= UpdateButtonVisibility;

        _subscribed = false;
        _inventoryManager = null;
    }
}
