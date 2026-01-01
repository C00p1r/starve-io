using StarveIO.Data;
using System;
using System.Collections;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] private ResourceData data; // 拖入對應的 ResourceData 資源檔

    private int _currentStock;
    private float _regenTimer;
    private event Action OnResourceEmpty;

    [Header("打擊感設定")]
    [SerializeField] private Transform visualTransform; // 拖入顯示圖片的子物件
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeStrength = 0.15f;
    [SerializeField] private GameObject stockVisual;

    private Coroutine _shakeCoroutine;
    private Vector3 _originalLocalPos;
    void Awake()
    {
        if (visualTransform == null) visualTransform = transform;
        _originalLocalPos = visualTransform.localPosition;
    }

    // 供 PlayerController 呼叫的介面
    public void TriggerHitEffect(Vector2 attackDirection)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine(attackDirection));
    }

    private IEnumerator ShakeRoutine(Vector2 direction)
    {
        float elapsed = 0f;
        // 確保震動方向是標準化的
        Vector3 pushDir = (Vector3)direction.normalized;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            // 使用 Sin 波形產生「推出去再彈回來」的效果
            // 0 -> 1 -> 0 的曲線
            float curve = Mathf.Sin((elapsed / shakeDuration) * Mathf.PI);

            visualTransform.localPosition = _originalLocalPos + (pushDir * curve * shakeStrength);

            yield return null;
        }

        visualTransform.localPosition = _originalLocalPos;
        _shakeCoroutine = null;
    }

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
        UpdateStockVisual();
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
            UpdateStockVisual();
        }
    }

    public int GatherResource()
    {
        if (_currentStock <= 0)
        {
            OnResourceEmpty?.Invoke();
            UIEventManager.TriggerNotify("No resources left. (Will regrow)");
            return 0;
        }

        int actualYield = Mathf.Min(data.yieldPerHit, _currentStock);
        _currentStock -= actualYield;

        // 採集時重置計時器
        _regenTimer = 0;
        UpdateStockVisual();

        return actualYield;
    }

    private void UpdateStockVisual()
    {
        if (stockVisual == null)
            return;

        stockVisual.SetActive(_currentStock > 0);
    }
}
