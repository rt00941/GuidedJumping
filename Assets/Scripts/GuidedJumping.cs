using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedJumping : MonoBehaviour
{
    private GameObject[] nodes;
    private GameObject currentNode;
    private GameObject ghostAvatar;
    private Transform currentWaypoints; 
    private Dictionary<int, Dictionary<int, GameObject>> ordered = new Dictionary<int, Dictionary<int, GameObject>>();
    private int chosenNode;

    // Start is called before the first frame update
    void Start()
    {
        nodes = GameObject.FindGameObjectsWithTag("Node");
        chosenNode = 0;
        ghostAvatar = GameObject.Find("GhostAvatar");
        orderNodes();
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
            currentNode = ordered[i][chosenNode];
            currentWaypoints = currentNode.GetComponent<Node>().thisnode.waypoints.transform;
            for (int j = 0; j < currentWaypoints.childCount; j++)
            {
                ghostAvatar.transform.parent = currentWaypoints.GetChild(j).transform;
                ghostAvatar.transform.localPosition = new Vector3(0, -0.7f, 0);
                ghostAvatar.GetComponent<MeshRenderer>().enabled = true;
                yield return new WaitForSeconds(2);
                ghostAvatar.GetComponent<MeshRenderer>().enabled = false;
                gameObject.transform.position = currentWaypoints.GetChild(j).transform.position;
                gameObject.transform.rotation = currentWaypoints.GetChild(j).transform.rotation;
            }
            ghostAvatar.transform.parent = currentNode.transform;
            ghostAvatar.transform.localPosition = new Vector3(0, -0.7f, 0);
            ghostAvatar.GetComponent<MeshRenderer>().enabled = true;
            yield return new WaitForSeconds(2);
            ghostAvatar.GetComponent<MeshRenderer>().enabled = false;
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
        Debug.Log(ordered[1][0]);
    }
    
    public void Choice(int index)
    {
        Debug.Log(index);
        chosenNode = index;
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
