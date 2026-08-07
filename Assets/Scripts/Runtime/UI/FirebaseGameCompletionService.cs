using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseGameCompletionService : MonoBehaviour
{
    private const string PlayerNameKey =
        "ARMAMATION_PlayerName";

    private const string LeaderboardCollectionName =
        "gameCompletionLeaderboard";

    private const string BestTimeFieldName =
        "bestTimeMs";

    private static FirebaseGameCompletionService instance;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    private bool firebaseReady;
    private bool uploadRunning;

    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitialiseFirebase();
    }

    private void InitialiseFirebase()
    {
        firebaseReady = false;

        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError(
                        "Firestore dependency check was cancelled."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Firestore dependency check failed: " +
                        task.Exception
                    );

                    return;
                }

                DependencyStatus dependencyStatus =
                    task.Result;

                if (dependencyStatus !=
                    DependencyStatus.Available)
                {
                    Debug.LogError(
                        "Firestore dependencies unavailable: " +
                        dependencyStatus
                    );

                    return;
                }

                auth =
                    FirebaseAuth.DefaultInstance;

                firestore =
                    FirebaseFirestore.DefaultInstance;

                firebaseReady = true;

                Debug.Log(
                    "Firebase Firestore is ready."
                );
            });
    }

    public void SaveBestCompletionTime(
        int completionTimeMilliseconds,
        string formattedTime
    )
    {
        if (completionTimeMilliseconds <= 0)
        {
            Debug.LogError(
                "The completion time must be greater than zero."
            );

            return;
        }

        if (!firebaseReady ||
            auth == null ||
            firestore == null)
        {
            Debug.LogError(
                "Firestore is not ready. " +
                "The completion time was not uploaded."
            );

            return;
        }

        if (uploadRunning)
        {
            Debug.LogWarning(
                "A completion-time upload is already running."
            );

            return;
        }

        FirebaseUser currentUser =
            auth.CurrentUser;

        if (currentUser == null)
        {
            Debug.LogError(
                "No Firebase user is signed in. " +
                "The completion time was not uploaded."
            );

            return;
        }

        string playerName =
            GetPlayerName(currentUser);

        long submittedTimeMilliseconds =
            completionTimeMilliseconds;

        Dictionary<string, object> leaderboardEntry =
            new Dictionary<string, object>
            {
                {
                    "userId",
                    currentUser.UserId
                },
                {
                    "playerName",
                    playerName
                },
                {
                    "bestTimeMs",
                    submittedTimeMilliseconds
                },
                {
                    "formattedTime",
                    formattedTime
                },
                {
                    "isGuest",
                    currentUser.IsAnonymous
                },
                {
                    "updatedAt",
                    FieldValue.ServerTimestamp
                }
            };

        DocumentReference playerDocument =
            firestore
                .Collection(
                    LeaderboardCollectionName
                )
                .Document(
                    currentUser.UserId
                );

        uploadRunning = true;

        firestore
            .RunTransactionAsync(transaction =>
            {
                return transaction
                    .GetSnapshotAsync(
                        playerDocument
                    )
                    .ContinueWith(snapshotTask =>
                    {
                        DocumentSnapshot snapshot =
                            snapshotTask.Result;

                        bool shouldSaveNewTime =
                            true;

                        if (snapshot.Exists &&
                            snapshot.TryGetValue<long>(
                                BestTimeFieldName,
                                out long existingBestTime
                            ))
                        {
                            shouldSaveNewTime =
                                submittedTimeMilliseconds
                                < existingBestTime;
                        }

                        if (shouldSaveNewTime)
                        {
                            transaction.Set(
                                playerDocument,
                                leaderboardEntry
                            );
                        }

                        return shouldSaveNewTime;
                    });
            })
            .ContinueWithOnMainThread(task =>
            {
                uploadRunning = false;

                if (task.IsCanceled)
                {
                    Debug.LogError(
                        "Completion-time upload was cancelled."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Completion-time upload failed: " +
                        task.Exception
                    );

                    return;
                }

                bool savedNewBestTime =
                    task.Result;

                if (savedNewBestTime)
                {
                    Debug.Log(
                        "New Firebase best time saved: " +
                        formattedTime
                    );
                }
                else
                {
                    Debug.Log(
                        "The existing Firebase best time " +
                        "is faster. No update was made."
                    );
                }
            });
    }

    private string GetPlayerName(
        FirebaseUser currentUser
    )
    {
        string savedPlayerName =
            PlayerPrefs.GetString(
                PlayerNameKey,
                string.Empty
            ).Trim();

        if (!string.IsNullOrEmpty(
            savedPlayerName
        ))
        {
            return savedPlayerName;
        }

        string prefix =
            currentUser.IsAnonymous
                ? "Guest"
                : "Player";

        string userId =
            currentUser.UserId;

        int identifierLength =
            Mathf.Min(
                6,
                userId.Length
            );

        string identifier =
            identifierLength > 0
                ? userId.Substring(
                    0,
                    identifierLength
                )
                : "User";

        return prefix + "-" + identifier;
    }
}