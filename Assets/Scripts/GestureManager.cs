using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
// referenced https://www.youtube.com/watch?v=lBzwUKQ3tbw
[System.Serializable]
public struct Gesture
{
    public string gestureName;
    public List<Vector3> fingersData;
    public UnityEvent onRecognized;
}*/

public class GestureManager : MonoBehaviour
{
    public float threshold = 0.1f;
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
        /*if (fingerBones.Count == 0)
        {
            fingerBones = new List<OVRBone>(skeleton.Bones);
        }
        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Save();
        }

        // check if gesture made in correct position
        if (GameObject.Find("Platform").GetComponent<GuidedJumping>().GestureInView(handtype))
        {
            Gesture currentGesture = Recognize();
            bool hasRecognized = !currentGesture.Equals(new Gesture());
            // check if new gesture
            if (hasRecognized && !currentGesture.Equals(previousGesture))
            {
                // it is a new gesture
                Debug.Log("RECOGNIZED");
                previousGesture = currentGesture;
                currentGesture.onRecognized.Invoke();
            }
        }*/
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

    /*public void PointingAt()
    {
        Debug.Log("POINTING GESTURE SELECTED");
        foreach (var bone in fingerBones)
        {
            if(bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                Choose(bone.Transform);
            }
        }
    }

    /*Gesture Recognize()
    {
        Gesture currentGesture = new Gesture();
        float currentMin = Mathf.Infinity;

        foreach(var gesture in gestures)
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i=0; i < fingerBones.Count; i++)
            {
                if (gesture.fingersData.Count != 0)
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
            }
            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;
    }
    /*public void Choose(Transform pointingTransform)
    {
        Debug.Log("CHOICE GESTURE SELECTED");
        int index = -1;
        float angle = 30;
        float minangle = Mathf.Infinity;
        for (int i = 0; i < ghostAvatars.Length; i++)
        {
            /*Debug.Log(Vector3.Angle(pointingTransform.forward, ghostAvatars[i].transform.position - pointingTransform.position));
            if (Vector3.Angle(pointingTransform.forward, ghostAvatars[i].transform.position - pointingTransform.position) < angle)
            {
            float temp = Vector3.Angle(pointingTransform.forward, ghostAvatars[i].transform.position - pointingTransform.position);
            Debug.Log(temp);
            temp = Vector3.Angle(ghostAvatars[i].transform.forward, pointingTransform.position - ghostAvatars[i].transform.position);
            Debug.Log(temp);
            if (temp < angle)
            {
                if (minangle < temp)
                {
                    minangle = temp;
                    index = i;
                }
            }
        }
        if (index >= 0)
        {
            jumping.SetChosenNode(index);
            Debug.Log("Node " + index + " selected!");
            jumping.SetChoice(false);
        }
    }

    public void Stop()
    {
        // Debug.Log("STOP GESTURE SELECTED");
        jumping.SetPaused(true);
        jumping.SetReset(true);
    }

    public void Restart()
    {
        /*Debug.Log("RESTART GESTURE SELECTED");
        jumping.SetPaused(false);
        jumping.SetReset(true);
    }*/
}
