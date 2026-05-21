using UnityEngine;
using TMPro;
using Photon.Pun;

public class NameTagSync : MonoBehaviourPun
{
    public TMP_Text nameText; 

    void Start()
    {
        if (photonView != null)
        {
            string playerName = photonView.Owner.NickName;
            
            if (string.IsNullOrEmpty(playerName))
            {
                nameText.text = "Player " + photonView.Owner.ActorNumber;
            }
            else
            {
                nameText.text = playerName;
            }

            if (photonView.IsMine)
            {
                nameText.color = Color.green; 
            }
            else
            {
                nameText.color = Color.white; 
            }
        }
    }
    
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}