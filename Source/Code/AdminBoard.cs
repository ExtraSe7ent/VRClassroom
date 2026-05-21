using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AdminBoard : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public GameObject playerItemPrefab; 
    public Transform contentParent;    

    void Start()
    {
        UpdateList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateList();
    }

    void UpdateList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject item = Instantiate(playerItemPrefab, contentParent);
            
            PlayerItem script = item.GetComponent<PlayerItem>();
            if (script != null)
            {
                script.Setup(p);
            }
        }
    }
}