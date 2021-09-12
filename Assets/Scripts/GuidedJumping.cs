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
        CheckFocus();
        if (countdown)
        {
            countdowntime += Time.deltaTime;
            if (countdowntime >= waitTime)
            {
                countdowntime = 0;
            }
            Countdown(countdowntime);
        }
        if (reset)
        {
            countdowntime = 0;
            reset = false;
        }
        for (int i = 0; i < ordered.Count; i++)
        {
            foreach (KeyValuePair<int, GameObject> node in ordered[i])
            {
                if (arrows[node.Key].activeSelf)
                {
                    arrows[node.Key].transform.LookAt(ghostAvatars[node.Key].transform);
                    arrows[node.Key].transform.localEulerAngles = new Vector3(0, arrows[node.Key].transform.localEulerAngles.y, arrows[node.Key].transform.localEulerAngles.z); 
                    arrows[node.Key].transform.GetChild(1).transform.LookAt(gameObject.transform);
                    Vector3 curRot = arrows[node.Key].transform.GetChild(1).transform.localEulerAngles;
                    arrows[node.Key].transform.GetChild(1).transform.localEulerAngles = new Vector3(0,curRot.y,curRot.z);
                }
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
        if (!paused && (choice || !focused || gestureactive == 0))
        {
            Stop();
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
    }

    IEnumerator jumping()
    {
        int nodenum = 0;
        label.GetComponentInChildren<TMPro.TextMeshPro>().text = "Paused";
        while (nodenum < ordered.Count)
        {
            GetComponent<Logging>().AddData("NODE " + nodenum.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString() + ", " + transform.eulerAngles.ToString());
            if (ordered[nodenum].Count > 1)
            {
                GetComponent<Logging>().AddData("CHOICE: " + timer.ToString() +", "+ transform.position + ", " + transform.eulerAngles);
                choice = true;
                paused = true;
                foreach (KeyValuePair<int,GameObject> node in ordered[nodenum])
                {
                    currentWaypoints = node.Value.GetComponent<Node>().thisnode.waypoints.transform;
                    SetPreview(node.Key,currentWaypoints.GetChild(0), choiceMat);
                    arrows[node.Key].SetActive(true);
                    arrows[node.Key].transform.localPosition = new Vector3(-0.3f * node.Key, -0.1f, 0);
                    arrows[node.Key].transform.LookAt(ghostAvatars[node.Key].transform);
                    arrows[node.Key].transform.localEulerAngles = new Vector3(0, arrows[node.Key].transform.localEulerAngles.y, arrows[node.Key].transform.localEulerAngles.z);
                    arrows[node.Key].GetComponentInChildren<TMPro.TextMeshPro>().text = node.Value.GetComponent<Node>().thisnode.description;
                    arrows[node.Key].transform.GetChild(1).transform.LookAt(gameObject.transform);
                    Vector3 curRot = arrows[node.Key].transform.GetChild(1).transform.localEulerAngles;
                    arrows[node.Key].transform.GetChild(1).transform.localEulerAngles = new Vector3(0, curRot.y, curRot.z);
                }
                yield return new WaitUntil(() => !paused);
                foreach (GameObject a in arrows)
                {
                    a.SetActive(false);
                }
                arrows[chosenNode].SetActive(true);
                arrows[chosenNode].GetComponentInChildren<TMPro.TextMeshPro>().text = "Selected!"; 
                GetComponent<Logging>().AddData("SELECTED " + chosenNode.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString() + ", " + transform.eulerAngles.ToString());
                yield return new WaitForSeconds(0.5f);
                arrows[chosenNode].SetActive(false);
                choice = false;
                if (paused && !choice && focused && gestureactive != 0)
                {
                    Restart();
                }
            }
            paused = false;
            ResetPreview(selectedMat);
            currentNode = ordered[nodenum][chosenNode];
            currentWaypoints = currentNode.GetComponent<Node>().thisnode.waypoints.transform;
            for (int j = 0; j < currentWaypoints.childCount; j++)
            {
                SetPreview(chosenNode, currentWaypoints.GetChild(j),selectedMat);
                while (!paused)
                {
                    countdown = true;
                    yield return new WaitForSeconds(waitTime);
                    countdown = false;
                    countdowntime = 0;
                    yield return new WaitUntil(() => !paused);
                    if (!paused)
                    {
                        if (reset)
                        {
                            countdown = true;
                            yield return new WaitForSeconds(waitTime);
                            countdown = false;
                            countdowntime = 0;
                            reset = false;
                        }
                        paused = true;
                    }
                }
                paused = false;
                ghostAvatars[chosenNode].GetComponent<MeshRenderer>().enabled = false;
                ghostAvatars[chosenNode].transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
                ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
                gameObject.transform.position = currentWaypoints.GetChild(j).transform.position;
                gameObject.transform.rotation = currentWaypoints.GetChild(j).transform.rotation; 
                GetComponent<Logging>().AddData(timer.ToString() + ", " + transform.position.ToString() + ", " + transform.eulerAngles.ToString());
            }
            SetPreview(chosenNode, currentNode.transform,selectedMat);
            while (!paused)
            {
                countdown = true;
                yield return new WaitForSeconds(waitTime);
                countdown = false;
                countdowntime = 0;
                yield return new WaitUntil(() => !paused);
                if (!paused)
                {
                    if (reset)
                    {
                        countdown = true;
                        yield return new WaitForSeconds(waitTime);
                        countdown = false;
                        countdowntime = 0;
                        reset = false;
                    }                    
                    paused = true;
                }
            }
            paused = false;
            ghostAvatars[chosenNode].GetComponent<MeshRenderer>().enabled = false;
            ghostAvatars[chosenNode].transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
            ghostAvatars[chosenNode].transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
            gameObject.transform.position = currentNode.transform.position;
            gameObject.transform.rotation = currentNode.transform.rotation;
            GetComponent<Logging>().AddData(timer.ToString() + ", " + transform.position.ToString() + ", " + transform.eulerAngles.ToString());
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
        Vector3 linepos1 = new Vector3(transform.position.x, 0.0f, transform.position.z);
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
            avatar.transform.GetComponent<MeshRenderer>().material = selectedMat;
            avatar.GetComponentInChildren<LineRenderer>().material = selectedMat;
            avatar.GetComponent<MeshRenderer>().enabled = false;
            avatar.transform.GetChild(0).GetComponent<LineRenderer>().enabled = false;
            avatar.transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
        }
    }

    public void Choose()
    {
        int index = -1;
        float minangle = Mathf.Infinity;
        for (int i = 0; i < ghostAvatars.Length; i++)
        {
            if (eyes != null)
            {
                float temp = Vector3.Angle(ghostAvatars[i].transform.forward, eyes.transform.position - ghostAvatars[i].transform.position);
                if (minangle < temp)
                {
                    minangle = temp;
                    index = i;
                }
            }
        }
        if (index >= 0)
        {
            chosenNode = index;
            choice = false;
            if (paused && !choice && focused && gestureactive != 0)
            {
                Restart();
            }
        }
        Debug.Log(minangle);
    }
    public void Stop()
    {
        paused = true;
        reset = true;
        GetComponent<Logging>().AddData("STOPPED: " + timer.ToString() + ", " + transform.position.ToString() + ", " + transform.eulerAngles.ToString());
    }

    public void Restart()
    {
        paused = false;
        reset = true; 
        GetComponent<Logging>().AddData("RESTART: " + timer.ToString() + ", " + transform.position.ToString() + ", " + transform.eulerAngles.ToString());
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

    void CheckFocus()
    {
        float angle = 60;
        if (eyes != null)
        {
            if (Vector3.Distance(eyes.transform.position, ghostAvatars[chosenNode].transform.position) > 2)
            {
                if (!(Vector3.Angle(eyes.transform.forward, ghostAvatars[chosenNode].transform.position - eyes.transform.position) < angle))
                {
                    focused = false;
                }
                else if ((Vector3.Angle(eyes.transform.forward, ghostAvatars[chosenNode].transform.position - eyes.transform.position) < angle))
                {
                    focused = true;
                    if (paused && !choice && focused && gestureactive != 0)
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
        GameObject hand;
        hand = GameObject.Find(handside);
        float angle = 50;
        if (hand != null && eyes != null)
        {
            if (Vector3.Distance(eyes.transform.position, hand.transform.position) > 0.25f)
            {
                if (!(Vector3.Angle(eyes.transform.forward, hand.transform.position - eyes.transform.position) < angle))
                {
                    return true;
                }
            }
        }
        return true;
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
        gestureactive = i; 
        GetComponent<Logging>().AddData("GESTURE " + i.ToString() + ": " + timer.ToString() + ", " + transform.position.ToString() + ", " + transform.eulerAngles.ToString());
    }
}
