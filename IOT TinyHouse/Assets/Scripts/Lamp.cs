using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lamp : MonoBehaviour


{
    public Light lightsource;
 
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (lightsource != null)
            {
                lightsource.enabled = !lightsource.enabled;
            }

        }
    }
}
