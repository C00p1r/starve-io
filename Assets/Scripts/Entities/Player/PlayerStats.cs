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
    [SerializeField] private float dayTempDecayRate = 0.5f;
    [SerializeField] private float nightTempDecayRate = 1.0f;
    [SerializeField] private float heatDamagePerSecond = 2.0f;
    [SerializeField] private float heatDamageInterval = 1.0f;
    [SerializeField] private float hotThresholdPercent = 0.95f;
    [SerializeField] private float heatNotifyInterval = 2.5f;
    [SerializeField] private Color heatFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private float heatFlashDuration = 0.2f;
    [SerializeField] private float heatFlashCooldown = 1.0f;
    [SerializeField] private PlayerFeedback playerFeedback;
    [SerializeField] private float coldDamagePerSecond = 2.0f;
    [SerializeField] private float coldDamageInterval = 1.0f;
    [SerializeField] private float coldThresholdPercent = 0.05f;
    [SerializeField] private float coldNotifyInterval = 2.5f;
    [SerializeField] private Color coldFlashColor = new Color(0.5f, 0.75f, 1f, 1f);
    [SerializeField] private float coldFlashDuration = 0.2f;
    [SerializeField] private float coldFlashCooldown = 1.0f;

    // --- 新增狀態標記 (不影響 Inspector 的變數) ---
    [HideInInspector] public bool isStandingInWater = false;
    [HideInInspector] public bool isInSnow = false;

    // 定義一個事件，當數值更新時通知 UI
    public event Action OnStatsUpdated;
    public event Action OnPlayerDeath; // 死亡事件
    private bool _isDead = false;
    private float _heatNotifyTimer = 0f;
    private float _heatDamageTimer = 0f;
    private float _heatFlashTimer = 0f;
    private float _coldNotifyTimer = 0f;
    private float _coldDamageTimer = 0f;
    private float _coldFlashTimer = 0f;
    private TimeManager _timeManager;
    [Header("受傷與死亡音效")]
    [SerializeField] private AudioSource get_hurt;
    [SerializeField] private AudioSource dead;

    void Awake()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentTemperature = 60f; // 初始體溫
        if (playerFeedback == null)
            playerFeedback = GetComponent<PlayerFeedback>();
        _timeManager = FindObjectOfType<TimeManager>();
    }

    void Update()
    {
        if (_isDead) return;

        // 1. 飢餓值隨時間扣除
        currentHunger = Mathf.Max(0, currentHunger - hungerDecayRate * Time.deltaTime);

        // --- 修改後的口渴邏輯：水中恢復，陸地扣除 ---
        if (isStandingInWater)
        {
            // 在水裡時，以自然消耗的 10 倍速恢復水分 (可自行調整係數)
            currentThirst = Mathf.Min(maxThirst, currentThirst + (thirstDecayRate * 15f) * Time.deltaTime);
        }
        else
        {
            currentThirst = Mathf.Max(0, currentThirst - thirstDecayRate * Time.deltaTime);
        }

        // 2. 飢餓或口渴歸零時，玩家會開始扣血
        if (currentHunger <= 0 || currentThirst <= 0)
        {
            TakeDamage(starvationDamage * Time.deltaTime);
        }

        // --- 體溫邏輯：整合環境因素 ---
        float tempDecay = GetTemperatureDecayRate();
        if (tempDecay > 0f)
            currentTemperature = Mathf.Max(0f, currentTemperature - tempDecay * Time.deltaTime);

        // 閃爍計時器
        _heatFlashTimer = Mathf.Max(0f, _heatFlashTimer - Time.deltaTime);
        _coldFlashTimer = Mathf.Max(0f, _coldFlashTimer - Time.deltaTime);

        // 3. 高溫傷害判定
        float hotThreshold = maxTemperature * hotThresholdPercent;
        if (currentTemperature >= hotThreshold)
        {
            HandleExtremeTempEffect(ref _heatDamageTimer, ref _heatNotifyTimer, ref _heatFlashTimer,
                                    heatDamageInterval, heatDamagePerSecond, heatNotifyInterval,
                                    "Scorching heat!", heatFlashColor, heatFlashDuration, heatFlashCooldown);
        }
        else
        {
            ResetHeatTimers();
        }

        // 4. 低溫傷害判定
        float coldThreshold = maxTemperature * coldThresholdPercent;
        if (currentTemperature <= coldThreshold)
        {
            HandleExtremeTempEffect(ref _coldDamageTimer, ref _coldNotifyTimer, ref _coldFlashTimer,
                                    coldDamageInterval, coldDamagePerSecond, coldNotifyInterval,
                                    "Freezing cold!", coldFlashColor, coldFlashDuration, coldFlashCooldown);
        }
        else
        {
            ResetColdTimers();
        }


        // 3. 通知所有訂閱者（如 StatsUIHandler）更新畫面
        OnStatsUpdated?.Invoke();
    }

    // --- 封裝重複的傷害邏輯以簡化 Update ---
    private void HandleExtremeTempEffect(ref float dmgTimer, ref float notifyTimer, ref float flashTimer,
                                         float interval, float dmgPerSec, float notifyInt,
                                         string msg, Color flashCol, float duration, float cooldown)
    {
        dmgTimer += Time.deltaTime;
        if (dmgTimer >= interval)
        {
            TakeDamage(dmgPerSec * interval);
            dmgTimer = 0f;

            if (playerFeedback != null && flashTimer <= 0f)
            {
                playerFeedback.TriggerDamageFlash(flashCol, duration);
                flashTimer = cooldown;
            }
        }

        notifyTimer += Time.deltaTime;
        if (notifyTimer >= notifyInt)
        {
            UIEventManager.TriggerNotify(msg);
            notifyTimer = 0f;
        }
    }

    private void ResetHeatTimers() { _heatNotifyTimer = 0f; _heatDamageTimer = 0f; _heatFlashTimer = 0f; }
    private void ResetColdTimers() { _coldNotifyTimer = 0f; _coldDamageTimer = 0f; _coldFlashTimer = 0f; }

    public void TakeDamage(float amount)
    {
        if (_isDead)
        {
            if (dead != null && dead.isPlaying == false)
            {
                dead.Play();
            }
            return;
        }
        if (get_hurt != null)
        {
            get_hurt.time = 0;
            get_hurt.Play();
        }
        currentHealth = Mathf.Max(0, currentHealth - amount);

        // 播放受傷音效
        if (get_hurt != null && !get_hurt.isPlaying)
        {
            get_hurt.time = 0;
            get_hurt.Play();
        }

        if (currentHealth <= 0)
        {
            _isDead = true;
            if (dead != null) dead.Play();
            OnPlayerDeath?.Invoke();
            Debug.Log("玩家已死亡");
        }
        OnStatsUpdated?.Invoke();
    }

    // 恢復方法保持不變...
    public void RestoreHealth(float amount) { currentHealth = Mathf.Min(maxHealth, currentHealth + amount); OnStatsUpdated?.Invoke(); }
    public void RestoreHunger(float amount) { currentHunger = Mathf.Min(maxHunger, currentHunger + amount); OnStatsUpdated?.Invoke(); }
    public void RestoreThirst(float amount) { currentThirst = Mathf.Min(maxThirst, currentThirst + amount); OnStatsUpdated?.Invoke(); }
    public void ModifyTemperature(float amount) { currentTemperature = Mathf.Clamp(currentTemperature + amount, 0f, maxTemperature); OnStatsUpdated?.Invoke(); }

    private float GetTemperatureDecayRate()
    {
        if (_timeManager == null)
            _timeManager = FindObjectOfType<TimeManager>();

        // 基礎日夜下降率
        bool isNight = _timeManager != null && _timeManager.IsNight;
        float baseRate = isNight ? nightTempDecayRate : dayTempDecayRate;

        // --- 根據環境加成下降率 ---
        if (isStandingInWater)
        {
            return baseRate + 5.0f; // 在水裡體溫掉最快
        }
        if (isInSnow)
        {
            return baseRate + 1.5f; // 在雪地體溫掉稍快
        }

        return baseRate;
    }

}
