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

    //�׽�Ʈ��
    [Header("�׽�Ʈ��")]
    public Text[] testUserPanel;
    public Text testNowState;
    public Text testroomName;
    //�׽�Ʈ��

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
        PhotonNetwork.EnableCloseConnection = true;             // ������� �� �ֵ��� true�� ��������

        // �κ����
        maxRoomPlayer = 2;                                      // maxRoomPlayer����Ʈ ����

        // ���� ����
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

    void Update()       // �׽�Ʈ��
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Info();
            Debug.Log("�� �����ΰ� ? " + PhotonNetwork.IsMasterClient);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 0; i < 5; i++)
            {
                if (playerCharacters[i] == null)
                    continue;
                Debug.Log(i.ToString() + "��° , ĳ���̸� : " + playerCharacters[i].name);
            }
        }
        //networkStateText.text = PhotonNetwork.NetworkClientState.ToString();
    }

    #region #START# 1. �������� (disconnect)
    public void OnDisconnectPanel()
    {
        ShowPanel(disconnectPanel);
    }
    public void Connect()
    {
        PhotonNetwork.GameVersion = gameVersion;    // ���� �������� (�ٸ� �� �ȸ���)
        GameManager.instance.nickName = nickNameInput.text;
        PhotonNetwork.ConnectUsingSettings();
        //sceneEffect.StartFadeInOut(0.8f);
    }
    public override void OnConnectedToMaster()      // �������ӽ� ����Ǵ� �ݹ��Լ� (�濡�� �������� �� �Լ����� �����)
    {
        //Debug.Log("OnConnectedToMaster ����");
        PhotonNetwork.JoinLobby();                  // �������ӽ� �ٷ� ����Ʈ�κ�� ����
    }
    public override void OnJoinedLobby()            // �κ����ӽ� ����Ǵ� �ݹ��Լ�
    {
        //Debug.Log("OnJoinedLobby ����");

        PhotonNetwork.LocalPlayer.NickName = GameManager.instance.nickName;            // ���� �� �г��� ����
        myNicknameText.text = GameManager.instance.nickName;                           // �κ�ȭ�鿡�� �г��� ǥ��
        //networkStateText.text = PhotonNetwork.NetworkClientState.ToString();
        InitPlayerProperties();                     // �κ����ӽ� �÷��̾�������Ƽ�� �ʱ�ȭ ����

        ShowPanel(lobbyPanel);
    }
    //public void Disconnect()        // 0517 ���� �̻����.
    //{
    //    PhotonNetwork.Disconnect();
    //}
    public override void OnDisconnected(DisconnectCause cause)  // �������� ����� ����Ǵ� �ݹ��Լ�
    {
        print("�������");

        ShowPanel(disconnectPanel);
    }
    private void InitPlayerProperties()
    {
        SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY);
        SetPlayerProperties("Character", selectCharacterIndex);
        SetPlayerProperties("Class", selectClassIndex);
    }

    #endregion #END# 1. �������� (disconnect)

    #region #START# 2. �κ�

    #region �游��� 
    public void OnRoomSettingButton()               // �游����ư Ŭ���� �漳��â ON
    {
        roomSettingPanel.SetActive(true);
    }
    public void OffRoomSettingButton()              // �漳��â �ݱ��ư Ŭ���� �漳��â OFF
    {
        ResetRoomSetting();
        roomSettingPanel.SetActive(false);
    }
    public void MaxPersonSettingDropDown(int maxPersonNum)          // ��Ӵٿ�ui�� �̿��� ���ִ��ο� ����
    {
        this.maxRoomPlayer = (byte)(maxPersonNum + 2);
        Debug.Log("�� �ִ��ο� : " + this.maxRoomPlayer);
    }
    private string[] roomName = new string[4] { "���� ���� ��ȯ�� ����", "���õ� �޸���!", "�Բ� �޷���~", "���� ���ϸ� ����!!" };
    public void DecisionCreateRoomButton()                          // �漳���� ����� ��ư Ŭ����
    {
        int max = maxRoomPlayer - 1;

        RoomOptions roomOptions = new RoomOptions();                // ������ ���� �ɼǼ���
        roomOptions.MaxPlayers = maxRoomPlayer;                     // ���ִ��ο� ����
        roomOptions.CustomRoomProperties = new Hashtable()          // �� �� �������Ի��� �� ���������� ���� �ؽ����̺����
        {
            {"0", 1 },{"1",(int)USER_SlOT_STATE.OPEN},
            {"2", 2 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"3", 3 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"4", 4 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE}
        };

        if (roomNameInput.text.Equals(""))                          // ���̸��� ������ ��
            PhotonNetwork.CreateRoom(roomName[(int)Random.Range(0, 4)], roomOptions);
        else                                                        // ���̸��� ������ �ƴ� ��
            PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }
    public override void OnCreatedRoom()            // ���� ���������� ������� �� ����Ǵ� �ݹ��Լ�
    {
        startButton.gameObject.SetActive(true);     // ���� ��������� �����̴� ���ӽ��۹�ư ON
    }
    public void ResetRoomSetting()                  // �� ����ų� ����� �����ϴ��� �ʱ�ȭ����.
    {
        roomNameInput.text = "";
        //this.maxRoomPlayer = 2;
    }
    public override void OnCreateRoomFailed(short returnCode, string message) => print("�游������");
    #endregion
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public override void OnJoinedRoom()         // ���� ���������� �����ϸ� ����Ǵ� �ݹ��Լ�
    {
        testNowState.text = "�� �г���: " + PhotonNetwork.LocalPlayer.NickName;  //�׽�Ʈ
        PhotonNetwork.AutomaticallySyncScene = true;                    // ������ ���� �̵��ϸ� ��� ���� �̵���

        ResetRoomSetting();                                             // �游��⼳���ϴٰ� ��������� ������ �ʱ�ȭ����
        StartCoroutine(nameof(DelayOnJoinedRoom));
    }
    private IEnumerator DelayOnJoinedRoom()     // ������� ĳ���� ��������ȭ �� �� �Լ� �����ϵ��� �����̸� ��
    {
        yield return new WaitForSeconds(0.05f);
        testroomName.text = PhotonNetwork.CurrentRoom.Name;
        ShowHostTag(PhotonNetwork.CurrentRoom.MasterClientId);
        ShowReadyTag();
        RoomRenewal();
        ShowPlayerCharacter();

        ShowPanel(roomPanel);
    }
    public override void OnJoinRoomFailed(short returnCode, string message) => print("����������");
    public override void OnJoinRandomFailed(short returnCode, string message) => print("�淣����������");

    #endregion #END# 2. �κ� 

    #region #START# 3. ����

    #region #START# 3-1. ��ư
    public void ClickSlotButton(int slotIndex)                              // ���������� �������� ����
    {
        int userSlotState = GetRoomProperties(slotIndex);
        if (PhotonNetwork.IsMasterClient)                                   // �����϶��� ����
        {
            if (userSlotState == (int)USER_SlOT_STATE.CLOSE)                // ���������� �������¸� ���� ���ִ��ο� �ø���
            {
                SetRoomProperties(slotIndex, (int)USER_SlOT_STATE.OPEN);
                PhotonNetwork.CurrentRoom.MaxPlayers++;
            }
            else if (userSlotState == (int)USER_SlOT_STATE.OPEN)            // ���������� �������¸� �ݰ� ���ִ��ο� ���̰�
            {
                SetRoomProperties(slotIndex, (int)USER_SlOT_STATE.CLOSE);
                PhotonNetwork.CurrentRoom.MaxPlayers--;
            }
        }

        if (userSlotState > 0)      // ������ �ִ� ���� Ŭ���� ����â ���� �뵵�� ���(���� �ð��� ������?..�չ̷�)
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
    public void OnKickConfirmPanelButton(int slotIndex)                         // ����Ȯ�� â
    {
        int playerActorNum = GetRoomProperties(slotIndex);
        if (PhotonNetwork.CurrentRoom.masterClientId != PhotonNetwork.LocalPlayer.ActorNumber ||    // ������ �ƴҶ�
            playerActorNum == PhotonNetwork.CurrentRoom.masterClientId)        // (������) ������ �����Ϸ��� �ҋ�
        {
            return;
        }
        else if (playerActorNum == (int)USER_SlOT_STATE.OPEN ||                  // ������ OPEN OR CLOSE�̸� ����Ȯ��â X
                playerActorNum == (int)USER_SlOT_STATE.CLOSE)
        {
            return;
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)     // �÷��̾���ͳѹ� �� �� ������ �÷��̾� ã��
            {
                if (playerActorNum == PhotonNetwork.PlayerList[i].ActorNumber)
                {
                    // Debug.Log(PhotonNetwork.PlayerList[i].NickName + "�� ������");
                    kickPlayer = PhotonNetwork.PlayerList[i];
                }
            }
            kickConfirmMessage.text = kickPlayer.NickName + "���� ���� �Ͻðڽ��ϱ�?";
            kickConfirmPanel.gameObject.SetActive(true);
        }
    }
    public void OffKickConfirmPanelButton()
    {
        kickConfirmPanel.gameObject.SetActive(false);
    }
    public void KickPlayerButton()                                              // �÷��̾� ����.
    {
        PhotonNetwork.CloseConnection(kickPlayer);
        kickConfirmPanel.gameObject.SetActive(false);
    }
    public void ReadyButton()
    {
        //Debug.Log("ReadyButton ����");
        if (GetPlayerProperties("State") == (int)PLAYER_STATE.NOT_READY)
        {
            SetPlayerProperties("State", (int)PLAYER_STATE.READY);
            //Debug.Log("������ -> \"����\"");
        }
        else if (GetPlayerProperties("State") == (int)PLAYER_STATE.READY)
        {
            SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY);
            //Debug.Log("���� -> \"������\"");
        }
        else    // �������϶� �� ��찡 ����� (0525 ������ ���� �Ⱦ��� �������.) ->0526 �����غ��� �� �� ���±���� �غ��ư����..
        {
            Debug.Log("�� �������Ӥ�");
        }
    }
    public void StartButton()
    {
        int currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (readyPlayerNumber == currentRoomPlayerCount - 1)  // ���� ���� ��� ������ ���¶��
        {
            Debug.Log("������ �����մϴ�");

            //chatManager.ClearChat();
            //startButton.gameObject.SetActive(false);

            PhotonNetwork.LoadLevel(1);
        }
        else
        {
            chatManager.ChatRPC("<color=red>��� �÷��̾ �غ� ���� �ʾҽ��ϴ�.</color>");
        }
    }
    public void LeaveRoomButton()
    {
        Debug.Log("�� ����");
        PhotonNetwork.LeaveRoom();
        ShowPanel(lobbyPanel);
        //sceneEffect.StartFadeOut(0.3f);
    }
    #endregion #END# 3-1. ��ư

    #region #START# 3-2. ����, ���� ����
    public void SelectCharacter(int selectCharacterIndex)
    {
        //Debug.Log("SelectCharacter ����, ����ĳ���ε��� : " + selectCharacterIndex);
        this.selectCharacterIndex = selectCharacterIndex;
        SetPlayerProperties("Character", selectCharacterIndex);
    }
    public void SelectClass(int selectClassIndex)
    {
        this.selectClassIndex = selectClassIndex;
        SetPlayerProperties("Class", selectClassIndex);
    }
    #endregion #END# 3-2. ���� ���� ����

    #region #START# 3-3. ���� ����ȭ 

    #region ���� �ݹ��Լ�
    public override void OnPlayerEnteredRoom(Player newPlayer)              // �濡 ������ ������ �� ����Ǵ� �ݹ��Լ�
    {
        chatManager.ChatRPC("<color=yellow>" + newPlayer.NickName + " ���� �����ϼ̽��ϴ�</color>");

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < MAX_PLAYER_NUM; i++)
            {
                if (GetRoomProperties(i) == (int)USER_SlOT_STATE.OPEN)
                {
                    SetRoomProperties(i, newPlayer.ActorNumber);
                    Debug.Log("<color=yellow>" + i + "��° ĭ " + newPlayer.NickName + " ���� �����ϼ̽��ϴ�</color>");

                    break;
                }
            }
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)               // �濡 ������ ������ �� ����Ǵ� �ݹ��Լ�
    {
        chatManager.ChatRPC("<color=yellow>" + otherPlayer.NickName + " ���� �����ϼ̽��ϴ�</color>");

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < MAX_PLAYER_NUM; i++)
            {
                if (GetRoomProperties(i) == otherPlayer.ActorNumber)
                {
                    SetRoomProperties(i, (int)USER_SlOT_STATE.OPEN);
                    //Debug.Log("<color=yellow>" + i + "��° ĭ " + otherPlayer.NickName + " ���� �����ϼ̽��ϴ�</color>");

                    break;
                }
            }
        }

    }
    public override void OnLeftRoom()                               // �濡�� ������(��������) �����
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
        startButton.gameObject.SetActive(false);        // �濡�� �������� ��ŸƮ��ư off
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)    // ��������Ƽ�� ����� ���� (��������Ƽ�� �Է¿� �����̰� �־� �ݹ��Լ��� �̿��� �渮��������)
    {                                                                               // ���Դݰ� ����, Player�� ���Խ� �����
        foreach (object slotNumber in propertiesThatChanged.Keys)
        {
            if (slotNumber.GetType() == typeof(string))                             // Ű Ÿ���� ��Ʈ���϶��� ���� (��ƽ��ο� ����� ����ȵǵ���)
            {
                if ((string)slotNumber == "curScn")     // PhotonNetwork.AutomaticallySyncScene ������ ���� ���϶� �ѱ��
                    return;
                //Debug.Log((string)slotNumber + ", " + (int)propertiesThatChanged[(string)slotNumber]);
                RoomRenewal((string)slotNumber, (int)propertiesThatChanged[(string)slotNumber]);
            }
        }
        ShowPlayerCharacter();
        CountReadyPlayer();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)  // �뿡 �ִ� �÷��̾�������Ƽ ����� ���� 
    {                                                                                           // ������ �غ�, �غ�����, ����
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
    public override void OnMasterClientSwitched(Player newMasterClient)     // ���� ����� ����
    {
        if (GetPlayerProperties("State", newMasterClient) == (int)PLAYER_STATE.READY)
            SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY, newMasterClient);

        if (newMasterClient == PhotonNetwork.LocalPlayer)
            startButton.gameObject.SetActive(true);

        ShowHostTag(newMasterClient.ActorNumber);
    }

    #endregion �����ݹ��Լ�

    #region �÷��̾�� ���� ����
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
    private void RoomRenewal()                      // �� ��������� 1ȸ ����
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            testUserPanel[i].text = i.ToString() + " : " + GetRoomProperties(i).ToString(); // �׽�Ʈ��

            int slotState = GetRoomProperties(i);
            // Debug.Log("RoomRenewal, " + i.ToString() + "��°, ���ͳѹ� : " + slotState);
            if (slotState == (int)USER_SlOT_STATE.CLOSE)                    // ���������� ���������� 
            {
                userNickname[i].text = "";
                //readyTag[i].gameObject.SetActive(false);                    // �����±� OFF
                closeSlot[i].gameObject.SetActive(true);                    // Xǥ�� ON
            }
            else if (slotState == (int)USER_SlOT_STATE.OPEN)                // ���������� ����������
            {
                userNickname[i].text = "";
                //readyTag[i].gameObject.SetActive(false);                    // �����±� OFF
                closeSlot[i].gameObject.SetActive(false);                    // Xǥ�� ON

            }
            else                                                            // �������Կ� ������ �ִٸ�
            {
                closeSlot[i].gameObject.SetActive(false);                   // Xǥ�� OFF
                for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)     // �������԰� ������ ActorNum�� ���Ͽ� ������ �� �г����� ����
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
    private void RoomRenewal(string slotNumString, int slotState)   // ���� ���Ի��°� ����ɶ����� ����
    {
        int slotNum = int.Parse(slotNumString);
        testUserPanel[slotNum].text = slotNum.ToString() + " : " + GetRoomProperties(slotNum).ToString(); // �׽�Ʈ��

        if (slotState == (int)USER_SlOT_STATE.CLOSE)                // ���������� ���������� 
        {
            userNickname[slotNum].text = "";
            readyTag[slotNum].gameObject.SetActive(false);          // �����±� OFF
            closeSlot[slotNum].gameObject.SetActive(true);          // Xǥ�� ON
        }
        else if (slotState == (int)USER_SlOT_STATE.OPEN)            // ���������� ����������
        {
            userNickname[slotNum].text = "";
            readyTag[slotNum].gameObject.SetActive(false);          // �����±� OFF
            closeSlot[slotNum].gameObject.SetActive(false);         // Xǥ�� OFF
        }
        else                                                        // �������Կ� ����� ������
        {
            for (int j = 0; j < PhotonNetwork.CurrentRoom.PlayerCount; j++)     // �������԰� ������ ActorNum�� ���Ͽ� ������ �� �г����� ����
            {
                if (slotState == PhotonNetwork.PlayerList[j].ActorNumber)
                {
                    userNickname[slotNum].text = PhotonNetwork.PlayerList[j].NickName;
                    break;
                }
            }
        }
    }
    private void ShowHostTag(int hostActorNumber)       // hostTag�� �������׸� �����
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            if (hostActorNumber == userActorNumber)     // ������ �����ϰ�� HostTag ON
            {
                hostTag[i].gameObject.SetActive(true);
            }
            else
            {
                hostTag[i].gameObject.SetActive(false);
            }
        }
    }
    private void ShowReadyTag()                         // �� ��������� 1ȸ ���� 
    {
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            if (userActorNumber == PhotonNetwork.CurrentRoom.masterClientId)    // ���Կ� �ִ� ����� �����̸� 
            {
                readyTag[i].gameObject.SetActive(false);                        // �غ��±� OFF
            }
            else if (userActorNumber == (int)USER_SlOT_STATE.CLOSE || userActorNumber == (int)USER_SlOT_STATE.OPEN)
            {
                readyTag[i].gameObject.SetActive(false);                        // �غ��±� OFF
            }
            else
            {
                //Debug.Log("ShowReadyTag, " + i.ToString() + "��°, ���ͳѹ� : " + userActorNumber);
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
    private void ShowReadyTag(int playerActNumber, int playerState)             // ������ ���°� ����ɶ����� ���� 
    {
        //Debug.Log("ShowReadyTag ����");
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
    private void ShowPlayerCharacter()                          // �� ���������, ���� ���������� �ٲ���� ��
    {
        //print("ShowPlayerCharacter() ���� ����");
        for (int i = 0; i < MAX_PLAYER_NUM; i++)
        {
            int userActorNumber = GetRoomProperties(i);
            //Debug.Log("character, " +i.ToString() + "��°, ���ͳѹ� : " + userActorNumber);
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
                    //Debug.Log(i.ToString() + "��°, ĳ���� �ε��� : " + GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]));
                    playerCharacters[i] = Instantiate(characterPrefab[userCharacter], new Vector3(-20 + (2 * i), 0, -5), Quaternion.Euler(0, 180, 0));
                }
            }
        }
        
    }
    private void ShowPlayerCharacter(int playerActNumber, int selectCharacterIndex)  // �÷��̾ ĳ���͸� �مf���� �ٲ�              
    {

        //Debug.Log("ShowReadyTag ���ӽ���");
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
    #endregion �÷��̾�� ���� ����

    #endregion #END# 3-3. ���� ����ȭ

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

    #endregion #END# 3. ����

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


    [ContextMenu("����")]
    void Info()
    {
        if (PhotonNetwork.InRoom)
        {
            print("���� ���� �г��� : " + PhotonNetwork.LocalPlayer);
            print("���� �� �̸� : " + PhotonNetwork.CurrentRoom.Name);
            print("���� �� �ο��� : " + PhotonNetwork.CurrentRoom.PlayerCount);
            print("���� �� �ִ��ο��� : " + PhotonNetwork.CurrentRoom.MaxPlayers);
            string playerStr = "�濡 �ִ� �÷��̾� ��� : ";
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) playerStr += PhotonNetwork.PlayerList[i].NickName + ", ";
            print(playerStr);
        }
        else
        {
            print("���� ���� �г��� : " + PhotonNetwork.LocalPlayer);
            print("������ �ο� �� : " + PhotonNetwork.CountOfPlayers);
            print("�� ���� : " + PhotonNetwork.CountOfRooms);
            print("��� �濡 �ִ� �ο� �� : " + PhotonNetwork.CountOfPlayersInRooms);
            print("�κ� �ִ���? : " + PhotonNetwork.InLobby);
            print("����ƴ���? : " + PhotonNetwork.IsConnected);
        }
    }

}

