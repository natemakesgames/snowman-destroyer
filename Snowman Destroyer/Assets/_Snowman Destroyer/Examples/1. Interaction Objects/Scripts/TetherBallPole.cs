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
	/// Ensures that the TetherBallPole is always
    /// rotated vertically no matter how the "pole top" is oriented.
    /// This is designed to ensure the smoothest possible movement
    /// while maintaining the pole's orientation.
	/// </summary>
    public class TetherBallPole : MonoBehaviour
    {
        [SerializeField]
        private Transform poleTopTransform;

        private void LateUpdate()
        {
            transform.position = poleTopTransform.position;
            transform.localRotation = Quaternion.identity;
        }
    }
}