using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class InventoryDeleteController : MonoBehaviour
{
    public static InventoryDeleteController Instance { get; private set; }

    [SerializeField] private InventoryManager inventoryManager;
    private VisualElement _confirmPanel;
    private Button _confirmYes;
    private Button _confirmNo;
    private Label _confirmLabel;
    private bool _isVisible;
    private Action _onConfirm;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogWarning("InventoryDeleteController requires a UIDocument on the same GameObject.");
            return;
        }

        if (Instance == null)
            Instance = this;

        var root = uiDocument.rootVisualElement;
        _confirmPanel = root.Q<VisualElement>("DeleteConfirmPanel");
        _confirmYes = root.Q<Button>("ConfirmDeleteYes");
        _confirmNo = root.Q<Button>("ConfirmDeleteNo");
        _confirmLabel = root.Q<Label>("DeleteConfirmLabel");

        if (_confirmPanel == null || _confirmYes == null || _confirmNo == null || _confirmLabel == null)
        {
            Debug.LogWarning("Delete confirmation UI elements not found.");
            return;
        }

        _confirmYes.clicked += HandleConfirm;
        _confirmNo.clicked += HideConfirm;

        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;

        HideConfirm();
    }

    private void OnDisable()
    {
        if (_confirmYes != null)
            _confirmYes.clicked -= HandleConfirm;
        if (_confirmNo != null)
            _confirmNo.clicked -= HideConfirm;

        if (Instance == this)
            Instance = null;
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

            ShowConfirm("Delete selected item?", DeleteSelectedItem);
        }
    }

    public void ShowConfirm(string message, Action onConfirm)
    {
        if (_confirmPanel == null)
            return;

        _confirmLabel.text = message;
        _onConfirm = onConfirm;
        _confirmPanel.style.display = DisplayStyle.Flex;
        _confirmPanel.style.opacity = 1f;
        _isVisible = true;
    }

    private void HideConfirm()
    {
        if (_confirmPanel == null)
            return;

        _confirmPanel.style.opacity = 0f;
        _confirmPanel.style.display = DisplayStyle.None;
        _isVisible = false;
        _onConfirm = null;
    }

    private void HandleConfirm()
    {
        _onConfirm?.Invoke();
        HideConfirm();
    }

    private void DeleteSelectedItem()
    {
        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;

        if (inventoryManager == null)
            return;

        inventoryManager.ClearSelectedSlot();
    }

}
