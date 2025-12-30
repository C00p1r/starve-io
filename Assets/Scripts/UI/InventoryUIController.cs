using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering.Universal;
using NUnit.Framework.Constraints;

public class InventoryUIController : MonoBehaviour
{
    private InventoryManager _inventoryManager;
    private VisualElement _root;
    private List<VisualElement> _slotElements = new List<VisualElement>();

    [Header("交互設定")]
    [SerializeField] private float _longPressThreshold = 0.2f;
    private Coroutine _pressTimer;
    private bool _isDragging = false;
    private int _dragStartIndex = -1;
    private int _lastPointerId = -1;

    private VisualElement _dragGhost;  // 實現脫拽物品效果
    private VisualElement _dropWindow;

    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _inventoryManager = InventoryManager.Instance;

        // 1. 獲取hotbar元件
        _slotElements = _root.Query<VisualElement>("Bar1").ToList();

        // 2. 註冊格子專屬事件 (Down)
        for (int i = 0; i < _slotElements.Count; i++)
        {
            int index = i; // 閉包陷阱
            _slotElements[i].RegisterCallback<PointerDownEvent>(evt => OnPointerDown(evt, index));
        }

        // 3. 註冊全域事件 (Move & Up) - 確保整個畫面都可拖拽、放開觸發
        _root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _root.RegisterCallback<PointerUpEvent>(OnPointerUpGlobal);

        // 4. 訂閱數據變更
        if (_inventoryManager != null)
        {
            _inventoryManager.OnInventoryChanged -= UpdateInventoryUI;
            _inventoryManager.OnInventoryChanged += UpdateInventoryUI;
        }

