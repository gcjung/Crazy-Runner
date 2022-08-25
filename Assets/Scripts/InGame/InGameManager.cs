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
    public Text[] testText = new Text[2]; // 테스트
    public const int MAX_PLAYER_NUM = 5;

    private GameObject gameCharacter;
    //private string[] characters = new string[27]
    //{"Astronaut", "BasketballPlayer", "Builder", "Businessman", "Casual", "Chef", "Clown", "Cowboy", "Cyclist", "Diver",
    //"Doctor", "Dummy", "Farmer", "FireFighter", "Hike", "Knight", "Lumberjack", "Ninja", "Pirate", "Police",
    //"Prehistoric", "Skater", "Soccser", "Soilder", "Swiming", "Wizard", "Zombie"};

    public Transform[] StartPoint;

    #region 룸, 플레이어 프로퍼티
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
    #endregion 룸, 플레이어 프로퍼티

    private void Awake()
    {
        if (FindObjectOfType<GameManager>() == null)        // 스타트씬부터 시작을 안한경우 테스트
            return;

        PhotonNetwork.AutomaticallySyncScene = false;               // 방장이 씬을 이동하면 모두 같이 이동X

        if (PhotonNetwork.MasterClient == PhotonNetwork.LocalPlayer)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

    }
    // 임시 테스트
    public override void OnConnectedToMaster()      // 서버접속시 실행되는 콜백함수 (방에서 나갔을때 이 함수부터 실행됨)
    {
        //Debug.Log("OnConnectedToMaster 실행");
        PhotonNetwork.JoinLobby();                  // 서버접속시 바로 디폴트로비로 접속
    }
    public override void OnJoinedLobby()            // 로비접속시 실행되는 콜백함수
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
    public override void OnJoinedRoom()         // 방을 성공적으로 참가하면 실행되는 콜백함수
    {
        gameCharacter = PhotonNetwork.Instantiate("Person", StartPoint[0].position, Quaternion.Euler(0, 180, 0)); //Quaternion.Euler(0,180,0)
        gameCharacter.transform.GetChild(0).GetChild(9).gameObject.gameObject.SetActive(true);

    }
    // 임시 테스트
    private void Start()
    {
        if (FindObjectOfType<GameManager>() == null)        // 스타트씬부터 시작을 안한경우 테스트
        {
            PhotonNetwork.ConnectUsingSettings();

            return;
        }

        // 테스트
        // testText[0].text = PhotonNetwork.LocalPlayer.ActorNumber.ToString() + " : " + characters[GetPlayerProperties("Class")];
        Debug.Log("캐릭터 인덱스 : " + GetPlayerProperties("Character"));
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            if (GetRoomProperties(i) == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                //Debug.Log("position : " + StartPoint[i].position);

                gameCharacter = PhotonNetwork.Instantiate("Person", StartPoint[i].position, Quaternion.Euler(0, 180, 0)); //Quaternion.Euler(0,180,0)
                gameCharacter.transform.GetChild(0).GetChild(GetPlayerProperties("Character")).gameObject.gameObject.SetActive(true);
                //Debug.Log("뭘까용 ? : " + gameCharacter.transform.GetChild(0).GetChild(GetPlayerProperties("Character")));
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
            Debug.Log("지금 방장은 : " + PhotonNetwork.MasterClient.NickName + " 입니다.");
        }
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        //    {
        //        print("닉네임 : " + PhotonNetwork.PlayerList[i].NickName + ", 클라스 : " + characters[(int)PhotonNetwork.PlayerList[i].CustomProperties["Character"]]);
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
