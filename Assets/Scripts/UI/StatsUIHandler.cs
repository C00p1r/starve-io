using UnityEngine;
using UnityEngine.UIElements;

public class StatsUIHandler : MonoBehaviour
{
    private UIDocument uiDocument;
    [SerializeField] private PlayerStats statsSource;

    // UI 元素參照
    private ProgressBar _healthBar;
    private ProgressBar _hungerBar;
    private ProgressBar _thirstBar;
    private ProgressBar _tempBar;
    private VisualElement _tempIcon;
    [SerializeField] private Sprite coldTempSprite;
    [SerializeField] private Sprite goodTempSprite;
    [SerializeField] private Sprite hotTempSprite;
    private Sprite _currentTempSprite;

    void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;

        // --- 請確保 UXML 中的 Name 屬性與這裡的字串一致 ---
        _healthBar = root.Q<ProgressBar>("HealthBar");
        _hungerBar = root.Q<ProgressBar>("HungerBar");
        _thirstBar = root.Q<ProgressBar>("ThirstBar");
        _tempBar = root.Q<ProgressBar>("TempBar");
        if (_tempBar != null)
            _tempIcon = _tempBar.Q<VisualElement>("TempIcon");

        if (statsSource != null)
        {
            // 訂閱事件：當數據變動時，執行 UpdateHUD
            statsSource.OnStatsUpdated += UpdateHUD;
        }
    }

    void OnDisable()
    {
        if (statsSource != null)
        {
            statsSource.OnStatsUpdated -= UpdateHUD;
        }
    }

    private void UpdateHUD()
    {
        if (statsSource == null) return;

        // 計算百分比並更新 UI
        if (_healthBar != null) _healthBar.value = (statsSource.currentHealth / statsSource.maxHealth) * 100f;
        if (_hungerBar != null) _hungerBar.value = (statsSource.currentHunger / statsSource.maxHunger) * 100f;
        if (_thirstBar != null) _thirstBar.value = (statsSource.currentThirst / statsSource.maxThirst) * 100f;
        if (_tempBar != null) _tempBar.value = statsSource.currentTemperature;

        UpdateTempIcon();
    }

    private void UpdateTempIcon()
    {
        if (_tempIcon == null || statsSource == null)
            return;

        float tempPercent = statsSource.maxTemperature > 0f
            ? statsSource.currentTemperature / statsSource.maxTemperature
            : 0f;

        Sprite nextSprite = tempPercent < 0.5f
            ? coldTempSprite
            : tempPercent <= 0.95f ? goodTempSprite : hotTempSprite;

        if (nextSprite == null || nextSprite == _currentTempSprite)
            return;

        _tempIcon.style.backgroundImage = new StyleBackground(nextSprite);
        _currentTempSprite = nextSprite;
    }
}
