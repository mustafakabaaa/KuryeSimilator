using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour,Iinterectable
{
    public SCItem item;
    public SCInventory inventory;
   
    public void Interact()
    {
        if (inventory.AddItem(this.gameObject.GetComponent<Item>().item)) 
        {
            Destroy(this.gameObject);
            
            
        }
        
    }
}
