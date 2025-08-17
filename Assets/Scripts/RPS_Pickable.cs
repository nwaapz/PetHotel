using UnityEngine;

public class RPS_Pickable : MonoBehaviour
{
    public Choice choice;

    public void Select()
    {
        RpsController.Instance.OnPlayerChoice((int)choice,GetComponent<RectTransform>()); 
    }
}
