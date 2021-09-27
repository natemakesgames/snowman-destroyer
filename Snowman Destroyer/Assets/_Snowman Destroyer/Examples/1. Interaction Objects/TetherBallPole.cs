using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherBallPole : MonoBehaviour
{
    public Transform poleTopTransform;
    void LateUpdate()
    {
        transform.position = poleTopTransform.position;
        transform.localRotation = Quaternion.identity;
    }
}
