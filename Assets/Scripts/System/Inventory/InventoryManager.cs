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

    // 當背包變動時，通知 UI 更新
    public event Action OnInventoryChanged;
    public event Action OnInventoryExtended; // 合成出大背包
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
    }

    // 核心功能：增加物品
    public bool AddItem(ItemData item, int amount)
    {
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

        // 2. 如果還有剩餘數量，嘗試找尋新的格子
        while (amount > 0 && slots.Count < maxSlots)
        {
            int amountToAdd = Mathf.Min(amount, item.maxStack);
            slots.Add(new InventorySlot(item, amountToAdd));
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

    public void UseItem(int index)
    {
        if (index < 0 || index >= slots.Count || slots[index].item == null) return;

        InventorySlot slot = slots[index];
        ItemData item = slot.item;

        if (item is IUsable usableItem)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.Log("player is null wtf!");
                return;
            }
            // 根據 Enum 執行不同的分支邏輯
            usableItem.Use(player);
            Debug.Log("item is used");
            // 統一處理消耗邏輯：如果是消耗品分類，數量 -1
            if (item.IsConsumable)
            {
                slot.count--;
                if (slot.count <= 0)
                {
                    slot.item = null;
                    slot.count = 0;
                }
            }
        }
        else
        {
            Debug.Log("這是資源，不能直接使用");
        }

        OnInventoryChanged?.Invoke();
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

    public void DropItem(int index, int amount)
    {
        if (index < 0 || index >= slots.Count) return;

        InventorySlot slot = slots[index];

        // 限制丟出數量不能超過持有數量
        int actualDropAmount = Mathf.Min(amount, slot.count);

        // 執行生成掉落物邏輯
        //SpawnItemInWorld(slot.item, actualDropAmount);

        // 扣除數量
        slot.count -= actualDropAmount;
        if (slot.count <= 0)
        {
            slots.RemoveAt(index);
        }

        OnInventoryChanged?.Invoke();
    }

    public List<InventorySlot> GetSlots() => slots;
}