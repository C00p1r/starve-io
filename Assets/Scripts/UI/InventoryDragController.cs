using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryDragController: MonoBehaviour
{
    [SerializeField] private InventoryManager _inventoryManager;
    private VisualElement _root;
    private List<VisualElement> _slotElements = new List<VisualElement>();

    [Header("交互設定")]
    [SerializeField] private float _longPressThreshold = 0.2f;
    private Coroutine _pressTimer;
    private bool _isDragging = false;
    private int _dragStartIndex = -1;
    private int _lastPointerId = -1;
    private VisualElement _dragGhost;  // 實現脫拽物品效果
    

    private void OnEnable()
    {

        _root = GetComponent<UIDocument>().rootVisualElement;
        _slotElements = _root.Query<VisualElement>("Bar1").ToList();
        _dragGhost = _root.Q<VisualElement>("DragGhost");
        if (_inventoryManager == null)
            _inventoryManager = InventoryManager.Instance;
        if (_inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager instance not found for drag controller.");
            return;
        }
        if (_dragGhost == null)
        {
            Debug.LogWarning("DragGhost element not found.");
            return;
        }
        //_dropWindow = _root.Q<VisualElement>("DropWindow");

        _dragGhost.style.display = DisplayStyle.None;
        //_dropWindow.style.display = DisplayStyle.None;

        for (int i = 0; i < _slotElements.Count; i++)
        {
            int index = i; // 閉包陷阱
            _slotElements[i].RegisterCallback<PointerDownEvent>(evt => OnPointerDown(evt, index));
        }

        // 3. 註冊全域事件 (Move & Up) - 確保整個畫面都可拖拽、放開觸發
        _root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        _root.RegisterCallback<PointerUpEvent>(OnPointerUpGlobal);

    }
    private void OnPointerDown(PointerDownEvent evt, int index)
    {
        if (_inventoryManager == null) return;
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
                if (targetIndex != -1 && targetIndex < _inventoryManager.GetMaxSlots())
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

                //if (targetIndex == _dragStartIndex)
                //{
                //    _inventoryManager.UseItem();
                //    Debug.Log($"全域判定：使用物品 {_dragStartIndex}");
                //}
                _inventoryManager.UseItem(targetIndex);
                Debug.Log($"全域判定：使用物品 {_dragStartIndex}");
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
        _inventoryManager.UpdateInventoryUI();
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

        if (_inventoryManager == null)
            yield break;

        var slots = _inventoryManager.GetSlots();
        if (index >= slots.Count) yield break;
        if (slots[index].item == null || slots[index].count <= 0)
            yield break;

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
}
