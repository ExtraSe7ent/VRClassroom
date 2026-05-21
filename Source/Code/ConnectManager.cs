using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Photon.VR;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public TMP_InputField nameInput;
    public TMP_InputField roomInput;
    public TMP_Text feedbackText;
    
    public string classroomSceneName = "Classroom"; 

    void Start()
    {
        feedbackText.text = "Connecting to server...";
        
        if(!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        feedbackText.text = "Connected! Please enter information.";
        PhotonNetwork.AutomaticallySyncScene = true; 
    }


    public void OnClick_CreateRoom()
    {
        if (!ValidateInput()) return;

        SetMyName(); 

        feedbackText.text = "Creating room...";
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;
        PhotonNetwork.CreateRoom(roomInput.text, roomOptions);
    }

    public void OnClick_JoinRoom()
    {
        if (!ValidateInput()) return;

        SetMyName();

        feedbackText.text = "Joining room...";
        PhotonNetwork.JoinRoom(roomInput.text);
    }

    void SetMyName()
    {
        PhotonNetwork.NickName = nameInput.text;
        
        PhotonVRManager.SetUsername(nameInput.text); 
        
        PlayerPrefs.SetString("PlayerName", nameInput.text);
    }

    bool ValidateInput()
    {
        if (string.IsNullOrEmpty(nameInput.text)) { feedbackText.text = "Missing name!"; return false; }
        if (string.IsNullOrEmpty(roomInput.text)) { feedbackText.text = "Missing ID!"; return false; }
        return true;
    }

    public override void OnJoinedRoom()
    {
        feedbackText.text = "Success! Entering room";
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(classroomSceneName);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        feedbackText.text = "The room already exists! Please choose a different ID.";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        feedbackText.text = "This room was not found.";
    }
}