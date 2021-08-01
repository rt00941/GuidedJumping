using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NodeProperties
{
    public int index;
    // -1 if not optional else >= 0
    public int optionNumber;
    public GameObject waypoints;
}
public class Node : MonoBehaviour
{
    public NodeProperties thisnode;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
