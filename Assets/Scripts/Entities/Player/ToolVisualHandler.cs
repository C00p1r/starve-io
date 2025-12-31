using UnityEngine;
using StarveIO.Data;
using UnityEditor.UI; // 確保與你的 ItemData 命名空間一致

public class ToolVisualHandler : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    [SerializeField] public InventoryManager _inventoryManager;
    [SerializeField] public float _itemScale = 1;
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // 訂閱背包變更事件，這樣切換格子時會立刻更新外觀
        if (_inventoryManager != null)
        {
            _inventoryManager.OnInventoryChanged += UpdateToolVisual;
        }
    }

    void OnDestroy()
    {
        if (_inventoryManager != null)
            _inventoryManager.OnInventoryChanged -= UpdateToolVisual;
    }

    public void UpdateToolVisual()
    {
        ItemData selectedItem = _inventoryManager.GetSelectedItem();

        // 1. 檢查是否選中了東西，且該東西有工具圖片
        if (selectedItem != null && selectedItem.toolType > 0 && selectedItem.icon != null)
        {
            _spriteRenderer.enabled = true;
            _spriteRenderer.sprite = selectedItem.icon;
            this.transform.localScale = new Vector3(_itemScale, _itemScale, _itemScale);

            // 如果你的工具圖片是橫的，這裡可以微調旋轉角度
             //transform.localRotation = Quaternion.Euler(0, 0, -45);
        }
        else
        {
            // 2. 如果手裡沒拿東西（或是不可裝備的東西），就把圖片隱藏
            _spriteRenderer.enabled = false;
        }
    }
}