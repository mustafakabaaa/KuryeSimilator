using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class scenereset : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneView.lastActiveSceneView.rotation = Quaternion.identity;
        SceneView.lastActiveSceneView.pivot = Vector3.zero;
    }

    
}
