using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Controls the right-side panel (InventoryDetails) of the inventory.
// It draws the details of the item the player has selected.
public class ItemDetailsPanel : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemQuantityText;
    [SerializeField] private TMP_Text itemDescriptionText;

    private void Awake()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        // Pastikan referensi tidak tertukar jika namanya mengindikasikan sebaliknya
        if (itemQuantityText != null && itemDescriptionText != null)
        {
            string qtyName = itemQuantityText.gameObject.name.ToLower();
            string descName = itemDescriptionText.gameObject.name.ToLower();

            if (qtyName.Contains("desc") || descName.Contains("qty"))
            {
                TMP_Text temp = itemQuantityText;
                itemQuantityText = itemDescriptionText;
                itemDescriptionText = temp;
            }
        }
    }

    // Show the details of the selected item.
    // quantity is the stack size currently stored in the slot.
    public void Show(ItemDetails item, int quantity)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        ValidateReferences();

        if (itemImage != null)
        {
            itemImage.sprite = item.ItemIcon;
            itemImage.enabled = true;
        }
        if (itemNameText != null) itemNameText.text = item.ItemName;
        if (itemQuantityText != null) itemQuantityText.text = "Quantity: " + quantity;
        if (itemDescriptionText != null) itemDescriptionText.text = item.Description;
    }

    // Empty the panel, used when the selected slot has no item.
    public void Clear()
    {
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
        if (itemNameText != null) itemNameText.text = "";
        if (itemQuantityText != null) itemQuantityText.text = "";
        if (itemDescriptionText != null) itemDescriptionText.text = "";
    }
}
