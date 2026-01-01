using UnityEngine;
using System.Collections;

public class HazardHandler : MonoBehaviour
{
    [Header("參考設定")]
    public GameObject player;

    [Header("傷害與回饋")]
    public float damageAmount = 10f; // 傷害量
    public float colorChangeInterval = 1.5f; // 傷害間隔
    public Color alertColor = Color.red;
    public float alertDuration = 0.3f;

    private Color _originalColor;
    private float _contactTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    // --- 關鍵修正：必須傳入 Collision2D 參數 ---
    void OnCollisionStay2D(Collision2D collision)
    {
        // 檢查撞到的是不是玩家
        if (collision.gameObject.CompareTag("Player"))
        {
            _contactTimer -= Time.deltaTime;

            if (_contactTimer <= 0)
            {
                SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();
                if (playerSR != null)
                {
                    // 啟動變色
                    StartCoroutine(ChangeColorCoroutine(playerSR));

                    // 執行扣血
                    PlayerStats stats = player.GetComponent<PlayerStats>();
                    if (stats != null)
                    {
                        stats.TakeDamage(damageAmount);
                    }

                    _contactTimer = colorChangeInterval;
                }
            }
        }
    }

    // 優化：傳入 SpriteRenderer 並自動恢復顏色
    IEnumerator ChangeColorCoroutine(SpriteRenderer sr)
    {
        Color oldColor = sr.color; // 記住受傷前的顏色（萬一玩家中毒變綠，受傷閃紅後應該變回綠色）
        sr.color = alertColor;

        yield return new WaitForSeconds(alertDuration);

        if (sr != null) // 防止協程運行時物件被摧毀
        {
            sr.color = oldColor;
        }
    }
}
