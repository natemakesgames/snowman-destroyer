using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NateVR
{
    public class WheelThruster : MonoBehaviour
    {
        [SerializeField]
        private float velocityMult; 

        [SerializeField]
        private Transform handleTransform;
        [SerializeField]
        private Transform thrusterLowPointTransform;
        [SerializeField]
        private HingeJoint hinge;

        private float dist;
        private JointMotor motor;

        void Start()
        {
            handleTransform.position = thrusterLowPointTransform.position;
        }

        void Update()
        {
            motor = hinge.motor;
            dist = Vector3.Distance(handleTransform.position, thrusterLowPointTransform.position) - .02f;
            if(dist < 0) 
            { 
                dist = 0;
                motor.force = .08f;
            }
            else
            {
                motor.force = 200f;
            }
            motor.targetVelocity = dist * velocityMult;
            hinge.motor = motor;
        }
    }
}