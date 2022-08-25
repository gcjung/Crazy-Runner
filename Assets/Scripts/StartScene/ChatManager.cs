using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class ChatManager : MonoBehaviour
{
    public List<Text> chatTextList = new List<Text>();
    public InputField chatInput;
    public RectTransform chatContent;
    public ScrollRect scrollRect;

    public Text chatTextPrefab;
    private PhotonView photonView;
    
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        chatInput.onValueChanged.AddListener(delegate { MaxStringCheck(); });
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if ((chatInput.text != ""))
            {
                SendChat();
            }
            chatInput.ActivateInputField();
        }
    }
    public void MaxStringCheck()
    {
        if (chatInput.text.Length >= 70)
        {
            ChatRPC("<color=red>70자를 초과하여 더 이상 글자를 입력할 수 없습니다.</color>");
            //print("<color=red>70자를 초과하여 더 이상 글자를 입력할 수 없습니다.</color>");
        }
    }
    public void SendChat()
    {
        photonView.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + chatInput.text);
        chatInput.text = "";
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    public void ChatRPC(string msg)
    {
        int maxChatContentSize = 303;
        if (chatContent.sizeDelta.y >= maxChatContentSize)          // 채팅창의크기가 일정 크기 이상이면
            chatContent.pivot = new Vector2(0, 0);                  // 채팅을 치면 아래서부터 올라가도록 해줌

        msg = msg.Replace(' ', '\u00A0');
        
        bool isInput = false;
        for (int i = 0; i < chatTextList.Count; i++)
            if (chatTextList[i].text == "")
            {
                isInput = true;
                chatTextList[i].text = msg;
                break;
            }

        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            if (chatTextList.Count < 30)                // 30개의 채팅 텍스트까지 생성하기
            {
                //Text temp;
                chatTextList.Add(Instantiate(chatTextPrefab, chatContent));
                //temp.name = "text" + (chatTextList.Count - 1).ToString();
                chatTextList[chatTextList.Count - 1].text = msg;
            }
            else                                        // 한 칸씩 채팅 위로 올리기
            {
                for (int i = 1; i < chatTextList.Count; i++) chatTextList[i - 1].text = chatTextList[i].text;
                chatTextList[chatTextList.Count - 1].text = msg;
            }
        }
    }
    public void ClearChat()
    {
        Debug.Log("ClearChat 실행");
        photonView.RPC(nameof(ChatTextClear), RpcTarget.All);
    }
    
    public void ChatTextClear()                         // 방나갔을 시 채팅창을 초기상태로 만들어줌
    {
        chatContent.pivot = new Vector2(0, 1);          // 채팅이 위에서 부터 내려오도록 해줌

        int originTextCount = 1;                        // 프리팹으로 늘린 채팅텍스트 말고 원래 있는 채팅텍스트개수
        for (int i = 0; i < chatTextList.Count; i++)
        {
            if (i < originTextCount)
            {
                chatTextList[i].text = "";
            }
            else
            {
                Destroy(chatTextList[i].gameObject);
            }
        }
        
        if(chatTextList.Count > originTextCount)        // 채팅텍스트프리팹을 더 생성했을 경우 
        {
            chatTextList.RemoveRange(originTextCount, chatTextList.Count - originTextCount);    // 리스트에서 제거
        }
    }
}
