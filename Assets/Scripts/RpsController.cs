using System;
using System.Collections;
using UnityEngine;

public class RpsController : Singleton<RpsController>
{
    [SerializeField] private RpsView view;
    [SerializeField] private int WinRounds;
    private GameDataModel model;
    private System.Random rng;
    private bool IsRoundStarted = false;
    [SerializeField] float CountFadeDuration = 0.3f,CartAnimationDuration = 0.3f;    
    public event Action<string> GameEnd;


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

        IsRoundStarted = false; 

        Choice player = (Choice)choiceIndex;
        Choice bot = (Choice)rng.Next(0, 3);

        RoundResult result = model.PlayRound(player, bot);

        string resultMsg = result switch
        {
            RoundResult.PlayerWin => "You Scored",
            RoundResult.BotWin => "Bot Scored",
            _ => "Draw"
        };

        view.Select_Player_Item(item, CalculateScores,resultMsg,CartAnimationDuration,endGameConfirm);

        view.Show_Opponent_choice(bot);
        
        
        void CalculateScores()
        {
            view.UpdateUI(model.PlayerScore, model.BotScore);      
            
        }

        void endGameConfirm()
        {
            if(model.IsMatchOver())
            {
                GameStart();
                Round_Start();
            }
            else
            {
                StartCoroutine(callbackDelayed(() => { IsRoundStarted = true; }, CartAnimationDuration));
            }
            

        }
       


        if (model.IsMatchOver())
        {
            
            string finalMsg = model.PlayerScore > model.BotScore ? "You Won!" : "You Lost!";
            GameEnd?.Invoke(finalMsg);
            
        }
    }

    IEnumerator callbackDelayed(Action action,float delay)
    {
        yield return new WaitForSeconds(delay);
        print("can play now");
        action?.Invoke();   
    }

}
