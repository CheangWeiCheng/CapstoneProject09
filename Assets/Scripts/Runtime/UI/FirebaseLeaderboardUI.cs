using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;

public class FirebaseLeaderboardUI : MonoBehaviour
{
    private const string LeaderboardCollection =
        "gameCompletionLeaderboard";

    [Header("Rank 1")]
    [SerializeField] private TMP_Text rank1Text;
    [SerializeField] private TMP_Text player1Text;
    [SerializeField] private TMP_Text time1Text;

    [Header("Rank 2")]
    [SerializeField] private TMP_Text rank2Text;
    [SerializeField] private TMP_Text player2Text;
    [SerializeField] private TMP_Text time2Text;

    [Header("Rank 3")]
    [SerializeField] private TMP_Text rank3Text;
    [SerializeField] private TMP_Text player3Text;
    [SerializeField] private TMP_Text time3Text;

    [Header("Rank 4")]
    [SerializeField] private TMP_Text rank4Text;
    [SerializeField] private TMP_Text player4Text;
    [SerializeField] private TMP_Text time4Text;

    [Header("Rank 5")]
    [SerializeField] private TMP_Text rank5Text;
    [SerializeField] private TMP_Text player5Text;
    [SerializeField] private TMP_Text time5Text;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    private bool isLoading;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
    }

    private void OnEnable()
    {
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        if (isLoading)
        {
            return;
        }

        if (!AllReferencesAssigned())
        {
            Debug.LogError(
                "FirebaseLeaderboardUI: One or more leaderboard " +
                "text references have not been assigned."
            );

            return;
        }

        ClearLeaderboard();

        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        if (firestore == null)
        {
            firestore = FirebaseFirestore.DefaultInstance;
        }

        if (auth.CurrentUser == null)
        {
            Debug.LogWarning(
                "Leaderboard cannot load because no Firebase " +
                "user is currently signed in."
            );

            return;
        }

        isLoading = true;

        firestore
            .Collection(LeaderboardCollection)
            .OrderBy("bestTimeMs")
            .Limit(5)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                isLoading = false;

                if (task.IsCanceled)
                {
                    Debug.LogError(
                        "Leaderboard loading was cancelled."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Leaderboard failed to load: " +
                        task.Exception
                    );

                    return;
                }

                QuerySnapshot snapshot = task.Result;

                int index = 0;

                foreach (
                    DocumentSnapshot document
                    in snapshot.Documents
                )
                {
                    if (index >= 5)
                    {
                        break;
                    }

                    string playerName = "UNKNOWN";
                    string formattedTime = "--:--.---";

                    if (
                        document.TryGetValue<string>(
                            "playerName",
                            out string storedPlayerName
                        )
                    )
                    {
                        playerName = storedPlayerName;
                    }

                    if (
                        document.TryGetValue<string>(
                            "formattedTime",
                            out string storedTime
                        )
                    )
                    {
                        formattedTime = storedTime;
                    }

                    SetRow(
                        index,
                        playerName,
                        formattedTime
                    );

                    index++;
                }

                Debug.Log(
                    "Firebase leaderboard loaded. Players displayed: " +
                    index
                );
            });
    }

    private bool AllReferencesAssigned()
    {
        return
            rank1Text != null &&
            player1Text != null &&
            time1Text != null &&

            rank2Text != null &&
            player2Text != null &&
            time2Text != null &&

            rank3Text != null &&
            player3Text != null &&
            time3Text != null &&

            rank4Text != null &&
            player4Text != null &&
            time4Text != null &&

            rank5Text != null &&
            player5Text != null &&
            time5Text != null;
    }

    private void SetRow(
        int index,
        string playerName,
        string formattedTime
    )
    {
        switch (index)
        {
            case 0:
                rank1Text.text = "1";
                player1Text.text = playerName;
                time1Text.text = formattedTime;
                break;

            case 1:
                rank2Text.text = "2";
                player2Text.text = playerName;
                time2Text.text = formattedTime;
                break;

            case 2:
                rank3Text.text = "3";
                player3Text.text = playerName;
                time3Text.text = formattedTime;
                break;

            case 3:
                rank4Text.text = "4";
                player4Text.text = playerName;
                time4Text.text = formattedTime;
                break;

            case 4:
                rank5Text.text = "5";
                player5Text.text = playerName;
                time5Text.text = formattedTime;
                break;
        }
    }

    private void ClearLeaderboard()
    {
        rank1Text.text = "1";
        player1Text.text = "---";
        time1Text.text = "--:--.---";

        rank2Text.text = "2";
        player2Text.text = "---";
        time2Text.text = "--:--.---";

        rank3Text.text = "3";
        player3Text.text = "---";
        time3Text.text = "--:--.---";

        rank4Text.text = "4";
        player4Text.text = "---";
        time4Text.text = "--:--.---";

        rank5Text.text = "5";
        player5Text.text = "---";
        time5Text.text = "--:--.---";
    }
}