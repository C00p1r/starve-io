using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class InventoryUIController : MonoBehaviour
{
    private InventoryManager _inventoryManager;
    private VisualElement _root;
    private List<VisualElement> _slotElements = new List<VisualElement>();

    private void OnEnable()
    {
        // 2. 在這裡嘗試獲取單例 (如果 Awake 還沒跑，這裡會拿到 null，我們等下在 Update 處理)
        _inventoryManager = InventoryManager.Instance;

        _root = GetComponent<UIDocument>().rootVisualElement;
        _slotElements = _root.Query<VisualElement>("Bar1").ToList();

        // 3. 訂閱事件 (先檢查 null，防止報錯)
        if (_inventoryManager != null)
        {
            // 記得先退訂再訂閱，防止重複訂閱 (這是 UI Toolkit 的好習慣)
            _inventoryManager.OnInventoryChanged -= UpdateInventoryUI; 
            _inventoryManager.OnInventoryChanged += UpdateInventoryUI;
        }

        // 4. 初始化顯示
        UpdateInventoryUI();
    }

    // uncomment if  ERR NULL REFERENCE occured
    //private void Start()
    //{
    //    // 5. 如果 OnEnable 的時候單例還沒準備好，Start 執行得比較晚，這裡再做一次保險
    //    if (_inventoryManager == null)
    //    {
    //        _inventoryManager = InventoryManager.Instance;
    //        if (_inventoryManager != null)
    //        {
    //            _inventoryManager.OnInventoryChanged -= UpdateInventoryUI;
    //            _inventoryManager.OnInventoryChanged += UpdateInventoryUI;
    //            UpdateInventoryUI();
    //        }
    //    }
    //}

    private void OnDisable()
    {
        if (_inventoryManager != null)
            _inventoryManager.OnInventoryChanged -= UpdateInventoryUI;
    }

    private void UpdateInventoryUI()
    {
        var slotsData = _inventoryManager.GetSlots();

        for (int i = 0; i < _slotElements.Count; i++)
        {
            VisualElement currentSlot = _slotElements[i];

            // 先清空格子內的舊標籤（如果有）
            currentSlot.Clear();

            if (i < slotsData.Count)
            {
                var data = slotsData[i];

                // --- 設定 Icon ---
                // 我們直接將 ItemData 的 icon 設為格子的背景圖
                currentSlot.style.backgroundImage = new StyleBackground(data.item.icon);
                currentSlot.style.opacity = 1.0f; // 採集到東西後變回完全顯示

                // --- 設定 數量標籤 ---
                if (data.count > 0)
                {
                    Label countLabel = new Label(data.count.ToString());

                    // 用代碼設定一點簡單樣式，或在 USS 設定 .count-label
                    countLabel.style.position = Position.Absolute;
                    countLabel.style.right = 5;
                    countLabel.style.bottom = 5;
                    countLabel.style.color = Color.white;
                    countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    countLabel.style.fontSize = 15;

                    currentSlot.Add(countLabel);
                }
            }
            else
            {
                // 如果格子是空的
                currentSlot.style.backgroundImage = null;
            }
        }
    }
}