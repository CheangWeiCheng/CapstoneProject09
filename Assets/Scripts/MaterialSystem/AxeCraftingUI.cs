using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AxeCraftingUI : MonoBehaviour
{
    [Header("Material Inventory")]
    [SerializeField] private MaterialInventory materialInventory;

    [Header("Inventory Menu")]
    [SerializeField] private InventoryMenuController inventoryMenuController;

    [Header("Material Count Text")]
    [SerializeField] private TMP_Text shatteredArmorText;
    [SerializeField] private TMP_Text arrowSticksText;
    [SerializeField] private TMP_Text tatteredClothText;

    [Header("Axe UI")]
    [SerializeField] private GameObject lockedAxeImage;
    [SerializeField] private GameObject unlockedAxeImage;
    [SerializeField] private TMP_Text axeStatusText;

    [Header("Craft Button")]
    [SerializeField] private Button craftAxeButton;
    [SerializeField] private TMP_Text craftButtonText;

    private void Start()
    {
        FindMaterialInventory();
        RefreshUI();

        if (materialInventory != null &&
            materialInventory.axeUnlocked &&
            inventoryMenuController != null)
        {
            inventoryMenuController.NotifyAxeCrafted(materialInventory);
        }
    }

    private void OnEnable()
    {
        FindMaterialInventory();
        RefreshUI();

        if (materialInventory != null &&
            materialInventory.axeUnlocked &&
            inventoryMenuController != null)
        {
            inventoryMenuController.NotifyAxeCrafted(materialInventory);
        }
    }

    public void CraftAxe()
    {
        if (materialInventory == null)
        {
            Debug.LogError(
                "AXE CRAFTING: MaterialInventory is missing."
            );

            return;
        }

        bool crafted = materialInventory.CraftAxe();

        if (!crafted)
        {
            Debug.LogWarning(
                "AXE CRAFTING: CraftAxe returned false."
            );

            RefreshUI();
            return;
        }

        Debug.Log(
            "AXE CRAFTING: Axe crafted successfully."
        );

        Debug.Log(
            "AXE CRAFTING: Axe Unlocked = " +
            materialInventory.axeUnlocked
        );

        if (inventoryMenuController != null)
        {
            inventoryMenuController.NotifyAxeCrafted(
                materialInventory
            );

            Debug.Log(
                "AXE CRAFTING: InventoryMenuController notified."
            );
        }
        else
        {
            Debug.LogError(
                "AXE CRAFTING: InventoryMenuController is not assigned."
            );
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (materialInventory == null)
        {
            SetText(shatteredArmorText, "0/3");
            SetText(arrowSticksText, "0/3");
            SetText(tatteredClothText, "0/2");
            SetText(axeStatusText, "LOCKED");
            SetText(craftButtonText, "CRAFT AXE");

            if (craftAxeButton != null)
            {
                craftAxeButton.interactable = false;
            }

            if (lockedAxeImage != null)
            {
                lockedAxeImage.SetActive(true);
            }

            if (unlockedAxeImage != null)
            {
                unlockedAxeImage.SetActive(false);
            }

            return;
        }

        SetText(
            shatteredArmorText,
            materialInventory.shatteredArmor +
            "/" +
            materialInventory.RequiredShatteredArmor
        );

        SetText(
            arrowSticksText,
            materialInventory.arrowSticks +
            "/" +
            materialInventory.RequiredArrowSticks
        );

        SetText(
            tatteredClothText,
            materialInventory.tatteredCloth +
            "/" +
            materialInventory.RequiredTatteredCloth
        );

        if (materialInventory.axeUnlocked)
        {
            ShowOwnedState();
        }
        else
        {
            ShowLockedState();
        }
    }

    private void ShowOwnedState()
    {
        SetText(
            axeStatusText,
            "OWNED"
        );

        SetText(
            craftButtonText,
            "OWNED"
        );

        if (craftAxeButton != null)
        {
            craftAxeButton.interactable = false;
        }

        if (lockedAxeImage != null)
        {
            lockedAxeImage.SetActive(false);
        }

        if (unlockedAxeImage != null)
        {
            unlockedAxeImage.SetActive(true);
        }
    }

    private void ShowLockedState()
    {
        SetText(
            axeStatusText,
            "LOCKED"
        );

        SetText(
            craftButtonText,
            "CRAFT AXE"
        );

        if (craftAxeButton != null)
        {
            craftAxeButton.interactable =
                materialInventory.CanCraftAxe();
        }

        if (lockedAxeImage != null)
        {
            lockedAxeImage.SetActive(true);
        }

        if (unlockedAxeImage != null)
        {
            unlockedAxeImage.SetActive(false);
        }
    }

    private void FindMaterialInventory()
    {
        if (materialInventory != null)
        {
            return;
        }

        materialInventory =
            FindFirstObjectByType<MaterialInventory>();

        if (materialInventory == null)
        {
            Debug.LogError(
                "AXE CRAFTING: Could not find MaterialInventory."
            );
        }
    }

    private void SetText(
        TMP_Text textObject,
        string value
    )
    {
        if (textObject != null)
        {
            textObject.text = value;
        }
    }
}