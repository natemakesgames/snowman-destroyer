using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;
public class Gem : MonoBehaviour
{
    void OnJointBreak(float breakForce)
    {
        GetComponent<InteractionBehaviour>().enabled = true;
    }
}
