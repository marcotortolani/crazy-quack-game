using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public float speed;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetOffset = target.position + offset;
            Vector3 smooth = Vector3.Lerp(transform.position, targetOffset, speed * Time.deltaTime);
            transform.position = smooth;
        }
    }
}