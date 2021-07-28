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
