using System;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirebaseAuthenticationController : MonoBehaviour
{
    [Header("Screen Controller")]
    [SerializeField] private LoginPanelController loginPanelController;

    [Header("Login Panel")]
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private TMP_Text loginStatusText;

    [Header("Login Hitboxes")]
    [SerializeField] private GameObject loginEmailInputHitbox;
    [SerializeField] private GameObject loginPasswordInputHitbox;
    [SerializeField] private GameObject loginButtonHitbox;

    [Header("Create Account Panel")]
    [SerializeField] private TMP_InputField createEmailInput;
    [SerializeField] private TMP_InputField createPasswordInput;
    [SerializeField] private TMP_Text createStatusText;

    [Header("Create Account Hitboxes")]
    [SerializeField] private GameObject createEmailInputHitbox;
    [SerializeField] private GameObject createPasswordInputHitbox;
    [SerializeField] private GameObject createButtonHitbox;

    [Header("Guest Login")]
    [SerializeField] private GameObject guestButtonHitbox;

    private FirebaseAuth auth;

    private Button loginEmailHitboxButton;
    private Button loginPasswordHitboxButton;
    private Button loginSubmitHitboxButton;

    private Button createEmailHitboxButton;
    private Button createPasswordHitboxButton;
    private Button createSubmitHitboxButton;

    private Button guestHitboxButton;

    private bool firebaseReady;
    private bool firebaseInitializing;
    private bool requestRunning;

    private PendingRequest pendingRequest = PendingRequest.None;

    private enum PendingRequest
    {
        None,
        Login,
        CreateAccount,
        Guest
    }

    private void Awake()
    {
        firebaseReady = false;
        firebaseInitializing = false;
        requestRunning = false;
        pendingRequest = PendingRequest.None;

        SetupHitboxes();
        InitializeFirebase();
    }

    private void OnDestroy()
    {
        RemoveHitboxListeners();
    }

    private void InitializeFirebase()
    {
        if (firebaseReady)
        {
            return;
        }

        if (firebaseInitializing)
        {
            return;
        }

        firebaseInitializing = true;

        SetLoginStatus("Connecting to Firebase...");
        SetCreateStatus("Connecting to Firebase...");

        Debug.Log("FIREBASE AUTH: Starting dependency check.");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            firebaseInitializing = false;

            if (task.IsCanceled)
            {
                firebaseReady = false;

                SetLoginStatus("Firebase initialization cancelled.");
                SetCreateStatus("Firebase initialization cancelled.");

                Debug.LogError("FIREBASE AUTH: Dependency check was cancelled.");
                return;
            }

            if (task.IsFaulted)
            {
                firebaseReady = false;

                SetLoginStatus("Firebase initialization failed.");
                SetCreateStatus("Firebase initialization failed.");

                Debug.LogError("FIREBASE AUTH: Dependency check failed.");
                Debug.LogException(task.Exception);

                return;
            }

            DependencyStatus dependencyStatus = task.Result;

            Debug.Log(
                "FIREBASE AUTH: Dependency status = " +
                dependencyStatus
            );

            if (dependencyStatus != DependencyStatus.Available)
            {
                firebaseReady = false;

                SetLoginStatus(
                    "Firebase unavailable: " +
                    dependencyStatus
                );

                SetCreateStatus(
                    "Firebase unavailable: " +
                    dependencyStatus
                );

                Debug.LogError(
                    "FIREBASE AUTH: Dependencies unavailable. Status = " +
                    dependencyStatus
                );

                return;
            }

            try
            {
                auth = FirebaseAuth.DefaultInstance;
                firebaseReady = true;

                SetLoginStatus("Firebase ready.");
                SetCreateStatus("Firebase ready.");

                Debug.Log("FIREBASE AUTH READY.");

                RunPendingRequest();
            }
            catch (Exception exception)
            {
                firebaseReady = false;

                SetLoginStatus("Firebase Auth failed to start.");
                SetCreateStatus("Firebase Auth failed to start.");

                Debug.LogError(
                    "FIREBASE AUTH: FirebaseAuth.DefaultInstance failed."
                );

                Debug.LogException(exception);
            }
        });
    }

    private void SetupHitboxes()
    {
        loginEmailHitboxButton = GetOrCreateButton(
            loginEmailInputHitbox,
            "Login Email Input Hitbox"
        );

        loginPasswordHitboxButton = GetOrCreateButton(
            loginPasswordInputHitbox,
            "Login Password Input Hitbox"
        );

        loginSubmitHitboxButton = GetOrCreateButton(
            loginButtonHitbox,
            "Login Button Hitbox"
        );

        createEmailHitboxButton = GetOrCreateButton(
            createEmailInputHitbox,
            "Create Email Input Hitbox"
        );

        createPasswordHitboxButton = GetOrCreateButton(
            createPasswordInputHitbox,
            "Create Password Input Hitbox"
        );

        createSubmitHitboxButton = GetOrCreateButton(
            createButtonHitbox,
            "Create Button Hitbox"
        );

        guestHitboxButton = GetOrCreateButton(
            guestButtonHitbox,
            "Guest Button Hitbox"
        );

        if (loginEmailHitboxButton != null)
        {
            loginEmailHitboxButton.onClick.AddListener(FocusLoginEmail);
        }

        if (loginPasswordHitboxButton != null)
        {
            loginPasswordHitboxButton.onClick.AddListener(FocusLoginPassword);
        }

        if (loginSubmitHitboxButton != null)
        {
            loginSubmitHitboxButton.onClick.AddListener(
                LoginWithEmailAndPassword
            );
        }

        if (createEmailHitboxButton != null)
        {
            createEmailHitboxButton.onClick.AddListener(FocusCreateEmail);
        }

        if (createPasswordHitboxButton != null)
        {
            createPasswordHitboxButton.onClick.AddListener(
                FocusCreatePassword
            );
        }

        if (createSubmitHitboxButton != null)
        {
            createSubmitHitboxButton.onClick.AddListener(CreateAccount);
        }

        if (guestHitboxButton != null)
        {
            guestHitboxButton.onClick.AddListener(SignInAsGuest);
        }
    }

    private Button GetOrCreateButton(
        GameObject hitboxObject,
        string hitboxName
    )
    {
        if (hitboxObject == null)
        {
            Debug.LogWarning(
                "FIREBASE AUTH: " +
                hitboxName +
                " is not assigned."
            );

            return null;
        }

        Button button = hitboxObject.GetComponent<Button>();

        if (button == null)
        {
            button = hitboxObject.AddComponent<Button>();

            Graphic graphic = hitboxObject.GetComponent<Graphic>();

            if (graphic != null)
            {
                graphic.raycastTarget = true;
                button.targetGraphic = graphic;
            }

            Debug.Log(
                "FIREBASE AUTH: Added Button component to " +
                hitboxName
            );
        }

        button.interactable = true;

        return button;
    }

    private void RemoveHitboxListeners()
    {
        if (loginEmailHitboxButton != null)
        {
            loginEmailHitboxButton.onClick.RemoveListener(FocusLoginEmail);
        }

        if (loginPasswordHitboxButton != null)
        {
            loginPasswordHitboxButton.onClick.RemoveListener(
                FocusLoginPassword
            );
        }

        if (loginSubmitHitboxButton != null)
        {
            loginSubmitHitboxButton.onClick.RemoveListener(
                LoginWithEmailAndPassword
            );
        }

        if (createEmailHitboxButton != null)
        {
            createEmailHitboxButton.onClick.RemoveListener(FocusCreateEmail);
        }

        if (createPasswordHitboxButton != null)
        {
            createPasswordHitboxButton.onClick.RemoveListener(
                FocusCreatePassword
            );
        }

        if (createSubmitHitboxButton != null)
        {
            createSubmitHitboxButton.onClick.RemoveListener(CreateAccount);
        }

        if (guestHitboxButton != null)
        {
            guestHitboxButton.onClick.RemoveListener(SignInAsGuest);
        }
    }

    public void LoginWithEmailAndPassword()
    {
        Debug.Log("LOGIN BUTTON HITBOX CLICKED.");

        if (!firebaseReady || auth == null)
        {
            pendingRequest = PendingRequest.Login;

            SetLoginStatus(
                "Firebase is starting. Login will continue automatically."
            );

            Debug.Log(
                "FIREBASE AUTH: Login request queued."
            );

            InitializeFirebase();
            return;
        }

        ExecuteLogin();
    }

    private void ExecuteLogin()
    {
        if (requestRunning)
        {
            SetLoginStatus("Please wait...");
            return;
        }

        if (loginEmailInput == null)
        {
            SetLoginStatus("Login Email Input is not assigned.");
            Debug.LogError("LOGIN: Login Email Input is not assigned.");
            return;
        }

        if (loginPasswordInput == null)
        {
            SetLoginStatus("Login Password Input is not assigned.");
            Debug.LogError("LOGIN: Login Password Input is not assigned.");
            return;
        }

        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrWhiteSpace(email))
        {
            SetLoginStatus("Login failed: Email is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            SetLoginStatus("Login failed: Password is empty.");
            return;
        }

        requestRunning = true;

        SetLoginStatus("Logging in...");

        Debug.Log(
            "FIREBASE AUTH: Attempting login for " +
            email
        );

        auth.SignInWithEmailAndPasswordAsync(
            email,
            password
        ).ContinueWithOnMainThread(task =>
        {
            requestRunning = false;

            if (task.IsCanceled)
            {
                SetLoginStatus("Login cancelled.");

                Debug.LogWarning(
                    "FIREBASE AUTH: Login cancelled."
                );

                return;
            }

            if (task.IsFaulted)
            {
                string errorMessage = GetFirebaseErrorMessage(
                    task.Exception
                );

                SetLoginStatus(
                    "Login failed: " +
                    errorMessage
                );

                Debug.LogError(
                    "FIREBASE AUTH: Login failed."
                );

                Debug.LogException(task.Exception);

                return;
            }

            FirebaseUser user = auth.CurrentUser;

            if (user == null)
            {
                SetLoginStatus(
                    "Login failed: Firebase returned no user."
                );

                Debug.LogError(
                    "FIREBASE AUTH: Login succeeded but CurrentUser is null."
                );

                return;
            }

            SetLoginStatus("Login successful.");

            Debug.Log(
                "FIREBASE LOGIN SUCCESS. UID = " +
                user.UserId
            );

            FinishAuthentication();
        });
    }

    public void CreateAccount()
    {
        Debug.Log("CREATE ACCOUNT BUTTON HITBOX CLICKED.");

        if (!firebaseReady || auth == null)
        {
            pendingRequest = PendingRequest.CreateAccount;

            SetCreateStatus(
                "Firebase is starting. Account creation will continue automatically."
            );

            Debug.Log(
                "FIREBASE AUTH: Create Account request queued."
            );

            InitializeFirebase();
            return;
        }

        ExecuteCreateAccount();
    }

    private void ExecuteCreateAccount()
    {
        if (requestRunning)
        {
            SetCreateStatus("Please wait...");
            return;
        }

        if (createEmailInput == null)
        {
            SetCreateStatus("Create Email Input is not assigned.");

            Debug.LogError(
                "CREATE ACCOUNT: Create Email Input is not assigned."
            );

            return;
        }

        if (createPasswordInput == null)
        {
            SetCreateStatus("Create Password Input is not assigned.");

            Debug.LogError(
                "CREATE ACCOUNT: Create Password Input is not assigned."
            );

            return;
        }

        string email = createEmailInput.text.Trim();
        string password = createPasswordInput.text;

        if (string.IsNullOrWhiteSpace(email))
        {
            SetCreateStatus("Create failed: Email is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            SetCreateStatus("Create failed: Password is empty.");
            return;
        }

        if (password.Length < 6)
        {
            SetCreateStatus(
                "Create failed: Password must be at least 6 characters."
            );

            return;
        }

        requestRunning = true;

        SetCreateStatus("Creating account...");

        Debug.Log(
            "FIREBASE AUTH: Attempting account creation for " +
            email
        );

        auth.CreateUserWithEmailAndPasswordAsync(
            email,
            password
        ).ContinueWithOnMainThread(task =>
        {
            requestRunning = false;

            if (task.IsCanceled)
            {
                SetCreateStatus("Account creation cancelled.");

                Debug.LogWarning(
                    "FIREBASE AUTH: Account creation cancelled."
                );

                return;
            }

            if (task.IsFaulted)
            {
                string errorMessage = GetFirebaseErrorMessage(
                    task.Exception
                );

                SetCreateStatus(
                    "Create failed: " +
                    errorMessage
                );

                Debug.LogError(
                    "FIREBASE AUTH: Account creation failed."
                );

                Debug.LogException(task.Exception);

                return;
            }

            FirebaseUser user = auth.CurrentUser;

            if (user == null)
            {
                SetCreateStatus(
                    "Create failed: Firebase returned no user."
                );

                Debug.LogError(
                    "FIREBASE AUTH: Account created but CurrentUser is null."
                );

                return;
            }

            SetCreateStatus("Account created successfully.");

            Debug.Log(
                "FIREBASE ACCOUNT CREATED SUCCESSFULLY. UID = " +
                user.UserId
            );

            FinishAuthentication();
        });
    }

    public void SignInAsGuest()
    {
        Debug.Log("GUEST BUTTON HITBOX CLICKED.");

        if (!firebaseReady || auth == null)
        {
            pendingRequest = PendingRequest.Guest;

            SetLoginStatus(
                "Firebase is starting. Guest login will continue automatically."
            );

            Debug.Log(
                "FIREBASE AUTH: Guest request queued."
            );

            InitializeFirebase();
            return;
        }

        ExecuteGuestLogin();
    }

    private void ExecuteGuestLogin()
    {
        if (requestRunning)
        {
            SetLoginStatus("Please wait...");
            return;
        }

        requestRunning = true;

        SetLoginStatus("Signing in as guest...");

        if (auth.CurrentUser != null)
        {
            auth.SignOut();
        }

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            requestRunning = false;

            if (task.IsCanceled)
            {
                SetLoginStatus("Guest login cancelled.");
                return;
            }

            if (task.IsFaulted)
            {
                string errorMessage = GetFirebaseErrorMessage(
                    task.Exception
                );

                SetLoginStatus(
                    "Guest login failed: " +
                    errorMessage
                );

                Debug.LogError(
                    "FIREBASE AUTH: Guest login failed."
                );

                Debug.LogException(task.Exception);

                return;
            }

            FirebaseUser user = auth.CurrentUser;

            if (user == null)
            {
                SetLoginStatus(
                    "Guest login failed: Firebase returned no user."
                );

                return;
            }

            SetLoginStatus("Guest login successful.");

            Debug.Log(
                "FIREBASE GUEST LOGIN SUCCESS. UID = " +
                user.UserId
            );

            FinishAuthentication();
        });
    }

    public void LoginAsGuest()
    {
        SignInAsGuest();
    }

    public void GuestLogin()
    {
        SignInAsGuest();
    }

    public void SignInAnonymously()
    {
        SignInAsGuest();
    }

    private void RunPendingRequest()
    {
        PendingRequest requestToRun = pendingRequest;

        pendingRequest = PendingRequest.None;

        if (requestToRun == PendingRequest.Login)
        {
            Debug.Log(
                "FIREBASE AUTH: Running queued Login request."
            );

            ExecuteLogin();
            return;
        }

        if (requestToRun == PendingRequest.CreateAccount)
        {
            Debug.Log(
                "FIREBASE AUTH: Running queued Create Account request."
            );

            ExecuteCreateAccount();
            return;
        }

        if (requestToRun == PendingRequest.Guest)
        {
            Debug.Log(
                "FIREBASE AUTH: Running queued Guest request."
            );

            ExecuteGuestLogin();
        }
    }

    private void FinishAuthentication()
    {
        if (loginPasswordInput != null)
        {
            loginPasswordInput.text = string.Empty;
        }

        if (createPasswordInput != null)
        {
            createPasswordInput.text = string.Empty;
        }

        Debug.Log(
            "FIREBASE AUTH: Authentication complete. Closing Login Screen."
        );

        if (loginPanelController != null)
        {
            loginPanelController.CompleteAuthentication();
        }
        else
        {
            // Preserve the old failure-safe behavior if scene wiring is absent.
            gameObject.SetActive(false);
        }
    }

    private void FocusLoginEmail()
    {
        FocusInputField(
            loginEmailInput,
            "Login Email"
        );
    }

    private void FocusLoginPassword()
    {
        FocusInputField(
            loginPasswordInput,
            "Login Password"
        );
    }

    private void FocusCreateEmail()
    {
        FocusInputField(
            createEmailInput,
            "Create Email"
        );
    }

    private void FocusCreatePassword()
    {
        FocusInputField(
            createPasswordInput,
            "Create Password"
        );
    }

    private void FocusInputField(
        TMP_InputField inputField,
        string fieldName
    )
    {
        if (inputField == null)
        {
            Debug.LogError(
                fieldName +
                " TMP_InputField is not assigned."
            );

            return;
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(
                inputField.gameObject
            );
        }

        inputField.Select();
        inputField.ActivateInputField();

        Debug.Log(
            fieldName +
            " hitbox clicked."
        );
    }

    private string GetFirebaseErrorMessage(
        AggregateException exception
    )
    {
        if (exception == null)
        {
            return "Unknown Firebase error.";
        }

        AggregateException flattenedException = exception.Flatten();

        if (flattenedException.InnerExceptions.Count > 0)
        {
            Exception innerException =
                flattenedException.InnerExceptions[0];

            FirebaseException firebaseException =
                innerException as FirebaseException;

            if (firebaseException != null)
            {
                return
                    "Firebase error code " +
                    firebaseException.ErrorCode +
                    ": " +
                    firebaseException.Message;
            }

            return innerException.Message;
        }

        return flattenedException.Message;
    }

    private void SetLoginStatus(string message)
    {
        SetStatus(
            loginStatusText,
            message
        );
    }

    private void SetCreateStatus(string message)
    {
        SetStatus(
            createStatusText,
            message
        );
    }

    private void SetStatus(
        TMP_Text targetText,
        string message
    )
    {
        if (targetText != null)
        {
            targetText.text = message;
        }

        Debug.Log(
            "LOGIN STATUS: " +
            message
        );
    }
}
