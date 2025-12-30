using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class InventoryDeleteController : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;

    private VisualElement _confirmPanel;
    private Button _confirmYes;
    private Button _confirmNo;
    private bool _isVisible;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogWarning("InventoryDeleteController requires a UIDocument on the same GameObject.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        _confirmPanel = root.Q<VisualElement>("DeleteConfirmPanel");
        _confirmYes = root.Q<Button>("ConfirmDeleteYes");
        _confirmNo = root.Q<Button>("ConfirmDeleteNo");

        if (_confirmPanel == null || _confirmYes == null || _confirmNo == null)
        {
            Debug.LogWarning("Delete confirmation UI elements not found.");
            return;
        }

        _confirmYes.clicked += ConfirmDelete;
        _confirmNo.clicked += HideConfirm;

        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;

        HideConfirm();
    }

    private void OnDisable()
    {
        if (_confirmYes != null)
            _confirmYes.clicked -= ConfirmDelete;
        if (_confirmNo != null)
            _confirmNo.clicked -= HideConfirm;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (_isVisible)
            {
                HideConfirm();
                return;
            }

            ShowConfirm();
        }
    }

    private void ShowConfirm()
    {
        if (_confirmPanel == null)
            return;

        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;

        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager instance not found.");
            return;
        }

        if (inventoryManager.GetSelectedItem() == null)
        {
            UIEventManager.TriggerNotify("No item selected.");
            return;
        }

        _confirmPanel.style.display = DisplayStyle.Flex;
        _isVisible = true;
    }

    private void HideConfirm()
    {
        if (_confirmPanel == null)
            return;

        _confirmPanel.style.display = DisplayStyle.None;
        _isVisible = false;
    }

    private void ConfirmDelete()
    {
        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;

        if (inventoryManager == null)
            return;

        inventoryManager.ClearSelectedSlot();
        HideConfirm();
    }
}
