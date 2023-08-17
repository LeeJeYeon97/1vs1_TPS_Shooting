using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEditor;

public class PhotonManager : MonoBehaviourPunCallbacks
{

    #region Private Serializable Fields
    [SerializeField]
    public byte maxPlayerPerRoom = 4;
    #endregion

    #region Private Fields
    private string gameVersion = "1";

    public GameObject panel;
    public GameObject roomPanel;
    public Text connectText;

    #endregion

    public Text userName;
    public InputField roomNameInput;
    public Text gameName;

    // 룸 목록에 대한 데이터를 저장하기 위한 딕셔너리 자료형
    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    // 룸 목록을 표시할 프리팹
    private GameObject roomItemPrefab;
    // RoomItem 프리팹이 추가될 ScrollContent
    public Transform scrollContent;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.GameVersion = gameVersion;

        // 포톤 서버와의 데이터의 초당 전송 횟수
        Debug.Log(PhotonNetwork.SendRate);

        Cursor.lockState = CursorLockMode.Confined;
        roomItemPrefab = Resources.Load<GameObject>("RoomPanel");

    }
    void Start()
    {
        panel.SetActive(true);
        connectText.gameObject.SetActive(false);
        roomPanel.SetActive(false);
    }

    // Connect 버튼 클릭시 실행되는 함수
    public void Connect()
    {
        panel.SetActive(false);
        connectText.gameObject.SetActive(true);
        roomPanel.SetActive(false);

        // 이미 연결되어있으면 Lobby로 입장
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon already Connected!");
            PhotonNetwork.JoinLobby();
        }
        else // 연결 시도
        {
            Debug.Log("Photon not Connected! Try Connect.." );
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings(); // 연결시도 함수
        }
    }

    #region --------------------------------- Photon Callbacks
    // 포톤 서버에 접속하면 제일먼저 호출되는 함수
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!");
        Debug.Log("Called : OnConnectedToMaster()");
        // 로비에 들어왔는지 확인
        // 포톤은 로비에 자동으로 입장시키지 않기때문에 false가 출력
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby();
    }

    // 로비 입장에 성공했을시 호출
    // 룸 목록에 대한 정보 수신이 필요없을경우 JoinLobby말고 바로 룸을 생성해서 들어간다.
    public override void OnJoinedLobby()
    {
        // 로비 UI 보이기
        panel.SetActive(false);
        roomPanel.SetActive(true);
        connectText.gameObject.SetActive(false);
        gameName.gameObject.SetActive(false);

        Debug.Log("Called OnJoinedLobby()");
        Debug.Log($"PhotonNetWork.InLobby = {PhotonNetwork.InLobby}");
        //PhotonNetwork.JoinRandomRoom();
    }

    // 룸 목록을 수신하는 콜백함수
    /// <summary>
    /// 룸 정보의 변화가 발생할 때마다 콜백 함수가 호출된다.
    /// 다만 삭제된 룸에 대한 정보도 넘어온다. 룸의 삭제여부는 RemovedFromList 속성으로 확인할 수 있다.
    /// 따라서 룸 목록을 Dictionary 타입의 자료형으로 관리
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 삭제된 RoomItem 프리팹을 저장할 임시변수
        GameObject tempRoom = null;

        foreach(var room in roomList)
        {
            // 룸이 삭제된 경우
            if(room.RemovedFromList == true)
            {
                // 딕셔너리에서 룸 이름으로 검색해 저장된 RoomItem 프리팹을 추출
                rooms.TryGetValue(room.Name, out tempRoom);

                // RoomItem프리팹 삭제
                Destroy(tempRoom);

                // 딕셔너리에서 해당 룸 이름의 데이터를 삭제
                rooms.Remove(room.Name);
            }
            else // 룸 정보가 변경된 경우
            {
                // 룸 이름이 딕셔너리에 없는 경우 새로 추가
                if(rooms.ContainsKey(room.Name) == false)
                {
                    // RoomPanel 프리팹을 scrollContent하위에 생성
                    GameObject roomPrefab = Instantiate(roomItemPrefab);
                    roomPrefab.transform.SetParent(scrollContent);
                    roomPrefab.transform.localScale = new Vector3(1, 1, 1);
                    // 룸 정보를 표시하기 위해 RoomInfo정보 전달
                    roomPrefab.GetComponent<RoomData>().RoomInfo = room;

                    rooms.Add(room.Name, roomPrefab);
                }
                else //룸 이름이 딕셔너리에 있는 경우에 룸 정보 수신
                {
                    rooms.TryGetValue(room.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = room;
                }
            }
            // room.ToString();
            //Debug.Log($"Room = {room.Name} ({room.PlayerCount}/{room.MaxPlayers})");

        }
    }
    // 랜덤 룸 입장 실패시 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRandom Failed {returnCode}:{message}");

        // 룸 직접 생성하기

        //// 룸 속성정의
        //RoomOptions ro = new RoomOptions();
        //ro.MaxPlayers = maxPlayerPerRoom; // 룸에 입장할 수 있는 최대 접속자 수
        //ro.IsOpen = true;                 // 룸의 오픈 여부
        //ro.IsVisible = true;              // 로비에서 룸 목록에 노출시킬지 여부
        //
        //// 룸 생성
        //PhotonNetwork.CreateRoom("My Room", ro);
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = maxPlayerPerRoom;
        ro.IsOpen = true;
        ro.IsVisible = true;
        PhotonNetwork.CreateRoom(SetRoomName(), ro);
    }

    // 룸이 생성이 완료된 후 호출되는 콜백 함수
    public override void OnCreatedRoom()
    {
        Debug.Log("Create Room");
        Debug.Log($"Room name = {PhotonNetwork.CurrentRoom.Name}");
    }

    // 룸에 입장한 후 호출되는 콜백함수
    public override void OnJoinedRoom()
    {
        Debug.Log("On Joined Room");
        Debug.Log($"PhotonNetwork.InRoom = {PhotonNetwork.InRoom}");
        Debug.Log($"Player Count = {PhotonNetwork.CurrentRoom.PlayerCount}");

        foreach(var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{player.Value.NickName} , {player.Value.ActorNumber}");
        }

        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    #endregion

    #region public Methods
    public string SetRoomName()
    {
        if(string.IsNullOrEmpty(roomNameInput.text))
        {
            roomNameInput.text = $"Room_{UnityEngine.Random.Range(1, 101)}";
        }
        return roomNameInput.text;
    }
    public void OnRandomRoomClick()
    {
        // 랜덤 룸 입장
        PhotonNetwork.JoinRandomRoom();
    }
    public void OnMakeRoom()
    {
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = maxPlayerPerRoom;
        ro.IsOpen = true;
        ro.IsVisible = true;
        
        PhotonNetwork.CreateRoom(SetRoomName(), ro);
    }
    #endregion
}
