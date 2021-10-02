/*--------------------------------------
	Nate Danziger Proprietary

	© 2021

	Not licensed for 3rd party use
---------------------------------------*/

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

            Vector3 rotationDirection = endTransform.position - startTransform.position;
            transform.up = rotationDirection;
        }
    }
}