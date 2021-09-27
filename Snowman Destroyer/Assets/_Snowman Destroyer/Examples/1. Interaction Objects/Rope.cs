using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public LineRenderer lineRend;
    public Transform poleTopTransform;
    public Transform ballTransform;

    private void Update()
    {
        lineRend.SetPosition(0, poleTopTransform.position);
        lineRend.SetPosition(1, ballTransform.position);
    }
}
