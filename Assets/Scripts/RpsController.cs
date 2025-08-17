using System.Collections;
using UnityEngine;

public class RpsController : Singleton<RpsController>
{
    [SerializeField] private RpsView view;
    [SerializeField] private int WinRounds;
    private GameDataModel model;
    private System.Random rng;
    private bool IsRoundStarted = false;
    [SerializeField] float CountFadeDuration = 0.3f;    

    private void Start()
    {
        GameStart();
        Round_Start();
    }

    private void GameStart()
    {
        model = new GameDataModel(WinRounds);
        rng = new System.Random();
        view.UpdateUI(0, 0);
    }

    private void Round_Start()
    {

        StartCoroutine(CountDown());    

        IEnumerator CountDown()
        {
            for(int i = 3;i > 0; i--)
            {
                view.Show_Message(i.ToString(),true,CountFadeDuration);
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


    public void OnPlayerChoice(int choiceIndex,RectTransform item)
    {
        if(!IsRoundStarted)
        {
            Debug.LogWarning("Round has not started yet!");
            return;
        }


        Choice player = (Choice)choiceIndex;
        Choice bot = (Choice)rng.Next(0, 3);

        RoundResult result = model.PlayRound(player, bot);

        string resultMsg = result switch
        {
            RoundResult.PlayerWin => "Player Wins",
            RoundResult.BotWin => "Bot Wins",
            _ => "Draw"
        };

        view.Select_Player_Item(item, CalculateScores);

        view.Show_Opponent_choice(bot);
        
        
        void CalculateScores()
        {
            view.UpdateUI(model.PlayerScore, model.BotScore);      
            
        }

       


        if (model.IsMatchOver())
        {
            string finalMsg = model.PlayerScore > model.BotScore ? "You Win!" : "Bot Wins!";
            view.ShowMatchOver(finalMsg);
        }
    }
}
