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
    public Color winner, loser, draw, ReadyPlay;


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
            for(int i = 5;i > 0; i--)
            {
                view.Show_Message(i.ToString(),true,CountFadeDuration);
                
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

        view.Select_Player_Item(item, UpdateScoresInView,resultMsg,CartAnimationDuration,endGameConfirm);

        view.Show_Opponent_choice(bot);
        
        
        void UpdateScoresInView()
        {

            StartCoroutine(callbackDelayed(updateAndSfxAfterCartSelection, 0.5f));

            void updateAndSfxAfterCartSelection()
            {
                view.UpdateUI(model.PlayerScore, model.BotScore);
                if (result == RoundResult.PlayerWin)
                {
                    SFX_Player.Instance.Play_Player_Score();
                    view.AnimatePlayerScore();
                    view.SetPlayerCardColor(item.GetComponent<Item_Color>(), winner);
                    view.SetResultBoardColor(winner);
                    view.SetOpponentCardColor(loser);
                }
                else if (result == RoundResult.BotWin)
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

            StartCoroutine(callbackDelayed(finalResults, CartAnimationDuration + 0.5f));

            void finalResults()
            {
                string finalMsg = null;
                Action finalAction =  model.PlayerScore > model.BotScore ? playerWonSetup : PlayerLostSetup;
                finalAction?.Invoke();

                void playerWonSetup()
                {
                    finalMsg = "You Won";
                    SFX_Player.Instance.Play_Player_Win();
                }

                void PlayerLostSetup()
                {
                    finalMsg = "You Lost";
                    SFX_Player.Instance.Play_Player_Lose();
                }

                GameEnd?.Invoke(finalMsg);
            }                        
        }
    }

    IEnumerator callbackDelayed(Action action,float delay)
    {
        yield return new WaitForSeconds(delay);
        print("can play now");
        action?.Invoke();   
    }

}
