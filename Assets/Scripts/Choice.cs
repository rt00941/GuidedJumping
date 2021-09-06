using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Choice : MonoBehaviour
{
    GameObject arrows;
    GameObject label;
    // Start is called before the first frame update
    void Start()
    {
        arrows = GameObject.Find("All Arrows");
        arrows.SetActive(false);
        label = GameObject.Find("Pause Label");
        label.GetComponentInChildren<TMPro.TextMeshPro>().text = "Starting";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            arrows.SetActive(true);
            label.GetComponentInChildren<TMPro.TextMeshPro>().text = "1";
            GetComponent<GuidedJumping>().enabled = true;
            GetComponent<JumpingVisual>().enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            arrows.SetActive(true);
            label.GetComponentInChildren<TMPro.TextMeshPro>().text = "2";
            GetComponent<GuidedJumping>().enabled = false;
            GetComponent<JumpingVisual>().enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            label.GetComponentInChildren<TMPro.TextMeshPro>().text = "Switching Room";
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
