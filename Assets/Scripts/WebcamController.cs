using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamController : MonoBehaviour
{
    static WebCamTexture backCam;

    private void Start()
    {
        if (backCam == null) backCam = new WebCamTexture();
        GetComponent<Renderer>().material.mainTexture = backCam;

        if (!backCam.isPlaying)
        {
            backCam.Play();
        }
    }
}
