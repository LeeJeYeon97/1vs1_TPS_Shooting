using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform playerSpawnPos;
    public List<Transform> spawnPosList;

    public GameObject hud;
    public GameObject crossHair;
    public GameObject roomInfo;
    public Text roomName;
    public Text connectInfo;
    public Button exitBtn;

    private bool gameStart = false;

    public static Action DieInfo;
    public static Action StartInfo;
    public Text startText;
    
    private void Awake()
    {
        playerSpawnPos = GameObject.Find("PlayerSpawnList").transform;
        playerSpawnPos.GetComponentsInChildren<Transform>(spawnPosList);
        if (spawnPosList[0].name == playerSpawnPos.name)
        {
            spawnPosList.RemoveAt(0);
        }
        SetRoomInfo();

        exitBtn.onClick.AddListener(() => OnExitClick());

        if(!gameStart)
            Cursor.lockState = CursorLockMode.None;

        DieInfo += DieInfoUI;
        StartInfo += StartGame;
    }
    private void Update()
    {
        Room room = PhotonNetwork.CurrentRoom;
        if (room == null) return;
        if (room.PlayerCount == 1 && !gameStart)
        {
            // ���� ��ƾ
            gameStart = true;
            startText.gameObject.SetActive(true);
            StartCoroutine(CoStartGame());
        }
    }
    private void PlayerSpawn()
    {
        int idx = UnityEngine.Random.Range(0,spawnPosList.Count);
        PhotonNetwork.Instantiate("Soldier", spawnPosList[idx].position, Quaternion.identity, 0);
    }
    private void StartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        roomInfo.SetActive(false);
        startText.gameObject.SetActive(false);
        hud.SetActive(true);
        crossHair.SetActive(true);
        GetComponent<ItemRandomSpawn>().GameStartSpawn();
    }
    private void DieInfoUI()
    {
        Cursor.lockState = CursorLockMode.None;
        roomInfo.SetActive(true);
        hud.SetActive(false);
        crossHair.SetActive(false);
    }
    void SetRoomInfo()
    {
        Room room = PhotonNetwork.CurrentRoom;
        roomName.text = room.Name;
        connectInfo.text = $"({room.PlayerCount}/{room.MaxPlayers})";
    }
    private void OnExitClick()
    {
        PhotonNetwork.LeaveRoom();
    }
    // ���� �뿡�� ���������� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Launcher");
    }

    // ������ ���ο� ��Ʈ��ũ ������ ���������� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetRoomInfo();
    }

    IEnumerator CoStartGame()
    {
        yield return new WaitForSeconds(5);
        StartGame();
        PlayerSpawn();
    }
    // �뿡�� ��Ʈ��ũ ������ ���������� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetRoomInfo();
    }
}
