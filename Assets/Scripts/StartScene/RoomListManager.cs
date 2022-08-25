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

    private int currentPage = 1, maxPage, pageInitIndex;

    private void roomListRenewal()
    {
        // 최대페이지
        maxPage = (roomList.Count % roomButton.Length == 0) ? roomList.Count / roomButton.Length : roomList.Count / roomButton.Length + 1;

        // 이전, 다음버튼
        previousButton.interactable = (currentPage <= 1) ? false : true;
        nextButton.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지의 제일 첫번째 버튼에 맞는 인덱스값 대입
        pageInitIndex = (currentPage - 1) * roomButton.Length;

        for (int i = 0; i < roomButton.Length; i++)
        {
            roomButton[i].interactable = (pageInitIndex + i < roomList.Count) ? true : false;
            roomButton[i].transform.GetChild(0).GetComponent<Text>().text = (pageInitIndex + i < roomList.Count) ? roomList[pageInitIndex + i].Name : "";
            roomButton[i].transform.GetChild(1).GetComponent<Text>().text = (pageInitIndex + i < roomList.Count) ? roomList[pageInitIndex + i].PlayerCount + "/" + roomList[pageInitIndex + i].MaxPlayers : "";
            roomButton[i].transform.GetChild(2).GetComponent<Text>().text = (pageInitIndex + i < roomList.Count) ? (roomList[pageInitIndex + i].IsOpen == true) ?  "대기중" : "게임중" : "";
        }
    }

    // ◀버튼 -2 , ▶버튼 -1 , 버튼 숫자
    public void ButtonClick(int num)
    {
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(roomList[pageInitIndex + num].Name);

        roomListRenewal();
    }
    public override void OnRoomListUpdate(List<RoomInfo> tempRoomList)
    {
        //Debug.Log("OnRoomListUpdate 실행!!");
        int roomCount = tempRoomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!tempRoomList[i].RemovedFromList)
            {
                if (!roomList.Contains(tempRoomList[i])) roomList.Add(tempRoomList[i]);
                else roomList[roomList.IndexOf(tempRoomList[i])] = tempRoomList[i];
            }
            else if (roomList.IndexOf(tempRoomList[i]) != -1) roomList.RemoveAt(roomList.IndexOf(tempRoomList[i]));
        }
        
        roomListRenewal();
    }


    //[ContextMenu("리스트추가")]
    //void ListAdd() { myList.Add("뚝배기"); Start(); }


    //[ContextMenu("리스트제거")]
    //void ListRemove() { myList.RemoveAt(0); Start(); }
}
