using UnityEngine;

public class RSP_Pickable : MonoBehaviour
{
    public Choice choice;

    public void Select()
    {
        RpsController.Instance.OnPlayerChoice((int)choice,GetComponent<RectTransform>()); 
    }
}
