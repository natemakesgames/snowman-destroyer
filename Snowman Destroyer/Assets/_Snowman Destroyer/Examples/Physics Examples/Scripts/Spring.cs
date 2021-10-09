using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NateVR
{
    /// <summary>
    /// Updates the model of the spring at the end of every frame
    /// based on startTransform and endTransform's positions.
    /// </summary>
    public class Spring : MonoBehaviour
    {
        [SerializeField]
        private Transform startTransform;
        [SerializeField]
        private Transform endTransform;
        [SerializeField]
        private float distanceMult; //should be roughly half of spring model height

        private Vector3 initialScale;

        private void Start()
        {
            initialScale = transform.localScale;
        }

        private void LateUpdate()
        {
            UpdateTransformScale();
        }

        private void UpdateTransformScale()
        {
            float distance = Vector3.Distance(startTransform.position, endTransform.position);
            transform.localScale = new Vector3(initialScale.x, distance * distanceMult, initialScale.z);

            Vector3 middlePoint = (startTransform.position + endTransform.position) / 2f;
            transform.position = middlePoint;

            Vector3 rotationDirectionVector = endTransform.position - startTransform.position;
            transform.up = rotationDirectionVector;


            Quaternion rotationDirection = transform.localRotation;
            rotationDirection = new Quaternion(rotationDirection.x, 0, rotationDirection.z, rotationDirection.w);
            transform.localRotation = rotationDirection;
        }
    }
}