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
            ChatRPC("<color=red>70�ڸ� �ʰ��Ͽ� �� �̻� ���ڸ� �Է��� �� �����ϴ�.</color>");
            //print("<color=red>70�ڸ� �ʰ��Ͽ� �� �̻� ���ڸ� �Է��� �� �����ϴ�.</color>");
        }
    }
    public void SendChat()
    {
        photonView.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + chatInput.text);
        chatInput.text = "";
    }

    [PunRPC] // RPC�� �÷��̾ �����ִ� �� ��� �ο����� �����Ѵ�
    public void ChatRPC(string msg)
    {
        int maxChatContentSize = 303;
        if (chatContent.sizeDelta.y >= maxChatContentSize)          // ä��â��ũ�Ⱑ ���� ũ�� �̻��̸�
            chatContent.pivot = new Vector2(0, 0);                  // ä���� ġ�� �Ʒ������� �ö󰡵��� ����

        msg = msg.Replace(' ', '\u00A0');
        
        bool isInput = false;
        for (int i = 0; i < chatTextList.Count; i++)
            if (chatTextList[i].text == "")
            {
                isInput = true;
                chatTextList[i].text = msg;
                break;
            }

        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            if (chatTextList.Count < 30)                // 30���� ä�� �ؽ�Ʈ���� �����ϱ�
            {
                //Text temp;
                chatTextList.Add(Instantiate(chatTextPrefab, chatContent));
                //temp.name = "text" + (chatTextList.Count - 1).ToString();
                chatTextList[chatTextList.Count - 1].text = msg;
            }
            else                                        // �� ĭ�� ä�� ���� �ø���
            {
                for (int i = 1; i < chatTextList.Count; i++) chatTextList[i - 1].text = chatTextList[i].text;
                chatTextList[chatTextList.Count - 1].text = msg;
            }
        }
    }
    public void ClearChat()
    {
        Debug.Log("ClearChat ����");
        photonView.RPC(nameof(ChatTextClear), RpcTarget.All);
    }
    
    public void ChatTextClear()                         // �泪���� �� ä��â�� �ʱ���·� �������
    {
        chatContent.pivot = new Vector2(0, 1);          // ä���� ������ ���� ���������� ����

        int originTextCount = 1;                        // ���������� �ø� ä���ؽ�Ʈ ���� ���� �ִ� ä���ؽ�Ʈ����
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
        
        if(chatTextList.Count > originTextCount)        // ä���ؽ�Ʈ�������� �� �������� ��� 
        {
            chatTextList.RemoveRange(originTextCount, chatTextList.Count - originTextCount);    // ����Ʈ���� ����
        }
    }
}
