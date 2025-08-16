using UnityEngine;

public class GameDataModel
{
    public int PlayerScore { get; private set; }
    public int BotScore { get; private set; }
    public int PointsToWin { get; private set; }

    public GameDataModel(int pointsToWin = 2)
    {
        PointsToWin = pointsToWin;
    }

    public RoundResult PlayRound(Choice player, Choice bot)
    {
        if (player == bot) return RoundResult.Draw;

        if ((player == Choice.Rock && bot == Choice.Scissors) ||
            (player == Choice.Scissors && bot == Choice.Paper) ||
            (player == Choice.Paper && bot == Choice.Rock))
        {
            PlayerScore++;
            return RoundResult.PlayerWin;
        }
        BotScore++;
        return RoundResult.BotWin;
    }

    public bool IsMatchOver() =>
        PlayerScore >= PointsToWin || BotScore >= PointsToWin;
}

public enum Choice 
{
    Rock, Paper, Scissors 
}
public enum RoundResult
{
    Draw,PlayerWin,BotWin
}

