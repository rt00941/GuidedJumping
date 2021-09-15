using System;
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
    private int currentIndex;
    private GameObject currentNode;
    private GameObject prevnode;
    private Transform currentWaypoints;
    private GameObject[] ghostAvatars;
    private GameObject[] arrows;
    private GameObject label;
    private GameObject rhand;
    private Material choiceMat;
    private Material selectedMat;
    private Dictionary<int, Dictionary<int, GameObject>> ordered = new Dictionary<int, Dictionary<int, GameObject>>();
    private int chosenNode;
    public bool choice;
    public bool pause;
    public bool chosen;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
        teleportIndicator = GameObject.Find("TeleIndicator");
        teleportIndicator.GetComponentInChildren<MeshRenderer>().enabled = false;
        teleportEnabled = false;
        controllerObject = GameObject.Find("RightHandAnchor");
        platformObject = GameObject.Find("Platform");
        rotateObject = GameObject.Find("CenterEyeAnchor");
        teleportIndicator.transform.SetParent(platformObject.transform);
        rhand = GameObject.Find("RightControllerAnchor").transform.GetChild(0).GetChild(3).gameObject;
        rhand.SetActive(true);
        ghostAvatars = GameObject.FindGameObjectsWithTag("GhostAvatar");
        arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject a in arrows)
        {
            a.SetActive(false);
        }
        label = GameObject.Find("Pause Label"); 
        label.GetComponentInChildren<TMPro.TextMeshPro>().text = "Paused";
        label.SetActive(false);
        choiceMat = Resources.Load<Material>("Materials/HighlightedGhostAvatarMaterial");
        selectedMat = Resources.Load<Material>("Materials/GhostAvatarMaterial");
        nodes = GameObject.FindGameObjectsWithTag("Node");
        ResetPreview(selectedMat);
        orderNodes();
        chosenNode = 0;
        currentIndex = 0;
        currentNode = ordered[0][chosenNode];
        prevnode = currentNode;
        currentWaypoints = currentNode.GetComponent<Node>().thisnode.waypoints.transform;
        gameObject.transform.position = currentNode.transform.position;
        gameObject.transform.rotation = currentNode.transform.rotation;
        choice = false;
        pause = false;
        chosen = false;
    }

    // Update is called once per frame
    void Update()
    {
        rhand.SetActive(true);
        timer += Time.deltaTime;
        if (!pause)
        {
            if (!choice)
            {
                currentNode = ordered[currentIndex][chosenNode]; 
                currentWaypoints = currentNode.GetComponent<Node>().thisnode.waypoints.transform;
                UpdateIndex();
                SetPreview(chosenNode, currentNode.transform, selectedMat, currentWaypoints);
            }
            else
            {
                StartCoroutine(Choose());
            }
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
            for (int i = 0; i < arrows.Length; i++)
            {
                if (arrows[i].activeSelf)
                {
                    arrows[i].transform.LookAt(ghostAvatars[i].transform);
                    arrows[i].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, arrows[i].transform.localEulerAngles.y, 0);
                    arrows[i].transform.GetChild(1).transform.LookAt(gameObject.transform);
                    Vector3 curRot = arrows[i].transform.GetChild(1).transform.localEulerAngles;
                    arrows[i].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, curRot.y, 0);
                }
            }
            bezierCheck = bezier.endPointDetected;
            ToggleTeleportMode(teleportEnabled); 
        }
        else
        {
            ResetPreview(selectedMat);
            label.SetActive(true);
            label.GetComponentInChildren<TMPro.TextMeshPro>().text = "END OF TASK";
            GetComponent<Logging>().AddData("END OF TASK");
            GetComponent<Logging>().AddData("Time Taken: " + timer.ToString());
        }
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
                
                GetComponent<Logging>().AddData(timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
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
    private void orderNodes()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            NodeProperties n = nodes[i].GetComponent<Node>().thisnode;
            if (ordered.ContainsKey(n.index))
            {
                ordered[n.index].Add(n.optionNumber, nodes[i]);
            }
            else
            {
                ordered.Add(n.index, new Dictionary<int, GameObject>());
                ordered[n.index].Add(n.optionNumber, nodes[i]);
            }
        }
    }
    private void SetPreview(int index, Transform current, Material mat, Transform waypoints)
    {
        ghostAvatars[index].transform.parent = current;
        ghostAvatars[index].transform.localPosition = new Vector3(0, -0.7f, 0);
        ghostAvatars[index].transform.localEulerAngles = new Vector3(0, 0, 0);
        ghostAvatars[index].GetComponent<MeshRenderer>().enabled = true;
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().positionCount = waypoints.childCount + 2;
        Vector3 linepos = new Vector3(prevnode.transform.position.x, 0.5f, prevnode.transform.position.z); 
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(0, linepos);
        for (int i = 0; i < waypoints.childCount; i++)
        {
            linepos = new Vector3(waypoints.GetChild(i).position.x, 0.5f, waypoints.GetChild(i).position.z);
            ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(i + 1, linepos);
        }
        linepos = new Vector3(ghostAvatars[index].transform.position.x, 0.5f, ghostAvatars[index].transform.position.z);
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(waypoints.childCount + 1, linepos);
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().enabled = true;
        ghostAvatars[index].transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
        ghostAvatars[index].GetComponent<MeshRenderer>().material = mat;
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().material = mat;
    }
    private void ResetPreview(Material mat)
    {
        foreach (GameObject avatar in ghostAvatars)
        {
            avatar.transform.GetComponent<MeshRenderer>().material = mat;
            avatar.GetComponentInChildren<LineRenderer>().material = mat;
            avatar.GetComponent<MeshRenderer>().enabled = false;
            avatar.transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
            avatar.transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
        }
    }

    private void UpdateIndex()
    {
        float distance = Vector3.Distance(gameObject.transform.position, currentNode.transform.position);
        if (distance < 1.5f)
        {
            prevnode = currentNode;
            currentIndex = currentNode.GetComponent<Node>().thisnode.nextnode;
            GetComponent<Logging>().AddData("NODE " + currentIndex.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            chosenNode = 0;
            chosen = false;
            ResetPreview(selectedMat);
        }
        if (currentIndex >= ordered.Count)
        {
            pause = true;           
        } 
        else if (ordered[currentIndex].Count > 1 && !choice && !chosen)
        {
            GetComponent<Logging>().AddData("CHOICE: " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            choice = true; 
        }
    }

    IEnumerator Choose()
    {
        Transform waypoints;
        Dictionary<int, float> distances = new Dictionary<int, float>();
        foreach (KeyValuePair<int, GameObject> node in ordered[currentIndex])
        {
            waypoints = node.Value.GetComponent<Node>().thisnode.waypoints.transform;
            SetPreview(node.Key, node.Value.transform, choiceMat, waypoints);
            arrows[node.Key].SetActive(true);
            arrows[node.Key].transform.localPosition = new Vector3(-0.3f * node.Key, -0.1f, 0);
            arrows[node.Key].transform.LookAt(ghostAvatars[node.Key].transform);
            arrows[node.Key].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, arrows[node.Key].transform.localEulerAngles.y, 0);
            arrows[node.Key].GetComponentInChildren<TMPro.TextMeshPro>().text = node.Value.GetComponent<Node>().thisnode.description;
            arrows[node.Key].transform.GetChild(1).transform.LookAt(gameObject.transform);
            Vector3 curRot = arrows[node.Key].transform.GetChild(1).transform.localEulerAngles;
            arrows[node.Key].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, curRot.y, 0);
            distances.Add(node.Key, Vector3.Distance(gameObject.transform.position, waypoints.GetChild(0).transform.position));
        }
        float min = Mathf.Infinity;
        int minindex = -1;
        foreach (KeyValuePair<int, float> dist in distances)
        {
            if (min > dist.Value)
            {
                min = dist.Value;
                minindex = dist.Key;
            }
        }
        if (min < 1.0f)
        {
            chosenNode = minindex;
            chosen = true;
            foreach (GameObject a in arrows)
            {
                a.SetActive(false);
            }
            arrows[chosenNode].SetActive(true);
            arrows[chosenNode].GetComponentInChildren<TMPro.TextMeshPro>().text = "Selected!"; 
            GetComponent<Logging>().AddData("SELECTED " + chosenNode.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            yield return new WaitForSeconds(0.5f);
            arrows[chosenNode].SetActive(false);
            ResetPreview(selectedMat);
            prevnode = currentNode;
            choice = false;
        }
        else
        {
            yield break;
        }
    }
}
