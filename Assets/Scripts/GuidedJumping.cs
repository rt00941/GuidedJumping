using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedJumping : MonoBehaviour
{
    private GameObject[] nodes;
    // Start is called before the first frame update
    void Start()
    {
        nodes = GameObject.FindGameObjectsWithTag("Node");
        Debug.Log(nodes.Length);
        StartCoroutine(jumping());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator jumping()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            yield return new WaitForSeconds(2);

            gameObject.transform.SetParent(((GameObject)nodes.GetValue(i)).transform);
            gameObject.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    private void CalculateWaypoints(GameObject startnode, GameObject endnode)
    {
        float xdiff = endnode.transform.position.x - startnode.transform.position.x;
        float ydiff = endnode.transform.position.y - startnode.transform.position.y;
        float zdiff = endnode.transform.position.z - startnode.transform.position.z;

        float xincrement = 0.1f;
        float yincrement = 0.1f;
        float zincrement = 0.1f;
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
