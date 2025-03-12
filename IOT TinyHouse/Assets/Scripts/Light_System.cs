using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_System : MonoBehaviour
{
    public List<Light> lightSources = new List<Light>(); // List to hold multiple lights
    public Material material;
    public KeyCode tempKey = KeyCode.E;
    void Update()
    {
        if (Input.GetKeyDown(tempKey))
        {
            ToggleLights();
        }
    }

    void ToggleLights()
    {
        bool anyLightOn = false;
        foreach (Light light in lightSources)
        {
            if (light != null)
            {
                light.enabled = !light.enabled;
                if (light.enabled) anyLightOn = true;
            }
           
        }
        if (anyLightOn)
        {
            
            material.EnableKeyword("_EMISSION");
        }
        else
        {
            material.DisableKeyword("_EMISSION");

        }
    }
}