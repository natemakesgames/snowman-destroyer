using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;
using Leap.Unity.Attachments;
using DG.Tweening;

namespace NateVR
{
    /// <summary>
    /// Starts teleporting the player when they do a Vulcan salute with thumb down.
    /// Moves the player when they put their thumb up.
    /// 
    /// This script also controls all special effects related to teleportation,
    /// including the "orb", "circle", "line", pink point "light" attached to hand, 
    /// "galaxy" (circular effect near orb),
    /// and "pulse wave" (effect on camera when you teleport).
    /// 
    /// When you start saluting with your thumb down, the "orb", "light", and "galaxy" effects 
    /// tween their scale up from zero and are attatched to your "teleportation hand".
    /// The "line" and "circle" effects update during this time, and appear where your hand is pointing.
    /// 
    /// If your hand loses tracking or stops saluting during this time, 
    /// the special effects disappear, and you do not teleport.
    /// However, if you raise your thumb up while saluting, you will teleport.
    /// This plays the teleportation animation, which consists of the orb moving to the circle,
    /// the galaxy shrinking, and the "pulse wave" effect playing in front of the player's eyes.
    /// </summary>
    public class VulcanTeleportation : MonoBehaviour
    {
        [SerializeField]
        private AttachmentHand teleportationHand;
        [SerializeField]
        private LayerMask floorLayer;

        [Header("Salute Definitions")]
        // Saluting if over this value
        [SerializeField]
        private float middleTipRingTipDistanceThreshold;

        // Saluting if under this value. 
        [Tooltip("Side finger distance is determined by the average " +
        "distance between the pinky & ring fingers, and the middle & index fingers.")]
        [SerializeField]
        private float sideFingerTipDistanceThreshold;

        // Thumb is "close to palm" if under this value
        [SerializeField]
        private float thumbToPalmThreshold;

        [SerializeField]
        private float minSaluteDurationToTeleport;


        [Header("Orb Settings")]
        [SerializeField]
        private float waitBeforeOrbSpawn;
        [SerializeField]
        private float orbEntranceDur;
        [SerializeField]
        private float orbExitDur;
        [SerializeField]
        private float orbTravelDur;
        [SerializeField]
        private Ease orbScaleEase;
        [SerializeField]
        private Ease orbTravelEase;


        [Header("Circle Settings")]
        [SerializeField]
        private float circleEntranceDelay;
        [SerializeField]
        private float circleEnteranceDur;
        [SerializeField]
        private float circleExitDur;
        [SerializeField]
        private Ease circleScaleEase;


        [Header("Light Settings Settings")]
        [SerializeField]
        private float lightEntranceDelay;
        [SerializeField]
        private float lightEntranceDur;


        [Header("Scene Objects")]
        [SerializeField]
        private Transform leapRigTransform;
        [SerializeField]
        private Transform orbTransform;
        [SerializeField]
        private Transform circleTransform;
        [SerializeField]
        private Transform galaxyTransform;
        [SerializeField]
        private Light orbLight;
        [SerializeField]
        private LineRenderer line;
        [SerializeField]
        private ParticleSystem pulseWave;


        // Not visible in inspector
        private Vector3 originalOrbScale;
        private Vector3 originalCircleScale;
        private Vector3 originalGalaxyScale;
        private Vector3 circleBufferY = new Vector3(0, .001f, 0);
        private float originalLightIntensity;
        private bool currentlyTeleporting;
        private Transform camTransform;

        public InputDevice headsetDevice;
        private void Start()
        {
            // Cache original scales so that you can edit them in scene view
            originalOrbScale = orbTransform.localScale;
            originalCircleScale = circleTransform.localScale;
            originalLightIntensity = orbLight.intensity;
            originalGalaxyScale = galaxyTransform.localScale;

            camTransform = Camera.main.transform;

            var allHeadsets = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, allHeadsets);
            headsetDevice = allHeadsets[0];
        }

        private void Update()
        {
            if (!currentlyTeleporting && CurrentlySaluting() && !ThumbIsCloseToPalm())
            {
                StartCoroutine(Teleport());
            }
        }

