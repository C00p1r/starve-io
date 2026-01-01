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

    // --- 新增狀態標記 (隱藏在 Inspector，避免弄亂介面) ---
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

        // 1. 隨時間慢慢扣飢餓值
        currentHunger = Mathf.Max(0, currentHunger - hungerDecayRate * Time.deltaTime);

        // --- 核心修改：湖泊恢復口渴邏輯 ---
        if (isStandingInWater)
        {
            // 在水裡時快速恢復 (使用 thirstDecayRate 的 30 倍作為強度，這樣不需新增變數)
            currentThirst = Mathf.Min(maxThirst, currentThirst + (thirstDecayRate * 30f) * Time.deltaTime);
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

        // --- 核心修改：體溫邏輯整合環境加成 ---
        float tempDecay = GetTemperatureDecayRate();
        if (tempDecay > 0f)
            currentTemperature = Mathf.Max(0f, currentTemperature - tempDecay * Time.deltaTime);

        // 以下處理高低溫計時器與閃爍 (保持你原本的代碼邏輯)
        _heatFlashTimer = Mathf.Max(0f, _heatFlashTimer - Time.deltaTime);
        _coldFlashTimer = Mathf.Max(0f, _coldFlashTimer - Time.deltaTime);

        // --- 高溫判定 ---
        float hotThreshold = maxTemperature * hotThresholdPercent;
        if (currentTemperature >= hotThreshold)
        {
            _heatDamageTimer += Time.deltaTime;
            if (_heatDamageTimer >= heatDamageInterval)
            {
                TakeDamage(heatDamagePerSecond * heatDamageInterval);
                _heatDamageTimer = 0f;
                if (playerFeedback != null && _heatFlashTimer <= 0f)
                {
                    playerFeedback.TriggerDamageFlash(heatFlashColor, heatFlashDuration);
                    _heatFlashTimer = heatFlashCooldown;
                }
            }
            _heatNotifyTimer += Time.deltaTime;
            if (_heatNotifyTimer >= heatNotifyInterval)
            {
                UIEventManager.TriggerNotify("Scorching heat!");
                _heatNotifyTimer = 0f;
            }
        }
        else { _heatNotifyTimer = 0f; _heatDamageTimer = 0f; }

        // --- 低溫判定 ---
        float coldThreshold = maxTemperature * coldThresholdPercent;
        if (currentTemperature <= coldThreshold)
        {
            _coldDamageTimer += Time.deltaTime;
            if (_coldDamageTimer >= coldDamageInterval)
            {
                TakeDamage(coldDamagePerSecond * coldDamageInterval);
                _coldDamageTimer = 0f;
                if (playerFeedback != null && _coldFlashTimer <= 0f)
                {
                    playerFeedback.TriggerDamageFlash(coldFlashColor, coldFlashDuration);
                    _coldFlashTimer = coldFlashCooldown;
                }
            }
            _coldNotifyTimer += Time.deltaTime;
            if (_coldNotifyTimer >= coldNotifyInterval)
            {
                UIEventManager.TriggerNotify("Freezing cold!");
                _coldNotifyTimer = 0f;
            }
        }
        else { _coldNotifyTimer = 0f; _coldDamageTimer = 0f; }

        OnStatsUpdated?.Invoke();
    }

    public void TakeDamage(float amount)
    {
        if (_isDead)
        {
            if (dead != null && !dead.isPlaying) dead.Play();
            return;
        }
        if (get_hurt != null && !get_hurt.isPlaying)
        {
            get_hurt.time = 0;
            get_hurt.Play();
        }
        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnStatsUpdated?.Invoke();

        if (currentHealth <= 0)
        {
            _isDead = true;
            if (dead != null) dead.Play();
            OnPlayerDeath?.Invoke();
            Debug.Log("玩家已死亡");
        }
    }

    public void RestoreHealth(float amount) { currentHealth = Mathf.Min(maxHealth, currentHealth + amount); OnStatsUpdated?.Invoke(); }
    public void RestoreHunger(float amount) { currentHunger = Mathf.Min(maxHunger, currentHunger + amount); OnStatsUpdated?.Invoke(); }
    public void RestoreThirst(float amount) { currentThirst = Mathf.Min(maxThirst, currentThirst + amount); OnStatsUpdated?.Invoke(); }

    public void ModifyTemperature(float amount)
    {
        currentTemperature = Mathf.Clamp(currentTemperature + amount, 0f, maxTemperature);
        OnStatsUpdated?.Invoke();
    }

    private float GetTemperatureDecayRate()
    {
        if (_timeManager == null)
            _timeManager = FindObjectOfType<TimeManager>();

        bool isNight = _timeManager != null && _timeManager.IsNight;
        float rate = isNight ? nightTempDecayRate : dayTempDecayRate;

        // --- 核心修改：環境對體溫的影響 ---
        if (isStandingInWater)
        {
            rate += 5.0f; // 站在水裡體溫掉超快
        }
        else if (isInSnow)
        {
            rate += 1.5f; // 雪地體溫掉較快
        }

        // 保留你原本的特殊過熱判定邏輯
        if (currentTemperature >= 95)
            rate = 2f;

        return rate;
    }
}
