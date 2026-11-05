using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    InputAction inventoryAction;
    [SerializeField] private GameObject InventoryCanvas;
    [SerializeField] private int slotCount = 16;
    private bool InventoryActive;
    public ItemSlot[] itemSlot;
public static InventoryManager Instance { get; private set; }

private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    // add detail panel
    [SerializeField] private ItemDetailsPanel detailsPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Cari semua ItemSlot yang sudah kamu susun di dalam Canvas
    itemSlot = InventoryCanvas.GetComponentsInChildren<ItemSlot>(true);

    InventoryCanvas.SetActive(false);
    inventoryAction = InputSystem.actions.FindAction("Inventory");
    InventoryActive = false;

        if (detailsPanel == null)
        {
            Debug.LogWarning("No ItemDetailsPanel is assigned", this);
            // Cari otomatis jika belum ditarik di Inspector
        detailsPanel = InventoryCanvas.GetComponentInChildren<ItemDetailsPanel>(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool toggle = (inventoryAction != null && inventoryAction.triggered) ||
                      (Keyboard.current != null && (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame));

        if (toggle)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (InventoryCanvas == null) return;

        InventoryActive = !InventoryActive;
        Time.timeScale = InventoryActive ? 0f : 1f;
        InventoryCanvas.SetActive(InventoryActive);
        if (InventoryActive)
        {
            ShowFirstItem();
        }
    }

    private void OnGUI()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 13;
        buttonStyle.fontStyle = FontStyle.Bold;

        string label = InventoryActive ? "Tutup Inventory [I]" : "Buka Inventory [I]";
        if (GUI.Button(new Rect(Screen.width - 180, 20, 160, 36), label, buttonStyle))
        {
            ToggleInventory();
        }
    }

    // Build the slot objects at runtime so no manual wiring is needed in the scene
    private void CreateSlots()
    {
        ItemSlot slotPrefab = Resources.Load<ItemSlot>("ItemSlot");
        if (slotPrefab == null)
        {
            Debug.LogError("ItemSlot prefab was not found in a Resources folder.", this);
            itemSlot = new ItemSlot[0];
            return;
        }

        // Put the slots inside the GridLayoutGroup container (InventorySlots), if there is one
        Transform parent = InventoryCanvas.transform;
        GridLayoutGroup grid = InventoryCanvas.GetComponentInChildren<GridLayoutGroup>(true);
        if (grid != null)
        {
            parent = grid.transform;
        }

        itemSlot = new ItemSlot[slotCount];
        for (int i = 0; i < itemSlot.Length; i++)
        {
            itemSlot[i] = Instantiate(slotPrefab, parent);
        }
    }

    public void AddItem(ItemDetails item)
    {
        Debug.Log("Item added to inventory: " + item.ItemName + " Quantity: " + item.Quantity);

        if (itemSlot == null || itemSlot.Length == 0)
        {
            Debug.LogWarning("Inventory has no slots, '" + item.ItemName + "' was lost.");
            return;
        }

        // for (int i = 0; i < itemSlot.Length; i++)
        // {
        //     if (!itemSlot[i].isFull)
        //     {
        //         itemSlot[i].AddItem(item);
        //         return;
        //     }
        // }

        // Logic to check if item already exists in the inventory and can be stacked
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].CanStackWith(item.ItemName))
            {
                itemSlot[i].AddQuantity(item.Quantity);
                return;
            }
        }

        // Logic to add the item to an empty slot if it cannot be stacked
        for (int i = 0; i < itemSlot.Length; i++)
        {
            if (itemSlot[i].IsEmpty)
            {
                itemSlot[i].AddItem(item);
                return;
            }
        }

        Debug.LogWarning("Inventory is full, '" + item.ItemName + "' was lost.");
    }

   public void SelectSlot(ItemSlot slot)
{
    DesselectAllSlot();

    if (slot.selectedPanel != null)
    {
        slot.selectedPanel.SetActive(true);
    }
    slot.isItemSelected = true;

    if (detailsPanel != null)
    {
        if (!slot.IsEmpty)
        {
            detailsPanel.Show(slot.ItemDetails, slot.itemQuantity);
        }
        else
        {
            detailsPanel.Clear(); // Bersihkan teks jika slot kosong
        }
    }
}

    private void ShowFirstItem()
    {
        if (itemSlot != null)
        {
            foreach (ItemSlot slot in itemSlot)
            {
                if (!slot.IsEmpty)
                {
                    SelectSlot(slot);
                    return;
                }
            }
        }

        if (detailsPanel != null)
        {
            detailsPanel.Clear();
        }
    }

    public void DesselectAllSlot()
    {
        foreach (ItemSlot slot in itemSlot)
        {
            slot.isItemSelected = false;
            slot.selectedPanel.SetActive(false);
        }
    }
}


//nanti ada update to check is empty, file yang berhubungan inventorymanager,itemdetailpanel,itemdetail. tujuan nya pas nge klik inventory yang ada item nya nanti munculin deskripsi
//buat 2 method selectslot(isinya desselectAllSlot() , slot.selectedpanel.SetActive, slot.isitemselected dan nanti ada if detailspanel != null detailpanel.show(slot.itemdetails, slot.itemQuantity))
//dan showfirst item isinya nanti ada if item slsot != null yang isinya foreach items;ot slot in item slot yang berisi if !slot.isEmpty = selectslot(slot); return;