using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("最大值")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxTemperature = 100f;

    [Header("當前數值")]
    public float currentHealth;
    public float currentHunger;
    public float currentThirst;
    public float currentTemperature;

    [Header("每秒扣除率")]
    [SerializeField] private float hungerDecayRate = 0.3f;
    [SerializeField] private float thirstDecayRate = 0.5f;
    [SerializeField] private float starvationDamage = 1.0f;

    // 定義一個事件，當數值更新時通知 UI
    public event Action OnStatsUpdated;
    public event Action OnPlayerDeath; // 死亡事件
    private bool _isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentTemperature = 50f; // 初始體溫
    }

    void Update()
    {
        // 1. 隨時間慢慢扣飢餓值與口渴值
        currentHunger = Mathf.Max(0, currentHunger - hungerDecayRate * Time.deltaTime);
        currentThirst = Mathf.Max(0, currentThirst - thirstDecayRate * Time.deltaTime);

        // 2. 飢餓或口渴歸零時，玩家會開始扣血
        if (currentHunger <= 0 || currentThirst <= 0)
        {
            TakeDamage(starvationDamage * Time.deltaTime);
        }

        // 3. 通知所有訂閱者（如 StatsUIHandler）更新畫面
        OnStatsUpdated?.Invoke();
    }

    // 被怪物攻擊可呼叫
    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnStatsUpdated?.Invoke();

        if (currentHealth <= 0)
        {
            _isDead = true;
            OnPlayerDeath?.Invoke(); // 觸發死亡
            Debug.Log("玩家已死亡");
        }
    }

    // 未來可以給食物或水呼叫
    public void RestoreHunger(float amount)
    {
        currentHunger = Mathf.Min(maxHunger, currentHunger + amount);
        OnStatsUpdated?.Invoke();
    }
    public void RestoreThirst(float amount)
    {
        currentThirst = Mathf.Min(maxThirst, currentThirst + amount);
        OnStatsUpdated?.Invoke();
    }
}