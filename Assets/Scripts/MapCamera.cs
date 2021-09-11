using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public Shader unlitShader;
    private Transform player;

    public List<Light> Lights;
    public bool cullLights = true;


    void Start()
    {
        player = ThirdPersonMovement.LocalPlayerInstance.transform;
        // unlitShader = Shader.Find("Unlit/Texture");
        GetComponent<Camera>().SetReplacementShader(unlitShader, "");
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 newPos = player.position;
            newPos.y = transform.position.y;

            transform.position = newPos;
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
        
    }
}
