using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class DeathHandler : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private float fadeDuration = 2.0f; // 變暗的時間
    [SerializeField] private float fontSize = 40f;
    private VisualElement _darkScreen;

    void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnPlayerDeath += StartDeathSequence;
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (playerRigidbody == null)
            playerRigidbody = GetComponent<Rigidbody2D>();

        if (uiDocument != null)
        {
            // 假設你在 UXML 裡有一個滿版的黑色 VisualElement 叫 "DarkScreen"
            _darkScreen = uiDocument.rootVisualElement.Q<VisualElement>("DarkScreen");
            if (_darkScreen != null) _darkScreen.style.opacity = 0; // 初始透明
        }
    }

    void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnPlayerDeath -= StartDeathSequence;
    }

    private void StartDeathSequence()
    {
        DisablePlayerControl();
        // 1. 觸發 UI 通知 (使用你現有的 UI 系統)
        UIEventManager.TriggerNotify("YOU DIED", 150);

        // 2. 開始變暗並結束遊戲的協程
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // Disable player movement
        DisablePlayerControl();

        // 禁止玩家移動 (如果需要，可以在這裡把 PlayerController 的 enabled 設為 false)

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (_darkScreen != null)
            {
                // 逐漸增加透明度 (0 到 1)
                _darkScreen.style.opacity = elapsed / fadeDuration;
            }
            yield return null;
        }

        // 等待一小段時間讓玩家看清楚 YOU DIED
        yield return new WaitForSeconds(2.0f);

        Debug.Log("正在結束遊戲...");

        // 3. 結束遊戲
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 在編輯器中停止
#else
            Application.Quit(); // 在打包後的遊戲中退出
#endif
    }

    private void DisablePlayerControl()
    {
        if (playerController != null)
            playerController.enabled = false;
        if (playerRigidbody != null)
            playerRigidbody.linearVelocity = Vector2.zero;
    }
}