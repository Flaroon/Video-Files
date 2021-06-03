using UnityEngine;

public class CameraCollisionDetection : MonoBehaviour
{
    public Transform referenceTransform;
    public float collisionOffset = 0.3f; 
    public float cameraSpeed = 15f;

    Vector3 defaultPos, directionNormalized;
    Transform parentTransform;
    float defaultDistance;

    private void Start()
    {
        defaultPos = transform.localPosition;
        directionNormalized = defaultPos.normalized;
        parentTransform = transform.parent;
        defaultDistance = Vector3.Distance(defaultPos, Vector3.zero);
    }
    private void LateUpdate()
    {
        RaycastHit hit;
        Vector3 dirTmp = parentTransform.TransformPoint(defaultPos) - referenceTransform.position;
        if (Physics.SphereCast(referenceTransform.position, collisionOffset, dirTmp, out hit, defaultDistance)){
            transform.localPosition = (directionNormalized * (hit.distance - collisionOffset));
        }
        else transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPos, Time.deltaTime * cameraSpeed);
    }
}
