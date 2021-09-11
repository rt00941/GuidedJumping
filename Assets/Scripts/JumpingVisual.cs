using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingVisual : MonoBehaviour
{

    public BezierCurve bezier;
    private bool teleportEnabled;

    public OVRInput.Button teleportAction;

    private GameObject controllerObject;
    private GameObject platformObject;
    private GameObject rotateObject;
    private GameObject teleportIndicator;

    private bool bezierCheck;

    // For visuals

    private GameObject[] nodes;
    private GameObject currentNode;
    private GameObject[] ghostAvatars;
    private GameObject[] arrows;
    private GameObject label;
    private GameObject lhand;
    private GameObject rhand;

    // Start is called before the first frame update
    void Start()
    {
        teleportIndicator = GameObject.Find("TeleIndicator");
        teleportIndicator.GetComponentInChildren<MeshRenderer>().enabled = false;
        teleportEnabled = false;
        controllerObject = GameObject.Find("RightHandAnchor");
        platformObject = GameObject.Find("TrackingSpace");
        rotateObject = GameObject.Find("CenterEyeAnchor");
        teleportIndicator.transform.SetParent(transform);
        lhand = GameObject.Find("LeftControllerAnchor").transform.GetChild(0).GetChild(2).gameObject;
        lhand.SetActive(true);
        rhand = GameObject.Find("RightControllerAnchor").transform.GetChild(0).GetChild(3).gameObject;
        rhand.SetActive(true);
        ghostAvatars = GameObject.FindGameObjectsWithTag("GhostAvatar");
        arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject a in arrows)
        {
            a.SetActive(false);
        }
        label = GameObject.Find("Pause Label"); label.GetComponentInChildren<TMPro.TextMeshPro>().text = "Paused";
        label.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(teleportAction))
        {
                teleportEnabled = true;
        }
        if (teleportEnabled)
        {
            HandleTeleport();
        }
        if (OVRInput.GetUp(teleportAction))
        {
            teleportEnabled = false;
        }
        bezierCheck = bezier.endPointDetected;
        ToggleTeleportMode(teleportEnabled);
    }

    void HandleTeleport()
    {
        if (bezierCheck)
        {
            if (teleportIndicator.GetComponentInChildren<MeshRenderer>().enabled == false)
            {
                teleportIndicator.GetComponentInChildren<MeshRenderer>().enabled = true;
            }
            else
            {
                teleportIndicator.transform.position = bezier.EndPoint; //Sets teleport indicator position
                Vector3 IndicatorRotation = new Vector3(0, controllerObject.transform.eulerAngles.y - controllerObject.transform.eulerAngles.z, 0); //Adding controller Z rotation to teleport indicator  
                teleportIndicator.transform.rotation = Quaternion.Euler(IndicatorRotation); //Set teleport indicator rotation mapped to controller rotation
            }
            if (OVRInput.GetUp(teleportAction))
            {
                float ControllerRotation = controllerObject.transform.localEulerAngles.z;
                float HeadRotation = rotateObject.transform.localEulerAngles.y;

                if (ControllerRotation > 180)
                {
                    ControllerRotation = ControllerRotation - 360;
                }
                Vector3 groundPosition = new Vector3(rotateObject.transform.position.x, platformObject.transform.position.y, rotateObject.transform.position.z);

                Vector3 translateVector = bezier.EndPoint - groundPosition;

                platformObject.transform.position += translateVector;

                platformObject.transform.position = new Vector3(platformObject.transform.position.x, 0.8f, platformObject.transform.position.z);

                platformObject.transform.RotateAround(rotateObject.transform.position, Vector3.up, -ControllerRotation); //Sets the User rotaion as the indicator rotation
            }
        }
        else
        {
            teleportIndicator.GetComponentInChildren<MeshRenderer>().enabled = false; ;//set teleport indicator object inactive
        }
    }

    void ToggleTeleportMode(bool check)
    {
        bezier.GetDrawLine(check);
        if (!teleportEnabled)
        {
            teleportIndicator.GetComponentInChildren<MeshRenderer>().enabled = false; //set teleport indicator object inactive
        }
    }
}
