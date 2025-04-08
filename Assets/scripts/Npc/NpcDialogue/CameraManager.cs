using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public float focusSpeed = 5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFocusing;

    private void Awake() => Instance = this;

    public void FocusOn(Vector3 targetPosition)
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        StartCoroutine(FocusCoroutine(targetPosition));
    }

    private IEnumerator FocusCoroutine(Vector3 target)
    {
        isFocusing = true;
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, target, focusSpeed * Time.deltaTime);
            transform.LookAt(target);
            yield return null;
        }
    }

    public void ReturnToPlayer()
    {
        StartCoroutine(ReturnCoroutine());
    }

    private IEnumerator ReturnCoroutine()
    {
        while (Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, originalPosition, focusSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, focusSpeed * Time.deltaTime);
            yield return null;
        }
        isFocusing = false;
    }
}