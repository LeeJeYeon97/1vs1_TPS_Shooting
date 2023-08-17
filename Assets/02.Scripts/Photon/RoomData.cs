using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    private RoomInfo _roomInfo;

    public Text roomInfoText;
    public Button button;

    private PhotonManager photonManager;
    public RoomInfo RoomInfo
    {
        get { return _roomInfo; }
        set
        {
            _roomInfo = value;
            roomInfoText.text = $"{_roomInfo.Name} ({_roomInfo.PlayerCount}/{_roomInfo.MaxPlayers})";
            //버튼 클릭 이벤트에 함수 연결
            button.onClick.AddListener(() => OnEnterRoom(_roomInfo.Name));
        }
    }
    private void Awake()
    {
        photonManager = GameObject.Find("PhotonManager").GetComponent<PhotonManager>();
    }
    void OnEnterRoom(string roomName)
    {
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = photonManager.maxPlayerPerRoom;
        ro.IsOpen = true;
        ro.IsVisible = true;

        PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
    }
}
