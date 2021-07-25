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
        StartCoroutine(jumping());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator jumping()
    {
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            yield return new WaitForSeconds(2);
            //gameObject.transform.SetParent(((GameObject)nodes.GetValue(i)).transform);
            //gameObject.transform.localPosition = new Vector3(0, 0, 0);
            List<Vector3> waypoints = CalculateWaypoints(((GameObject)nodes.GetValue(i)), ((GameObject)nodes.GetValue(i + 1)));
            Debug.Log("WAYPOINTS " + waypoints.Count);
            /*for (int j = 0; j < waypoints.Count; j++)
            {
                
                yield return new WaitForSeconds(1);
                gameObject.transform.position = waypoints[j];
            }*/
        }
    }

    private List<Vector3> CalculateWaypoints(GameObject startnode, GameObject endnode)
    {
        List<Vector3> waypoints = new List<Vector3>();

        float startx = startnode.transform.position.x;
        float starty = startnode.transform.position.y;
        float startz = startnode.transform.position.z;

        float xdiff = endnode.transform.position.x - startnode.transform.position.x;
        float ydiff = endnode.transform.position.y - startnode.transform.position.y;
        float zdiff = endnode.transform.position.z - startnode.transform.position.z;

        float xincrement = 1.0f;
        float yincrement = 1.0f;
        float zincrement = 1.0f;

        
        while (transform.position.x <= endnode.transform.position.x)
        {
            startx += xincrement;
            waypoints.Add(new Vector3(startx, starty, startz));
        }
        startx = endnode.transform.position.x;
        waypoints.Add(new Vector3(startx, starty, startz));
        
        while (transform.position.y <= endnode.transform.position.y)
        {
            starty += yincrement;
            waypoints.Add(new Vector3(startx, starty, startz));
        }
        starty = endnode.transform.position.y;
        waypoints.Add(new Vector3(startx, starty, startz));

        while (transform.position.z <= endnode.transform.position.z)
        {
            startz += zincrement;
            waypoints.Add(new Vector3(startx, starty, startz));
        }
        startz = endnode.transform.position.z;
        waypoints.Add(new Vector3(startx, starty, startz));

        return waypoints;
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
