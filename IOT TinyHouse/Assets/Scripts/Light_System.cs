using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_System : MonoBehaviour
{
    private int light_amount = 1;
    [SerializeField] private List<GameObject> lightObjects = new List<GameObject>(); // Store GameObjects
    // Start is called before the first frame update
    void Start()
    {
        // Resize the list based on light_amount
        while (lightObjects.Count < light_amount)
        {
            lightObjects.Add(null);
        }
        while (lightObjects.Count > light_amount)
        {
            lightObjects.RemoveAt(lightObjects.Count - 1);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
