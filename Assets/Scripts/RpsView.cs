using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles all UI and animation for Rock-Paper-Scissors.
/// 
/// Responsibilities:
/// - Updates score texts.
/// - Shows/hides panels, messages, player cards, opponent choice.
/// - Animates UI (cards, text, scores).
/// - Subscribes to <see cref="RpsController.GameEnd"/> event to show match results.
/// - Provides buttons for round flow and quitting.
/// 
/// This script contains no game logic — it only reflects the
/// state of the game visually (View in MVC).
/// </summary>
public class RpsView : MonoBehaviour
{
    [Header("Score UI")]
    public TMP_Text PlayerScoreText;   // Player score text
    public TMP_Text BotScoreText;      // Bot score text

    [Space(6)]
    [Header("Panels")]
    public GameObject MatchOverPanel;  // End-game panel
    public GameObject MessageBox;      // Temporary message box
    public GameObject ResultBoard;     // Result display board

    [Space(6)]
    [Header("Text Elements")]
    public TMP_Text matchOverText;     // Text on end panel
    public TMP_Text MessageText;       // Countdown or temporary messages
    public TMP_Text ResultBoardText;   // Round result text

    [Space(6)]
    [Header("Player & Opponent Items")]
    [SerializeField] RectTransform Player_Item_1;
    [SerializeField] RectTransform Player_Item_2;
    [SerializeField] RectTransform Player_Item_3;
    [SerializeField] RectTransform Opponent_Item;

    [Space(6)]
    [Header("Sprites & Images")]
    [SerializeField] Sprite _Rock;
    [SerializeField] Sprite _Paper;
    [SerializeField] Sprite _Scissors;
    [SerializeField] Image _OpponentImage;

    [Space(6)]
    [Header("Buttons")]
    [SerializeField] Button RoundFinishBtn;
    [SerializeField] Button ExitBtn;

    // Runtime data
    private float cartAnimationDuration;
    private float Player_Item_Hide_Height, Player_Item_Show_Height, Player_Item_Select_Height;
    private float Opponent_Item_Hide_Height, Opponent_Item_Show_Height;

    // Cached colors for cards
    private Item_Color opponentItemColor, player1Color, player2Color, player3Color;

    // Round control
    Action ConfirmAction;       // Callback to resume round
    Coroutine AutoNextRound;    // Auto next-round coroutine

    private void Awake()
    {
        // Cache Item_Color components for later coloring
        opponentItemColor = Opponent_Item.GetComponent<Item_Color>();
        player1Color = Player_Item_1.GetComponent<Item_Color>();
        player2Color = Player_Item_2.GetComponent<Item_Color>();
        player3Color = Player_Item_3.GetComponent<Item_Color>();
    }

    private void OnDisable()
    {
        // Unsubscribe from controller event
        if (RpsController.Instance != null)
            RpsController.Instance.GameEnd -= ShowFinalBoard;
    }

    private void OnEnable()
    {
        // Subscribe to controller event
        if (RpsController.Instance != null)
            RpsController.Instance.GameEnd += ShowFinalBoard;
    }

    private void Start()
    {
        ExitBtn.onClick.AddListener(() => { Application.Quit(); });

        // Cache card positions
        Player_Item_Show_Height = Player_Item_1.anchoredPosition.y;
        Opponent_Item_Show_Height = Opponent_Item.anchoredPosition.y;

        Player_Item_Hide_Height = Player_Item_Show_Height - 100f;
        Opponent_Item_Hide_Height = Opponent_Item_Show_Height + 230f;
        Player_Item_Select_Height = Player_Item_Show_Height + 65f;

        // Initialize UI
        Hide_Player_Items();
        Hide_Opponent_choice();

        RoundFinishBtn.gameObject.SetActive(false);
        RoundFinishBtn.onClick.AddListener(() => RoundFinishClicked());
    }

    /// <summary>
    /// Updates the scoreboard UI.
    /// </summary>
    public void UpdateUI(int playerScore, int botScore)
    {
        PlayerScoreText.text = playerScore.ToString();
        BotScoreText.text = botScore.ToString();
    }

    /// <summary>
    /// Shows round result board.
    /// </summary>
    public void ShowResult(string result)
    {
        ResultBoard.SetActive(true);
        RoundFinishBtn.gameObject.SetActive(true);
        RoundFinishBtn.interactable = true;
        ResultBoardText.text = result;
    }

    /// <summary>
    /// Called when round finish button is clicked.
    /// - Resets player items and opponent choice.
    /// - Hides round panels.
    /// - Invokes callback to continue gameplay.
    /// </summary>
    void RoundFinishClicked()
    {
        Show_Player_Items();
        Hide_Opponent_choice();
        MatchOverPanel.SetActive(false);
        ResultBoard.SetActive(false);
        ConfirmAction?.Invoke();
        ExitBtn.gameObject.SetActive(false);

        RpsController.Instance.RoundResult = RoundResult.playing;

        if (AutoNextRound != null)
        {
            StopCoroutine(AutoNextRound);
            AutoNextRound = null;
        }
    }

