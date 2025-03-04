using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoorController : MonoBehaviour,Iinterectable
{
    [SerializeField] private Transform door;

    private bool isOpen = true;
    private float doorZValue;

    public void Interact()
    {
        ToogleDoor();
    }

    private void ToogleDoor()
    {
        isOpen = !isOpen; 
        doorZValue=isOpen ? 0 : 3; // 0'dan 3'e gidecek 
        door.DOMoveZ(doorZValue, 1f); // Z ekseninde nereye gideceði ve ne kadar süreceðini belirtiyoruz
    }
}
