using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// referenced https://www.youtube.com/watch?v=lBzwUKQ3tbw
[System.Serializable]
public struct Gesture
{
    public string gestureName;
    public List<Vector3> fingersData;
    public UnityEvent onRecognized;
}

public class GestureManager : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public bool debugMode = true;
    public List<Gesture> gestures;
    private List<OVRBone> fingerBones;

    // Start is called before the first frame update
    void Start()
    {
        fingerBones = new List<OVRBone>(skeleton.Bones);
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }
    }

    void Save()
    {
        Gesture g = new Gesture();
        g.gestureName = "New Gesture";
        List<Vector3> data = new List<Vector3>();

        foreach (var bone in fingerBones)
        {
            data.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
        }
        g.fingersData = data;
        gestures.Add(g);
    }

}
