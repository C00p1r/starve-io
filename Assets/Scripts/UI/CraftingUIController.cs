using UnityEngine;
using UnityEngine.UIElements;

public class CraftingUIController : MonoBehaviour
{
    [SerializeField] private CraftingManager craftingManager;

    private Button _craftButton;
    private Button _stoneCraftButton;
    private Button _goldCraftButton;
    private Button _diamondCraftButton;
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

        _craftButton.clicked += HandleCraftClicked;
        _stoneCraftButton.clicked += HandleStoneCraftClicked;
        _goldCraftButton.clicked += HandleGoldCraftClicked;
        _diamondCraftButton.clicked += HandleDiamondCraftClicked;
        TryHookInventory();

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

    private void UpdateButtonVisibility()
    {
        if (_craftButton == null || _stoneCraftButton == null || _goldCraftButton == null ||
            _diamondCraftButton == null || craftingManager == null)
            return;

        bool canCraftWood = craftingManager.CanCraftPickaxe();
        bool canCraftStone = craftingManager.CanCraftStonePickaxe();
        bool canCraftGold = craftingManager.CanCraftGoldenPickaxe();
        bool canCraftDiamond = craftingManager.CanCraftDiamondPickaxe();

        _craftButton.style.display = canCraftWood ? DisplayStyle.Flex : DisplayStyle.None;
        _stoneCraftButton.style.display = canCraftStone ? DisplayStyle.Flex : DisplayStyle.None;
        _goldCraftButton.style.display = canCraftGold ? DisplayStyle.Flex : DisplayStyle.None;
        _diamondCraftButton.style.display = canCraftDiamond ? DisplayStyle.Flex : DisplayStyle.None;
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
