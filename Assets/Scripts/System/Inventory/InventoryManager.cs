using UnityEngine;
using System.Collections.Generic;
using StarveIO.Data;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("背包設定")]
    [SerializeField] private int maxSlots = 10;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private int selectedIndex = 0;

    // 當背包變動時，通知 UI 更新
    public event Action OnInventoryChanged;
    public event Action OnInventoryExtended; // 合成出大背包
    public event Action<int> OnSelectedIndexChanged;
    //private event Action OnInventoryFull;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        slots.Clear();
        for (int i = 0; i < maxSlots; i++)
            slots.Add(new InventorySlot(null, 0));
    }

    public void UpdateInventoryUI() {  OnInventoryChanged?.Invoke(); }
    // 核心功能：增加物品
    public bool AddItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        // 1. 嘗試找尋現有的堆疊 (且未滿)
        foreach (var slot in slots)
        {
            if (slot.item == item && slot.count < item.maxStack)
            {
                int canAdd = Mathf.Min(amount, item.maxStack - slot.count);
                slot.count += canAdd;
                amount -= canAdd;

                if (amount <= 0)
                {
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // 2. 如果還有剩餘數量，嘗試找尋空格
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            Debug.Log("trying to find empty slot...");
            var slot = slots[i];
            if (slot.item != null && slot.count > 0)
                continue;
            Debug.Log("trying to find empty slot...Found!");
            int amountToAdd = Mathf.Min(amount, item.maxStack);
            slot.item = item;
            slot.count = amountToAdd;
            amount -= amountToAdd;
        }

        OnInventoryChanged?.Invoke();

        // 如果 amount 還有剩，代表背包滿了放不下
        if (amount > 0)
        {
            UIEventManager.TriggerNotify("The Inventory is Full!");
            return false;
        }
        
        return true;
    
    }

    public List<InventorySlot> GetSlots() => slots;
    public int GetSelectedIndex() => selectedIndex;
    public int GetMaxSlots() => maxSlots;

    public ItemData GetSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Count)
            return null;

        var slot = slots[selectedIndex];
        if (slot.item == null || slot.count <= 0)
            return null;

        return slot.item;
    }

    public void SelectIndex(int index)
    {
        int clampedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, maxSlots - 1));
        if (clampedIndex == selectedIndex)
            return;

        selectedIndex = clampedIndex;
        OnSelectedIndexChanged?.Invoke(selectedIndex);
    }

    public void CycleSelection(int direction)
    {
        if (maxSlots <= 0)
            return;

        int nextIndex = selectedIndex + direction;
        if (nextIndex < 0)
            nextIndex = maxSlots - 1;
        else if (nextIndex >= maxSlots)
            nextIndex = 0;

        SelectIndex(nextIndex);
    }

    public int GetItemCount(ItemData item)
    {
        if (item == null)
            return 0;

        int total = 0;
        foreach (var slot in slots)
        {
            if (slot.item == item)
                total += slot.count;
        }

        return total;
    }

    public bool TryRemoveItem(ItemData item, int amount)
    {
        if (item == null || amount <= 0)
            return false;

        if (GetItemCount(item) < amount)
            return false;

        for (int i = slots.Count - 1; i >= 0 && amount > 0; i--)
        {
            var slot = slots[i];
            if (slot.item != item)
                continue;

            int take = Mathf.Min(amount, slot.count);
            slot.count -= take;
            amount -= take;

            if (slot.count <= 0)
            {
                slot.count = 0;
                slot.item = null;
            }
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool ClearSelectedSlot()
    {
        return ClearSlot(selectedIndex);
    }

    public bool ClearSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
            return false;

        var slot = slots[index];
        if (slot.item == null || slot.count <= 0)
            return false;

        slot.item = null;
        slot.count = 0;
        OnInventoryChanged?.Invoke();
        return true;
    }
    public void SwapItems(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex || fromIndex >= slots.Count || toIndex >= slots.Count) return;

        // 交換 List 中的元素
        var temp = slots[fromIndex];
        slots[fromIndex] = slots[toIndex];
        slots[toIndex] = temp;

        OnInventoryChanged?.Invoke();
    }

}
