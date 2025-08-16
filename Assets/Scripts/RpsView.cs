using TMPro;
using UnityEngine;

public class RpsView : MonoBehaviour
{
    public TMP_Text playerChoiceText, botChoiceText, resultText, scoreText;
    public GameObject matchOverPanel;
    public TMP_Text matchOverText;

    public void UpdateUI(string playerChoice, string botChoice, string result, int playerScore, int botScore)
    {
        playerChoiceText.text = "Player: " + playerChoice;
        botChoiceText.text = "Bot: " + botChoice;
        resultText.text = "Result: " + result;
        scoreText.text = $"Score: {playerScore} - {botScore}";
    }

    public void ShowMatchOver(string msg)
    {
        matchOverText.text = msg;
        matchOverPanel.SetActive(true);
    }
}
