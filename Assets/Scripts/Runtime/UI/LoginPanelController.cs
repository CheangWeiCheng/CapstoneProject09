using TMPro;
using UnityEngine;

public class LoginPanelController : MonoBehaviour
{
    [Header("Screen Objects")]
    [SerializeField] private GameObject authChoicePanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject createPanel;

    [Header("Optional Status Text")]
    [SerializeField] private TMP_Text loginStatusText;
    [SerializeField] private TMP_Text createStatusText;

    private bool authenticationScreenOpen;

    private void Awake()
    {
        OpenAuthenticationScreen();
        ShowAuthChoicePanel();
    }

    private void LateUpdate()
    {
        if (!authenticationScreenOpen)
        {
            return;
        }

        // Prevent other gameplay scripts from hiding
        // or locking the cursor during authentication.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OpenAuthenticationScreen()
    {
        authenticationScreenOpen = true;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowAuthChoicePanel()
    {
        SetObjectActive(authChoicePanel, true);
        SetObjectActive(loginPanel, false);
        SetObjectActive(createPanel, false);

        ClearStatusMessages();
    }

    public void ShowLoginPanel()
    {
        SetObjectActive(authChoicePanel, false);
        SetObjectActive(loginPanel, true);
        SetObjectActive(createPanel, false);

        ClearStatusMessages();
    }

    public void ShowCreatePanel()
    {
        SetObjectActive(authChoicePanel, false);
        SetObjectActive(loginPanel, false);
        SetObjectActive(createPanel, true);

        ClearStatusMessages();
    }

    public void CompleteAuthentication()
    {
        authenticationScreenOpen = false;

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        gameObject.SetActive(false);
    }

    public void ClearStatusMessages()
    {
        if (loginStatusText != null)
        {
            loginStatusText.text = string.Empty;
        }

        if (createStatusText != null)
        {
            createStatusText.text = string.Empty;
        }
    }

    private void SetObjectActive(
        GameObject targetObject,
        bool active
    )
    {
        if (targetObject != null)
        {
            targetObject.SetActive(active);
        }
    }

    private void OnDisable()
    {
        if (!authenticationScreenOpen)
        {
            return;
        }

        Time.timeScale = 1f;
    }
}