    /// <summary>
    /// Shows opponent's chosen card with animation.
    /// </summary>
    public void Show_Opponent_choice(Choice choice)
    {
        Sprite opponentSprite = choice switch
        {
            Choice.Rock => _Rock,
            Choice.Paper => _Paper,
            Choice.Scissors => _Scissors,
            _ => null
        };

        if (opponentSprite != null)
        {
            _OpponentImage.sprite = opponentSprite;
            Opponent_Item.DOAnchorPosY(Opponent_Item_Show_Height, cartAnimationDuration)
                .SetEase(Ease.InOutSine)
                .OnComplete(() => Debug.Log("Opponent choice shown: " + choice));
        }
        else
        {
            Debug.LogError("Invalid choice for opponent sprite.");
        }
    }

    /// <summary>
    /// Hides opponent card with animation and resets its color.
    /// </summary>
    public void Hide_Opponent_choice()
    {
        Opponent_Item.DOAnchorPosY(Opponent_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        SFX_Player.Instance.Play_whosh();
        opponentItemColor.SetColor(RpsController.Instance.ReadyPlay);
    }

    /// <summary>
    /// Handles player card selection:
    /// - Plays animation.
    /// - Shows result after 1 sec.
    /// - Auto continues to next round (if not game over).
    /// </summary>
    public void Select_Player_Item(RectTransform item, Action callbackAfterSelection,
        string result, float animationDuration, Action callbackAfterRoundConfirm, bool GameEnded)
    {
        SFX_Player.Instance.Play_whosh();

        RoundFinishBtn.GetComponentInChildren<TMP_Text>().text = "Next Round";
        cartAnimationDuration = animationDuration;
        ConfirmAction = callbackAfterRoundConfirm;

        item.DOAnchorPosY(Player_Item_Select_Height, cartAnimationDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => callbackAfterSelection?.Invoke());

        StartCoroutine(showResCo());

        if (!GameEnded)
            AutoNextRound = StartCoroutine(RpsController.Instance.callbackDelayed(RoundFinishClicked, 5));

        IEnumerator showResCo()
        {
            yield return new WaitForSeconds(1);
            ShowResult(result);
        }
    }

    /// <summary>
    /// Hides player cards (off-screen).
    /// </summary>
    public void Hide_Player_Items()
    {
        Player_Item_1.DOAnchorPosY(Player_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_2.DOAnchorPosY(Player_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_3.DOAnchorPosY(Player_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// Resets player cards to visible positions & ready color.
    /// </summary>
    public void Show_Player_Items()
    {
        Player_Item_1.DOAnchorPosY(Player_Item_Show_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_2.DOAnchorPosY(Player_Item_Show_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_3.DOAnchorPosY(Player_Item_Show_Height, cartAnimationDuration).SetEase(Ease.InOutSine);

        player1Color.SetColor(RpsController.Instance.ReadyPlay);
        player2Color.SetColor(RpsController.Instance.ReadyPlay);
        player3Color.SetColor(RpsController.Instance.ReadyPlay);
    }

    /// <summary>
    /// Shows countdown or temporary message.
    /// Supports fading text in/out.
    /// </summary>
    public void Show_Message(string msg, bool fade = false, float fadeDuration = 0)
    {
        MessageBox.SetActive(true);
        MessageText.gameObject.SetActive(true);
        MessageText.text = msg;

        MessageText.DOFade(1, 0).From(0f).SetEase(Ease.InOutSine);

        if (fade)
        {
            MessageText.DOFade(0, fadeDuration).From(1f).SetEase(Ease.InOutSine)
                .OnComplete(() => MessageText.gameObject.SetActive(false));
        }
    }

    public void Hide_Message()
    {
        MessageText.gameObject.SetActive(false);
        MessageBox.SetActive(false);
    }

    public void AnimatePlayerScore()
    {
        PlayerScoreText.transform.DOScale(Vector3.one * 2, 0.6f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void AnimateOpponentScore()
    {
        BotScoreText.transform.DOScale(Vector3.one * 2, 0.6f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void SetPlayerCardColor(Item_Color item_Color, Color color)
    {
        item_Color.SetColor(color);
    }

    public void SetOpponentCardColor(Color color)
    {
        Opponent_Item.GetComponent<Item_Color>().SetColor(color);
    }

    public void SetResultBoardColor(Color color)
    {
        ResultBoard.GetComponent<Item_Color>().SetColor(color);
    }

    /// <summary>
    /// Shows the final match-over board (via GameEnd event).
    /// </summary>
    void ShowFinalBoard(string text)
    {
        MatchOverPanel.gameObject.SetActive(true);
        matchOverText.text = text;
        ExitBtn.gameObject.SetActive(true);

        RoundFinishBtn.GetComponentInChildren<TMP_Text>().text = "Play Again";

        if (RpsController.Instance.GameResult == GameResult.PlayerWon)
            MatchOverPanel.GetComponent<Image>().color = RpsController.Instance.winner;
        else
            MatchOverPanel.GetComponent<Image>().color = RpsController.Instance.loser;
    }
}
