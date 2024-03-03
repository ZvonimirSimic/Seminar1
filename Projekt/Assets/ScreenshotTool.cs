using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class ScreenshotTool : MonoBehaviour
{
    public void captureScreen()
    {
        ScreenCapture.CaptureScreenshot("Screenshot.png");
        Debug.Log("Took a screenshot!");
    }
}