        UpdateInventoryUI();
    }

    private void Start()
    {
        // 5.如果 OnEnable 的時候單例還沒準備好，Start 執行得比較晚，這裡再做一次保險
        if (_inventoryManager == null)
        {
            _inventoryManager = InventoryManager.Instance;
            if (_inventoryManager != null)
            {
                _inventoryManager.OnInventoryChanged -= UpdateInventoryUI;
                _inventoryManager.OnInventoryChanged += UpdateInventoryUI;
                UpdateInventoryUI();
            }
        }
        
        _dragGhost = _root.Q<VisualElement>("DragGhost");
        _dropWindow = _root.Q<VisualElement>("DropWindow");

        _dragGhost.style.display = DisplayStyle.None;
        _dropWindow.style.display = DisplayStyle.None;
    }

    private void OnPointerDown(PointerDownEvent evt, int index)
    {
        if (index < 0 || index >= _inventoryManager.GetSlots().Count) return;
        if (evt.button == 0) // 左鍵
        {
            _dragStartIndex = index;
            _lastPointerId = evt.pointerId;
            if (_pressTimer != null) StopCoroutine(_pressTimer);
            _pressTimer = StartCoroutine(LongPressRoutine(index));

            // 為了確保全螢幕拖拽不丟失，可以讓 root 捕獲滑鼠 (可選)
            _root.CapturePointer(evt.pointerId); 
        }
        else if (evt.button == 1) // 右鍵
        {
            Debug.Log("UI 右鍵");
            ShowDropWindow(index);
        }
    }

    private void OnPointerUpGlobal(PointerUpEvent evt)
    {
        // 如果根本沒點下任何東西就放開，直接無視
        if (_dragStartIndex == -1) return;

        if (evt.button == 0) // 左鍵放開
        {
            if (_pressTimer != null) StopCoroutine(_pressTimer);

            if (_isDragging)
            {
                // 1. 判定放開位置下的 UI
                VisualElement pickedElement = _root.panel.Pick(evt.position);
                int targetIndex = FindSlotIndex(pickedElement);

                // 2. 執行交換邏輯 (僅當拖到另一個有效格子時)
                if (targetIndex != -1 && targetIndex != _dragStartIndex)
                {
                    _inventoryManager.SwapItems(_dragStartIndex, targetIndex);
                    Debug.Log($"全域判定：交換成功 {targetIndex}");
                }
                else
                {
                    Debug.Log("全域判定：放開位置無效，恢復原位");
                }

                // 3. 結束拖拽狀態
                ResetDragState();
            }
            else
            {
                // --- 處理短按 (Use Item) ---
                // 即使是短按，現在也是在 _root 放開
                // 我們檢查滑鼠放開時是否還在原本按下的那一格
                VisualElement pickedElement = _root.panel.Pick(evt.position);
                int targetIndex = FindSlotIndex(pickedElement);

                if (targetIndex == _dragStartIndex)
                {
                    _inventoryManager.UseItem(_dragStartIndex);
                    Debug.Log($"全域判定：使用物品 {_dragStartIndex}");
                }

                ResetDragState();
            }
        }
    }

    private void ResetDragState()
    {
        // 釋放捕獲 (非常重要，否則 UI 會卡死無法點擊其他東西)
        if (_root.HasPointerCapture(_lastPointerId)) // 需要在 Down 時存下 evt.pointerId
        {
            _root.ReleasePointer(_lastPointerId);
        }

        _isDragging = false;
        _dragGhost.style.display = DisplayStyle.None;
        if (_dragStartIndex != -1) _slotElements[_dragStartIndex].style.opacity = 1f;
        _dragStartIndex = -1;
        UpdateInventoryUI();
    }


    // 輔助方法：根據被 Pick 到的元件找回它的 Index
    private int FindSlotIndex(VisualElement element)
    {
        VisualElement current = element;
        while (current != null && current != _root)
        {
            if (_slotElements.Contains(current)) return _slotElements.IndexOf(current);
            current = current.parent;
        }
        return -1;
    }
    private IEnumerator LongPressRoutine(int index)
    {
        yield return new WaitForSeconds(_longPressThreshold);

        var slots = _inventoryManager.GetSlots();
        if (index >= slots.Count) yield break;

        _isDragging = true;

        // 視覺更新：拿取效果
        _dragGhost.style.width = _slotElements[index].resolvedStyle.width;
        _dragGhost.style.height = _slotElements[index].resolvedStyle.height;
        _dragGhost.style.backgroundImage = new StyleBackground(slots[index].item.icon);
        _dragGhost.style.display = DisplayStyle.Flex;
        _dragGhost.pickingMode = PickingMode.Ignore; // 防止阻擋 panel.Pick

        _slotElements[index].style.opacity = 0.3f;
    }
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (_isDragging && _dragGhost != null)
        {
            // 確保我們拿到的是最精確的當前尺寸
            float width = _dragGhost.resolvedStyle.width;
            float height = _dragGhost.resolvedStyle.height;

            // 直接使用 panel 坐標
            // 如果你的 UI 有縮放（Scale），可能需要除以 panel 的縮放倍率
            _dragGhost.style.left = evt.position.x - (width / 2);
            _dragGhost.style.top = evt.position.y - (height / 2);
        }
    }


    private void UpdateInventoryUI()
    {
        var slotsData = _inventoryManager.GetSlots();
        for (int i = 0; i < _slotElements.Count; i++)
        {
            VisualElement currentSlot = _slotElements[i];
            currentSlot.Clear();

            if (i < slotsData.Count)
            {
                var data = slotsData[i];
                currentSlot.style.backgroundImage = new StyleBackground(data.item.icon);
                //currentSlot.style.opacity = 1.0f; // 採集到東西後變回完全顯示
                if (data.count > 1)
                {
                    Label countLabel = new Label(data.count.ToString());
                    countLabel.AddToClassList("count-label"); // 建議在 USS 定義樣式
                    currentSlot.Add(countLabel);
                }
            }
            else
            {
                currentSlot.style.backgroundImage = null;
            }
        }
    }


    private void ShowDropWindow(int index)
    {
        _dropWindow.style.display = DisplayStyle.Flex;

        // 2. 獲取按鈕並重新分配「點擊動作」
        // 使用 new Clickable() 會直接覆蓋掉舊的動作，防止重複觸發

        // 選項 1：丟出一個
        _dropWindow.Q<Button>("BtnOne").clickable = new Clickable(() =>
        {
            _inventoryManager.DropItem(index, 1); // 這裡需確保 Manager 有接收數量的 DropItem
            _dropWindow.style.display = DisplayStyle.None;
        });

        // 選項 2：丟出全部
        _dropWindow.Q<Button>("BtnAll").clickable = new Clickable(() =>
        {
            int totalCount = _inventoryManager.GetSlots()[index].count;
            _inventoryManager.DropItem(index, totalCount);
            _dropWindow.style.display = DisplayStyle.None;
        });

        // 選項 3：取消
        _dropWindow.Q<Button>("BtnCancel").clickable = new Clickable(() =>
        {
            _dropWindow.style.display = DisplayStyle.None;
        });
    }
    private void OnDisable()
    {
        if (_inventoryManager != null)
            _inventoryManager.OnInventoryChanged -= UpdateInventoryUI;
    }

}












