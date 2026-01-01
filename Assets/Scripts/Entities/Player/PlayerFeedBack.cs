using UnityEngine;
using System.Collections;

public class PlayerFeedback : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Color _originalColor;
    private Coroutine _flashCoroutine;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _originalColor = _sr.color; // 在一開始就存好真正的原始顏色
    }

    public void TriggerDamageFlash(Color flashColor, float duration)
    {
        // 如果已經在閃爍了，先停止舊的，避免顏色卡死
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(FlashRoutine(flashColor, duration));
    }

    private IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        _sr.color = flashColor;
        yield return new WaitForSeconds(duration);
        _sr.color = _originalColor; // 永遠變回 Awake 存好的那個顏色
        _flashCoroutine = null;
    }
}
