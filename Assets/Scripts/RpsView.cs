using DG.Tweening;
using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RpsView : MonoBehaviour
{
    public TMP_Text PlayerScoreText, BotScoreText;
    public GameObject MatchOverPanel,MessageBox,ResultBoard;
    public TMP_Text matchOverText,MessageText,ResultBoardText;
    [SerializeField] RectTransform Player_Item_1, Player_Item_2, Player_Item_3, Opponent_Item;
    [SerializeField] Sprite _Rock, _Paper, _Scissors;
    [SerializeField] Image _OpponentImage;
    [SerializeField] Button RoundFinishBtn,ExitBtn;
    private float cartAnimationDuration;
    //caching height of the items for animation
    private float Player_Item_Hide_Height, Player_Item_Show_Height, Player_Item_Select_Height,
        Opponent_Item_Hide_Height, Opponent_Item_Show_Height;

    Action ConfirmAction;
   

    private void OnDisable()
    {
        RpsController.Instance.GameEnd -= FinalMessage; 
    }


    private void Start()
    {
        RpsController.Instance.GameEnd += FinalMessage;

        Player_Item_Show_Height = Player_Item_1.anchoredPosition.y;
        Opponent_Item_Show_Height = Opponent_Item.anchoredPosition.y;   


        Player_Item_Hide_Height = Player_Item_Show_Height - 100f; 
        Opponent_Item_Hide_Height = Opponent_Item_Show_Height + 230f; 

        Player_Item_Select_Height = Player_Item_Show_Height + 65f; 

        Hide_Player_Items();
        Hide_Opponent_choice();

        RoundFinishBtn.gameObject.SetActive(false); 
        RoundFinishBtn.onClick.AddListener(() =>
        {
            RoundFinishClicked();
        });
    }

    public void ShowResult(string result)
    {
        ResultBoard.SetActive(true);    
        RoundFinishBtn.gameObject.SetActive(true);
        RoundFinishBtn.interactable = true;
        
        ResultBoardText.text = result;

    }

    void RoundFinishClicked()
    {
        
        Show_Player_Items();
        Hide_Opponent_choice();
        MatchOverPanel.SetActive(false);
        ResultBoard.SetActive(false);   
        ConfirmAction?.Invoke();
        ExitBtn.gameObject.SetActive(false);
    }


    public void UpdateUI(int playerScore, int botScore)
    {
       
        PlayerScoreText.text = playerScore.ToString();
        BotScoreText.text = botScore.ToString();    
    }

    public void ShowMatchOver(string msg)
    {
        matchOverText.text = msg;
        MatchOverPanel.SetActive(true);
    }

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
            Opponent_Item.DOAnchorPosY(Opponent_Item_Show_Height, cartAnimationDuration).SetEase(Ease.InOutSine)
                .OnComplete(() => UnityEngine.Debug.Log("Opponent choice shown: " + choice));
        }
        else
        {
            UnityEngine.Debug.LogError("Invalid choice for opponent sprite.");
        }
    }

    public void Hide_Opponent_choice()
    {
        Opponent_Item.DOAnchorPosY(Opponent_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        SFX_Player.Instance.Play_whosh();
        Opponent_Item.GetComponent<Item_Color>().SetColor(RpsController.Instance.ReadyPlay);

    }

    public void Select_Player_Item(RectTransform item,Action callbackAfterSelection,
        string result,float animationDuration,Action callbackAfterRoundConfirm)
    {

        SFX_Player.Instance.Play_whosh();

        RoundFinishBtn.GetComponentInChildren<TMP_Text>().text = "Next Round";
        cartAnimationDuration = animationDuration;
        ConfirmAction = callbackAfterRoundConfirm;

        item.DOAnchorPosY(Player_Item_Select_Height, cartAnimationDuration).SetEase(Ease.InOutSine)
         .OnComplete(() => callbackAfterSelection?.Invoke());


        StartCoroutine(showResCo());

        IEnumerator showResCo()
        {
            yield return new WaitForSeconds(1);
            ShowResult(result);
        }   

    }

    public void Hide_Player_Items()
    {
        Player_Item_1.DOAnchorPosY(Player_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_2.DOAnchorPosY(Player_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_3.DOAnchorPosY(Player_Item_Hide_Height, cartAnimationDuration).SetEase(Ease.InOutSine);

       
    }

    public void Show_Player_Items()
    {
        Player_Item_1.DOAnchorPosY(Player_Item_Show_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_2.DOAnchorPosY(Player_Item_Show_Height, cartAnimationDuration).SetEase(Ease.InOutSine);
        Player_Item_3.DOAnchorPosY(Player_Item_Show_Height, cartAnimationDuration).SetEase(Ease.InOutSine);

        Player_Item_1.GetComponent<Item_Color>().SetColor(RpsController.Instance.ReadyPlay);
        Player_Item_2.GetComponent<Item_Color>().SetColor(RpsController.Instance.ReadyPlay);
        Player_Item_3.GetComponent<Item_Color>().SetColor(RpsController.Instance.ReadyPlay);
    }

    public void Show_Message(string msg,bool fade = false,float fadeDuration = 0)
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

    public void AnimatePlayerScore()
    {
        PlayerScoreText.transform.DOScale(Vector3.one*2,0.6f)
            .SetLoops(2,LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void AnimateOpponentScore()
    {
        BotScoreText.transform.DOScale(Vector3.one * 2, 0.6f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    public void SetPlayerCardColor(Item_Color item_Color,Color color)
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


    public void Hide_Message()
    {
        MessageText.gameObject.SetActive(false);
        MessageBox.SetActive(false);    
    }
    void FinalMessage(string text)
    {
        MatchOverPanel.gameObject.SetActive(true);
        matchOverText.text = text;  
        ExitBtn.gameObject.SetActive(true);
        ExitBtn.onClick.AddListener(() => {Application.Quit(); });
        RoundFinishBtn.GetComponentInChildren<TMP_Text>().text = "Play Again";
    }


    }