//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//{

//    if (stream.IsWriting && PhotonNetwork.InRoom)
//    {
//        stream.SendNext(readyPlayerNumber);
//        Debug.Log("����ȭ ������ ����");
//    }
//    else
//    {
//        readyPlayerNumber = (int)stream.ReceiveNext();
//        Debug.Log("����ȭ ������ ����");
//    }

//}

//public override void OnJoinedRoom()         // ���� ���������� �����ϸ� ����Ǵ� �ݹ��Լ�
//{
//    testNowState.text = "�� ���ͳѹ�: " + PhotonNetwork.LocalPlayer.ActorNumber.ToString();

//    //for (int i = 0; i < MAX_PLAYER_NUM; i++)                            // �� ó������ ��, ���� ������Ƽ�� �޾Ƽ�
//    //{
//    //    int userSlotState = GetRoomProperties(i);
//    //    if (userSlotState == (int)USER_SlOT_STATE.CLOSE)             // Close �����Ͻ�, ����ǥ��(Xǥ��) ON
//    //    {
//    //        closeSlot[i].SetActive(true);
//    //    }
//    //    else if (userSlotState == (int)USER_SlOT_STATE.OPEN)         // OPEN �����Ͻ�, ����ǥ��(Xǥ��) OFF
//    //    {
//    //        closeSlot[i].SetActive(false);
//    //    }

