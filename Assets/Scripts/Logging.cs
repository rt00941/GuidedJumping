using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class Logging : MonoBehaviour
{
    public int participantNumber;
    private static StreamWriter writer;
    private static string datatext;
    private static string path;

    // Start is called before the first frame update
    void Start()
    {
        string filename = "Participant " + participantNumber + " " + SceneManager.GetActiveScene().name;
        path = "D:/Documents/Masters/Thesis/GuidedJumping/Data/" + filename + ".txt";
        datatext = filename + '\n';
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void AddData(string line)
    {
        datatext += line;
        datatext += '\n';
    }

    public void Write()
    {
        WriteString();
    }

    static void WriteString()
    {
        //Write some text to the file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(datatext);
        writer.Close();
    }
}
