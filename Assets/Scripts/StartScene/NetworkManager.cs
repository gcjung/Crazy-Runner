using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum USER_SlOT_STATE { CLOSE = -2, OPEN }
public enum PLAYER_STATE { NOT_READY, READY }
public enum PLAYER_CLASS { RANDOM, }
public class NetworkManager : MonoBehaviourPunCallbacks//, IOnEventCallback
{

    private const int MAX_PLAYER_NUM = 5;

    [Header("DisconnectPanel")]
    public GameObject disconnectPanel;
    public InputField nickNameInput;


    [Header("lobbyPanel"), Space(20)]
    public GameObject lobbyPanel;
    public GameObject roomSettingPanel;
    public InputField roomNameInput;
    private int maxRoomPlayer;
    public Text myNicknameText;
    //public  Text         networkStateText;

    [Header("RoomPanel"), Space(20)]
    public GameObject roomPanel;
    public GameObject[] userSlot;
    public GameObject[] characterPrefab;
    private GameObject[] playerCharacters = new GameObject[MAX_PLAYER_NUM];
    private int selectCharacterIndex;
    private int selectClassIndex;

    private Text roomName;
    private Image CharacterSelectPanel;
    private Image ClassSelectPanel;
    public ChatManager chatManager;

    //UserSlot
    private Text[] userNickname = new Text[MAX_PLAYER_NUM];
    private Image[] closeSlot = new Image[MAX_PLAYER_NUM];
    private Image[] hostTag = new Image[MAX_PLAYER_NUM];
    private Image[] readyTag = new Image[MAX_PLAYER_NUM];
    private Button startButton;
    private Image kickConfirmPanel;
    private Text kickConfirmMessage;
    private Player kickPlayer;
    private int readyPlayerNumber;

    private PhotonView photonview;
    private string gameVersion = "ver0.2";
    private SceneConvertEffect sceneEffect;

    private void Awake()
    {
        Init();
        //PhotonNetwork.SendRate = 60;
        //PhotonNetwork.SerializationRate = 30;
    }

    private void Init()
    {
        photonview = photonView;
        PhotonNetwork.EnableCloseConnection = true;             // 강퇴당할 수 있도록 true로 변경해줌

        // 로비관련
        maxRoomPlayer = 2;                                      // maxRoomPlayer디폴트 설정

        // 대기실 관련
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            userNickname[i] = userSlot[i].transform.GetChild(1).GetComponent<Text>();
            closeSlot[i] = userSlot[i].transform.GetChild(3).GetComponent<Image>();
            hostTag[i] = userSlot[i].transform.GetChild(5).GetComponent<Image>();
            readyTag[i] = userSlot[i].transform.GetChild(6).GetComponent<Image>();
        }

        startButton = roomPanel.transform.GetChild(5).GetComponent<Button>();
        roomName = roomPanel.transform.GetChild(6).GetChild(0).GetComponent<Text>();
        kickConfirmPanel = roomPanel.transform.GetChild(9).GetComponent<Image>();
        kickConfirmMessage = kickConfirmPanel.transform.GetChild(1).GetComponent<Text>();
        CharacterSelectPanel = roomPanel.transform.GetChild(10).GetComponent<Image>();
        ClassSelectPanel = roomPanel.transform.GetChild(11).GetComponent<Image>();

