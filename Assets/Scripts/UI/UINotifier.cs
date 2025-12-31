using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using Unity.VisualScripting;

public class UINotifier : MonoBehaviour
{
    private Label _notifyLabel;
    private Coroutine _currentFadeRoutine;
    [Header("顯示設定")]
    [SerializeField] private float DisplayDuration = 2f;
    [SerializeField] private float FadeOutDuration = 0.5f;

    private float originFontSize;
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _notifyLabel = root.Q<Label>("NotifyLabel");
        _notifyLabel.style.visibility = Visibility.Hidden;
        // 訂閱事件
        UIEventManager.OnNotify += ShowNotification;
    }

    private void OnDisable()
    {
        UIEventManager.OnNotify -= ShowNotification;
    }
    private void ShowNotification(string message, float fontSize)
    {
        if (_notifyLabel == null) return;
        if (_currentFadeRoutine != null) StopCoroutine(_currentFadeRoutine);
        _notifyLabel.style.fontSize = fontSize;
        _currentFadeRoutine = StartCoroutine(FadeNotify(message));
    }
    private IEnumerator FadeNotify(string message)
    {
        _notifyLabel.text = message;
        _notifyLabel.style.visibility = Visibility.Visible;
        _notifyLabel.style.opacity = 1f;
        // 顯示 2 秒
        yield return new WaitForSeconds(DisplayDuration);

        // 簡單的淡出效果 (如果 USS 有設定 transition，這裡改透明度即可)
        _notifyLabel.style.opacity = 0f;

        yield return new WaitForSeconds(FadeOutDuration);
        _notifyLabel.style.visibility = Visibility.Hidden;
    }
}