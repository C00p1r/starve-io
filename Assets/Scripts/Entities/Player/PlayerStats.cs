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
        // 1. 隨時間慢慢扣飢餓值與口渴值
        currentHunger = Mathf.Max(0, currentHunger - hungerDecayRate * Time.deltaTime);
        currentThirst = Mathf.Max(0, currentThirst - thirstDecayRate * Time.deltaTime);

        // 2. 飢餓或口渴歸零時，玩家會開始扣血
        if (currentHunger <= 0 || currentThirst <= 0)
        {
            TakeDamage(starvationDamage * Time.deltaTime);
        }

        float tempDecay = GetTemperatureDecayRate();
        if (tempDecay > 0f)
            currentTemperature = Mathf.Max(0f, currentTemperature - tempDecay * Time.deltaTime);

        _heatFlashTimer = Mathf.Max(0f, _heatFlashTimer - Time.deltaTime);
        _coldFlashTimer = Mathf.Max(0f, _coldFlashTimer - Time.deltaTime);
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
        else
        {
            _heatNotifyTimer = 0f;
            _heatDamageTimer = 0f;
            _heatFlashTimer = 0f;
        }

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
        else
        {
            _coldNotifyTimer = 0f;
            _coldDamageTimer = 0f;
            _coldFlashTimer = 0f;
        }

        // 3. 通知所有訂閱者（如 StatsUIHandler）更新畫面
        OnStatsUpdated?.Invoke();
    }

    // 被怪物攻擊可呼叫
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
        if (currentTemperature >= 95)
            rate = 2f;
        return rate;
    }
}
