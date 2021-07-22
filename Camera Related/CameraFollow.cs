using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject Camera;
    public float PlayerSpeed = 10;
    public float CameraFollowSpeed;
    private Vector3 CamOffset;

    void Start()
    {
        CamOffset = Camera.transform.position - transform.position;
    }

    void Update()
    {
        transform.position += new Vector3(Input.GetAxis("Horizontal") * PlayerSpeed * Time.smoothDeltaTime, 0, Input.GetAxis("Vertical") * PlayerSpeed * Time.smoothDeltaTime);
        Camera.transform.position = Vector3.Lerp(Camera.transform.position, transform.position + CamOffset, Time.smoothDeltaTime * CameraFollowSpeed);
    }
}
