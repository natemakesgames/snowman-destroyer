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
	/// Draws a line between the poleTopTransform and
    /// the ballTransform at the end of every frame. This visually
    /// represents a rope for the tetherball.
	/// </summary>
    public class Rope : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer lineRend;
        [SerializeField]
        private Transform poleTopTransform;
        [SerializeField]
        private Transform ballTransform;

        private void LateUpdate()
        {
            lineRend.SetPosition(0, poleTopTransform.position);
            lineRend.SetPosition(1, ballTransform.position);
        }
    }
}