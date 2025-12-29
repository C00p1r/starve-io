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
    private event Action OnInventoryFull;
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

    public List<InventorySlot> GetSlots() => slots;
}