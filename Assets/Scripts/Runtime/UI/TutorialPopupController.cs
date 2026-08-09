using UnityEngine;
using UnityEngine.UI;

public class TutorialPopupController : MonoBehaviour
{
    [Header("Login")]
    [SerializeField] private GameObject loginScreen;

    [Header("Tutorial UI")]
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private GameObject normalGuideImage;
    [SerializeField] private GameObject attackGuideImage;

    [Header("Buttons")]
    [SerializeField] private Button nextGuideButton;
    [SerializeField] private Button previousGuideButton;
    [SerializeField] private Button closeTutorialButton;

    private bool tutorialStarted;

    private void Start()
    {
        tutorialStarted = false;

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
        }

        ShowNormalGuide();

        if (nextGuideButton != null)
        {
            nextGuideButton.onClick.RemoveListener(ShowAttackGuide);
            nextGuideButton.onClick.AddListener(ShowAttackGuide);
        }

        if (previousGuideButton != null)
        {
            previousGuideButton.onClick.RemoveListener(ShowNormalGuide);
            previousGuideButton.onClick.AddListener(ShowNormalGuide);
        }

        if (closeTutorialButton != null)
        {
            closeTutorialButton.onClick.RemoveListener(CloseTutorial);
            closeTutorialButton.onClick.AddListener(CloseTutorial);
        }
    }

    private void Update()
    {
        if (tutorialStarted)
        {
            return;
        }

        if (loginScreen == null)
        {
            return;
        }

        if (loginScreen.activeInHierarchy == false)
        {
            OpenTutorial();
        }
    }

    private void OpenTutorial()
    {
        tutorialStarted = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(true);
        }

        ShowNormalGuide();
    }

    private void ShowNormalGuide()
    {
        if (normalGuideImage != null)
        {
            normalGuideImage.SetActive(true);
        }

        if (attackGuideImage != null)
        {
            attackGuideImage.SetActive(false);
        }

        if (nextGuideButton != null)
        {
            nextGuideButton.gameObject.SetActive(true);
        }

        if (previousGuideButton != null)
        {
            previousGuideButton.gameObject.SetActive(false);
        }

        if (closeTutorialButton != null)
        {
            closeTutorialButton.gameObject.SetActive(false);
        }
    }

    private void ShowAttackGuide()
    {
        if (normalGuideImage != null)
        {
            normalGuideImage.SetActive(false);
        }

        if (attackGuideImage != null)
        {
            attackGuideImage.SetActive(true);
        }

        if (nextGuideButton != null)
        {
            nextGuideButton.gameObject.SetActive(false);
        }

        if (previousGuideButton != null)
        {
            previousGuideButton.gameObject.SetActive(true);
        }

        if (closeTutorialButton != null)
        {
            closeTutorialButton.gameObject.SetActive(true);
        }
    }

    private void CloseTutorial()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (tutorialUI != null)
        {
            tutorialUI.SetActive(false);
        }
    }
}