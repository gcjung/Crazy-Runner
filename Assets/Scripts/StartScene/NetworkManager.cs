using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum USER_SlOT_STATE { CLOSE = -2, OPEN = -1 }
public enum PLAYER_STATE { NOT_READY, MAINTENANCE, HOST, READY }
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
    private byte maxRoomPlayer;
    public Text myNicknameText;
    //public  Text         networkStateText;

    [Header("RoomPanel"), Space(20)]
    public GameObject roomPanel;
    public GameObject[] userSlot;
    public GameObject[] characterPrefab;
    private GameObject[] playerCharacters = new GameObject[MAX_PLAYER_NUM];
    private int selectCharacterIndex;
    private int selectClassIndex;

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

    //테스트용
    [Header("테스트용")]
    public Text[] testUserPanel;
    public Text testNowState;
    public Text testroomName;
    //테스트용

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
        kickConfirmPanel = roomPanel.transform.GetChild(9).GetComponent<Image>();
        kickConfirmMessage = kickConfirmPanel.transform.GetChild(1).GetComponent<Text>();
        CharacterSelectPanel = roomPanel.transform.GetChild(10).GetComponent<Image>();
        ClassSelectPanel = roomPanel.transform.GetChild(11).GetComponent<Image>();

        readyPlayerNumber = 0;
        selectCharacterIndex = Random.Range(0, 27);
        sceneEffect = FindObjectOfType<SceneConvertEffect>();
    }

    void Update()       // 테스트용
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Info();
            Debug.Log("난 방장인가 ? " + PhotonNetwork.IsMasterClient);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < 5; i++)
            {
                if (playerCharacters[i] == null)
                    continue;
                Debug.Log(i.ToString() + "번째 , 캐릭이름 : " + playerCharacters[i].name);
            }
        }
        //networkStateText.text = PhotonNetwork.NetworkClientState.ToString();
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
        //sceneEffect.StartFadeInOut(0.8f);
    }
    public override void OnConnectedToMaster()      // 서버접속시 실행되는 콜백함수 (방에서 나갔을때 이 함수부터 실행됨)
    {
        //Debug.Log("OnConnectedToMaster 실행");
        PhotonNetwork.JoinLobby();                  // 서버접속시 바로 디폴트로비로 접속
    }
    public override void OnJoinedLobby()            // 로비접속시 실행되는 콜백함수
    {
        //Debug.Log("OnJoinedLobby 실행");

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
        this.maxRoomPlayer = (byte)(maxPersonNum + 2);
        Debug.Log("방 최대인원 : " + this.maxRoomPlayer);
    }
    private string[] roomName = new string[4] { "무한 질주 대환장 게임", "오늘도 달린다!", "함께 달려요~", "레뒤 안하면 강퇴!!" };
    public void DecisionCreateRoomButton()                          // 방설정후 방생성 버튼 클릭시
    {
        int max = maxRoomPlayer - 1;

        RoomOptions roomOptions = new RoomOptions();                // 생성할 방의 옵션설정
        roomOptions.MaxPlayers = maxRoomPlayer;                     // 방최대인원 설정
        roomOptions.CustomRoomProperties = new Hashtable()          // 방 내 유저슬롯상태 및 유저정보를 갖는 해쉬테이블생성
        {
            {"0", 1 },{"1",(int)USER_SlOT_STATE.OPEN},
            {"2", 2 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"3", 3 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"4", 4 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE}
        };

        if (roomNameInput.text.Equals(""))                          // 방이름이 공백일 때
            PhotonNetwork.CreateRoom(roomName[(int)Random.Range(0, 4)], roomOptions);
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
    #endregion
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public override void OnJoinedRoom()         // 방을 성공적으로 참가하면 실행되는 콜백함수
    {
        testNowState.text = "내 닉네임: " + PhotonNetwork.LocalPlayer.NickName;  //테스트
        PhotonNetwork.AutomaticallySyncScene = true;                    // 방장이 씬을 이동하면 모두 같이 이동함

        ResetRoomSetting();                                             // 방만들기설정하다가 방들어왔을시 세팅을 초기화해줌
        StartCoroutine(nameof(DelayOnJoinedRoom));
    }
    private IEnumerator DelayOnJoinedRoom()     // 방입장시 캐릭간 정보동기화 한 후 함수 실행하도록 딜레이를 줌
    {
        yield return new WaitForSeconds(0.05f);
        testroomName.text = PhotonNetwork.CurrentRoom.Name;
        ShowHostTag(PhotonNetwork.CurrentRoom.MasterClientId);
        ShowReadyTag();
        RoomRenewal();
        ShowPlayerCharacter();

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
    public void OnKickConfirmPanelButton(int slotIndex)                         // 강퇴확인 창
    {
        int playerActorNum = GetRoomProperties(slotIndex);
        if (PhotonNetwork.CurrentRoom.masterClientId != PhotonNetwork.LocalPlayer.ActorNumber ||    // 방장이 아닐때
            playerActorNum == PhotonNetwork.CurrentRoom.masterClientId)        // (방장이) 방장을 강퇴하려고 할떄
        {
            return;
        }
        else if (playerActorNum == (int)USER_SlOT_STATE.OPEN ||                  // 슬롯이 OPEN OR CLOSE이면 강퇴확인창 X
                playerActorNum == (int)USER_SlOT_STATE.CLOSE)
        {
            return;
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)     // 플레이어액터넘버 비교 후 강퇴할 플레이어 찾기
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
        //Debug.Log("ReadyButton 실행");
        if (GetPlayerProperties("State") == (int)PLAYER_STATE.NOT_READY)
        {
            SetPlayerProperties("State", (int)PLAYER_STATE.READY);
            //Debug.Log("낫레디 -> \"레디\"");
        }
        else if (GetPlayerProperties("State") == (int)PLAYER_STATE.READY)
        {
            SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY);
            //Debug.Log("레디 -> \"낫레디\"");
        }
        else    // 정비중일때 이 경우가 실행됨 (0525 지금은 쓸지 안쓸지 고민중임.) ->0526 생각해보니 쓸 수 없는기능임 준비버튼에선..
        {
            Debug.Log("난 정비중임ㅋ");
        }
    }
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
    public override void OnPlayerEnteredRoom(Player newPlayer)              // 방에 유저가 들어왔을 때 실행되는 콜백함수
    {
        chatManager.ChatRPC("<color=yellow>" + newPlayer.NickName + " 님이 참가하셨습니다</color>");

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < MAX_PLAYER_NUM; i++)
            {
                if (GetRoomProperties(i) == (int)USER_SlOT_STATE.OPEN)
                {
                    SetRoomProperties(i, newPlayer.ActorNumber);
                    Debug.Log("<color=yellow>" + i + "번째 칸 " + newPlayer.NickName + " 님이 참가하셨습니다</color>");

                    break;
                }
            }
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)               // 방에 유저가 나갔을 때 실행되는 콜백함수
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
    public override void OnLeftRoom()                               // 방에서 나가면(강퇴포함) 실행됨
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
        startButton.gameObject.SetActive(false);        // 방에서 나갔으니 스타트버튼 off
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)    // 룸프로퍼티가 변경시 실행 (룸프로퍼티는 입력에 딜레이가 있어 콜백함수를 이용해 방리뉴얼해줌)
    {                                                                               // 슬롯닫고 열때, Player이 출입시 실행됨
        foreach (object slotNumber in propertiesThatChanged.Keys)
        {
            if (slotNumber.GetType() == typeof(string))                             // 키 타입이 스트링일때만 동작 (룸맥스인원 변경시 실행안되도록)
            {
                if ((string)slotNumber == "curScn")     // PhotonNetwork.AutomaticallySyncScene 설정에 대한 것일때 넘기기
                    return;
                //Debug.Log((string)slotNumber + ", " + (int)propertiesThatChanged[(string)slotNumber]);
                RoomRenewal((string)slotNumber, (int)propertiesThatChanged[(string)slotNumber]);
            }
        }
        ShowPlayerCharacter();
        CountReadyPlayer();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)  // 룸에 있는 플레이어프로퍼티 변경시 실행 
    {                                                                                           // 유저의 준비, 준비해제, 정비
        foreach (object key in changedProps.Keys)
        {
            //Debug.Log("key : " + (string)key + ", state : " + (int)GetPlayerProperties((string)key, targetPlayer));
            if ((string)key == "State")
            {
                ShowReadyTag(targetPlayer.ActorNumber, (int)GetPlayerProperties((string)key, targetPlayer));
            }
            else if ((string)key == "Character")
            {
                ShowPlayerCharacter(targetPlayer.ActorNumber, (int)GetPlayerProperties((string)key, targetPlayer));
            }
        }

        CountReadyPlayer();
    }
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
    private void CountReadyPlayer()
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
            testUserPanel[i].text = i.ToString() + " : " + GetRoomProperties(i).ToString(); // 테스트용

            int slotState = GetRoomProperties(i);
            // Debug.Log("RoomRenewal, " + i.ToString() + "번째, 액터넘버 : " + slotState);
            if (slotState == (int)USER_SlOT_STATE.CLOSE)                    // 유저슬롯이 닫혀있으면 
            {
                userNickname[i].text = "";
                //readyTag[i].gameObject.SetActive(false);                    // 레디태그 OFF
                closeSlot[i].gameObject.SetActive(true);                    // X표시 ON
            }
            else if (slotState == (int)USER_SlOT_STATE.OPEN)                // 유저슬롯이 열려있으면
            {
                userNickname[i].text = "";
                //readyTag[i].gameObject.SetActive(false);                    // 레디태그 OFF
                closeSlot[i].gameObject.SetActive(false);                    // X표시 ON

            }
            else                                                            // 유저슬롯에 유저가 있다면
            {
                closeSlot[i].gameObject.SetActive(false);                   // X표시 OFF
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
        testUserPanel[slotNum].text = slotNum.ToString() + " : " + GetRoomProperties(slotNum).ToString(); // 테스트용

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
    private void ShowReadyTag(int playerActNumber, int playerState)             // 유저의 상태가 변경될때마다 실행 
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
    private void ShowPlayerCharacter()                          // 방 최초입장시, 방의 슬롯정보가 바뀌었을 때
    {
        //print("ShowPlayerCharacter() 최초 실행");
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            //Debug.Log("character, " +i.ToString() + "번째, 액터넘버 : " + userActorNumber);
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
    private void ShowPlayerCharacter(int playerActNumber, int selectCharacterIndex)  // 플레이어가 캐릭터를 바꿧을때 바뀜              
    {

        //Debug.Log("ShowReadyTag 지속실행");
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            if (userActorNumber == playerActNumber)
            {
                Destroy(playerCharacters[i]);

                playerCharacters[i] = Instantiate(characterPrefab[selectCharacterIndex], new Vector3(-20 + (2 * i), 0, -5), Quaternion.Euler(0, 180, 0));
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


    [ContextMenu("정보")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            print("현재 포톤 닉네임 : " + PhotonNetwork.LocalPlayer);
            print("현재 방 이름 : " + PhotonNetwork.CurrentRoom.Name);
            print("현재 방 인원수 : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("현재 방 최대인원수 : " + PhotonNetwork.CurrentRoom.MaxPlayers);
            string playerStr = "방에 있는 플레이어 목록 : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerStr);
        }
        else
        {
            print("현재 포톤 닉네임 : " + PhotonNetwork.LocalPlayer);
            print("접속한 인원 수 : " + PhotonNetwork.CountOfPlayers);
            print("방 개수 : " + PhotonNetwork.CountOfRooms);
            print("모든 방에 있는 인원 수 : " + PhotonNetwork.CountOfPlayersInRooms);
            print("로비에 있는지? : " + PhotonNetwork.InLobby);
            print("연결됐는지? : " + PhotonNetwork.IsConnected);
        }
    }

}

//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//{

//    if (stream.IsWriting && PhotonNetwork.InRoom)
//    {
//        stream.SendNext(readyPlayerNumber);
//        Debug.Log("동기화 데이터 전송");
//    }
//    else
//    {
//        readyPlayerNumber = (int)stream.ReceiveNext();
//        Debug.Log("동기화 데이터 수신");
//    }

//}

//public override void OnJoinedRoom()         // 방을 성공적으로 참가하면 실행되는 콜백함수
//{
//    testNowState.text = "내 액터넘버: " + PhotonNetwork.LocalPlayer.ActorNumber.ToString();

//    //for (int i = 0; i < MAX_PLAYER_NUM; i++)                            // 방 처음들어갔을 시, 방의 프로퍼티를 받아서
//    //{
//    //    int userSlotState = GetRoomProperties(i);
//    //    if (userSlotState == (int)USER_SlOT_STATE.CLOSE)             // Close 슬롯일시, 닫힌표시(X표시) ON
//    //    {
//    //        closeSlot[i].SetActive(true);
//    //    }
//    //    else if (userSlotState == (int)USER_SlOT_STATE.OPEN)         // OPEN 슬롯일시, 닫힌표시(X표시) OFF
//    //    {
//    //        closeSlot[i].SetActive(false);
//    //    }

//    //}

//    ResetRoomSetting();                                                 // 방만들기설정하다가 방들어왔을시 세팅을 초기화해줌
//    RoomRenewal();

//    OffAllPanel();
//    roomPanel.SetActive(true);
//}



//void SendEvent()
//{

//    object[] content = new object[] { playerUIList[0], playerUIList[1], playerUIList[2], playerUIList[3], playerUIList[4]};
//    PhotonNetwork.RaiseEvent(0, content, RaiseEventOptions.Default, SendOptions.SendUnreliable);
//    Debug.Log("이벤트 전송");
//}
//public void OnEvent(EventData photonEvent)
//{
//    Debug.Log("이벤트 수신");
//    if (photonEvent.Code == 0)
//    {
//        Debug.Log("이벤트 수신 코드 0!!");
//        object[] data = (object[])photonEvent.CustomData;
//        for (int i = 0; i < data.Length; i++) playerUIList[i] = (PlayerUIList)data[i];
//        //for (int i = 0; i < MAX_PLAYER_NUM; i++)
//        //{
//        //    Debug.Log(i + "번째, 스테이트 : " + playerUIList[i].userPanelState);
//        //    if (playerUIList[i].player != null)
//        //        Debug.Log(i + "번째, 닉네임 : " + playerUIList[i].player.NickName);
//        //}
//    }
//}

//private void RoomRenewal()
//{
//    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
//        userNickname[i].text = PhotonNetwork.PlayerList[i].NickName;
//    //for (int i = PhotonNetwork.PlayerList.Length; i < MAX_USER_NUM; i++)
//    //    userNickname[i].text = "";


//}


//public override void OnPlayerEnteredRoom(Player newPlayer)
//{
//    RoomRenewal();
//    //ChatRPC("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
//    for (int i = 0; i < playerList.Length; i++)
//    {
//        if (playerList[i] == null)
//        {
//            playerList[i] = newPlayer;
//            break;
//        }
//    }
//    Debug.Log("나 참가");
//    //photonview.RPC("PlayerListSync", RpcTarget.All, playerList);
//    Debug.Log("<color=yellow>" + newPlayer.NickName + "님이 참가하셨습니다</color>");

//}

//public override void OnPlayerLeftRoom(Player otherPlayer)
//{
//    RoomRenewal();
//    //ChatRPC("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
//    for (int i = 0; i < playerList.Length; i++)
//    {
//        if (playerList[i] == otherPlayer)
//        {
//            playerList[i] = null;
//            break;
//        }
//    }
//    Debug.Log("나 퇴장");
//    //photonview.RPC("PlayerListSync", RpcTarget.All, );
//    Debug.Log("<color=yellow>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");

//}



//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//{

//        if (stream.IsWriting && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
//        {
//            stream.SendNext(playerList);
//            Debug.Log("동기화 데이터 전송");
//        }
//        else
//        {
//            playerList = (Player[])stream.ReceiveNext();
//            Debug.Log("동기화 데이터 수신");
//        }

//}



//// UI에서 값 얻어오기.
//byte maxPlayers = byte.Parse(m_dropdown_RoomMaxPlayers.options[m_dropdown_RoomMaxPlayers.value].text); // 드롭다운에서 값 얻어오기.
//byte maxTime = byte.Parse(m_dropdown_MaxTime.options[m_dropdown_MaxTime.value].text);

//// 방 옵션 설정한 것 그대로 HashTable 만들어준다. 이걸로 필터링하는 것이다.
//ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() { { "maxTime", maxTime } };

//// 방 입장할 때, 필터링할 옵션 넣어주기.
//// maxPlayers는 자주 쓰이니까, 따로 변수로 빼준 듯하다.
//PhotonNetwork.JoinRandomRoom(customProperties, maxPlayers);



// 쓸지 안쓸지 모름

//public void CreateRoom()
//{

//    PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 });
//}



//public void JoinLobby() => PhotonNetwork.JoinLobby();
//public override void OnJoinedLobby() => print("로비접속완료");