        readyPlayerNumber = 0;
        selectCharacterIndex = Random.Range(0, 27);
        sceneEffect = FindObjectOfType<SceneConvertEffect>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber);
    }

    #region #START# 1. 서버연결 (disconnect)
    public void OnDisconnectPanel()
    {
        ShowPanel(disconnectPanel);
    }
    public void Connect()
    {
        PhotonNetwork.GameVersion = gameVersion;    // 게임 버전설정 (다를 시 안만남)
        GameManager.instance.nickName = nickNameInput.text;
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()      // 서버접속시 실행되는 콜백함수 (방에서 나갔을때 이 함수부터 실행됨)
    {
        PhotonNetwork.JoinLobby();                  // 서버접속시 바로 디폴트로비로 접속
    }
    public override void OnJoinedLobby()            // 로비접속시 실행되는 콜백함수
    {
        PhotonNetwork.LocalPlayer.NickName = GameManager.instance.nickName;            // 서버 내 닉네임 설정
        myNicknameText.text = GameManager.instance.nickName;                           // 로비화면에서 닉네임 표시
        //networkStateText.text = PhotonNetwork.NetworkClientState.ToString();
        InitPlayerProperties();                     // 로비접속시 플레이어프로퍼티를 초기화 해줌

        ShowPanel(lobbyPanel);
    }
    //public void Disconnect()        // 0517 현재 미사용중.
    //{
    //    PhotonNetwork.Disconnect();
    //}
    public override void OnDisconnected(DisconnectCause cause)  // 서버연결 끊길시 실행되는 콜백함수
    {
        print("연결끊김");

        ShowPanel(disconnectPanel);
    }
    private void InitPlayerProperties()
    {
        SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY);
        SetPlayerProperties("Character", selectCharacterIndex);
        SetPlayerProperties("Class", selectClassIndex);
    }

    #endregion #END# 1. 서버연결 (disconnect)

    #region #START# 2. 로비

    #region 방만들기 
    public void OnRoomSettingButton()               // 방만들기버튼 클릭시 방설정창 ON
    {
        roomSettingPanel.SetActive(true);
    }
    public void OffRoomSettingButton()              // 방설정창 닫기버튼 클릭시 방설정창 OFF
    {
        ResetRoomSetting();
        roomSettingPanel.SetActive(false);
    }
    public void MaxPersonSettingDropDown(int maxPersonNum)          // 드롭다운ui를 이용한 방최대인원 설정
    {
        maxRoomPlayer = (maxPersonNum + 2);
    }

    private string[] randomRoomName = new string[4] { "무한 질주 대환장 게임", "오늘도 달린다!", "함께 달려요~", "레뒤 안하면 강퇴!!" };
    public void DecisionCreateRoomButton()                          // 방설정후 방생성 버튼 클릭시
    {
        int max = maxRoomPlayer;
        int hostActorNumber = 1;

        RoomOptions roomOptions = new RoomOptions();                // 생성할 방의 옵션설정
        roomOptions.MaxPlayers = (byte)maxRoomPlayer;               // 방최대인원 설정
        roomOptions.CustomRoomProperties = new Hashtable()          // 방 내 유저슬롯에 슬롯상태(닫힘, 열림)와 유저정보를 갖는 해쉬테이블생성
        {
            {"0", hostActorNumber }, {"1", (int)USER_SlOT_STATE.OPEN},
            {"2", 3 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"3", 4 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"4", 5 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE}
        };

        if (roomNameInput.text.Equals(""))                          // 방이름이 공백일 때
            PhotonNetwork.CreateRoom(randomRoomName[Random.Range(0, 4)], roomOptions);
        else                                                        // 방이름이 공백이 아닐 때
            PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }
    public override void OnCreatedRoom()            // 방을 성공적으로 만들었을 때 실행되는 콜백함수
    {
        startButton.gameObject.SetActive(true);     // 방을 만들었으면 방장이니 게임시작버튼 ON
    }
    public void ResetRoomSetting()                  // 방 만들거나 방들어가면 설정하던거 초기화해줌.
    {
        roomNameInput.text = "";
        //this.maxRoomPlayer = 2;
    }
    public override void OnCreateRoomFailed(short returnCode, string message) => print("방만들기실패");
    #endregion 방만들기
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public override void OnJoinedRoom()         // 방을 입장시 실행되는 콜백함수
    {
        PhotonNetwork.AutomaticallySyncScene = true;    // 방장이 씬을 이동하면 모두 같이 이동함

        ResetRoomSetting();                             // 방 만들기설정 초기화해줌
        StartCoroutine(nameof(DelayOnJoinedRoom));      // 유저 상태 표시를 해줌 (준비상태, 방장, 캐릭터)         
    }
    private IEnumerator DelayOnJoinedRoom()     // 캐릭간 정보동기화 한 후 함수 실행하도록 딜레이를 줌
    {
        yield return new WaitForSeconds(0.05f);
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        ShowHostTag(PhotonNetwork.CurrentRoom.MasterClientId);  // 방장 표시
        ShowReadyTag();                         // 준비상태 표시
        ShowPlayerCharacter();                  // 플레이어 캐릭터 표시
        RoomRenewal();                          // 유저슬롯이 닫힘, 열림 표시
        
        ShowPanel(roomPanel);
    }
    public override void OnJoinRoomFailed(short returnCode, string message) => print("방참가실패");
    public override void OnJoinRandomFailed(short returnCode, string message) => print("방랜덤참가실패");

    #endregion #END# 2. 로비 

    #region #START# 3. 대기방

    #region #START# 3-1. 버튼
    public void ClickSlotButton(int slotIndex)                              // 유저슬롯을 눌렀을때 실행
    {
        int userSlotState = GetRoomProperties(slotIndex);
        if (PhotonNetwork.IsMasterClient)                                   // 방장일때만 실행
        {
            if (userSlotState == (int)USER_SlOT_STATE.CLOSE)                // 누른슬롯이 닫힌상태면 열고 방최대인원 늘리기
            {
                SetRoomProperties(slotIndex, (int)USER_SlOT_STATE.OPEN);
                PhotonNetwork.CurrentRoom.MaxPlayers++;
            }
            else if (userSlotState == (int)USER_SlOT_STATE.OPEN)            // 누른슬롯이 열린상태면 닫고 방최대인원 줄이고
            {
                SetRoomProperties(slotIndex, (int)USER_SlOT_STATE.CLOSE);
                PhotonNetwork.CurrentRoom.MaxPlayers--;
            }
        }

        if (userSlotState > 0)      // 유저가 있는 슬롯 클릭시 정보창 띄우는 용도로 사용(만들 시간이 없을듯?..먼미래)
        {

        }
    }
    public void UseCharacterSelectButton(bool state)
    {
        CharacterSelectPanel.gameObject.SetActive(state);
    }
    public void UseClassSelectButton(bool state)
    {
        ClassSelectPanel.gameObject.SetActive(state);
    }
    public void OnKickConfirmPanelButton(int slotIndex)                     // 강퇴확인 창
    {
        int playerActorNum = GetRoomProperties(slotIndex);
        if (PhotonNetwork.CurrentRoom.masterClientId != PhotonNetwork.LocalPlayer.ActorNumber ||    // 방장이 아닐때
            playerActorNum == PhotonNetwork.CurrentRoom.masterClientId)     // (방장이) 방장을 강퇴하려고 할떄
        {
            return;
        }
        else if (playerActorNum == (int)USER_SlOT_STATE.OPEN ||             // 슬롯이 OPEN OR CLOSE이면 강퇴확인창 X
                playerActorNum == (int)USER_SlOT_STATE.CLOSE)
        {
            return;
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++) // 플레이어액터넘버 비교 후 강퇴할 플레이어 찾기
            {
                if (playerActorNum == PhotonNetwork.PlayerList[i].ActorNumber)
                {
                    // Debug.Log(PhotonNetwork.PlayerList[i].NickName + "를 강퇴함");
                    kickPlayer = PhotonNetwork.PlayerList[i];
                }
            }
            kickConfirmMessage.text = kickPlayer.NickName + "님을 강퇴 하시겠습니까?";
            kickConfirmPanel.gameObject.SetActive(true);
        }
    }
    public void OffKickConfirmPanelButton()
    {
        kickConfirmPanel.gameObject.SetActive(false);
    }
    public void KickPlayerButton()                                              // 플레이어 강퇴.
    {
        PhotonNetwork.CloseConnection(kickPlayer);
        kickConfirmPanel.gameObject.SetActive(false);
    }
    public void ReadyButton()
    {
        if (GetPlayerProperties("State") == (int)PLAYER_STATE.NOT_READY)
            SetPlayerProperties("State", (int)PLAYER_STATE.READY);
        else if (GetPlayerProperties("State") == (int)PLAYER_STATE.READY)
            SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY);
    }

    /*
        else    // 정비중일때 이 경우가 실행됨 (0525 지금은 쓸지 안쓸지 고민중임.) ->0526 생각해보니 쓸 수 없는기능임 준비버튼에선..
        {
            Debug.Log("난 정비중임ㅋ");
        }
    */
    public void StartButton()
    {
        int currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (readyPlayerNumber == currentRoomPlayerCount - 1)  // 방장 제외 모두 레디한 상태라면
        {
            Debug.Log("게임을 시작합니다");

            //chatManager.ClearChat();
            //startButton.gameObject.SetActive(false);

            PhotonNetwork.LoadLevel(1);
        }
        else
        {
            chatManager.ChatRPC("<color=red>모든 플레이어가 준비를 하지 않았습니다.</color>");
        }
    }
    public void LeaveRoomButton()
    {
        Debug.Log("방 나감");
        PhotonNetwork.LeaveRoom();
        ShowPanel(lobbyPanel);
        //sceneEffect.StartFadeOut(0.3f);
    }
    #endregion #END# 3-1. 버튼

    #region #START# 3-2. 외형, 직업 변경
    public void SelectCharacter(int selectCharacterIndex)
    {
        //Debug.Log("SelectCharacter 실행, 선택캐릭인덱스 : " + selectCharacterIndex);
        this.selectCharacterIndex = selectCharacterIndex;
        SetPlayerProperties("Character", selectCharacterIndex);
    }
    public void SelectClass(int selectClassIndex)
    {
        this.selectClassIndex = selectClassIndex;
        SetPlayerProperties("Class", selectClassIndex);
    }
    #endregion #END# 3-2. 외형 직업 변경

    #region #START# 3-3. 대기방 동기화 

    #region 포톤 콜백함수
    public override void OnPlayerEnteredRoom(Player newPlayer)  // 방에 다른 유저가 들어왔을 때 실행되는 콜백함수
    {
        chatManager.ChatRPC("<color=yellow>" + newPlayer.NickName + " 님이 참가하셨습니다</color>");

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < MAX_PLAYER_NUM; i++)
            {
                if (GetRoomProperties(i) == (int)USER_SlOT_STATE.OPEN)
                {
                    SetRoomProperties(i, newPlayer.ActorNumber);
                    //Debug.Log("<color=yellow>" + i + "번째 칸 " + newPlayer.NickName + " 님이 참가하셨습니다</color>");

                    break;
                }
            }
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)   // 방에 다른 유저가 나갔을 때 실행되는 콜백함수
    {
        chatManager.ChatRPC("<color=yellow>" + otherPlayer.NickName + " 님이 퇴장하셨습니다</color>");

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < MAX_PLAYER_NUM; i++)
            {
                if (GetRoomProperties(i) == otherPlayer.ActorNumber)
                {
                    SetRoomProperties(i, (int)USER_SlOT_STATE.OPEN);
                    //Debug.Log("<color=yellow>" + i + "번째 칸 " + otherPlayer.NickName + " 님이 퇴장하셨습니다</color>");

                    break;
                }
            }
        }
    }

    public override void OnLeftRoom()               // 방에서 본인이 나가면(강퇴포함) 실행됨
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            if (playerCharacters[i] != null)
            {
                Destroy(playerCharacters[i].gameObject);
                playerCharacters[i] = null;
            }
        }

        chatManager.ChatTextClear();
        startButton.gameObject.SetActive(false);    // 방에서 나갔으니 스타트버튼 off
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)    // 룸프로퍼티가 변경시 실행                        (룸프로퍼티는 입력에 딜레이가 있어 콜백함수를 이용해 방리뉴얼해줌)
    {                                                                               // 슬롯닫고 열때, Player이 입퇴장시 실행됨
        foreach (object slotNumber in propertiesThatChanged.Keys)
        {
            if (slotNumber.GetType() == typeof(string))     // 슬롯정보 변경 시에만 실행 ('0' ~ '4'),                              키 타입이 스트링일때만 동작 (룸맥스인원 변경시 실행안되도록)
            {
                if ((string)slotNumber == "curScn") return; // PhotonNetwork.AutomaticallySyncScene때문에 생김.

                ShowPlayerCharacter((string)slotNumber);    // 플레이어 캐릭터 표시
                CountReadyPlayer();                         // 준비중인 플레이어 수 확인
                RoomRenewal((string)slotNumber, (int)propertiesThatChanged[(string)slotNumber]); // X표시, 준비표시, 닉네임표시
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)  // 룸에 있는 플레이어프로퍼티 변경시 실행 
    {                                                                                           // 유저의 준비상태, 캐릭터변경
        foreach (object key in changedProps.Keys)
        {
            if ((string)key == "State")             // 플레이어의 준비상태
            {
                ShowReadyTag(targetPlayer.ActorNumber, (int)GetPlayerProperties((string)key, targetPlayer));
            }
            else if ((string)key == "Character")    // 플레이어의 외형
            {
                ShowPlayerCharacter(targetPlayer.ActorNumber, (int)GetPlayerProperties((string)key, targetPlayer));
            }
        }

        CountReadyPlayer();
    }
    //Debug.Log("key : " + (string)key + ", state : " + (int)GetPlayerProperties((string)key, targetPlayer));

    public override void OnMasterClientSwitched(Player newMasterClient)     // 방장 변경시 실행
    {
        if (GetPlayerProperties("State", newMasterClient) == (int)PLAYER_STATE.READY)
            SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY, newMasterClient);

        if (newMasterClient == PhotonNetwork.LocalPlayer)
            startButton.gameObject.SetActive(true);

        ShowHostTag(newMasterClient.ActorNumber);
    }

    #endregion 포톤콜백함수

    #region 플레이어들 현재 정보
    private void CountReadyPlayer()         // 게임준비 중인 인원 파악 (방장만 실행됨) 
    {
        if (PhotonNetwork.IsMasterClient)
        {
            readyPlayerNumber = 0;
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                if (GetPlayerProperties("State", PhotonNetwork.PlayerList[i]) == (int)PLAYER_STATE.READY)
                {
                    readyPlayerNumber++;
                }
            }
        }

    }
    private void RoomRenewal()                      // 방 최초입장시 1회 실행
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int slotState = GetRoomProperties(i);

            if (slotState == (int)USER_SlOT_STATE.CLOSE)            // 유저슬롯이 닫혀있으면 
            {
                userNickname[i].text = "";
                closeSlot[i].gameObject.SetActive(true);            // X표시 ON
            }
            else if (slotState == (int)USER_SlOT_STATE.OPEN)        // 유저슬롯이 열려있으면
            {
                userNickname[i].text = "";
                closeSlot[i].gameObject.SetActive(false);           // X표시 OFF

            }
            else                                                    // 유저슬롯에 유저가 있다면
            {
                closeSlot[i].gameObject.SetActive(false);           // X표시 OFF
                for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)     // 유저슬롯과 유저의 ActorNum를 비교하여 같으면 그 닉네임을 넣음
                {
                    if (slotState == PhotonNetwork.PlayerList[j].ActorNumber)
                    {
                        userNickname[i].text = PhotonNetwork.PlayerList[j].NickName;
                        break;
                    }
                }
            }
        }
    }
    private void RoomRenewal(string slotNumString, int slotState)   // 방의 슬롯상태가 변경될때마다 실행
    {
        int slotNum = int.Parse(slotNumString);

        if (slotState == (int)USER_SlOT_STATE.CLOSE)                // 유저슬롯이 닫혀있으면 
        {
            userNickname[slotNum].text = "";
            readyTag[slotNum].gameObject.SetActive(false);          // 레디태그 OFF
            closeSlot[slotNum].gameObject.SetActive(true);          // X표시 ON
        }
        else if (slotState == (int)USER_SlOT_STATE.OPEN)            // 유저슬롯이 열려있으면
        {
            userNickname[slotNum].text = "";
            readyTag[slotNum].gameObject.SetActive(false);          // 레디태그 OFF
            closeSlot[slotNum].gameObject.SetActive(false);         // X표시 OFF
        }
        else                                                        // 유저슬롯에 사람이 있으면
        {
            for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)     // 유저슬롯과 유저의 ActorNum를 비교하여 같으면 그 닉네임을 넣음
            {
                if (slotState == PhotonNetwork.PlayerList[j].ActorNumber)
                {
                    userNickname[slotNum].text = PhotonNetwork.PlayerList[j].NickName;
                    break;
                }
            }
        }
    }
    private void ShowHostTag(int hostActorNumber)       // hostTag를 방장한테만 띄어줌
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            if (hostActorNumber == userActorNumber)     // 유저가 방장일경우 HostTag ON
            {
                hostTag[i].gameObject.SetActive(true);
            }
            else
            {
                hostTag[i].gameObject.SetActive(false);
            }
        }
    }
    private void ShowReadyTag()                         // 방 최초입장시 1회 실행 
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            if (userActorNumber == PhotonNetwork.CurrentRoom.masterClientId)    // 슬롯에 있는 사람이 방장이면 
            {
                readyTag[i].gameObject.SetActive(false);                        // 준비태그 OFF
            }
            else if (userActorNumber == (int)USER_SlOT_STATE.CLOSE || userActorNumber == (int)USER_SlOT_STATE.OPEN)
            {
                readyTag[i].gameObject.SetActive(false);                        // 준비태그 OFF
            }
            else
            {
                //Debug.Log("ShowReadyTag, " + i.ToString() + "번째, 액터넘버 : " + userActorNumber);
                for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)
                {
                    if (userActorNumber == PhotonNetwork.PlayerList[j].ActorNumber)
                    {
                        int userState = GetPlayerProperties("State", PhotonNetwork.PlayerList[j]);

                        if (userState == (int)PLAYER_STATE.NOT_READY)
                        {
                            readyTag[i].gameObject.SetActive(false);
                        }
                        else if (userState == (int)PLAYER_STATE.READY)
                        {
                            readyTag[i].gameObject.SetActive(true);
                        }

                    }
                }
            }
        }
    }
    private void ShowReadyTag(int playerActNumber, int playerState)    // 유저의 상태가 변경될때마다 실행 
    {
        //Debug.Log("ShowReadyTag 실행");
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);

            if (userActorNumber == playerActNumber)
            {
                if (playerState == (int)PLAYER_STATE.NOT_READY)
                {
                    readyTag[i].gameObject.SetActive(false);
                }
                else if (playerState == (int)PLAYER_STATE.READY)
                {
                    readyTag[i].gameObject.SetActive(true);
                }
            }
        }
    }
    private void ShowPlayerCharacter()          // 방 최초입장시 1회만 실행됨
    {

        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);

            if (userActorNumber == (int)USER_SlOT_STATE.CLOSE || userActorNumber == (int)USER_SlOT_STATE.OPEN)
            {
                if (playerCharacters[i] != null)
                {
                    //Debug.Log("playerCharacters[i] : " + playerCharacters[i].name);
                    Destroy(playerCharacters[i]);
                    playerCharacters[i] = null;
                }
            }
            else
            {
                for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)
                {
                    if (userActorNumber == PhotonNetwork.PlayerList[j].ActorNumber)
                    {
                        if (playerCharacters[i] != null)
                        {
                            Destroy(playerCharacters[i]);
                            playerCharacters[i] = null;
                        }
                        int userCharacter = GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]);
                        Debug.Log(i.ToString() + "번째, 캐릭터 인덱스 : " + GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]));
                        playerCharacters[i] = Instantiate(characterPrefab[userCharacter], new Vector3(-20 + (2 * i), 0, -5), Quaternion.Euler(0, 180, 0));
                    }
                }
            }
        }
    }
    private void ShowPlayerCharacter(string slotNumberString)   // 방의 슬롯정보가 바뀌었을 때 실행됨 (룸프로퍼티)
    {
        int slotNumber = int.Parse(slotNumberString);
        int userActorNumber = GetRoomProperties(slotNumber);

        if (userActorNumber == (int)USER_SlOT_STATE.CLOSE || userActorNumber == (int)USER_SlOT_STATE.OPEN)
        {
            if (playerCharacters[slotNumber] != null)
            {
                Destroy(playerCharacters[slotNumber]);
                playerCharacters[slotNumber] = null;
            }
        }
        else
        {
            for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)
            {
                if (userActorNumber == PhotonNetwork.PlayerList[j].ActorNumber)
                {
                    if (playerCharacters[slotNumber] != null)
                    {
                        Destroy(playerCharacters[slotNumber]);
                        playerCharacters[slotNumber] = null;
                    }

                    int userCharacter = GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]);
                    playerCharacters[slotNumber] = Instantiate(characterPrefab[userCharacter], new Vector3(-20 + (2 * slotNumber), 0, -5), Quaternion.Euler(0, 180, 0));
                    return;
                }
            }
        }

    }

    private void ShowPlayerCharacter(int playerActNumber, int selectCharacterIndex)  // 플레이어가 캐릭터를 바꿧을때 실행됨 (플레이어 프로퍼티)       
    {
        //Debug.Log("ShowReadyTag 지속실행");
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            if (userActorNumber == playerActNumber)
            {
                Destroy(playerCharacters[i]);

                playerCharacters[i] = Instantiate(characterPrefab[selectCharacterIndex], new Vector3(-20 + (2 * i), 0, -5), Quaternion.Euler(0, 180, 0));
                return;
            }
        }

    }
    #endregion 플레이어들 현재 정보

    #endregion #END# 3-3. 대기방 동기화

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

    #endregion #END# 3. 대기방

    private void ShowPanel(GameObject currentPanel)
    {
        disconnectPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        roomSettingPanel.SetActive(false);
        roomPanel.SetActive(false);
        kickConfirmPanel.gameObject.SetActive(false);
        CharacterSelectPanel.gameObject.SetActive(false);

        currentPanel.SetActive(true);
    }




}


/* 08/27 수정 전
    private void ShowPlayerCharacter()                  // 방 최초입장시, 방의 슬롯정보가 바뀌었을 때
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);

            if (userActorNumber == (int)USER_SlOT_STATE.CLOSE || userActorNumber == (int)USER_SlOT_STATE.OPEN)
            {
                if (playerCharacters[i] != null)
                {
                    //Debug.Log("playerCharacters[i] : " + playerCharacters[i].name);
                    Destroy(playerCharacters[i]);
                    playerCharacters[i] = null;
                }
                continue;
            }

            for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)
            {
                if (userActorNumber == PhotonNetwork.PlayerList[j].ActorNumber)
                {
                    if (playerCharacters[i] != null)
                    {
                        Destroy(playerCharacters[i]);
                        playerCharacters[i] = null;
                    }
                    int userCharacter = GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]);
                    //Debug.Log(i.ToString() + "번째, 캐릭터 인덱스 : " + GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]));
                    playerCharacters[i] = Instantiate(characterPrefab[userCharacter], new Vector3(-20 + (2 * i), 0, -5), Quaternion.Euler(0, 180, 0));
                }
            }
        }
        
    }

 */