using UnityEngine;
using System.Collections;

public class LightSwitchController : MonoBehaviour
{
    [SerializeField] private Light[] lightsToToggle; // Assign bathroom lights in Inspector
    [SerializeField] private GameObject[] lightCovers; // Assign light covers in Inspector (the objects with the LightShader)
    [SerializeField] private AudioSource fanAudioSource; // Assign the fan AudioSource in Inspector (already attached to the exhaust fan)
    [SerializeField] private AudioSource clickAudioSource; // Assign the click sound AudioSource in Inspector
    [SerializeField] private Transform playerTransform; // Reference to the player's transform
    [SerializeField] private float maxDistance = 10f; // Max distance for full volume
    [SerializeField] private Collider bathroomTriggerZone; // Reference to the bathroom trigger zone

    private bool isLightOn = false;
    private bool isPlayerInBathroom = false; // Flag to track if player is in the bathroom

    public void ToggleLights()
    {
        // Play click sound when the switch is clicked
        if (clickAudioSource != null && !clickAudioSource.isPlaying)
        {
            clickAudioSource.Play();
        }

        // Switch the light state
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
                            
                            // Play fan sound after a short delay (simulating the fan turning on after the click sound)
                            if (fanAudioSource != null && !fanAudioSource.isPlaying)
                            {
                                // Play fan sound after click sound
                                StartCoroutine(PlayFanSoundAfterDelay(0.5f)); // Add delay if you want
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

    // Coroutine to play the fan sound after a short delay
    private IEnumerator PlayFanSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay before playing the fan sound
        if (fanAudioSource != null && !fanAudioSource.isPlaying)
        {
            fanAudioSource.Play();
        }
    }

    private void Update()
    {
        // Check if the player is inside the bathroom area
        if (bathroomTriggerZone != null)
        {
            isPlayerInBathroom = bathroomTriggerZone.bounds.Contains(playerTransform.position);
        }

        // Only adjust fan volume if the light is on and the player is inside the bathroom
        if (isLightOn && fanAudioSource != null && playerTransform != null && isPlayerInBathroom)
        {
            // Calculate the distance between the player and the exhaust fan's position (instead of a new fan position)
            float distance = Vector3.Distance(playerTransform.position, fanAudioSource.transform.position);

            // Clamp the volume based on the distance (volume decreases with distance)
            float volume = Mathf.Clamp01(1 - (distance / maxDistance)); // Volume decreases from 1 to 0 as distance increases
            fanAudioSource.volume = Mathf.Min(volume, 0.2f);
        }
        else
        {
            // Mute the fan sound if the player is outside the bathroom area
            if (fanAudioSource != null && fanAudioSource.isPlaying)
            {
                fanAudioSource.volume = 0f; // Set to 0 when outside the bathroom area
            }
        }
    }
}