//    //}

//    ResetRoomSetting();                                                 // �游��⼳���ϴٰ� ��������� ������ �ʱ�ȭ����
//    RoomRenewal();

//    OffAllPanel();
//    roomPanel.SetActive(true);
//}



//void SendEvent()
//{

//    object[] content = new object[] { playerUIList[0], playerUIList[1], playerUIList[2], playerUIList[3], playerUIList[4]};
//    PhotonNetwork.RaiseEvent(0, content, RaiseEventOptions.Default, SendOptions.SendUnreliable);
//    Debug.Log("�̺�Ʈ ����");
//}
//public void OnEvent(EventData photonEvent)
//{
//    Debug.Log("�̺�Ʈ ����");
//    if (photonEvent.Code == 0)
//    {
//        Debug.Log("�̺�Ʈ ���� �ڵ� 0!!");
//        object[] data = (object[])photonEvent.CustomData;
//        for (int i = 0; i < data.Length; i++) playerUIList[i] = (PlayerUIList)data[i];
//        //for (int i = 0; i < MAX_PLAYER_NUM; i++)
//        //{
//        //    Debug.Log(i + "��°, ������Ʈ : " + playerUIList[i].userPanelState);
//        //    if (playerUIList[i].player != null)
//        //        Debug.Log(i + "��°, �г��� : " + playerUIList[i].player.NickName);
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
//    //ChatRPC("<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
//    for (int i = 0; i < playerList.Length; i++)
//    {
//        if (playerList[i] == null)
//        {
//            playerList[i] = newPlayer;
//            break;
//        }
//    }
//    Debug.Log("�� ����");
//    //photonview.RPC("PlayerListSync", RpcTarget.All, playerList);
//    Debug.Log("<color=yellow>" + newPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");

