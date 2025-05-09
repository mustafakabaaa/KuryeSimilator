using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralElectronicDevice : MonoBehaviour, Iinterectable
{
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public bool CanInteract()
    {
        return true;
    }

    public string GetInteractionText()
    {
        return "Cihazý açmak için Y tuþuna basýn."; 
    }

    public void Interact()
    {
        throw new System.NotImplementedException();
    }
}
