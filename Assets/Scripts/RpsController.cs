using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Main gameplay controller for Rock-Paper-Scissors.
/// 
/// Responsibilities:
/// - Creates and manages the <see cref="GameDataModel"/>.
/// - Coordinates round flow (countdown, inputs, results).
/// - Sends updates to <see cref="RpsView"/> (UI, colors, animations).
/// - Plays sound effects via <see cref="SFX_Player"/>.
/// - Raises <see cref="GameEnd"/> event when the match is over.
/// 
/// This acts as the **Controller** in an MVC-like structure,
/// bridging the Model (GameDataModel) with the View (RpsView).
/// </summary>
public class RpsController : Singleton<RpsController>
{
    [Header("References")]
    [SerializeField] private RpsView view; // UI manager

    [Space(6)]
    [Header("Gameplay Settings")]
    [SerializeField] private int WinRounds;                 // How many rounds required to win
    [SerializeField] private float CountFadeDuration = 0.3f;// Time between countdown numbers
    [SerializeField] private float CartAnimationDuration = 0.3f;// Time for card animation

    [Space(6)]
    [Header("Runtime State")]
    private GameDataModel model;   // Game data & rules
    private System.Random rng;     // Bot's RNG choice
    private bool IsRoundStarted;   // Prevents multiple inputs

    /// <summary>
    /// Event fired when the match ends (with final message string).
    /// </summary>
    public event Action<string> GameEnd;

    [Space(6)]
    [Header("Colors")]
    public Color winner;   // Card/result color if won
    public Color loser;    // Card/result color if lost
    public Color draw;     // Card/result color if draw
    public Color ReadyPlay;// Color used when ready

    [Space(6)]
    [Header("Results")]
    public RoundResult RoundResult; // Result of last round
    public GameResult GameResult;   // Result of overall game

    private void Start()
    {
        GameStart();
        Round_Start();
    }

    /// <summary>
    /// Initializes a new game session:
    /// - Creates a new <see cref="GameDataModel"/>.
    /// - Resets scores to 0.
    /// - Updates UI.
    /// </summary>
    private void GameStart()
    {
        model = new GameDataModel(WinRounds);
        rng = new System.Random();
        view.UpdateUI(0, 0);
        GameResult = GameResult.Playing;
    }

    /// <summary>
    /// Begins a new round:
    /// - Sets RoundResult to "playing".
    /// - Starts a countdown coroutine before showing player items.
    /// </summary>
    private void Round_Start()
    {
        RoundResult = RoundResult.playing;
        StartCoroutine(CountDown());

        IEnumerator CountDown()
        {
            for (int i = 5; i > 0; i--)
            {
                view.Show_Message(i.ToString(), true, CountFadeDuration);

                if (i == 1) SFX_Player.Instance.PlayInitBeepFinal();
                else SFX_Player.Instance.PlayInitBeep();

                yield return new WaitForSeconds(CountFadeDuration);
            }

            view.Hide_Message();
            startRound();
        }

        void startRound()
        {
            IsRoundStarted = true;
            view.Show_Player_Items();
        }
    }

    /// <summary>
    /// Called when the player makes a choice.
    /// - Validates round state.
    /// - Calculates result via <see cref="GameDataModel"/>.
    /// - Updates UI (scores, colors, opponent card).
    /// - Checks for game end condition.
    /// </summary>
    /// <param name="choice">Player's selected choice.</param>
    /// <param name="item">UI element of the selected card.</param>
    public void OnPlayerChoice(Choice choice, RectTransform item)
    {
        // Prevent early/invalid input
        if (!IsRoundStarted)
        {
            Debug.LogWarning("Round has not started yet!");
            return;
        }

        IsRoundStarted = false;

        // Player vs Bot choices
        Choice player = choice;
        Choice bot = (Choice)rng.Next(0, 3);

        // Calculate winner via data model
        RoundResult = model.PlayRound(player, bot);

        string resultMsg = RoundResult switch
        {
            RoundResult.PlayerWin => "You Scored",
            RoundResult.BotWin => "Bot Scored",
            _ => "Draw"
        };

        // Animate player's card + update scores in view
        view.Select_Player_Item(item, UpdateScoresInView, resultMsg, CartAnimationDuration,
                                endGameConfirm, model.IsMatchOver());

        // Show bot’s chosen card
        view.Show_Opponent_choice(bot);

        // Local helper: Update UI + play sounds based on result
        void UpdateScoresInView()
        {
            StartCoroutine(callbackDelayed(updateAndSfxAfterCartSelection, 0.5f));

            
            void updateAndSfxAfterCartSelection()
            {
                view.UpdateUI(model.PlayerScore, model.BotScore);

                if (RoundResult == RoundResult.PlayerWin)
                {
                    SFX_Player.Instance.Play_Player_Score();
                    view.AnimatePlayerScore();
                    view.SetPlayerCardColor(item.GetComponent<Item_Color>(), winner);
                    view.SetResultBoardColor(winner);
                    view.SetOpponentCardColor(loser);
                }
                else if (RoundResult == RoundResult.BotWin)
                {
                    SFX_Player.Instance.Play_Bot_Score();
                    view.AnimateOpponentScore();
                    view.SetPlayerCardColor(item.GetComponent<Item_Color>(), loser);
                    view.SetResultBoardColor(loser);
                    view.SetOpponentCardColor(winner);
                }
                else
                {
                    SFX_Player.Instance.Play_Draw();
                    view.SetPlayerCardColor(item.GetComponent<Item_Color>(), draw);
                    view.SetResultBoardColor(draw);
                    view.SetOpponentCardColor(draw);
                }
            }
        }

        // Local helper: handle end-of-round or new round setup -
        // being sent as an action to view.Select_Player_Item()
        void endGameConfirm()
        {
            if (model.IsMatchOver())
            {
                GameStart();
                Round_Start();
            }
            else
            {
                StartCoroutine(callbackDelayed(() => { IsRoundStarted = true; }, CartAnimationDuration));
            }
        }

        // If match is over → trigger final results
        if (model.IsMatchOver())
        {
            StartCoroutine(callbackDelayed(finalResults, CartAnimationDuration + 0.5f));

            void finalResults()
            {
                string finalMsg = null;
                Action finalAction = model.PlayerScore > model.BotScore ? playerWonSetup : playerLostSetup;
                finalAction?.Invoke();

                void playerWonSetup()
                {
                    finalMsg = "You Won";
                    SFX_Player.Instance.Play_Player_Win();
                    GameResult = GameResult.PlayerWon;
                }

                void playerLostSetup()
                {
                    finalMsg = "You Lost";
                    SFX_Player.Instance.Play_Player_Lose();
                    GameResult = GameResult.BotWon;
                }
                
                GameEnd?.Invoke(finalMsg);
            }
        }
    }
    
    /// <summary>
    /// Utility coroutine: invoke an action after a delay.
    /// </summary>
    public IEnumerator callbackDelayed(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        print("can play now");
        action?.Invoke();
    }
}
