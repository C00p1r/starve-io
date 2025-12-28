using StarveIO.Data;

[System.Serializable] // 讓它可以在 Inspector 顯示
public class InventorySlot
{
    public ItemData item;
    public int count;

    public InventorySlot(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
    }
}