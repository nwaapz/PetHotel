using UnityEngine;

public class RpsController : MonoBehaviour
{
    [SerializeField] private RpsView view;
    [SerializeField] private int WinRounds;
    private GameDataModel model;
    private System.Random rng;

    private void Start()
    {
        GameStart();
    }

    private void GameStart()
    {
        model = new GameDataModel(WinRounds);
        rng = new System.Random();
        view.UpdateUI("—", "—", "—", 0, 0);
    }

    public void OnPlayerChoice(int choiceIndex)
    {
        Choice player = (Choice)choiceIndex;
        Choice bot = (Choice)rng.Next(0, 3);

        RoundResult result = model.PlayRound(player, bot);

        string resultMsg = result switch
        {
            RoundResult.PlayerWin => "Player Wins",
            RoundResult.BotWin => "Bot Wins",
            _ => "Draw"
        };

        view.UpdateUI(player.ToString(), bot.ToString(), resultMsg, model.PlayerScore, model.BotScore);

        if (model.IsMatchOver())
        {
            string finalMsg = model.PlayerScore > model.BotScore ? "You Win!" : "Bot Wins!";
            view.ShowMatchOver(finalMsg);
        }
    }
}
