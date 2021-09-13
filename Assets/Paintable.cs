using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Paintable : MonoBehaviourPunCallbacks
{
    public Transform holder;
    public GameObject brush;
    public float brushSize = 1f;
    private List<GameObject> dots;

    private void Start()
    {
        InvokeRepeating("LessUpdate", 1f, 0.02f);
    }

    private void LessUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (!hit.collider.CompareTag("Board") && !hit.collider.CompareTag("Brush")) return;
                Vector3 pos = hit.point;
                if (hit.collider.CompareTag("Board")) pos += Vector3.right * 0.5f;
                photonView.RPC("HandleDraw", RpcTarget.All, pos.x, pos.y, pos.z);
            }
        }
    }

    public void EraseBoard()
    {
        foreach(Transform child in holder)
        {
            Destroy(child.gameObject);
        }
    }

    [PunRPC]
    private void HandleDraw(float x, float y, float z)
    {
        
        var go = Instantiate(brush, new Vector3(x,y,z), Quaternion.identity);
        
        go.transform.localScale = Vector3.one * brushSize;

        go.transform.SetParent(holder);
        go.transform.localRotation = Quaternion.identity;
    }
}
