using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class OpenMenu : MonoBehaviour
{
    public GameObject panel, editpanel, infoPanel, capturePanel;
    public GameObject target;
    float lastTime;
    // Start is called before the first frame update
    void Start()
    {
        lastTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (ControllerBinding.RightThumbstick.GetDown())
        {
            if (panel.active)
            {
                if (Time.time - lastTime > 0.3f)
                {
                    panel.SetActive(false);
                    Debug.Log("Menu opened.");
                    lastTime = Time.time;
                }
            }
            else if(editpanel.active)
            {
                if (Time.time - lastTime > 0.3f)
                {
                    editpanel.SetActive(false);
                    Debug.Log("Menu closed.");
                    lastTime = Time.time;
                }
            }
            else if (infoPanel.active)
            {
                if (Time.time - lastTime > 0.3f)
                {
                    infoPanel.SetActive(false);
                    Debug.Log("Edit menu closed.");
                    lastTime = Time.time;
                }
            }
            else if (capturePanel.active)
            {
                if (Time.time - lastTime > 0.3f)
                {
                    capturePanel.SetActive(false);
                    Debug.Log("Edit menu closed.");
                    lastTime = Time.time;
                }
            }
            else
            {
                if (Time.time - lastTime > 0.3f)
                {
                    panel.SetActive(true);
                    Debug.Log("Menu opened.");
                    lastTime = Time.time;
                }
            }
        }
    }
}
