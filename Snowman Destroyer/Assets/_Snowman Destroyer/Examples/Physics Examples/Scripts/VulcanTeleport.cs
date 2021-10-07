﻿/*--------------------------------------
	Nate Danziger Proprietary

	© 2021

	Not licensed for 3rd party use
---------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Attachments;
using DG.Tweening;

/// <summary>
/// Teleports the player when they do a Vulkan salute
/// </summary>
public class VulcanTeleport : MonoBehaviour
{
    [SerializeField]
    private float minSaluteDurationToTeleport;
    [SerializeField]
    private float waitBeforeTeleport;
    [SerializeField]
    private float waitBeforeOrbSpawn;

    [SerializeField]
    private float orbEnteranceDur;
    [SerializeField]
    private float orbExitDur;
    [SerializeField]
    private Ease orbScaleEase;

    [SerializeField]
    private float circleEnteranceDelay;
    [SerializeField]
    private float circleEnteranceDur;
    [SerializeField]
    private float circleExitDur;
    [SerializeField]
    private Ease circleScaleEase;

    // Refers to the distance between middle and ring finger tips
    [SerializeField]
    private float minDistanceToTrigger;

    // Refers to the distance between side finger tips.
    [SerializeField]
    private float maxAverageDistanceToTrigger;

    //[SerializeField]
    //private float fingerToPalmDistanceForFist;

    //[SerializeField]
    //private float waitBeforeJudingFist;

    [SerializeField]
	private AttachmentHand leftHand;
    [SerializeField]
    private Transform orbTransform;
    [SerializeField]
    private Transform circleTransform;
    [SerializeField]
    private Transform leapRigTransform;
    [SerializeField]
    private Light orbLight;
    [SerializeField]
    private LineRenderer line;
    [SerializeField]
    private LayerMask floorLayer;

    private Vector3 originalOrbScale;
    private Vector3 originalCircleScale;
    private Vector3 circleBufferY = new Vector3(0, .001f, 0);
    private float originalLightIntensity;
    private bool currentlyTeleporting;
    private Transform camTransform;

    private void Start()
    {
        originalOrbScale = orbTransform.localScale;
        originalCircleScale = circleTransform.localScale;
        originalLightIntensity = orbLight.intensity;
        camTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (!currentlyTeleporting && CurrentlySaluting())
        {
            StartCoroutine(Teleport());
        }
    }

    IEnumerator Teleport()
    {
        currentlyTeleporting = true;
        yield return new WaitForSeconds(waitBeforeOrbSpawn);

        StartCoroutine("UpdateOrbTransformEveryFrame");

        orbTransform.gameObject.SetActive(true);
        orbTransform.localScale = Vector3.zero;

        circleTransform.gameObject.SetActive(true);
        circleTransform.localScale = Vector3.zero;

        orbLight.intensity = 0;

        DOTween.Kill(orbTransform);
        orbTransform.DOScale(originalOrbScale, orbEnteranceDur).SetEase(orbScaleEase);

        DOTween.Kill(circleTransform);
        circleTransform.DOScale(originalCircleScale, circleEnteranceDur).SetEase(circleScaleEase).SetDelay(circleEnteranceDelay);

        DOTween.Kill(orbLight);
        orbLight.DOIntensity(originalLightIntensity, orbEnteranceDur).SetEase(orbScaleEase);

        float saluteDuration = 0;
        while (CurrentlySaluting())
        {
            yield return null;
            saluteDuration += Time.deltaTime;
            DrawRay();
        }

        line.positionCount = 0;

        DOTween.Kill(orbTransform);
        orbTransform.DOScale(Vector3.zero, orbExitDur);

        DOTween.Kill(orbLight);
        orbLight.DOIntensity(0, orbExitDur);

        yield return new WaitForSeconds(orbExitDur + waitBeforeTeleport);  // + waitBeforeJudgingFist
        StopCoroutine("UpdateOrbTransformEveryFrame");
        orbTransform.gameObject.SetActive(false);
        currentlyTeleporting = false;

        if (saluteDuration > minSaluteDurationToTeleport && leftHand.isTracked && !CurrentlySaluting()) // && CurrentlyFist()
        {
            MovePlayer();
        }

        circleTransform.DOScale(Vector3.zero, orbExitDur).OnComplete(DisableCircle);

    }

    private void DisableCircle()
    {
        circleTransform.gameObject.SetActive(false);
    }

    private void MovePlayer()
    {
        Vector3 diff = hit.point - camTransform.position;
        leapRigTransform.position += new Vector3(diff.x, leapRigTransform.position.y, diff.z);
    }

    private RaycastHit hit;
    private void DrawRay()
    {
        if(Physics.Raycast(orbTransform.position, -orbTransform.up, out hit, 1000f, floorLayer))
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

    /*
    private bool CurrentlyFist()
    {
        float averageDist = 0;
        averageDist += DistanceToPalm(leftHand.pinkyTip);
        averageDist += DistanceToPalm(leftHand.ringTip);
        averageDist += DistanceToPalm(leftHand.middleTip);
        averageDist += DistanceToPalm(leftHand.indexTip);
        averageDist /= 4f;

        if(averageDist < fingerToPalmDistanceForFist)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    */
    private bool CurrentlySaluting()
    {
        if (MiddleToRingTipDistance() > minDistanceToTrigger)
        {
            if(AverageDistanceOfSideFingerTips() < maxAverageDistanceToTrigger)
            {
                return true;
            }
        }
        return false;
    }

    private float DistanceToPalm(AttachmentPointBehaviour handPoint)
    {
        return Vector3.Distance(handPoint.transform.position, leftHand.palm.transform.position);
    }

    private float MiddleToRingTipDistance()
    {
        return Vector3.Distance(leftHand.middleTip.transform.position, leftHand.ringTip.transform.position);
    }

    private float dist1;
    private float dist2;
    private float AverageDistanceOfSideFingerTips()
    {
        dist1 = Vector3.Distance(leftHand.pinkyTip.transform.position, leftHand.ringTip.transform.position);
        dist2 = Vector3.Distance(leftHand.middleTip.transform.position, leftHand.indexTip.transform.position);

        // Weighted average, which cares more about the fingers that usually track more consistently
        return Mathf.Lerp(dist1, dist2, .75f); 
    }

    private void UpdateOrbTransform()
    {
        orbTransform.position = OrbPosition();
        orbTransform.rotation = OrbRotation();
    }

    private Vector3 OrbPosition()
    {
        return Vector3.Lerp(
            leftHand.middleDistalJoint.transform.position, 
            leftHand.ringDistalJoint.transform.position, 
            .5f
            );
    }

    private Quaternion OrbRotation()
    {
        return leftHand.palm.transform.rotation;
    }

    IEnumerator UpdateOrbTransformEveryFrame()
    {
        while (true)
        {
            UpdateOrbTransform();
            yield return null;
        }
    }

}