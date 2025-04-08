using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneOnEnableSC : MonoBehaviour
{
    OrderUI OrderUI =new OrderUI();
    private void OnEnable()
    {
        OrderUI.OnOrderListUpdated();
    }
   
}
