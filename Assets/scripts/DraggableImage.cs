using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragonNation : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    [HideInInspector] public Transform parentAfterDrag;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin drag");
        parentAfterDrag = transform.parent; // Mevcut ebeveyni sakla
        transform.SetParent(transform.root); // Nesneyi kök hiyerarþiye taþý
        transform.SetAsLastSibling(); // En üstte görünmesini saðla
        image.raycastTarget = false; // Raycast'i devre dýþý býrak
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        transform.position = Input.mousePosition; // Fare pozisyonunu takip et
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End drag");
        transform.SetParent(parentAfterDrag); // Ebeveyni eski haline getir
        image.raycastTarget = true; // Raycast'i yeniden etkinleþtir
    }
}