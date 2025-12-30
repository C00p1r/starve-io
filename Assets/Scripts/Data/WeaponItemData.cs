using StarveIO.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Items/Weapon")]
public class WeaponItemData : ItemData, IUsable
{
    [Header("武器戰鬥參數")]
    public int damage = 20;            // 攻擊力
    public float attackCooldown = 0.5f; // 攻擊冷卻 (秒)
    //public float attackRangeOffset = 1.2f; // 攻擊中心偏移    
    //public Vector2 attackArea = new Vector2(1.5f, 2.5f); // 攻擊膠囊體大小

    [Header("視覺與特效")]
    public GameObject weaponOnHandPrefab; // 手持武器的預製體 (顯示在玩家手上)
    public string attackAnimationTrigger = "Attack_Sword"; // 對應的動畫名稱

    public void Use(GameObject user)
    {
        // 當點擊背包武器時，通知玩家的裝備管理器換刀
        Debug.Log("try equip weapon!");
        if (user.TryGetComponent<WeaponHandler>(out var handler))
        {
            Debug.Log("equip weapon!");
            handler.EquipWeapon(this);
        }
    }
    public ItemCategory GetCategory() => ItemCategory.Weapon;
}