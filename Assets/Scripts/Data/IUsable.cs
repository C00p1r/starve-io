using UnityEngine;

public interface IUsable
{
    // 每個可以使用的物品都要實作這個方法
    void Use(GameObject user);
    ItemCategory GetCategory();
}