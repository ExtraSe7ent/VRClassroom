using UnityEngine;
using Photon.VR;

public class SceneConnector : MonoBehaviour
{
    public Transform myHead;
    public Transform myLeftHand;
    public Transform myRightHand;

    void Start()
    {
        PhotonVRManager manager = FindObjectOfType<PhotonVRManager>();

        if (manager != null)
        {
            manager.Head = myHead;
            manager.LeftHand = myLeftHand;
            manager.RightHand = myRightHand;

            manager.transform.position = transform.position;
            
            Debug.Log("PhotonVRManager has been connected to the new case!");
        }
    }
}