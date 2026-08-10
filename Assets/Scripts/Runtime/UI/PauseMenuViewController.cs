using UnityEngine;
using UnityEngine.UI;

public class PauseMenuViewController : MonoBehaviour
{
    [Header("Pause Menu Root")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject mainMenuView;
    [SerializeField] private Image pauseMenuPanelImage;

    [Header("Sub Views")]
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject shopUI;
    [SerializeField] private GameObject guideUI;
    [SerializeField] private GameObject attackUI;
    [SerializeField] private GameObject leaderboardUI;
    [SerializeField] private GameObject settingsUI;

    [Header("Open Buttons")]
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button guideButton;
    [SerializeField] private Button attackGuideButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button settingsButton;

    [Header("Back Buttons")]
    [SerializeField] private Button[] backToPauseMenuButtons;

    private bool previousPausePanelState;

    private void Start()
    {
        HideAllSubViews();
        ShowMainMenuView();

        if (inventoryButton != null)
        {
            inventoryButton.onClick.RemoveListener(OpenInventory);
            inventoryButton.onClick.AddListener(OpenInventory);
        }

        if (shopButton != null)
        {
            shopButton.onClick.RemoveListener(OpenShop);
            shopButton.onClick.AddListener(OpenShop);
        }

        if (guideButton != null)
        {
            guideButton.onClick.RemoveListener(OpenGuide);
            guideButton.onClick.AddListener(OpenGuide);
        }

        if (attackGuideButton != null)
        {
            attackGuideButton.onClick.RemoveListener(OpenAttackGuide);
            attackGuideButton.onClick.AddListener(OpenAttackGuide);
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.RemoveListener(OpenLeaderboard);
            leaderboardButton.onClick.AddListener(OpenLeaderboard);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OpenSettings);
            settingsButton.onClick.AddListener(OpenSettings);
        }

        if (backToPauseMenuButtons != null)
        {
            for (int i = 0; i < backToPauseMenuButtons.Length; i++)
            {
                if (backToPauseMenuButtons[i] != null)
                {
                    backToPauseMenuButtons[i].onClick.RemoveListener(ReturnToPauseMainMenu);
                    backToPauseMenuButtons[i].onClick.AddListener(ReturnToPauseMainMenu);
                }
            }
        }

        previousPausePanelState = pauseMenuPanel != null && pauseMenuPanel.activeInHierarchy;
    }

    private void Update()
    {
        if (pauseMenuPanel == null)
        {
            return;
        }

        bool currentPausePanelState = pauseMenuPanel.activeInHierarchy;

        if (currentPausePanelState && previousPausePanelState == false)
        {
            ReturnToPauseMainMenu();
        }

        previousPausePanelState = currentPausePanelState;
    }

    public void OpenInventory()
    {
        OpenSubView(inventoryUI);
    }

    public void OpenShop()
    {
        OpenSubView(shopUI);
    }

    public void OpenGuide()
    {
        OpenSubView(guideUI);
    }

    public void OpenAttackGuide()
    {
        OpenSubView(attackUI);
    }

    public void OpenLeaderboard()
    {
        OpenSubView(leaderboardUI);
    }

    public void OpenSettings()
    {
        OpenSubView(settingsUI);
    }

    public void ReturnToPauseMainMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        HideAllSubViews();
        ShowMainMenuView();

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OpenSubView(GameObject targetView)
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        HideMainMenuView();
        HideAllSubViews();

        if (targetView != null)
        {
            targetView.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ShowMainMenuView()
    {
        if (mainMenuView != null)
        {
            mainMenuView.SetActive(true);
        }

        if (pauseMenuPanelImage != null)
        {
            pauseMenuPanelImage.enabled = true;
        }
    }

    private void HideMainMenuView()
    {
        if (mainMenuView != null)
        {
            mainMenuView.SetActive(false);
        }

        if (pauseMenuPanelImage != null)
        {
            pauseMenuPanelImage.enabled = false;
        }
    }

    private void HideAllSubViews()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }

        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }

        if (guideUI != null)
        {
            guideUI.SetActive(false);
        }

        if (attackUI != null)
        {
            attackUI.SetActive(false);
        }

        if (leaderboardUI != null)
        {
            leaderboardUI.SetActive(false);
        }

        if (settingsUI != null)
        {
            settingsUI.SetActive(false);
        }
    }
}