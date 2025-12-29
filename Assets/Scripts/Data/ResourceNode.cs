using StarveIO.Data;
using System;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] private ResourceData data; // 拖入對應的 ResourceData 資源檔

    private int _currentStock;
    private float _regenTimer;
    private event Action OnResourceEmpty;
    public ResourceData GetResourceData() { return data; }
    public ItemData GetItemData()
    {
        return data != null ? data.item : null;
    }
    private void Start()
    {
        if (data == null)
        {
            Debug.LogError($"{gameObject.name} 缺少 ResourceData！");
            return;
        }
        _currentStock = data.maxStock;
    }

    private void Update()
    {
        // 使用設定檔中的數值進行判定
        if (data.canRegenerate && _currentStock < data.maxStock)
        {
            HandleRegeneration();
        }
    }

    private void HandleRegeneration()
    {
        _regenTimer += Time.deltaTime;
        if (_regenTimer >= data.regenInterval)
        {
            _currentStock = Mathf.Min(_currentStock + data.regenAmount, data.maxStock);
            _regenTimer = 0;
            Debug.Log($"{gameObject.name} 恢復中... 目前: {_currentStock}");
        }
    }

    public int GatherResource()
    {
        if (_currentStock <= 0)
        {
            OnResourceEmpty?.Invoke();
            return 0;
        }

        int actualYield = Mathf.Min(data.yieldPerHit, _currentStock);
        _currentStock -= actualYield;

        // 採集時重置計時器
        _regenTimer = 0;

        return actualYield;
    }
}