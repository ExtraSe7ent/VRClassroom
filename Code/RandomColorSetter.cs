using UnityEngine;
using Photon.Pun;
using Photon.VR.Player; 

public class RandomColorSetter : MonoBehaviourPun
{
    private void Start()
    {
        if (photonView.IsMine)
        {
            Color myRandomColor = new Color(Random.value, Random.value, Random.value);

            string colorString = JsonUtility.ToJson(myRandomColor);

            var hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("Colour", colorString);

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

            GetComponent<PhotonVRPlayer>().RefreshPlayerValues();
        }
    }
}