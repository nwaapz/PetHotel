using UnityEngine;
using UnityEngine.UI;

public class Item_Color : MonoBehaviour
{
    [SerializeField] Image image;

    public void SetColor(Color color)
    {
        image.color = color;    
    }
}
