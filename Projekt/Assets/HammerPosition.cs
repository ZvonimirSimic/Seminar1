using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.transform.localRotation *= Quaternion.Euler(-15,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
