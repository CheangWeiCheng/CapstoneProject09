using System;
using Firebase.Auth;
using TMPro;
using UnityEngine;

public class FirebasePlayerIdDisplay : MonoBehaviour
{
    [Header("Player ID UI")]
    [SerializeField] private TMP_Text playerIdText;

    private FirebaseAuth auth;
    private bool authListenerAdded;

    private void Awake()
    {
        TryConnectToFirebaseAuth();
    }

    private void Start()
    {
        RefreshPlayerId();
    }

    private void OnEnable()
    {
        TryConnectToFirebaseAuth();
        RefreshPlayerId();
    }

    private void OnDestroy()
    {
        RemoveAuthListener();
    }

    private void TryConnectToFirebaseAuth()
    {
        if (auth != null)
        {
            AddAuthListener();
            return;
        }

        try
        {
            auth = FirebaseAuth.DefaultInstance;

            AddAuthListener();
            RefreshPlayerId();

            Debug.Log(
                "PLAYER ID DISPLAY: Connected to Firebase Auth."
            );
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "PLAYER ID DISPLAY: Firebase Auth is not ready yet. " +
                exception.Message
            );

            SetPlayerIdText("Loading...");
        }
    }

    private void AddAuthListener()
    {
        if (auth == null)
        {
            return;
        }

        if (authListenerAdded)
        {
            return;
        }

        auth.StateChanged += OnAuthStateChanged;
        authListenerAdded = true;
    }

    private void RemoveAuthListener()
    {
        if (auth == null)
        {
            return;
        }

        if (!authListenerAdded)
        {
            return;
        }

        auth.StateChanged -= OnAuthStateChanged;
        authListenerAdded = false;
    }

    private void OnAuthStateChanged(
        object sender,
        EventArgs eventArgs
    )
    {
        RefreshPlayerId();
    }

    public void RefreshPlayerId()
    {
        if (playerIdText == null)
        {
            Debug.LogWarning(
                "PLAYER ID DISPLAY: Player ID Text is not assigned."
            );

            return;
        }

        if (auth == null)
        {
            TryConnectToFirebaseAuth();

            if (auth == null)
            {
                SetPlayerIdText("Loading...");
                return;
            }
        }

        FirebaseUser user = auth.CurrentUser;

        if (user == null)
        {
            SetPlayerIdText("Not signed in");
            return;
        }

        string firebaseUid = user.UserId;

        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            SetPlayerIdText("Unavailable");
            return;
        }

        string shortId = CreateShortId(firebaseUid);

        string displayId;

        if (user.IsAnonymous)
        {
            displayId = "Guest-" + shortId;
        }
        else
        {
            displayId = "Player-" + shortId;
        }

        SetPlayerIdText(displayId);

        Debug.Log(
            "PLAYER ID DISPLAY: " +
            displayId +
            " | Firebase UID = " +
            firebaseUid
        );
    }

    private string CreateShortId(string firebaseUid)
    {
        string cleanedUid =
            firebaseUid.Trim().ToUpperInvariant();

        if (cleanedUid.Length <= 6)
        {
            return cleanedUid;
        }

        return cleanedUid.Substring(0, 6);
    }

    private void SetPlayerIdText(string value)
    {
        if (playerIdText == null)
        {
            return;
        }

        playerIdText.text = value;
    }
}