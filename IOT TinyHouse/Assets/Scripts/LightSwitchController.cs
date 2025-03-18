using UnityEngine;
using System.Collections;

public class LightSwitchController : MonoBehaviour
{
    [SerializeField] private Light[] lightsToToggle;
    [SerializeField] private GameObject[] lightCovers;
    [SerializeField] private AudioSource fanAudioSource;
    [SerializeField] private AudioSource clickAudioSource;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private Collider bathroomTriggerZone;
    [SerializeField] private Collider frontHouseTriggerZone;
    [SerializeField] private GameObject fan;

    public float fanRotationSpeed = 200f;
    public bool isLightOn = false;
    private bool isPlayerInBathroom = false;
    private bool isPlayerInFrontHouse = false;
    private float frontHouseIdleTime = 0f;
    private const float idleTimeout = 5f;
    private bool isPlayerMoving = false;
    private bool isFrontHouseLightOn = false;

    private void Start()
    {
        ResetControlledLights();  // Turn off all switch-controlled lights at the start
    }

    private void Update()
    {
        if (bathroomTriggerZone != null)
        {
            isPlayerInBathroom = bathroomTriggerZone.bounds.Contains(playerTransform.position);
        }

        if (isLightOn && fanAudioSource != null && playerTransform != null && isPlayerInBathroom)
        {
            RotateFan();
            float distance = Vector3.Distance(playerTransform.position, fanAudioSource.transform.position);
            float volume = Mathf.Clamp01(1 - (distance / maxDistance));
            fanAudioSource.volume = Mathf.Min(volume, 0.2f);
        }
        else
        {
            if (fanAudioSource != null && fanAudioSource.isPlaying)
            {
                fanAudioSource.volume = 0f;
            }
        }

        if (frontHouseTriggerZone != null)
        {
            isPlayerInFrontHouse = frontHouseTriggerZone.bounds.Contains(playerTransform.position);

            if (isPlayerInFrontHouse)
            {
                float horizInput = Input.GetAxis("Horizontal");
                float vertInput = Input.GetAxis("Vertical");
                isPlayerMoving = Mathf.Abs(horizInput) > 0.1f || Mathf.Abs(vertInput) > 0.1f;

                if (isPlayerMoving && !isLightOn)
                {
                    ToggleLights();
                    isFrontHouseLightOn = true;
                }

                if (isPlayerMoving)
                {
                    frontHouseIdleTime = 0f;
                }
                else
                {
                    frontHouseIdleTime += Time.deltaTime;
                }

                if (frontHouseIdleTime >= idleTimeout && isLightOn)
                {
                    ToggleLights();
                    isFrontHouseLightOn = false;
                }
            }
        } 
    }
    private void RotateFan()
    {
        if (fan != null)
        {
            fan.transform.Rotate(Vector3.forward * fanRotationSpeed * Time.deltaTime);
        }
    }
        public void ToggleLights()
    {
        if (clickAudioSource != null && !clickAudioSource.isPlaying)
        {
            clickAudioSource.Play();
        }

        isLightOn = !isLightOn;

        foreach (Light light in lightsToToggle)
        {
            if (light != null)
            {
                light.enabled = isLightOn;
            }
        }

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
                            coverMaterial.EnableKeyword("_EMISSION");
                            Color emissionColor = Color.white * 3.4f;
                            coverMaterial.SetColor("_EmissionColor", emissionColor);

                            if (fanAudioSource != null && !fanAudioSource.isPlaying)
                            {
                                StartCoroutine(PlayFanSoundAfterDelay(0.5f));
                            }
                        }
                        else
                        {
                            coverMaterial.DisableKeyword("_EMISSION");

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

    private IEnumerator PlayFanSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (fanAudioSource != null && !fanAudioSource.isPlaying)
        {
            fanAudioSource.Play();
        }
    }

    private void ResetControlledLights()
    {
        LightSwitchController[] lightControllers = FindObjectsOfType<LightSwitchController>();

        foreach (LightSwitchController controller in lightControllers)
        {
            controller.SetLightState(false);
        }
    }

    public void SetLightState(bool state)
    {
        isLightOn = state;
        foreach (Light light in lightsToToggle)
        {
            if (light != null)
            {
                light.enabled = state;
            }
        }

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
                        if (state)
                        {
                            coverMaterial.EnableKeyword("_EMISSION");
                            Color emissionColor = Color.white * 3.4f;
                            coverMaterial.SetColor("_EmissionColor", emissionColor);
                        }
                        else
                        {
                            coverMaterial.DisableKeyword("_EMISSION");
                        }
                    }
                }
            }
        }
    }
}