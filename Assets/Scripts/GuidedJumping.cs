using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedJumping : MonoBehaviour
{
    private GameObject[] nodes;
    private GameObject currentNode;
    private GameObject[] ghostAvatars;
    public GameObject ghostAvatarPrefab;
    private Transform currentWaypoints;
    public bool paused;
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
        waitTime = 2;
        ghostAvatars = GameObject.FindGameObjectsWithTag("GhostAvatar");
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
                }
                yield return new WaitUntil(() => !paused);
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
                    //yield return new WaitForSeconds(waitTime);
                    for (int t = 0; t < waitTime; t++)
                    {
                        Debug.Log(waitTime - t + " Seconds left");
                        yield return new WaitForSeconds(1);
                    }
                    yield return new WaitUntil(() => !paused); 
                    if (!paused)
                    {
                        //yield return new WaitForSeconds(waitTime);
                        for (int t = 0; t < waitTime; t++)
                        {
                            Debug.Log(waitTime - t + " Seconds left");
                            yield return new WaitForSeconds(1);
                        }
                        paused = true;
                    }
                }
                paused = false;
                ghostAvatars[chosenNode].GetComponent<MeshRenderer>().enabled = false;
                ghostAvatars[chosenNode].GetComponentInChildren<LineRenderer>().enabled = false;
                gameObject.transform.position = currentWaypoints.GetChild(j).transform.position;
                gameObject.transform.rotation = currentWaypoints.GetChild(j).transform.rotation;
            }
            SetPreview(chosenNode, currentNode.transform,selectedMat);
            while (!paused)
            {
                //yield return new WaitForSeconds(waitTime);
                for (int t = 0; t < waitTime; t++)
                {
                    Debug.Log(waitTime - t + " Seconds left");
                    yield return new WaitForSeconds(1);
                }
                yield return new WaitUntil(() => !paused);
                if (!paused)
                {
                    //yield return new WaitForSeconds(waitTime);
                    for (int t = 0; t < waitTime; t++)
                    {
                        Debug.Log(waitTime - t + " Seconds left");
                        yield return new WaitForSeconds(1);
                    }
                    paused = true;
                }
            }
            paused = false;
            ghostAvatars[chosenNode].GetComponent<MeshRenderer>().enabled = false;
            ghostAvatars[chosenNode].GetComponentInChildren<LineRenderer>().enabled = false;
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
        ghostAvatars[index].GetComponentInChildren<LineRenderer>().SetPosition(0, linepos);
        ghostAvatars[index].GetComponentInChildren<LineRenderer>().SetPosition(1, linepos1);
        ghostAvatars[index].GetComponentInChildren<LineRenderer>().enabled = true;
        ghostAvatars[index].GetComponent<MeshRenderer>().material = mat;
        ghostAvatars[index].GetComponentInChildren<LineRenderer>().material = mat;
    }

    private void ResetPreview(Material mat)
    {
        foreach (GameObject avatar in ghostAvatars)
        {
            avatar.transform.GetComponent<MeshRenderer>().material = selectedMat;
            avatar.GetComponentInChildren<LineRenderer>().material = selectedMat;
            avatar.GetComponent<MeshRenderer>().enabled = false;
            avatar.GetComponentInChildren<LineRenderer>().enabled = false;
        }
    }

    public void Choice(int index)
    {
        chosenNode = index;
        paused = false;
    }

    public void Stop()
    {
        paused = true;
    }

    public void Restart()
    {
        paused = false;
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
