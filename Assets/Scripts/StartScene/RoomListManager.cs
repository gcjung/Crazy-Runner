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
        // 최대페이지
        maxPage = (roomList.Count % roomButton.Length == 0) ? roomList.Count / roomButton.Length : roomList.Count / roomButton.Length + 1;

        // 이전, 다음버튼 활성화/비활성화
        previousButton.interactable = (currentPage <= 1) ? false : true;
        nextButton.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지의 첫번째 버튼에 맞는 인덱스값 대입
        pageInitIndex = (currentPage - 1) * roomButton.Length;

        // 방이름, 방인원, 방상태 텍스트 띄우기
        for (int i = 0; i < roomButton.Length; i++)
        {
            if (pageInitIndex + i < roomList.Count)
            {
                roomButton[i].interactable = true;
                roomName[i].text = roomList[pageInitIndex + i].Name;
                roomPlayerCount[i].text = roomList[pageInitIndex + i].PlayerCount + "/" + roomList[pageInitIndex + i].MaxPlayers;
                roomState[i].text = (roomList[pageInitIndex + i].IsOpen) ? "대기중" : "게임중";
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
            roomState[i].text = (pageInitIndex + i < roomList.Count) ? roomList[pageInitIndex + i].IsOpen == true ?  "대기중" : "게임중" : "";
        }
     */


    // ◀버튼 -2, ▶버튼 -1, 버튼 숫자
    public void OnButtonClick(int num)
    {
        if (num == -2) --currentPage;       // 이전 페이지로 이동
        else if (num == -1) ++currentPage;  // 다음 페이지로 이동
        else PhotonNetwork.JoinRoom(roomList[pageInitIndex + num].Name);    // 선택한 방에 접속

        RoomListRenewal();                  // 페이지에 맞춰 방리스트 리뉴얼
    }
    
    public override void OnRoomListUpdate(List<RoomInfo> photonRoomList)
    {
        for (int i = 0; i < photonRoomList.Count; i++)
        {
            if (!photonRoomList[i].RemovedFromList)   // [닫힌방, 풀방, 숨긴방] 아니라면
            {
                if (!roomList.Contains(photonRoomList[i])) roomList.Add(photonRoomList[i]); // 룸리스트에 없을시 방리스트에 추가하기
                else roomList[roomList.IndexOf(photonRoomList[i])] = photonRoomList[i];     // 룸리스트에 있다면 다시 할당
            }
            // [닫힌방, 풀방, 숨긴방]지우기
            else if (roomList.IndexOf(photonRoomList[i]) != -1) roomList.RemoveAt(roomList.IndexOf(photonRoomList[i])); 
        }

        RoomListRenewal();          // 방들의 정보들을 텍스트로 보여줌
    }

}
