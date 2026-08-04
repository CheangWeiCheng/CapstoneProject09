using System;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class FirebaseAuthenticationController : MonoBehaviour
{
    private const string PlayerNameKey =
        "ARMAMATION_PlayerName";

    [Header("Screen Controller")]
    [SerializeField]
    private LoginPanelController loginPanelController;

    [Header("Login Panel")]
    [SerializeField]
    private TMP_InputField loginEmailInput;

    [SerializeField]
    private TMP_InputField loginPasswordInput;

    [SerializeField]
    private TMP_Text loginStatusText;

    [Header("Create Account Panel")]
    [SerializeField]
    private TMP_InputField createEmailInput;

    [SerializeField]
    private TMP_InputField createPasswordInput;

    [SerializeField]
    private TMP_Text createStatusText;

    private FirebaseAuth auth;

    private bool firebaseReady;
    private bool requestRunning;

    private void Start()
    {
        InitialiseFirebase();
    }

    private void InitialiseFirebase()
    {
        firebaseReady = false;

        SetBothStatusTexts(
            "Connecting to Firebase..."
        );

        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    SetBothStatusTexts(
                        "Firebase setup was cancelled."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Firebase dependency check failed: " +
                        task.Exception
                    );

                    SetBothStatusTexts(
                        "Could not connect to Firebase."
                    );

                    return;
                }

                DependencyStatus dependencyStatus =
                    task.Result;

                if (dependencyStatus !=
                    DependencyStatus.Available)
                {
                    Debug.LogError(
                        "Firebase dependencies unavailable: " +
                        dependencyStatus
                    );

                    SetBothStatusTexts(
                        "Firebase is unavailable."
                    );

                    return;
                }

                auth = FirebaseAuth.DefaultInstance;
                firebaseReady = true;

                ClearStatusTexts();

                Debug.Log(
                    "Firebase Authentication is ready."
                );
            });
    }

    public void LoginWithEmailAndPassword()
    {
        if (!CanStartRequest(loginStatusText))
        {
            return;
        }

        string email =
            loginEmailInput != null
                ? loginEmailInput.text.Trim()
                : string.Empty;

        string password =
            loginPasswordInput != null
                ? loginPasswordInput.text
                : string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            SetLoginStatus(
                "Enter your email address."
            );

            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetLoginStatus(
                "Enter your password."
            );

            return;
        }

        requestRunning = true;
        SetLoginStatus("Logging in...");

        auth
            .SignInWithEmailAndPasswordAsync(
                email,
                password
            )
            .ContinueWithOnMainThread(task =>
            {
                requestRunning = false;

                if (task.IsCanceled)
                {
                    SetLoginStatus(
                        "Login was cancelled."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Firebase login failed: " +
                        task.Exception
                    );

                    SetLoginStatus(
                        GetFirebaseErrorMessage(
                            task.Exception
                        )
                    );

                    return;
                }

                FirebaseUser user =
                    task.Result.User;

                if (user == null)
                {
                    SetLoginStatus(
                        "Login failed."
                    );

                    return;
                }

                SaveAccountPlayerName(user);
                CompleteAuthentication(user);
            });
    }

    public void CreateAccount()
    {
        if (!CanStartRequest(createStatusText))
        {
            return;
        }

        string email =
            createEmailInput != null
                ? createEmailInput.text.Trim()
                : string.Empty;

        string password =
            createPasswordInput != null
                ? createPasswordInput.text
                : string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            SetCreateStatus(
                "Enter your email address."
            );

            return;
        }

        if (password.Length < 6)
        {
            SetCreateStatus(
                "Password must contain at least 6 characters."
            );

            return;
        }

        requestRunning = true;

        SetCreateStatus(
            "Creating your account..."
        );

        auth
            .CreateUserWithEmailAndPasswordAsync(
                email,
                password
            )
            .ContinueWithOnMainThread(task =>
            {
                requestRunning = false;

                if (task.IsCanceled)
                {
                    SetCreateStatus(
                        "Account creation was cancelled."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Firebase account creation failed: " +
                        task.Exception
                    );

                    SetCreateStatus(
                        GetFirebaseErrorMessage(
                            task.Exception
                        )
                    );

                    return;
                }

                FirebaseUser user =
                    task.Result.User;

                if (user == null)
                {
                    SetCreateStatus(
                        "Account creation failed."
                    );

                    return;
                }

                SaveAccountPlayerName(user);
                CompleteAuthentication(user);
            });
    }

    public void PlayAsGuest()
    {
        if (!CanStartRequest(loginStatusText))
        {
            return;
        }

        requestRunning = true;

        SetBothStatusTexts(
            "Starting guest session..."
        );

        /*
         * Always sign out first.
         *
         * This clears any locally cached account, including
         * an anonymous account that was deleted manually
         * from the Firebase Console.
         */
        if (auth.CurrentUser != null)
        {
            auth.SignOut();

            Debug.Log(
                "Cleared the cached Firebase user."
            );
        }

        auth
            .SignInAnonymouslyAsync()
            .ContinueWithOnMainThread(task =>
            {
                requestRunning = false;

                if (task.IsCanceled)
                {
                    SetBothStatusTexts(
                        "Guest login was cancelled."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Firebase guest login failed: " +
                        task.Exception
                    );

                    SetBothStatusTexts(
                        GetFirebaseErrorMessage(
                            task.Exception
                        )
                    );

                    return;
                }

                FirebaseUser user =
                    task.Result.User;

                if (user == null)
                {
                    SetBothStatusTexts(
                        "Guest login failed."
                    );

                    return;
                }

                SaveGuestPlayerName(user);
                CompleteAuthentication(user);
            });
    }

    private bool CanStartRequest(
        TMP_Text targetStatusText
    )
    {
        if (requestRunning)
        {
            SetStatusText(
                targetStatusText,
                "Please wait..."
            );

            return false;
        }

        if (!firebaseReady || auth == null)
        {
            SetStatusText(
                targetStatusText,
                "Firebase is still connecting."
            );

            return false;
        }

        return true;
    }

    private void CompleteAuthentication(
        FirebaseUser user
    )
    {
        requestRunning = false;

        ClearPasswordInputs();
        ClearStatusTexts();

        Debug.Log(
            "Firebase authentication successful. " +
            "User ID: " + user.UserId +
            ", Anonymous: " + user.IsAnonymous
        );

        if (loginPanelController != null)
        {
            loginPanelController
                .CompleteAuthentication();
        }
        else
        {
            Debug.LogError(
                "LoginPanelController is not assigned."
            );
        }
    }

    private void SaveAccountPlayerName(
        FirebaseUser user
    )
    {
        SavePlayerName(
            CreateGeneratedName(
                user,
                "Player"
            )
        );
    }

    private void SaveGuestPlayerName(
        FirebaseUser user
    )
    {
        SavePlayerName(
            CreateGeneratedName(
                user,
                "Guest"
            )
        );
    }

    private string CreateGeneratedName(
        FirebaseUser user,
        string prefix
    )
    {
        string userId =
            user != null
                ? user.UserId
                : string.Empty;

        int identifierLength =
            Mathf.Min(6, userId.Length);

        string identifier =
            identifierLength > 0
                ? userId.Substring(
                    0,
                    identifierLength
                )
                : "Player";

        return prefix + "-" + identifier;
    }

    private void SavePlayerName(
        string playerName
    )
    {
        PlayerPrefs.SetString(
            PlayerNameKey,
            playerName
        );

        PlayerPrefs.Save();
    }

    private void ClearPasswordInputs()
    {
        if (loginPasswordInput != null)
        {
            loginPasswordInput.text =
                string.Empty;
        }

        if (createPasswordInput != null)
        {
            createPasswordInput.text =
                string.Empty;
        }
    }

    private void SetLoginStatus(
        string message
    )
    {
        SetStatusText(
            loginStatusText,
            message
        );
    }

    private void SetCreateStatus(
        string message
    )
    {
        SetStatusText(
            createStatusText,
            message
        );
    }

    private void SetBothStatusTexts(
        string message
    )
    {
        SetLoginStatus(message);
        SetCreateStatus(message);
    }

    private void ClearStatusTexts()
    {
        SetBothStatusTexts(string.Empty);
    }

    private void SetStatusText(
        TMP_Text targetText,
        string message
    )
    {
        if (targetText != null)
        {
            targetText.text = message;
        }
    }

    private string GetFirebaseErrorMessage(
        Exception exception
    )
    {
        FirebaseException firebaseException =
            FindFirebaseException(exception);

        if (firebaseException == null)
        {
            return
                "Authentication failed. Please try again.";
        }

        AuthError authError =
            (AuthError)firebaseException.ErrorCode;

        switch (authError)
        {
            case AuthError.InvalidEmail:
            case AuthError.MissingEmail:
                return
                    "Enter a valid email address.";

            case AuthError.MissingPassword:
                return
                    "Enter your password.";

            case AuthError.WrongPassword:
            case AuthError.InvalidCredential:
            case AuthError.UserNotFound:
                return
                    "Incorrect email or password.";

            case AuthError.EmailAlreadyInUse:
                return
                    "An account already uses this email.";

            case AuthError.WeakPassword:
                return
                    "The password is too weak.";

            case AuthError.UserDisabled:
                return
                    "This account has been disabled.";

            case AuthError.NetworkRequestFailed:
                return
                    "Check your internet connection.";

            case AuthError.TooManyRequests:
                return
                    "Too many attempts. Try again later.";

            case AuthError.OperationNotAllowed:
                return
                    "This login method is not enabled.";

            default:
                return
                    "Authentication failed: " +
                    authError;
        }
    }

    private FirebaseException FindFirebaseException(
        Exception exception
    )
    {
        if (exception == null)
        {
            return null;
        }

        if (exception is FirebaseException
            firebaseException)
        {
            return firebaseException;
        }

        if (exception is AggregateException
            aggregateException)
        {
            AggregateException flattenedException =
                aggregateException.Flatten();

            foreach (
                Exception innerException
                in flattenedException.InnerExceptions
            )
            {
                FirebaseException result =
                    FindFirebaseException(
                        innerException
                    );

                if (result != null)
                {
                    return result;
                }
            }
        }

        return FindFirebaseException(
            exception.InnerException
        );
    }
}