using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomListManager : MonoBehaviourPunCallbacks
{
    public Button[] roomButton;
    public Button previousButton, nextButton;
    public List<RoomInfo> roomList = new List<RoomInfo>();

    private Text[] roomName = new Text[4];
    private Text[] roomPlayerCount = new Text[4];
    private Text[] roomState = new Text[4];
    private int currentPage = 1, maxPage, pageInitIndex;

    private void Awake()
    {
        for (int i = 0; i < roomButton.Length; i++)
        {
            roomName[i] = roomButton[i].transform.GetChild(0).GetComponent<Text>();
            roomPlayerCount[i] = roomButton[i].transform.GetChild(1).GetComponent<Text>();
            roomState[i] = roomButton[i].transform.GetChild(2).GetComponent<Text>();
        }
        
    }
    private void RoomListRenewal()
    {
        // �ִ�������
        maxPage = (roomList.Count % roomButton.Length == 0) ? roomList.Count / roomButton.Length : roomList.Count / roomButton.Length + 1;

        // ����, ������ư Ȱ��ȭ/��Ȱ��ȭ
        previousButton.interactable = (currentPage <= 1) ? false : true;
        nextButton.interactable = (currentPage >= maxPage) ? false : true;

        // �������� ù��° ��ư�� �´� �ε����� ����
        pageInitIndex = (currentPage - 1) * roomButton.Length;

        // ���̸�, ���ο�, ����� �ؽ�Ʈ ����
        for (int i = 0; i < roomButton.Length; i++)
        {
            if (pageInitIndex + i < roomList.Count)
            {
                roomButton[i].interactable = true;
                roomName[i].text = roomList[pageInitIndex + i].Name;
                roomPlayerCount[i].text = roomList[pageInitIndex + i].PlayerCount + "/" + roomList[pageInitIndex + i].MaxPlayers;
                roomState[i].text = (roomList[pageInitIndex + i].IsOpen) ? "�����" : "������";
            }
            else
            {
                roomButton[i].interactable = false;
                roomName[i].text = "";
                roomPlayerCount[i].text = "";
                roomState[i].text = "";
            }
        }
    }
    /*
        for (int i = 0; i < roomButton.Length; i++)
        {
            roomButton[i].interactable = (pageInitIndex + i < roomList.Count) ? true : false;
            roomName[i].text = (pageInitIndex + i < roomList.Count) ? roomList[pageInitIndex + i].Name : "";
            roomPlayerCount[i].text = (pageInitIndex + i < roomList.Count) ? roomList[pageInitIndex + i].PlayerCount + "/" + roomList[pageInitIndex + i].MaxPlayers : "";
            roomState[i].text = (pageInitIndex + i < roomList.Count) ? roomList[pageInitIndex + i].IsOpen == true ?  "�����" : "������" : "";
        }
     */


    // ����ư -2, ����ư -1, ��ư ����
    public void OnButtonClick(int num)
    {
        if (num == -2) --currentPage;       // ���� �������� �̵�
        else if (num == -1) ++currentPage;  // ���� �������� �̵�
        else PhotonNetwork.JoinRoom(roomList[pageInitIndex + num].Name);    // ������ �濡 ����

        RoomListRenewal();                  // �������� ���� �渮��Ʈ ������
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> photonRoomList)
    {
        for (int i = 0; i < photonRoomList.Count; i++)
        {
            if (!photonRoomList[i].RemovedFromList)   // [������, Ǯ��, �����] �ƴ϶��
            {
                if (!roomList.Contains(photonRoomList[i])) roomList.Add(photonRoomList[i]); // �븮��Ʈ�� ������ �渮��Ʈ�� �߰��ϱ�
                else roomList[roomList.IndexOf(photonRoomList[i])] = photonRoomList[i];     // �븮��Ʈ�� �ִٸ� �ٽ� �Ҵ�
            }
            // [������, Ǯ��, �����]�����
            else if (roomList.IndexOf(photonRoomList[i]) != -1) roomList.RemoveAt(roomList.IndexOf(photonRoomList[i])); 
        }

        RoomListRenewal();          // ����� �������� �ؽ�Ʈ�� ������
    }

}
