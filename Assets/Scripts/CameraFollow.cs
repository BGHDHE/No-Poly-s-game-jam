using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private float smoothSpeed = 1f;
    private Vector3 offset = new Vector3(0, 70f, -25f);

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        transform.position = smoothedPosition;
    }
}