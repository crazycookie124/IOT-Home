using UnityEngine;

public class LightSwitchController : MonoBehaviour
{
    [SerializeField] private Light[] lightsToToggle; // Assign bathroom lights in Inspector
    [SerializeField] private GameObject[] lightCovers; // Assign light covers in Inspector (the objects with the LightShader)
    [SerializeField] private AudioSource fanAudioSource; // Assign the fan AudioSource in Inspector
    [SerializeField] private Transform playerTransform; // Reference to the player's transform
    [SerializeField] private float maxDistance = 10f; // Max distance for full volume

    private bool isLightOn = false;

    public void ToggleLights()
    {
        isLightOn = !isLightOn;

        foreach (Light light in lightsToToggle)
        {
            if (light != null)
            {
                light.enabled = isLightOn; // Turns the light ON/OFF
            }
        }

        // Toggle emission on the LightCover objects when the lights are toggled
        foreach (GameObject cover in lightCovers)
        {
            if (cover != null)
            {
                Renderer coverRenderer = cover.GetComponent<Renderer>();
                if (coverRenderer != null)
                {
                    Material coverMaterial = coverRenderer.material;

                    if (coverMaterial != null)
                    {
                        if (isLightOn)
                        {
                            coverMaterial.EnableKeyword("_EMISSION");  // Enable emission
                            Color emissionColor = Color.white * 3.4f;  // Multiply color by intensity
                            coverMaterial.SetColor("_EmissionColor", emissionColor);  // Set emission color
                            
                            // Play fan sound when lights are turned on
                            if (fanAudioSource != null && !fanAudioSource.isPlaying)
                            {
                                fanAudioSource.Play();
                            }
                        }
                        else
                        {
                            coverMaterial.DisableKeyword("_EMISSION");  // Disable emission
                            
                            // Stop fan sound when lights are turned off
                            if (fanAudioSource != null && fanAudioSource.isPlaying)
                            {
                                fanAudioSource.Stop();
                            }
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        // Only adjust fan volume if the light is on
        if (isLightOn && fanAudioSource != null && playerTransform != null)
        {
            // Calculate the distance between the player and the light switch
            float distance = Vector3.Distance(playerTransform.position, transform.position);

            // Clamp the volume based on the distance (volume decreases with distance)
            float volume = Mathf.Clamp01(1 - (distance / maxDistance)); // Volume decreases from 1 to 0 as distance increases
            fanAudioSource.volume = volume;
        }
    }
}