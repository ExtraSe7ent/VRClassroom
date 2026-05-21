using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class AnimateHandOnInput : MonoBehaviour 
{
    public InputActionProperty PinchAnimationAction;
    public InputActionProperty GripAnimationAction;
    public Animator handAnimator;

    private PhotonView myPhotonView;

    void Start()
    {
        myPhotonView = GetComponentInParent<PhotonView>();
    }

    void Update()
    {
        if (myPhotonView != null && !myPhotonView.IsMine) return;

        if (PinchAnimationAction.action == null || GripAnimationAction.action == null || handAnimator == null)
        {
            return; 
        }

        float triggerValue = PinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);

        float gripvalue = GripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripvalue);
    }
}