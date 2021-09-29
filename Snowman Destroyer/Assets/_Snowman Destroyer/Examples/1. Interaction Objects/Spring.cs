using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public Transform startTransform;
    public Transform endTransform;
    private Vector3 initialScale;
    public float distanceMult; //should be half of spring model height
    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        UpdateTransformScale();
    }

    void UpdateTransformScale()
    {
        float distance = Vector3.Distance(startTransform.position, endTransform.position);
        transform.localScale = new Vector3(initialScale.x, distance * distanceMult, initialScale.z);

        Vector3 middlePoint  = (startTransform.position + endTransform.position) / 2f;
        transform.position = middlePoint;

        Vector3 rotationDirection = endTransform.position - startTransform.position;
        transform.up = rotationDirection;

    }
}
