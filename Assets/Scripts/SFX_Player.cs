using UnityEngine;

public class SFX_Player : Singleton<SFX_Player>
{
    [SerializeField] AudioClip PlayerScored_AudioClip,
        BotScored_AudioClip, Draw_AudioClip, Win_AudioClip, Lose_AudioClip,Whoosh_AudioClip,initBeep,initBeepFinal;
    [SerializeField] AudioSource Player;
    void Start()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void Play_Player_Score()
    {
        Player.PlayOneShot(PlayerScored_AudioClip); 
    }

    public void Play_Bot_Score()
    {
        Player.PlayOneShot(BotScored_AudioClip);    
    }

    public void Play_Draw()
    {
        Player.PlayOneShot(Draw_AudioClip); 
    }

    public void Play_Player_Win()
    {
        Player.PlayOneShot(Win_AudioClip);   
    }

    public void Play_Player_Lose()
    {
        Player.PlayOneShot(Lose_AudioClip); 
    }

    public void Play_whosh()
    {
        Player.PlayOneShot(Whoosh_AudioClip);   
    }

    public void PlayInitBeep()
    {
        Player.PlayOneShot(initBeep);
    }


    public void PlayInitBeepFinal()
    {
        Player.PlayOneShot(initBeepFinal);
    }
}
