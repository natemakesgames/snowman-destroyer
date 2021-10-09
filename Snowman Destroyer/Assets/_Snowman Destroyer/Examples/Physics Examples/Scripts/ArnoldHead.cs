using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NateVR
{
    public class ArnoldHead : MonoBehaviour
    {
        public Transform lookAt;

        void LateUpdate()
        {
            transform.LookAt(lookAt);
        }
    }
}