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
        choiceMat = (Material)Resources.Load("Materials/HighlightedGhostAvatarMaterial.mat");
        selectedMat = (Material)Resources.Load("Materials/GhostAvatarMaterial.mat");
        foreach (GameObject avatar in ghostAvatars)
        {
            avatar.GetComponent<MeshRenderer>().enabled = false;
            avatar.GetComponentInChildren<LineRenderer>().enabled = false;
        }
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
                    ghostAvatars[node.Key].transform.parent = currentWaypoints.GetChild(0).transform;
                    ghostAvatars[node.Key].transform.localPosition = new Vector3(0, -0.7f, 0);
                    ghostAvatars[node.Key].transform.localEulerAngles = new Vector3(0, 0, 0);
                    ghostAvatars[node.Key].GetComponent<MeshRenderer>().enabled = true;
                    ghostAvatars[node.Key].GetComponentInChildren<LineRenderer>().enabled = true;
                    ghostAvatars[node.Key].transform.GetComponent<MeshRenderer>().materials[0]=choiceMat;
                }
                yield return new WaitUntil(() => !paused);
                /*foreach (KeyValuePair<int, GameObject> node in ordered[i])
                {
                    ghostAvatars[node.Key].transform.GetComponent<MeshRenderer>().materials[0] = (Material)Resources.Load("Materials/GhostAvatarMaterial.mat");
                    ghostAvatars[node.Key].GetComponent<MeshRenderer>().enabled = false; 
                }*/
                //yield return new WaitUntil(() => Choice());
            }
            paused = false; 
            foreach (GameObject avatar in ghostAvatars)
            {
                avatar.transform.GetComponent<MeshRenderer>().materials[0] = selectedMat;
                avatar.GetComponent<MeshRenderer>().enabled = false;
                avatar.GetComponentInChildren<LineRenderer>().enabled = false;
            }
            currentNode = ordered[i][chosenNode];
            currentWaypoints = currentNode.GetComponent<Node>().thisnode.waypoints.transform;
            for (int j = 0; j < currentWaypoints.childCount; j++)
            {
                ghostAvatars[0].transform.parent = currentWaypoints.GetChild(j).transform;
                ghostAvatars[0].transform.localPosition = new Vector3(0, -0.7f, 0);
                ghostAvatars[0].transform.localEulerAngles = new Vector3(0, 0, 0);
                ghostAvatars[0].GetComponent<MeshRenderer>().enabled = true;
                ghostAvatars[0].GetComponentInChildren<LineRenderer>().enabled = true;
                Vector3 linepos = new Vector3(ghostAvatars[0].transform.position.x, transform.position.y, ghostAvatars[0].transform.position.z);
                ghostAvatars[0].GetComponentInChildren<LineRenderer>().SetPosition(0, linepos);
                ghostAvatars[0].GetComponentInChildren<LineRenderer>().SetPosition(1, transform.position);
                ghostAvatars[0].transform.GetComponent<MeshRenderer>().materials[0] = selectedMat;
                while (!paused) 
                {
                    yield return new WaitForSeconds(waitTime);
                    yield return new WaitUntil(() => !paused); 
                    if (!paused)
                    {
                        yield return new WaitForSeconds(waitTime);
                        paused = true;
                    }
                }
                paused = false;
                ghostAvatars[0].GetComponent<MeshRenderer>().enabled = false;
                ghostAvatars[0].GetComponentInChildren<LineRenderer>().enabled = false;
                gameObject.transform.position = currentWaypoints.GetChild(j).transform.position;
                gameObject.transform.rotation = currentWaypoints.GetChild(j).transform.rotation;
            }
            ghostAvatars[0].transform.parent = currentNode.transform;
            ghostAvatars[0].transform.localPosition = new Vector3(0, -0.7f, 0);
            ghostAvatars[0].transform.localEulerAngles = new Vector3(0, 0, 0);
            ghostAvatars[0].GetComponent<MeshRenderer>().enabled = true;
            ghostAvatars[0].GetComponentInChildren<LineRenderer>().enabled = true;
            ghostAvatars[0].transform.GetComponent<MeshRenderer>().materials[0] = selectedMat;
            while (!paused)
            {
                yield return new WaitForSeconds(waitTime);
                yield return new WaitUntil(() => !paused);
                if (!paused)
                {
                    yield return new WaitForSeconds(waitTime);
                    paused = true;
                }
            }
            paused = false;
            ghostAvatars[0].GetComponent<MeshRenderer>().enabled = false;
            ghostAvatars[0].GetComponentInChildren<LineRenderer>().enabled = false;
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
