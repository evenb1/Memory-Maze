using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        Debug.Log("Button hovered!");
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        Debug.Log("Button unhovered!");
    }
}