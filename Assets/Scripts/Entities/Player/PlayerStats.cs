using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("--- 核心數值 (最大值) ---")]
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float maxTemperature = 100f;

    [Header("--- 即時狀態 (僅供觀察) ---")]
    public float currentHealth;
    public float currentHunger;
    public float currentThirst;
    public float currentTemperature;

    [Header("--- 飢餓系統 (Hunger) ---")]
    [Tooltip("每秒自然消耗的飽食度")]
    public float hungerDecayRate = 0.3f;
    [Tooltip("飽食度歸零後，每秒扣除的生命值")]
    public float starvationDamage = 1.0f;

    [Header("--- 飲水系統 (Thirst) ---")]
    [Tooltip("每秒自然消耗的飲水度")]
    public float thirstDecayRate = 0.5f;
    [Tooltip("站在水裡時，每秒恢復的飲水度")]
    public float thirstRegenRate = 5f;

    [Header("--- 體溫系統 (Temperature) ---")]
    [Tooltip("一般環境下的每秒體溫下降速度")]
    public float ambientTempLoss = 0.2f;
    [Tooltip("站在雪地時的每秒體溫下降速度")]
    public float snowTempLoss = 1.5f;
    [Tooltip("站在水裡時的每秒體溫下降速度 (通常最快)")]
    public float waterTempLoss = 1f;
    [Tooltip("體溫歸零後，每秒扣除的生命值")]
    public float freezingDamage = 2f;

    [Header("--- 狀態標記 ---")]
    public bool isStandingInWater = false;
    public bool isInSnow = false;

    // 事件通知
    public event Action OnStatsUpdated;
    public event Action OnPlayerDeath;
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
        HandleThirst();
        HandleTemperature();
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
    void HandleTemperature()
    {
        // 1. 計算目前的體溫下降速率
        float currentDropRate = ambientTempLoss;

        if (isStandingInWater)
            currentDropRate = waterTempLoss; // 水中優先最冷
        else if (isInSnow)
            currentDropRate = snowTempLoss;  // 雪地次之

        // 2. 執行扣除
        currentTemperature -= currentDropRate * Time.deltaTime;
        currentTemperature = Mathf.Clamp(currentTemperature, 0, maxTemperature);

        // 3. 失溫扣血邏輯
        if (currentTemperature <= 0)
        {
            currentHealth -= freezingDamage * Time.deltaTime;
            if (currentHealth < 0) currentHealth = 0;
            Debug.Log("你快凍死了！生命值: " + (int)currentHealth);
        }
    }
    void HandleThirst()
    {
        if (isStandingInWater)
        {
            if (currentThirst < maxThirst)
            {
                currentThirst += thirstRegenRate * Time.deltaTime;
                currentThirst = Mathf.Min(currentThirst, maxThirst);
            }
        }
        else
        {
            currentThirst -= thirstDecayRate * Time.deltaTime;
        }
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
    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnStatsUpdated?.Invoke();
    }
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
