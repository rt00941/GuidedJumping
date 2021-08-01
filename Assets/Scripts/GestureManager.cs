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
    public float threshold = 0.1f;
    public OVRSkeleton skeleton;
    public bool debugMode = true;
    public List<Gesture> gestures;
    private List<OVRBone> fingerBones;
    private Gesture previousGesture;

    // Start is called before the first frame update
    void Start()
    {
        fingerBones = new List<OVRBone>(skeleton.Bones);
        previousGesture = new Gesture();
    }

    // Update is called once per frame
    void Update()
    {
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }

        Gesture currentGesture = Recognize();
        bool hasRecognized = !currentGesture.Equals(new Gesture());
        // check if new gesture
        if (hasRecognized && !currentGesture.Equals(previousGesture))
        {
            // it is a new gesture
            Debug.Log("New Gesture found: " + currentGesture.gestureName);
            previousGesture = currentGesture;
            currentGesture.onRecognized.Invoke();
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

    Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach(var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i=0; i < fingerBones.Count; i++)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingersData[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;

            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;
    }

}
