using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; 
using System.Collections.Generic;
using System; 

public class BanManager : MonoBehaviourPunCallbacks
{
    public static BanManager Instance;
    private const string BAN_KEY = "BannedUsersString"; 

    void Awake()
    {
        Instance = this;
        PhotonNetwork.EnableCloseConnection = true;
    }


    public void BanAndKickPlayer(Player playerToKick)
    {
        if (!PhotonNetwork.IsMasterClient) return; 

        long banEndTime = DateTime.UtcNow.AddMinutes(15).ToBinary();
        
        string newBanEntry = $"{playerToKick.UserId}|{banEndTime}"; 

        string currentBanString = "";
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(BAN_KEY))
        {
            currentBanString = (string)PhotonNetwork.CurrentRoom.CustomProperties[BAN_KEY];
        }

        if (string.IsNullOrEmpty(currentBanString))
        {
            currentBanString = newBanEntry;
        }
        else
        {
            currentBanString += ";" + newBanEntry;
        }

        ExitGames.Client.Photon.Hashtable newProps = new ExitGames.Client.Photon.Hashtable();
        newProps[BAN_KEY] = currentBanString;
        PhotonNetwork.CurrentRoom.SetCustomProperties(newProps);

        Debug.Log($"[ADMIN] Banned player {playerToKick.NickName} for 15 minutes.");

        PhotonNetwork.CloseConnection(playerToKick);
    }


    public override void OnJoinedRoom()
    {
        CheckIfBanned();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(BAN_KEY))
        {
            CheckIfBanned();
        }
    }

    void CheckIfBanned()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(BAN_KEY))
        {
            string banString = (string)PhotonNetwork.CurrentRoom.CustomProperties[BAN_KEY];
            string myUserId = PhotonNetwork.LocalPlayer.UserId;

            string[] banEntries = banString.Split(';');

            foreach (string entry in banEntries)
            {
                if (string.IsNullOrEmpty(entry)) continue;

                string[] parts = entry.Split('|');
                if (parts.Length < 2) continue;

                string bannedID = parts[0];
                
                if (long.TryParse(parts[1], out long bannedTimeBinary))
                {
                    if (bannedID == myUserId)
                    {
                        DateTime bannedTime = DateTime.FromBinary(bannedTimeBinary);
                        
                        if (DateTime.UtcNow < bannedTime)
                        {
                            double minutesLeft = (bannedTime - DateTime.UtcNow).TotalMinutes;
                            Debug.LogError($"YOU ARE BANNED! Time remaining: {minutesLeft:F2} minutes.");
                            
                            PhotonNetwork.LeaveRoom(); 
                            return; 
                        }
                    }
                }
            }
        }
    }
}