                   using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring2 : MonoBehaviour
{
    public Transform baseTransform;
    public Transform lookAtTransform;

    private void Start()
    {
        
    }

    private void LateUpdate()
    {
        transform.position = baseTransform.position;
        transform.LookAt(lookAtTransform, Vector3.forward);
        transform.localScale = new Vector3(1f, 1f, Vector3.Distance(transform.position, lookAtTransform.position));
    }
}
