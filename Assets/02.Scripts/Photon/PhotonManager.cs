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

    // �� ��Ͽ� ���� �����͸� �����ϱ� ���� ��ųʸ� �ڷ���
    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    // �� ����� ǥ���� ������
    private GameObject roomItemPrefab;
    // RoomItem �������� �߰��� ScrollContent
    public Transform scrollContent;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.GameVersion = gameVersion;

        // ���� �������� �������� �ʴ� ���� Ƚ��
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

    // Connect ��ư Ŭ���� ����Ǵ� �Լ�
    public void Connect()
    {
        panel.SetActive(false);
        connectText.gameObject.SetActive(true);
        roomPanel.SetActive(false);

        // �̹� ����Ǿ������� Lobby�� ����
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Photon already Connected!");
            PhotonNetwork.JoinLobby();
        }
        else // ���� �õ�
        {
            Debug.Log("Photon not Connected! Try Connect.." );
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings(); // ����õ� �Լ�
        }
    }

    #region --------------------------------- Photon Callbacks
    // ���� ������ �����ϸ� ���ϸ��� ȣ��Ǵ� �Լ�
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected!");
        Debug.Log("Called : OnConnectedToMaster()");
        // �κ� ���Դ��� Ȯ��
        // ������ �κ� �ڵ����� �����Ű�� �ʱ⶧���� false�� ���
        Debug.Log($"PhotonNetwork.InLobby = {PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby();
    }

    // �κ� ���忡 ���������� ȣ��
    // �� ��Ͽ� ���� ���� ������ �ʿ������� JoinLobby���� �ٷ� ���� �����ؼ� ����.
    public override void OnJoinedLobby()
    {
        // �κ� UI ���̱�
        panel.SetActive(false);
        roomPanel.SetActive(true);
        connectText.gameObject.SetActive(false);
        gameName.gameObject.SetActive(false);

        Debug.Log("Called OnJoinedLobby()");
        Debug.Log($"PhotonNetWork.InLobby = {PhotonNetwork.InLobby}");
        //PhotonNetwork.JoinRandomRoom();
    }

    // �� ����� �����ϴ� �ݹ��Լ�
    /// <summary>
    /// �� ������ ��ȭ�� �߻��� ������ �ݹ� �Լ��� ȣ��ȴ�.
    /// �ٸ� ������ �뿡 ���� ������ �Ѿ�´�. ���� �������δ� RemovedFromList �Ӽ����� Ȯ���� �� �ִ�.
    /// ���� �� ����� Dictionary Ÿ���� �ڷ������� ����
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ������ RoomItem �������� ������ �ӽú���
        GameObject tempRoom = null;

        foreach(var room in roomList)
        {
            // ���� ������ ���
            if(room.RemovedFromList == true)
            {
                // ��ųʸ����� �� �̸����� �˻��� ����� RoomItem �������� ����
                rooms.TryGetValue(room.Name, out tempRoom);

                // RoomItem������ ����
                Destroy(tempRoom);

                // ��ųʸ����� �ش� �� �̸��� �����͸� ����
                rooms.Remove(room.Name);
            }
            else // �� ������ ����� ���
            {
                // �� �̸��� ��ųʸ��� ���� ��� ���� �߰�
                if(rooms.ContainsKey(room.Name) == false)
                {
                    // RoomPanel �������� scrollContent������ ����
                    GameObject roomPrefab = Instantiate(roomItemPrefab);
                    roomPrefab.transform.SetParent(scrollContent);
                    roomPrefab.transform.localScale = new Vector3(1, 1, 1);
                    // �� ������ ǥ���ϱ� ���� RoomInfo���� ����
                    roomPrefab.GetComponent<RoomData>().RoomInfo = room;

                    rooms.Add(room.Name, roomPrefab);
                }
                else //�� �̸��� ��ųʸ��� �ִ� ��쿡 �� ���� ����
                {
                    rooms.TryGetValue(room.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = room;
                }
            }
            // room.ToString();
            //Debug.Log($"Room = {room.Name} ({room.PlayerCount}/{room.MaxPlayers})");

        }
    }
    // ���� �� ���� ���н� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRandom Failed {returnCode}:{message}");

        // �� ���� �����ϱ�

        //// �� �Ӽ�����
        //RoomOptions ro = new RoomOptions();
        //ro.MaxPlayers = maxPlayerPerRoom; // �뿡 ������ �� �ִ� �ִ� ������ ��
        //ro.IsOpen = true;                 // ���� ���� ����
        //ro.IsVisible = true;              // �κ񿡼� �� ��Ͽ� �����ų�� ����
        //
        //// �� ����
        //PhotonNetwork.CreateRoom("My Room", ro);
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = maxPlayerPerRoom;
        ro.IsOpen = true;
        ro.IsVisible = true;
        PhotonNetwork.CreateRoom(SetRoomName(), ro);
    }

    // ���� ������ �Ϸ�� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnCreatedRoom()
    {
        Debug.Log("Create Room");
        Debug.Log($"Room name = {PhotonNetwork.CurrentRoom.Name}");
    }

    // �뿡 ������ �� ȣ��Ǵ� �ݹ��Լ�
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
        // ���� �� ����
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
