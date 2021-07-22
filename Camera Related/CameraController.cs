using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Sensitivity, xlim, MovementSpeed, jumpHeight, CamFollowSpeed;
    public GameObject Camera;
    Vector2 rotation = Vector2.zero;
    private float variableSpeed, cameramovementlerp;
    public Rigidbody rb;
    public bool grounded;
    private Vector3 Offset;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
        Offset = Camera.transform.position - transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {

        rotation.y += Input.GetAxis("Mouse X") * Sensitivity;
        rotation.x += -Input.GetAxis("Mouse Y") * Sensitivity;
        rotation.x = Mathf.Clamp(rotation.x, -xlim, xlim);
        Camera.transform.localRotation = Quaternion.Euler(rotation.x, transform.eulerAngles.y, 0);
        transform.eulerAngles = new Vector2(0, rotation.y);

        if (Input.GetKey("w")) transform.position = transform.position + transform.forward * variableSpeed / 10;
        if (Input.GetKey("s")) transform.position = transform.position + -transform.forward * variableSpeed / 10;
        if (Input.GetKey("d")) transform.position = transform.position + transform.right * variableSpeed / 10;
        if (Input.GetKey("a")) transform.position = transform.position + -transform.right * variableSpeed / 10;

        if (Input.GetKeyDown(KeyCode.Space) && grounded) rb.AddForce(new Vector3(0, jumpHeight, 0));
        
        if (Input.GetKey(KeyCode.LeftShift)){
            variableSpeed = MovementSpeed * 2f;
            cameramovementlerp = 70;
        }
        else{
            variableSpeed = MovementSpeed;
            cameramovementlerp = 60;
        }
        Camera.transform.position = Vector3.Lerp(Camera.transform.position, transform.position + Offset, Time.smoothDeltaTime * CamFollowSpeed);

        if (Input.GetKey(KeyCode.LeftControl)) variableSpeed = MovementSpeed / 3f;
        else variableSpeed = MovementSpeed;
        float cml = Camera.GetComponentInChildren<Camera>().fieldOfView;
        Camera.GetComponentInChildren<Camera>().fieldOfView = Mathf.Lerp(cml, cameramovementlerp, Time.deltaTime * 6);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") grounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Floor") grounded = false;
        
    }
}
