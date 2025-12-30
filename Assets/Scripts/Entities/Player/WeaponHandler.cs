using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public WeaponItemData currentWeapon; // 當前裝備的武器
    public Transform weaponHoldPoint;   // 玩家手上的一個空物件，用來掛載武器模型

    private GameObject _spawnedWeaponModel;
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        if (currentWeapon != null ) EquipWeapon(currentWeapon);
    }

    public void EquipWeapon(WeaponItemData newWeapon)
    {
        currentWeapon = newWeapon;

        // 1. 更新視覺：更換玩家手上的模型
        if (_spawnedWeaponModel != null) Destroy(_spawnedWeaponModel);
        if (currentWeapon.weaponOnHandPrefab != null)
        {
            _spawnedWeaponModel = Instantiate(currentWeapon.weaponOnHandPrefab, weaponHoldPoint);
        }

        // 2. 更新參數：將武器數值同步給 PlayerController
        _playerController.UpdateWeaponStats(
            currentWeapon.damage,
            currentWeapon.attackCooldown
        );

        Debug.Log($"已裝備武器：{currentWeapon.itemName}，攻擊力：{currentWeapon.damage}");
    }
}