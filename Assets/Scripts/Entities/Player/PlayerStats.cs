using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxHunger = 100f;
    public float currentHunger;

    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHunger = maxHunger;
        currentHealth = maxHealth;
    }

    // 被 FoodItemData 呼叫
    public void RestoreHunger(int amount)
    {
        currentHunger = Mathf.Min(currentHunger + amount, maxHunger);
    }

    public void RestoreHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    // 之後可以在這裡寫 Update 每秒扣除飢餓度
}