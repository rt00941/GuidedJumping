using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedJumping : MonoBehaviour
{
    private GameObject[] nodes;
    private GameObject currentNode;
    private GameObject[] ghostAvatars;
    public GameObject[] arrows;
    private GameObject label;
    private Transform currentWaypoints;
    public bool paused;
    public bool reset;
    private Dictionary<int, Dictionary<int, GameObject>> ordered = new Dictionary<int, Dictionary<int, GameObject>>();
    private int chosenNode;
    private int waitTime;
    private Material selectedMat;
    private Material choiceMat;

    // Start is called before the first frame update
    void Start()
    {
        nodes = GameObject.FindGameObjectsWithTag("Node");
        chosenNode = 0;
        paused = false;
        reset = false;
        waitTime = 3;
        ghostAvatars = GameObject.FindGameObjectsWithTag("GhostAvatar");
        arrows = GameObject.FindGameObjectsWithTag("Arrow");
        foreach (GameObject a in arrows)
        {
            a.SetActive(false);
        }
        label = GameObject.Find("Pause Label");
        label.SetActive(false);
        choiceMat = Resources.Load<Material>("Materials/HighlightedGhostAvatarMaterial");
        selectedMat = Resources.Load<Material>("Materials/GhostAvatarMaterial");
        ResetPreview(selectedMat);
        orderNodes();
        currentNode = ordered[0][chosenNode];
        gameObject.transform.position = currentNode.transform.position;
        gameObject.transform.rotation = currentNode.transform.rotation;
        StartCoroutine(jumping());
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < ordered.Count; i++)
        {
            foreach (KeyValuePair<int, GameObject> node in ordered[i])
            {
                if (arrows[node.Key].activeSelf)
                {
                    arrows[node.Key].transform.LookAt(ghostAvatars[node.Key].transform);
                    arrows[node.Key].transform.localEulerAngles = new Vector3(0, arrows[node.Key].transform.localEulerAngles.y, arrows[node.Key].transform.localEulerAngles.z); 
                    arrows[node.Key].transform.GetChild(1).transform.LookAt(gameObject.transform);
                }
            }
        }
    }

    IEnumerator jumping()
    {
        for (int i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].Count > 1)
            {
                Debug.Log("THERE IS A CHOICE HERE!!");
                paused = true;
                foreach (KeyValuePair<int,GameObject> node in ordered[i])
                {
                    currentWaypoints = node.Value.GetComponent<Node>().thisnode.waypoints.transform;
                    SetPreview(node.Key,currentWaypoints.GetChild(0), choiceMat);
                    arrows[node.Key].SetActive(true);
                    arrows[node.Key].transform.localPosition = new Vector3(-0.2f * node.Key, -0.1f, 0);
                    arrows[node.Key].transform.LookAt(ghostAvatars[node.Key].transform);
                    arrows[node.Key].transform.localEulerAngles = new Vector3(0, arrows[node.Key].transform.localEulerAngles.y, arrows[node.Key].transform.localEulerAngles.z);
                    arrows[node.Key].GetComponentInChildren<TMPro.TextMeshPro>().text = node.Key.ToString() + " out of " + ordered[i].Count;
                    arrows[node.Key].transform.GetChild(1).transform.LookAt(gameObject.transform);
                }
                yield return new WaitUntil(() => !paused);
                foreach (GameObject a in arrows)
                {
                    a.SetActive(false);
                }
                arrows[chosenNode].SetActive(true);
                arrows[chosenNode].GetComponentInChildren<TMPro.TextMeshPro>().text = "Selected!";
                yield return new WaitForSeconds(0.5f);
                arrows[chosenNode].SetActive(false);
            }
            paused = false;
            ResetPreview(selectedMat);
            currentNode = ordered[i][chosenNode];
            currentWaypoints = currentNode.GetComponent<Node>().thisnode.waypoints.transform;
            for (int j = 0; j < currentWaypoints.childCount; j++)
            {
                SetPreview(chosenNode, currentWaypoints.GetChild(j),selectedMat);
                while (!paused) 
                {
                    for (int t = 0; t <= waitTime; t++)
                    {
                        Countdown(t); 
                        if (t != waitTime)
                        {
                            yield return new WaitForSeconds(1);
                        }
                        else
                        {
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    yield return new WaitUntil(() => !paused); 
                    if (!paused)
                    {
                        if (reset)
                        {
                            for (int t = 0; t <= waitTime; t++)
                            {
                                Countdown(t); 
                                if (t != waitTime)
                                {
                                    yield return new WaitForSeconds(1);
                                }
                                else
                                {
                                    yield return new WaitForSeconds(0.1f);
                                }
                            }
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
            }
            SetPreview(chosenNode, currentNode.transform,selectedMat);
            while (!paused)
            {
                for (int t = 0; t <= waitTime; t++)
                {
                    Countdown(t); 
                    if (t != waitTime)
                    {
                        yield return new WaitForSeconds(1);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                yield return new WaitUntil(() => !paused);
                if (!paused)
                {
                    if (reset)
                    {
                        for (int t = 0; t <= waitTime; t++)
                        {
                            Countdown(t);
                            if (t != waitTime)
                            {
                                yield return new WaitForSeconds(1);
                            }
                            else
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                        }
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

    public void Choice(Vector3 pointingPosition)
    {
        Debug.Log("CHOICE GESTURE SELECTED");
        Debug.Log(pointingPosition);
        int index = 0;
        float minDist = Mathf.Infinity;
        float dist;
        for (int i = 0; i < ghostAvatars.Length; i++)
        {
            dist = Vector3.Distance(ghostAvatars[i].transform.position, pointingPosition);
            if (dist < minDist)
            {
                index = i;
                minDist = dist;
            }
        }
        if (minDist < 1)
        {
            chosenNode = index;
            paused = false;
        }
    }

    public void Stop()
    {
        Debug.Log("STOP GESTURE SELECTED");
        label.SetActive(true);
        paused = true;
    }

    public void Restart()
    {
        Debug.Log("RESTART GESTURE SELECTED");
        label.SetActive(false);
        paused = false;
        reset = true;
    }

    public void Countdown(int t)
    {
        Debug.Log(waitTime - t + " Seconds left");
        float width = ghostAvatars[chosenNode].transform.GetChild(0).GetComponent<LineRenderer>().startWidth;
        if (t == waitTime)
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

    private object WaitForSeconds(int v)
    {
        throw new NotImplementedException();
    }

    private object WaitForSecondsRealtime(int v)
    {
        throw new NotImplementedException();
    }
}
