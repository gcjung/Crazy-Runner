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
    }
    public override void OnConnectedToMaster()      // �������ӽ� ����Ǵ� �ݹ��Լ� (�濡�� �������� �� �Լ����� �����)
    {
        PhotonNetwork.JoinLobby();                  // �������ӽ� �ٷ� ����Ʈ�κ�� ����
    }
    public override void OnJoinedLobby()            // �κ����ӽ� ����Ǵ� �ݹ��Լ�
    {
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
        maxRoomPlayer = (maxPersonNum + 2);
    }

    private string[] randomRoomName = new string[4] { "���� ���� ��ȯ�� ����", "���õ� �޸���!", "�Բ� �޷���~", "���� ���ϸ� ����!!" };
    public void DecisionCreateRoomButton()                          // �漳���� ����� ��ư Ŭ����
    {
        int max = maxRoomPlayer;
        int hostActorNumber = 1;

        RoomOptions roomOptions = new RoomOptions();                // ������ ���� �ɼǼ���
        roomOptions.MaxPlayers = (byte)maxRoomPlayer;               // ���ִ��ο� ����
        roomOptions.CustomRoomProperties = new Hashtable()          // �� �� �������Կ� ���Ի���(����, ����)�� ���������� ���� �ؽ����̺����
        {
            {"0", hostActorNumber }, {"1", (int)USER_SlOT_STATE.OPEN},
            {"2", 3 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"3", 4 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE},
            {"4", 5 <= max ? (int)USER_SlOT_STATE.OPEN : (int)USER_SlOT_STATE.CLOSE}
        };

        if (roomNameInput.text.Equals(""))                          // ���̸��� ������ ��
            PhotonNetwork.CreateRoom(randomRoomName[Random.Range(0, 4)], roomOptions);
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
    #endregion �游���
    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();
    public override void OnJoinedRoom()         // ���� ����� ����Ǵ� �ݹ��Լ�
    {
        PhotonNetwork.AutomaticallySyncScene = true;    // ������ ���� �̵��ϸ� ��� ���� �̵���

        ResetRoomSetting();                             // �� ����⼳�� �ʱ�ȭ����
        StartCoroutine(nameof(DelayOnJoinedRoom));      // ���� ���� ǥ�ø� ���� (�غ����, ����, ĳ����)         
    }
    private IEnumerator DelayOnJoinedRoom()     // ĳ���� ��������ȭ �� �� �Լ� �����ϵ��� �����̸� ��
    {
        yield return new WaitForSeconds(0.05f);
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        ShowHostTag(PhotonNetwork.CurrentRoom.MasterClientId);  // ���� ǥ��
        ShowReadyTag();                         // �غ���� ǥ��
        ShowPlayerCharacter();                  // �÷��̾� ĳ���� ǥ��
        RoomRenewal();                          // ���������� ����, ���� ǥ��
        
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
    public void OnKickConfirmPanelButton(int slotIndex)                     // ����Ȯ�� â
    {
        int playerActorNum = GetRoomProperties(slotIndex);
        if (PhotonNetwork.CurrentRoom.masterClientId != PhotonNetwork.LocalPlayer.ActorNumber ||    // ������ �ƴҶ�
            playerActorNum == PhotonNetwork.CurrentRoom.masterClientId)     // (������) ������ �����Ϸ��� �ҋ�
        {
            return;
        }
        else if (playerActorNum == (int)USER_SlOT_STATE.OPEN ||             // ������ OPEN OR CLOSE�̸� ����Ȯ��â X
                playerActorNum == (int)USER_SlOT_STATE.CLOSE)
        {
            return;
        }
        else
        {
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++) // �÷��̾���ͳѹ� �� �� ������ �÷��̾� ã��
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
        if (GetPlayerProperties("State") == (int)PLAYER_STATE.NOT_READY)
            SetPlayerProperties("State", (int)PLAYER_STATE.READY);
        else if (GetPlayerProperties("State") == (int)PLAYER_STATE.READY)
            SetPlayerProperties("State", (int)PLAYER_STATE.NOT_READY);
    }

    /*
        else    // �������϶� �� ��찡 ����� (0525 ������ ���� �Ⱦ��� �������.) ->0526 �����غ��� �� �� ���±���� �غ��ư����..
        {
            Debug.Log("�� �������Ӥ�");
        }
    */
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
    public override void OnPlayerEnteredRoom(Player newPlayer)  // �濡 �ٸ� ������ ������ �� ����Ǵ� �ݹ��Լ�
    {
        chatManager.ChatRPC("<color=yellow>" + newPlayer.NickName + " ���� �����ϼ̽��ϴ�</color>");

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < MAX_PLAYER_NUM; i++)
            {
                if (GetRoomProperties(i) == (int)USER_SlOT_STATE.OPEN)
                {
                    SetRoomProperties(i, newPlayer.ActorNumber);
                    //Debug.Log("<color=yellow>" + i + "��° ĭ " + newPlayer.NickName + " ���� �����ϼ̽��ϴ�</color>");

                    break;
                }
            }
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)   // �濡 �ٸ� ������ ������ �� ����Ǵ� �ݹ��Լ�
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

    public override void OnLeftRoom()               // �濡�� ������ ������(��������) �����
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
        startButton.gameObject.SetActive(false);    // �濡�� �������� ��ŸƮ��ư off
    }
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)    // ��������Ƽ�� ����� ����                        (��������Ƽ�� �Է¿� �����̰� �־� �ݹ��Լ��� �̿��� �渮��������)
    {                                                                               // ���Դݰ� ����, Player�� ������� �����
        foreach (object slotNumber in propertiesThatChanged.Keys)
        {
            if (slotNumber.GetType() == typeof(string))     // �������� ���� �ÿ��� ���� ('0' ~ '4'),                              Ű Ÿ���� ��Ʈ���϶��� ���� (��ƽ��ο� ����� ����ȵǵ���)
            {
                if ((string)slotNumber == "curScn") return; // PhotonNetwork.AutomaticallySyncScene������ ����.

                ShowPlayerCharacter((string)slotNumber);    // �÷��̾� ĳ���� ǥ��
                CountReadyPlayer();                         // �غ����� �÷��̾� �� Ȯ��
                RoomRenewal((string)slotNumber, (int)propertiesThatChanged[(string)slotNumber]); // Xǥ��, �غ�ǥ��, �г���ǥ��
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)  // �뿡 �ִ� �÷��̾�������Ƽ ����� ���� 
    {                                                                                           // ������ �غ����, ĳ���ͺ���
        foreach (object key in changedProps.Keys)
        {
            if ((string)key == "State")             // �÷��̾��� �غ����
            {
                ShowReadyTag(targetPlayer.ActorNumber, (int)GetPlayerProperties((string)key, targetPlayer));
            }
            else if ((string)key == "Character")    // �÷��̾��� ����
            {
                ShowPlayerCharacter(targetPlayer.ActorNumber, (int)GetPlayerProperties((string)key, targetPlayer));
            }
        }

        CountReadyPlayer();
    }
    //Debug.Log("key : " + (string)key + ", state : " + (int)GetPlayerProperties((string)key, targetPlayer));

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
    private void CountReadyPlayer()         // �����غ� ���� �ο� �ľ� (���常 �����) 
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
            int slotState = GetRoomProperties(i);

            if (slotState == (int)USER_SlOT_STATE.CLOSE)            // ���������� ���������� 
            {
                userNickname[i].text = "";
                closeSlot[i].gameObject.SetActive(true);            // Xǥ�� ON
            }
            else if (slotState == (int)USER_SlOT_STATE.OPEN)        // ���������� ����������
            {
                userNickname[i].text = "";
                closeSlot[i].gameObject.SetActive(false);           // Xǥ�� OFF

            }
            else                                                    // �������Կ� ������ �ִٸ�
            {
                closeSlot[i].gameObject.SetActive(false);           // Xǥ�� OFF
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
    private void ShowReadyTag(int playerActNumber, int playerState)    // ������ ���°� ����ɶ����� ���� 
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
    private void ShowPlayerCharacter()          // �� ��������� 1ȸ�� �����
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
                        Debug.Log(i.ToString() + "��°, ĳ���� �ε��� : " + GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]));
                        playerCharacters[i] = Instantiate(characterPrefab[userCharacter], new Vector3(-20 + (2 * i), 0, -5), Quaternion.Euler(0, 180, 0));
                    }
                }
            }
        }
    }
    private void ShowPlayerCharacter(string slotNumberString)   // ���� ���������� �ٲ���� �� ����� (��������Ƽ)
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

    private void ShowPlayerCharacter(int playerActNumber, int selectCharacterIndex)  // �÷��̾ ĳ���͸� �مf���� ����� (�÷��̾� ������Ƽ)       
    {
        //Debug.Log("ShowReadyTag ���ӽ���");
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




}


/* 08/27 ���� ��
    private void ShowPlayerCharacter()                  // �� ���������, ���� ���������� �ٲ���� ��
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
                    //Debug.Log(i.ToString() + "��°, ĳ���� �ε��� : " + GetPlayerProperties("Character", PhotonNetwork.PlayerList[j]));
                    playerCharacters[i] = Instantiate(characterPrefab[userCharacter], new Vector3(-20 + (2 * i), 0, -5), Quaternion.Euler(0, 180, 0));
                }
            }
        }
        
    }

 */