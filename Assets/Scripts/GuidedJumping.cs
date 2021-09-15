using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// referenced https://www.youtube.com/watch?v=lBzwUKQ3tbw
[Serializable]
public struct Gesture
{
    public string gestureName;
    public List<Vector3> fingersData;
    public UnityEvent onRecognized;
}

public class GuidedJumping : MonoBehaviour
{
    private GameObject[] nodes;
    private GameObject currentNode;
    private GameObject[] ghostAvatars;
    public GameObject[] arrows;
    private GameObject label;
    private GameObject eyes;
    private Transform currentWaypoints;
    public bool paused;
    public bool choice;
    public bool reset;
    public bool focused;
    private bool countdown;
    private Dictionary<int, Dictionary<int, GameObject>> ordered = new Dictionary<int, Dictionary<int, GameObject>>();
    public int chosenNode;
    private int waitTime;
    private float countdowntime;
    private Material selectedMat;
    private Material choiceMat;
    private float timer;
    public int gestureactive; // none = -1, stop = 0, restart = 1, pointing = 2


    public float threshold = 0.1f;
    public OVRSkeleton skeleton;
    public List<Gesture> gestures;
    private List<OVRBone> fingerBones;
    private Gesture previousGesture;
    string handtype;
    private GameObject hand;
    public LayerMask layer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0.0f;
        nodes = GameObject.FindGameObjectsWithTag("Node");
        chosenNode = 0;
        paused = false;
        reset = false;
        focused = true;
        countdown = false;
        gestureactive = -1;
        waitTime = 3;
        countdowntime = 0;
        ghostAvatars = GameObject.FindGameObjectsWithTag("GhostAvatar");
        arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject a in arrows)
        {
            a.SetActive(false);
        }
        label = GameObject.Find("Pause Label");
        label.GetComponentInChildren<TMPro.TextMeshPro>().text = "Paused";
        label.SetActive(false);
        eyes = GameObject.Find("CenterEyeAnchor");
        choiceMat = Resources.Load<Material>("Materials/HighlightedGhostAvatarMaterial");
        selectedMat = Resources.Load<Material>("Materials/GhostAvatarMaterial");
        ResetPreview(selectedMat);
        orderNodes();
        currentNode = ordered[0][chosenNode];
        gameObject.transform.position = currentNode.transform.position;
        gameObject.transform.rotation = currentNode.transform.rotation;
        choice = false;
        fingerBones = new List<OVRBone>(skeleton.Bones);
        previousGesture = new Gesture();
        handtype = skeleton.transform.parent.name;
        StartCoroutine(jumping());
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (currentNode.GetComponent<Node>().thisnode.index != 0 && !choice)
        {
            CheckFocus();
        }
        if (countdown)
        {
            countdowntime += Time.deltaTime;
            if (paused)
            {
                countdowntime = 0;
            }
        }
        Countdown(countdowntime);
        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i].activeSelf)
            {
                arrows[i].transform.LookAt(ghostAvatars[i].transform);
                arrows[i].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, arrows[i].transform.localEulerAngles.y, 0);
                arrows[i].transform.GetChild(1).transform.LookAt(gameObject.transform);
                Vector3 curRot = arrows[i].transform.GetChild(1).transform.localEulerAngles;
                arrows[i].transform.GetChild(1).transform.localEulerAngles = new Vector3(0,curRot.y,0);
            }
        }
        if (fingerBones.Count == 0)
        {
            fingerBones = new List<OVRBone>(skeleton.Bones);
        }

        // check if gesture made in correct position
        if (GestureInView(handtype))
        {
            Gesture currentGesture = Recognize();
            bool hasRecognized = !currentGesture.Equals(new Gesture());
            // check if new gesture
            if (hasRecognized && !currentGesture.Equals(previousGesture))
            {
                // it is a new gesture
                previousGesture = currentGesture;
                currentGesture.onRecognized.Invoke();
            }
        }

        if (paused)
        {
            label.SetActive(true);
        }
        else
        {
            label.SetActive(false);
        }
    }

    IEnumerator jumping()
    {
        int nodenum = 0;
        label.GetComponentInChildren<TMPro.TextMeshPro>().text = "Paused";
        while (nodenum < ordered.Count)
        {
            GetComponent<Logging>().AddData("NODE " + nodenum.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            if (ordered[nodenum].Count > 1)
            {
                GetComponent<Logging>().AddData("CHOICE: " + timer.ToString() +", "+ transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
                choice = true;
                Stop();
                foreach (KeyValuePair<int,GameObject> node in ordered[nodenum])
                {
                    currentWaypoints = node.Value.GetComponent<Node>().thisnode.waypoints.transform;
                    SetPreview(node.Key,currentWaypoints.GetChild(0), choiceMat);
                    arrows[node.Key].SetActive(true);
                    arrows[node.Key].transform.localPosition = new Vector3(-0.3f * node.Key, -0.1f, 0);
                    arrows[node.Key].transform.LookAt(ghostAvatars[node.Key].transform);
                    arrows[node.Key].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, arrows[node.Key].transform.localEulerAngles.y, 0);
                    arrows[node.Key].GetComponentInChildren<TMPro.TextMeshPro>().text = node.Value.GetComponent<Node>().thisnode.description;
                    arrows[node.Key].transform.GetChild(1).transform.LookAt(gameObject.transform);
                    Vector3 curRot = arrows[node.Key].transform.GetChild(1).transform.localEulerAngles;
                    arrows[node.Key].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, curRot.y, 0);
                }
                yield return new WaitUntil(() => !choice);
                foreach (GameObject a in arrows)
                {
                    a.SetActive(false);
                }
                arrows[chosenNode].SetActive(true);
                arrows[chosenNode].GetComponentInChildren<TMPro.TextMeshPro>().text = "Selected!"; 
                GetComponent<Logging>().AddData("SELECTED " + chosenNode.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
                yield return new WaitForSeconds(0.5f);
                arrows[chosenNode].SetActive(false);
                Restart();
            }
            paused = false;
            ResetPreview(selectedMat);
            currentNode = ordered[nodenum][chosenNode];
            currentWaypoints = currentNode.GetComponent<Node>().thisnode.waypoints.transform;
            for (int j = 0; j < currentWaypoints.childCount; j++)
            {
                SetPreview(chosenNode, currentWaypoints.GetChild(j),selectedMat);
                countdowntime = 0;
                countdown = true;
                countdowntime = 0;
                yield return new WaitUntil(() => countdowntime >= waitTime);
                countdown = false;
                countdowntime = 0;
                reset = false;
                ghostAvatars[chosenNode].GetComponent<MeshRenderer>().enabled = false;
                ghostAvatars[chosenNode].transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
                ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
                gameObject.transform.position = currentWaypoints.GetChild(j).transform.position;
                gameObject.transform.rotation = currentWaypoints.GetChild(j).transform.rotation;
                GetComponent<Logging>().AddData(timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            }
            SetPreview(chosenNode, currentNode.transform,selectedMat);
            countdowntime = 0;
            countdown = true;
            countdowntime = 0;
            yield return new WaitUntil(() => countdowntime >= waitTime);
            countdown = false;
            countdowntime = 0;
            reset = false;
            ghostAvatars[chosenNode].GetComponent<MeshRenderer>().enabled = false;
            ghostAvatars[chosenNode].transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
            ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
            gameObject.transform.position = currentNode.transform.position;
            gameObject.transform.rotation = currentNode.transform.rotation;
            GetComponent<Logging>().AddData(timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            chosenNode = 0;
            nodenum = currentNode.GetComponent<Node>().thisnode.nextnode;
        }
        paused = true;
        choice = true;
        focused = false;
        label.SetActive(true);
        label.GetComponentInChildren<TMPro.TextMeshPro>().text = "END OF TASK";
        GetComponent<Logging>().AddData("END OF TASK");
        GetComponent<Logging>().AddData("Time Taken: " + timer.ToString());
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

    private void SetPreview(int index, Transform current, Material mat)
    {
        ghostAvatars[index].transform.parent = current;
        ghostAvatars[index].transform.localPosition = new Vector3(0, -0.7f, 0);
        ghostAvatars[index].transform.localEulerAngles = new Vector3(0, 0, 0);
        ghostAvatars[index].GetComponent<MeshRenderer>().enabled = true;
        Vector3 linepos = new Vector3(ghostAvatars[index].transform.position.x, transform.position.y, ghostAvatars[index].transform.position.z);
        Vector3 linepos1 = new Vector3(transform.position.x, 0.5f, transform.position.z);
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(0, linepos);
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().SetPosition(1, linepos1); 
        ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().startWidth = 0.1f;
        ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().endWidth = 0.1f;
        ghostAvatars[index].transform.GetChild(0).GetComponent<LineRenderer>().enabled = true;
        ghostAvatars[index].transform.GetChild(1).GetComponent<LineRenderer>().SetPosition(0, linepos);
        ghostAvatars[index].transform.GetChild(1).GetComponent<LineRenderer>().SetPosition(1, linepos1);
        ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().startWidth = 0.01f;
        ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().endWidth = 0.01f;
        ghostAvatars[index].transform.GetChild(1).GetComponent<LineRenderer>().enabled = true;
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

    public void Choose()
    {
        if (choice)
        {
            hand = GameObject.Find(handtype);
            int index = -1;
            float minangle = Mathf.Infinity;
            if (fingerBones != null)
            {
                foreach (var bone in fingerBones)
                {
                    if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                    {
                        for (int i = 0; i < ghostAvatars.Length; i++)
                        {
                            float distance = Vector3.Distance(bone.Transform.position, ghostAvatars[i].transform.position);
                            index = i;
                            //float angle = Vector3.Angle(bone.Transform.position, ghostAvatars[i].transform.position);
                            Debug.Log(bone.Transform.forward);
                            float angle = Vector3.Angle(bone.Transform.forward, (bone.Transform.position - ghostAvatars[i].transform.position).normalized);
                            Debug.Log(angle + " " + i);
                        }
                        /*RaycastHit hit;
                        Debug.Log(bone.Transform.position);
                        Physics.SphereCast(bone.Transform.position, 10.0F, transform.TransformDirection(Vector3.forward), out hit, 10.0f, layer.value);
                        Debug.Log(hit.collider);
                        if (Physics.Raycast(bone.Transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layer.value))
                        {
                            Debug.Log(hit.collider);
                            for (int i = 0; i < ghostAvatars.Length; i++)
                            {
                                if (hit.collider.gameObject.Equals(ghostAvatars[i]))
                                {
                                    chosenNode = i;
                                    choice = false;
                                    break;
                                }
                            }
                        }*/
                    }
                }
            }
            /*if (index >= 0)// && minangle < anglelimit && minangle != 0)
            {
                chosenNode = index;
                choice = false;
            }*/
        }
    }
    public void Stop()
    {
        paused = true;
        GetComponent<Logging>().AddData("STOPPED: " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
    }

    public void Restart()
    {
        if (!choice)
        {
            paused = false;
            reset = true;
            GetComponent<Logging>().AddData("RESTART: " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
        }
    }

    public void Countdown(float t)
    {
        float width = ghostAvatars[chosenNode].transform.GetChild(0).GetComponent<LineRenderer>().startWidth;
        if (t >= waitTime)
        {
            width = 0;
        }
        else if (t != 0)
        {
            width = (width / (t + 1));
        }
        ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().startWidth = width;
        ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().endWidth = width;
    }

    bool IsVisible(GameObject toCheck)
    {
        if (eyes != null)
        {
            Vector3 point = eyes.transform.GetComponent<Camera>().WorldToViewportPoint(toCheck.transform.position);
            if (point.x > 0 && point.x < 1 && point.y >0 && point.y < 1 && point.z > 0)
            {
                return true;
            }
        }
        return false;
    }
    void CheckFocus()
    {
        float angle = 60;
        if (eyes != null)
        {
            if (Vector3.Distance(eyes.transform.position, ghostAvatars[chosenNode].transform.position) > 2)
            {
                if (!(Vector3.Angle(eyes.transform.forward, ghostAvatars[chosenNode].transform.position - eyes.transform.position) < angle) && !IsVisible(ghostAvatars[chosenNode]))
                {
                    focused = false;
                    Stop();
                }
                else if ((Vector3.Angle(eyes.transform.forward, ghostAvatars[chosenNode].transform.position - eyes.transform.position) < angle) && IsVisible(ghostAvatars[chosenNode]) && !focused)
                {
                    focused = true;
                    if (!choice && paused)
                    {
                        Restart();
                    }
                }
            }
        }
    }

    // Gestures Code
    public bool GestureInView(string handside)
    {
        hand = GameObject.Find(handside);
        if (hand != null && eyes != null)
        {
            if (IsVisible(hand) && hand.GetComponentInChildren<SkinnedMeshRenderer>().enabled)
            {
                if (Vector3.Distance(eyes.transform.position, hand.transform.position) > 0.05f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBones.Count; i++)
            {
                if (gesture.fingersData.Count != 0)
                {
                    Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                    float distance = Vector3.Distance(currentData, gesture.fingersData[i]);
                    if (distance > threshold)
                    {
                        isDiscarded = true;
                        break;
                    }
                    sumDistance += distance;
                }
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;
    }

    public void SetGestureActive(int i)
    {
        if (i == 2)
        {
            if (choice)
            {
                gestureactive = i; 
                GetComponent<Logging>().AddData("GESTURE " + i.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            }
        }
        else
        { 
            if (!choice)
            {
                gestureactive = i; 
                GetComponent<Logging>().AddData("GESTURE " + i.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString("F4") + ", " + transform.eulerAngles.ToString("F4"));
            }
        }
    }
}
