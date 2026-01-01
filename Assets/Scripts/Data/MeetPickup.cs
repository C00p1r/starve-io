// 2026/1/1 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using StarveIO.Data;
public class MeetPickup : MonoBehaviour
{
    [SerializeField] private ItemData itemData; // Reference to the ItemData asset
    [SerializeField] private int itemAmount = 1; // The amount of items to add to the player's inventory

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object colliding is the player
        if (collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Add the item to the player's inventory
                InventoryManager inventoryManager = playerController._inventoryManager;
                if (inventoryManager != null && itemData != null)
                {
                    inventoryManager.AddItem(itemData, itemAmount);
                    Debug.Log($"Player picked up {itemAmount} {itemData.itemName}.");
                }

                // Destroy the item object after pickup
                Destroy(gameObject);
            }
        }
    }
}