        IEnumerator Teleport()
        {
            currentlyTeleporting = true;
            yield return new WaitForSeconds(waitBeforeOrbSpawn);

            StartCoroutine("UpdateOrbTransformEveryFrame");

            // Enable orb, circle, light, and galaxy--but they start at a scale of zero
            orbTransform.gameObject.SetActive(true);
            orbTransform.localScale = Vector3.zero;
            circleTransform.gameObject.SetActive(true);
            circleTransform.localScale = Vector3.zero;
            orbLight.intensity = 0;
            galaxyTransform.localScale = originalGalaxyScale;

            // Tween orb, circle, and light intensity up to their original scales
            DOTween.Kill(orbTransform);
            orbTransform.DOScale(originalOrbScale, orbEntranceDur).SetEase(orbScaleEase);
            DOTween.Kill(circleTransform);
            circleTransform.DOScale(originalCircleScale, circleEnteranceDur).SetEase(circleScaleEase).SetDelay(circleEntranceDelay);
            DOTween.Kill(orbLight);
            orbLight.DOIntensity(originalLightIntensity, lightEntranceDur).SetDelay(lightEntranceDelay);

            // While saluting with thumb down, draw line and move circle
            float saluteDuration = 0;
            while (HandIsTracking() && CurrentlySaluting() && !ThumbIsCloseToPalm() )
            {
                yield return null;
                saluteDuration += Time.deltaTime;
                DrawLineAndMoveCircle();
            }

            // Disable line and stop moving orb with hand
            line.positionCount = 0;
            StopCoroutine("UpdateOrbTransformEveryFrame");

            // If now saluting with thumb *up*
            if (saluteDuration > minSaluteDurationToTeleport
                && HandIsTracking()
                && CurrentlySaluting()
                && ThumbIsCloseToPalm()
                )
            {
                // Move and scale orb toward circle
                DOTween.Kill(orbTransform);
                orbTransform.DOScale(originalOrbScale * 10f, orbTravelDur).SetEase(orbTravelEase);
                orbTransform.DOMove(circleTransform.position, orbTravelDur).SetEase(orbTravelEase);

                // Shrink the galaxy effect to a scale of zero
                DOTween.Kill(galaxyTransform);
                galaxyTransform.DOScale(Vector3.zero, orbTravelDur / 1.5f);

                // Play "pulse wave" effect just before moving player
                StartCoroutine(PlayPulseWaveSoon());

                yield return new WaitForSeconds(orbTravelDur + .01f);

                MovePlayer();
            }

            // Whether or not we actually teleported,
            // scale all effects down to zero
            DOTween.Kill(orbTransform);
            orbTransform.DOScale(Vector3.zero, orbExitDur).OnComplete(DisableOrb);
            DOTween.Kill(circleTransform);
            circleTransform.DOScale(Vector3.zero, orbExitDur).OnComplete(DisableCircle);
            DOTween.Kill(orbLight);
            orbLight.DOIntensity(0, orbExitDur);

            yield return new WaitForSeconds(orbExitDur);
            currentlyTeleporting = false;
        }

        private void MovePlayer()
        {
            Vector3 diff = hit.point - camTransform.position;
            leapRigTransform.position += new Vector3(diff.x, leapRigTransform.position.y, diff.z);
        }


        private RaycastHit hit;
        private void DrawLineAndMoveCircle()
        {
            if (Physics.Raycast(orbTransform.position, -orbTransform.up, out hit, 1000f, floorLayer))
            {
                line.positionCount = 2;
                line.SetPosition(0, orbTransform.position);
                line.SetPosition(1, hit.point);

                circleTransform.gameObject.SetActive(true);
                circleTransform.position = hit.point + circleBufferY;
            }
            else
            {
                line.positionCount = 0;
                circleTransform.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// This only tracks the four non-thumb fingers
        /// </summary>
        private bool CurrentlySaluting()
        {
            if (MiddleToRingTipDistance() > middleTipRingTipDistanceThreshold)
            {
                if (AverageDistanceOfSideFingerTips() < sideFingerTipDistanceThreshold)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ThumbIsCloseToPalm()
        {
            if (DistanceToPalm(teleportationHand.thumbTip) < thumbToPalmThreshold)
            {
                return true;
            }
            return false;
        }

        private float DistanceToPalm(AttachmentPointBehaviour handPoint)
        {
            return Vector3.Distance(handPoint.transform.position, teleportationHand.palm.transform.position);
        }

        private float MiddleToRingTipDistance()
        {
            return Vector3.Distance(teleportationHand.middleTip.transform.position, teleportationHand.ringTip.transform.position);
        }

        private float dist1;
        private float dist2;
        private float AverageDistanceOfSideFingerTips()
        {
            dist1 = Vector3.Distance(teleportationHand.pinkyTip.transform.position, teleportationHand.ringTip.transform.position);
            dist2 = Vector3.Distance(teleportationHand.middleTip.transform.position, teleportationHand.indexTip.transform.position);

            // Weighted average, which cares more about the fingers that usually track more consistently
            return Mathf.Lerp(dist1, dist2, .65f);
        }

        private void UpdateOrbTransform()
        {
            orbTransform.position = OrbPosition();
            orbTransform.rotation = OrbRotation();
        }

        private Vector3 OrbPosition()
        {
            return Vector3.Lerp(
                teleportationHand.middleDistalJoint.transform.position,
                teleportationHand.ringDistalJoint.transform.position,
                .5f
                );
        }

        private Quaternion OrbRotation()
        {
            return teleportationHand.palm.transform.rotation;
        }

        IEnumerator UpdateOrbTransformEveryFrame()
        {
            while (true)
            {
                UpdateOrbTransform();
                yield return null;
            }
        }

        /// <summary>
        /// This is written this way in order to play the "pulse wave"
        /// effect right before the player is moved.
        /// </summary>
        IEnumerator PlayPulseWaveSoon()
        {
            yield return new WaitForSeconds(orbTravelDur - .1f);
            pulseWave.Play();
        }

        private void DisableCircle()
        {
            circleTransform.gameObject.SetActive(false);
        }

        private void DisableOrb()
        {
            orbTransform.gameObject.SetActive(false);
        }

        /// <returns> 
        /// True when headset and "teleportation hand" are both tracking. 
        /// </returns>
        private bool HandIsTracking()
        {
            InputTrackingState trackingState;
            headsetDevice.TryGetFeatureValue(CommonUsages.trackingState, out trackingState);
            bool headsetIsTracking = trackingState == InputTrackingState.All;
            return headsetIsTracking && teleportationHand.isTracked;
        }
    }
}