using UnityEngine;
using System.Collections;

public class MoveObjectController : MonoBehaviour
{
    public float reachRange = 1.8f;  // Range to interact with objects

    private Animator anim;
    private Camera fpsCam;
    private GameObject player;

    private const string animBoolName = "isOpen_Obj_";

    private bool playerEntered;
    private bool showInteractMsg;
    private GUIStyle guiStyle;
    private string msg;

    private int rayLayerMask;  // Mask for raycasting to only interact with specific layers

    void Start()
    {
        // Initialize player and camera references
        player = GameObject.FindGameObjectWithTag("Player");

        fpsCam = Camera.main;
        if (fpsCam == null)  // Ensure there is a camera tagged 'MainCamera'
        {
            Debug.LogError("A camera tagged 'MainCamera' is missing.");
        }

        // Get the Animator component attached to the object
        anim = GetComponent<Animator>();
        anim.enabled = false;  // Disable animation by default  

        // Set up layer mask for raycasting
        LayerMask iRayLM = LayerMask.NameToLayer("InteractRaycast");
        rayLayerMask = 1 << iRayLM.value;

        // Set up GUI style for interaction messages
        setupGui();
    }

    void OnTriggerEnter(Collider other)
    {		
        if (other.gameObject == player)  // Check if the player has entered the trigger zone
        {			
            playerEntered = true;
        }
    }

    void OnTriggerExit(Collider other)
    {		
        if (other.gameObject == player)  // Check if the player has exited the trigger zone
        {			
            playerEntered = false;
            showInteractMsg = false;  // Hide the interaction message when the player exits
        }
    }

    void Update()
    {
        if (playerEntered)
        {	
            // Get the center point of the viewport in world space
            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            // Raycast to check if player is looking at interactable objects
            if (Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, reachRange, rayLayerMask))
            {
                // First, check if the hit object is a light switch
                LightSwitchController switchController = hit.collider.GetComponentInParent<LightSwitchController>();
                if (switchController != null)
                {
                    showInteractMsg = true;
                    msg = "Press E to Toggle Lights";

                    // Handle player interaction with light switch
                    if (Input.GetKeyUp(KeyCode.E) || Input.GetButtonDown("Fire1"))
                    {
                        switchController.ToggleLights();  // Toggle the lights on/off
                    }
                    return;  // Exit early to avoid checking for MoveableObject when it's a light switch
                }

                // Now check for MoveableObject if it's not a light switch
                MoveableObject moveableObject = null;

                // Check if the object the player is looking at is the same as the current one
                if (!isEqualToParent(hit.collider, out moveableObject))
                {
                    return;  // Exit if it's not the correct object
                }

                if (moveableObject != null)  // Make sure the hit object has the MoveableObject script attached
                {
                    showInteractMsg = true;
                    string animBoolNameNum = animBoolName + moveableObject.objectNumber.ToString();

                    bool isOpen = anim.GetBool(animBoolNameNum);  // Get current state for message
                    msg = getGuiMsg(isOpen);

                    // Handle player interaction with animated object
                    if (Input.GetKeyUp(KeyCode.E) || Input.GetButtonDown("Fire1"))
                    {
                        anim.enabled = true;
                        anim.SetBool(animBoolNameNum, !isOpen);  // Toggle object animation
                        msg = getGuiMsg(!isOpen);  // Update message
                    }
                }
            }
            else
            {
                showInteractMsg = false;  // Hide the interaction message if no object is hit
            }
        }
    }

    // Check if the current gameObject is the same as the hit object's gameObject or its parent
    private bool isEqualToParent(Collider other, out MoveableObject draw)
    {
        draw = null;
        bool rtnVal = false;
        try
        {
            int maxWalk = 6;
            draw = other.GetComponent<MoveableObject>();

            GameObject currentGO = other.gameObject;
            for (int i = 0; i < maxWalk; i++)
            {
                if (currentGO.Equals(this.gameObject))
                {
                    rtnVal = true;
                    if (draw == null) draw = currentGO.GetComponentInParent<MoveableObject>();
                    break;
                }

                // If not equal, move to the parent
                if (currentGO.transform.parent != null)
                {
                    currentGO = currentGO.transform.parent.gameObject;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }

        return rtnVal;
    }

    #region GUI Config

    // Set up the GUI style for interaction messages
    private void setupGui()
    {
        guiStyle = new GUIStyle();
        guiStyle.fontSize = 16;
        guiStyle.fontStyle = FontStyle.Bold;
        guiStyle.normal.textColor = Color.white;
        msg = "Press E/Fire1 to Open";
    }

    // Return the appropriate message based on the state (open/close)
    private string getGuiMsg(bool isOpen)
    {
        return isOpen ? "Press E/Fire1 to Close" : "Press E/Fire1 to Open";
    }

    // Display the GUI message on screen
    void OnGUI()
    {
        if (showInteractMsg)
        {
            GUI.Label(new Rect(50, Screen.height - 50, 200, 50), msg, guiStyle);
        }
    }

    #endregion
}