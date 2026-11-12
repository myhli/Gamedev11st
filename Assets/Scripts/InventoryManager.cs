using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private GameObject inventoryCanvas;
    [SerializeField] private ItemDetailsPanel detailsPanel;
    [SerializeField] private ItemSlot[] itemSlot;

    private bool inventoryActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (inventoryCanvas == null) return;

        itemSlot = inventoryCanvas.GetComponentsInChildren<ItemSlot>(true);
        inventoryCanvas.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryCanvas == null || itemSlot == null) return;

        inventoryActive = !inventoryActive;
        Time.timeScale = inventoryActive ? 0f : 1f;
        inventoryCanvas.SetActive(inventoryActive);

        if (inventoryActive) ShowFirstItem();
    }

    public void AddItem(ItemDetails item)
    {
        if (item == null) return;

        foreach (ItemSlot slot in itemSlot)
        {
            if (slot.CanStackWith(item.ItemName))
            {
                slot.AddQuantity(item.Quantity);
                return;
            }
        }

        foreach (ItemSlot slot in itemSlot)
        {
            if (slot.IsEmpty)
            {
                slot.AddItem(item);
                return;
            }
        }

        Debug.LogWarning("Inventory penuh, '" + item.ItemName + "' hilang.");
    }

    public void SelectSlot(ItemSlot slot)
    {
        DesselectAllSlot();
        slot.SetSelected(true);

        if (detailsPanel == null) return;

        if (slot.IsEmpty) detailsPanel.Clear();
        else detailsPanel.Show(slot.ItemDetails, slot.Quantity);
    }

    private void ShowFirstItem()
    {
        foreach (ItemSlot slot in itemSlot)
        {
            if (!slot.IsEmpty)
            {
                SelectSlot(slot);
                return;
            }
        }

        if (detailsPanel != null) detailsPanel.Clear();
    }

    public void DesselectAllSlot()
    {
        foreach (ItemSlot slot in itemSlot)
        {
            slot.SetSelected(false);
        }
    }
}
