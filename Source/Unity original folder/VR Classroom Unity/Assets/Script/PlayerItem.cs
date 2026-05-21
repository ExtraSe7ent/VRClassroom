using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using Photon.Pun;
using Photon.Realtime;

public class PlayerItem : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text playerNameText;
    public Button kickButton;       
    private Player _targetPlayer;  

    public void Setup(Player player)
    {
        _targetPlayer = player;
        
        if (string.IsNullOrEmpty(player.NickName))
            playerNameText.text = $"Player {player.ActorNumber}";
        else
            playerNameText.text = player.NickName;

        if (PhotonNetwork.IsMasterClient && !player.IsLocal)
        {
            kickButton.gameObject.SetActive(true);

            kickButton.onClick.RemoveAllListeners(); 
            kickButton.onClick.AddListener(OnKickButtonClicked);
        }
        else
        {
            kickButton.gameObject.SetActive(false);
        }
    }

    void OnKickButtonClicked()
    {
        if (BanManager.Instance != null)
        {
            Debug.Log($"Giáo viên đang kick: {_targetPlayer.NickName}");
            BanManager.Instance.BanAndKickPlayer(_targetPlayer);
        }
        else
        {
            Debug.LogError("Lỗi: Không tìm thấy BanManager trong Scene!");
        }
    }
}