using UnityEngine;

public class BonfireHeatSource : MonoBehaviour
{
    [SerializeField] private float heatRadius = 2.5f;
    [SerializeField] private float heatPerSecond = 6f;

    private PlayerStats _playerStats;

    private void Update()
    {
        if (!TryCachePlayer())
            return;

        float distance = Vector2.Distance(transform.position, _playerStats.transform.position);
        if (distance <= heatRadius)
            _playerStats.ModifyTemperature(heatPerSecond * Time.deltaTime);
    }

    private bool TryCachePlayer()
    {
        if (_playerStats != null)
            return true;

        _playerStats = FindFirstObjectByType<PlayerStats>();
        return _playerStats != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, heatRadius);
    }
}
