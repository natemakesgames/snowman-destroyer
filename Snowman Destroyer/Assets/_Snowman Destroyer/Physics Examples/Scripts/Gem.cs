/*--------------------------------------
	Nate Danziger Proprietary

	© 2021

	Not licensed for 3rd party use
---------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

namespace NateVR
{
    /// <summary>
	/// Enables a Gem's InteractionBehaviour
    /// when the OnJointBreak event occurs due to the gem 
    /// being "picked" off of its block.
	/// </summary>
    public class Gem : MonoBehaviour
    {
        private InteractionBehaviour interactionBehaviour;

        private void Awake()
        {
            interactionBehaviour = GetComponent<InteractionBehaviour>();
        }

        private void OnJointBreak(float breakForce)
        {
            interactionBehaviour.enabled = true;
        }
    }
}
