using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum CHARACTER
{
    Astronaut, BasketballPlayer, Builder, Businessman, Casual, Chef, Clown, Cowboy, Cyclist, Diver,
    Doctor, Dummy, Farmer, FireFighter, Hike, Knight, Lumberjack, Ninja, Pirate, Police,
    Prehistoric, Skater, Socer, Soilder, Swiming, Wizard, Zombie
}
public class InGameManager : MonoBehaviourPunCallbacks
{
    public Text[] testText = new Text[2]; // �׽�Ʈ
    public const int MAX_PLAYER_NUM = 5;

    private GameObject gameCharacter;
    //private string[] characters = new string[27]
    //{"Astronaut", "BasketballPlayer", "Builder", "Businessman", "Casual", "Chef", "Clown", "Cowboy", "Cyclist", "Diver",
    //"Doctor", "Dummy", "Farmer", "FireFighter", "Hike", "Knight", "Lumberjack", "Ninja", "Pirate", "Police",
    //"Prehistoric", "Skater", "Soccser", "Soilder", "Swiming", "Wizard", "Zombie"};

    public Transform[] StartPoint;

    #region ��, �÷��̾� ������Ƽ
    public void SetRoomProperties(int slotIndex, int value)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { slotIndex.ToString(), value } });
    }
    public int GetRoomProperties(int slotIndex)
    {
        object value = PhotonNetwork.CurrentRoom.CustomProperties[slotIndex.ToString()];
        return (int)value;
    }
    public void SetPlayerProperties(string key, object value, Player player = null)
    {
        if (player == null) player = PhotonNetwork.LocalPlayer;
        player.SetCustomProperties(new Hashtable { { key, value } });
    }
    public int GetPlayerProperties(string key, Player player = null)
    {
        if (player == null) player = PhotonNetwork.LocalPlayer;
        return (int)player.CustomProperties[key];
    }
    #endregion ��, �÷��̾� ������Ƽ

    private void Awake()
    {
        if (FindObjectOfType<GameManager>() == null)        // ��ŸƮ������ ������ ���Ѱ�� �׽�Ʈ
            return;

        PhotonNetwork.AutomaticallySyncScene = false;               // ������ ���� �̵��ϸ� ��� ���� �̵�X

        if (PhotonNetwork.MasterClient == PhotonNetwork.LocalPlayer)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

    }
    // �ӽ� �׽�Ʈ
    public override void OnConnectedToMaster()      // �������ӽ� ����Ǵ� �ݹ��Լ� (�濡�� �������� �� �Լ����� �����)
    {
        //Debug.Log("OnConnectedToMaster ����");
        PhotonNetwork.JoinLobby();                  // �������ӽ� �ٷ� ����Ʈ�κ�� ����
    }
    public override void OnJoinedLobby()            // �κ����ӽ� ����Ǵ� �ݹ��Լ�
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
    public override void OnJoinedRoom()         // ���� ���������� �����ϸ� ����Ǵ� �ݹ��Լ�
    {
        gameCharacter = PhotonNetwork.Instantiate("Person", StartPoint[0].position, Quaternion.Euler(0, 180, 0)); //Quaternion.Euler(0,180,0)
        gameCharacter.transform.GetChild(0).GetChild(9).gameObject.gameObject.SetActive(true);

    }
    // �ӽ� �׽�Ʈ
    private void Start()
    {
        if (FindObjectOfType<GameManager>() == null)        // ��ŸƮ������ ������ ���Ѱ�� �׽�Ʈ
        {
            PhotonNetwork.ConnectUsingSettings();

            return;
        }

        // �׽�Ʈ
        // testText[0].text = PhotonNetwork.LocalPlayer.ActorNumber.ToString() + " : " + characters[GetPlayerProperties("Class")];
        Debug.Log("ĳ���� �ε��� : " + GetPlayerProperties("Character"));
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            if (GetRoomProperties(i) == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                //Debug.Log("position : " + StartPoint[i].position);

                gameCharacter = PhotonNetwork.Instantiate("Person", StartPoint[i].position, Quaternion.Euler(0, 180, 0)); //Quaternion.Euler(0,180,0)
                gameCharacter.transform.GetChild(0).GetChild(GetPlayerProperties("Character")).gameObject.gameObject.SetActive(true);
                //Debug.Log("����� ? : " + gameCharacter.transform.GetChild(0).GetChild(GetPlayerProperties("Character")));
            }
        }

    }
    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {

            for (int i = 0; i < 5; i++)
            {
                print(PhotonNetwork.CurrentRoom.CustomProperties[i.ToString()]);
            }
            Debug.Log("���� ������ : " + PhotonNetwork.MasterClient.NickName + " �Դϴ�.");
        }
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        //    {
        //        print("�г��� : " + PhotonNetwork.PlayerList[i].NickName + ", Ŭ�� : " + characters[(int)PhotonNetwork.PlayerList[i].CustomProperties["Character"]]);
        //    }
        //}
        if (Input.GetKeyDown(KeyCode.B))
        {
            for (int i = 0; i < MAX_PLAYER_NUM; i++)
            {
                Debug.Log("position : " + StartPoint[i].position);
            }
        }

    }
    public void LeaveRoomButton()
    {
        SceneManager.LoadScene(0);
    }
}