//}

//public override void OnPlayerLeftRoom(Player otherPlayer)
//{
//    RoomRenewal();
//    //ChatRPC("<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");
//    for (int i = 0; i < playerList.Length; i++)
//    {
//        if (playerList[i] == otherPlayer)
//        {
//            playerList[i] = null;
//            break;
//        }
//    }
//    Debug.Log("�� ����");
//    //photonview.RPC("PlayerListSync", RpcTarget.All, );
//    Debug.Log("<color=yellow>" + otherPlayer.NickName + "���� �����ϼ̽��ϴ�</color>");

//}



//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
//{

//        if (stream.IsWriting && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
//        {
//            stream.SendNext(playerList);
//            Debug.Log("����ȭ ������ ����");
//        }
//        else
//        {
//            playerList = (Player[])stream.ReceiveNext();
//            Debug.Log("����ȭ ������ ����");
//        }

//}



//// UI���� �� ������.
//byte maxPlayers = byte.Parse(m_dropdown_RoomMaxPlayers.options[m_dropdown_RoomMaxPlayers.value].text); // ��Ӵٿ�� �� ������.
//byte maxTime = byte.Parse(m_dropdown_MaxTime.options[m_dropdown_MaxTime.value].text);

//// �� �ɼ� ������ �� �״�� HashTable ������ش�. �̰ɷ� ���͸��ϴ� ���̴�.
//ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable() { { "maxTime", maxTime } };

//// �� ������ ��, ���͸��� �ɼ� �־��ֱ�.
//// maxPlayers�� ���� ���̴ϱ�, ���� ������ ���� ���ϴ�.
//PhotonNetwork.JoinRandomRoom(customProperties, maxPlayers);



// ���� �Ⱦ��� ��

//public void CreateRoom()
//{

//    PhotonNetwork.CreateRoom(roomInput.text, new RoomOptions { MaxPlayers = 2 });
//}



//public void JoinLobby() => PhotonNetwork.JoinLobby();
//public override void OnJoinedLobby() => print("�κ����ӿϷ�");