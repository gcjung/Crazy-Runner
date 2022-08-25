using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private NetworkManager networkManager;
    public string nickName;
    public bool isFirstRun = true;
    private void Awake()
    {
        Debug.Log("GameManager Awake ����");
        networkManager = FindObjectOfType<NetworkManager>();
        if (instance == null)   // ���� �����
        {
            instance = this;

            Screen.SetResolution(960, 540, false);
            networkManager.OnDisconnectPanel();
        }
        else
        {
            instance.isFirstRun = false;
            networkManager.LeaveRoomButton();
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        Debug.Log("Start() ����");
    }
}
