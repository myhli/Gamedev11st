using UnityEngine;

public class WorldItem : MonoBehaviour, GrabableItem
{
    [Header("Data Item")]
    [SerializeField] private ItemDetails itemData; // Tarik ItemPotion ke sini

    public void Grab()
    {
        if (InventoryManager.Instance != null && itemData != null)
        {
            InventoryManager.Instance.AddItem(itemData);
            Destroy(gameObject); // Hancurkan objek setelah diambil
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Cek jika yang menabrak adalah Player
        if (collision.CompareTag("Player"))
        {
            Grab();
        }
    }
}