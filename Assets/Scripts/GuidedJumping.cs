using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidedJumping : MonoBehaviour
{

    private List<Vector3> nodeslist;

    // Start is called before the first frame update
    void Start()
    {
        MakeNodeList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MakeNodeList()
    {
        foreach (GameObject obj in Game)
    }
}
