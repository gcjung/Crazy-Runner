using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//[System.Serializable]
public class ClassDescription
{
    public void SetStat(float hp, float maxSpeed, float acceleration)
    {
        this.hp = hp;
        this.maxSpeed = maxSpeed;
        this.acceleration = acceleration;
    }
    public void SetDescriptText(string job, string summary, string skillDescript, string synergyCounterDescript)
    {
        this.job = job;
        this.summary = summary;
        this.skillDescript += skillDescript;
        this.synergyCounterDescript += synergyCounterDescript;
    }

    public string job = "";
    public string summary = "";
    public float hp = 0;
    public float maxSpeed = 0;
    public float acceleration = 0;
    public string skillDescript = "<size=45>��ų - </size>";
    public string synergyCounterDescript = "<size=45><color=blue>S</color>nergy&<color=red>C</color>ounter</size>\n\n";

    // ���α׷�����  
    /* �ִ�ӵ�
    // �����ӵ�
    // ���ӵ�
    // ü�� */

    // ĳ������ 
    // ��ų 
    // �ó��� �� ī���� ĳ��
}

public class ClassSelectManager : MonoBehaviour
{
    public GameObject[] descriptPage = new GameObject[2];
    public GameObject content;

    // descriptionPage0
    private Text className0;
    private Text classSummary;
    private Image hpBar;
    private Image maxSpeedBar;
    private Image accelerationBar;
    // descriptionPage1
    private Text className1;
    private Text skillDescript;
    private Text synergyCounterDescript;

    private ClassDescription[] classDescriptions = new ClassDescription[10];    // �ϴ� 10���� ��������.. ���� 3�������

    private NetworkManager networkManager;


    private void Start()
    {
        // descriptionPage0
        className0      = descriptPage[0].transform.GetChild(0).GetComponent<Text>();
        classSummary    = descriptPage[0].transform.GetChild(1).GetComponent<Text>();
        hpBar           = descriptPage[0].transform.GetChild(3).GetChild(0).GetComponent<Image>();
        maxSpeedBar     = descriptPage[0].transform.GetChild(4).GetChild(0).GetComponent<Image>();
        accelerationBar = descriptPage[0].transform.GetChild(5).GetChild(0).GetComponent<Image>();
        // descriptionPage1
        className1      = descriptPage[1].transform.GetChild(0).GetComponent<Text>();
        skillDescript   = descriptPage[1].transform.GetChild(1).GetComponent<Text>();
        synergyCounterDescript = descriptPage[1].transform.GetChild(2).GetComponent<Text>();

        networkManager = FindObjectOfType<NetworkManager>();

        for (int i = 0; i < classDescriptions.Length; i++)
        {
            classDescriptions[i] = new ClassDescription();
        }

        InitClassDescriptions();
    }

    public void SelectClassButton(int slotNumber)
    {
        //networkManager.SelectClass(slotNumber);

        className0.text = classDescriptions[slotNumber].job;
        classSummary.text = classDescriptions[slotNumber].summary;
        hpBar.fillAmount = (classDescriptions[slotNumber].hp / 100f);
        maxSpeedBar.fillAmount = (classDescriptions[slotNumber].maxSpeed / 100f);
        accelerationBar.fillAmount = (float)(classDescriptions[slotNumber].acceleration / 100f);

        className1.text = classDescriptions[slotNumber].job;
        skillDescript.text = classDescriptions[slotNumber].skillDescript;
        synergyCounterDescript.text = classDescriptions[slotNumber].synergyCounterDescript;
    }

    public void ChangeDescriptPageButton(int changePage)    // ���� 
    {
        if (changePage == 0)
        {
            descriptPage[0].SetActive(true);
            descriptPage[1].SetActive(false);
        }
        else if (changePage == 1)
        {
            descriptPage[0].SetActive(false);
            descriptPage[1].SetActive(true);
        }
    }
    //ClassDescription(string job, string summary, string skillDescript, string synergyCounterDescript)
    private void InitClassDescriptions()
    {
        // �Ϲ���
        classDescriptions[0].SetStat(60, 60, 50);
        classDescriptions[0].SetDescriptText("�Ϲ���", "�⺻������ ���� �뷱���� Ŭ����",
            "<size=45>���� ���</size>\n\n����ȿ���� ������ Ǯ���ϴ�.", "<color=blue>���󼱼�</color> ������ �ִ�ӷ��� �ö󰩴ϴ�.\n<color=red>�׷�����Ʈ</color>�� ���� ���ظ� �� �Խ��ϴ�");
        // �׷�����Ʈ
        classDescriptions[1].SetStat(80, 50, 30);
        classDescriptions[1].SetDescriptText("�׷�����Ʈ", "ü���� ���� ������ Ŭ����",
            "<size=45>��ź ��ġ</size>\n\n��ź�� ��ġ�Ͽ� ���ظ� �����ϴ�.", "<color=blue>�Ϲ���</color>�� ��ź��ġ�� ���ظ� �� �Խ��ϴ�.\n<color=red>����</color>�� ������ �ִ�ӷ��� �������ϴ�.");
        // ���󼱼�
        classDescriptions[2].SetStat(25, 90, 70);
        classDescriptions[2].SetDescriptText("���󼱼�", "�ִ�ӵ��� ���ӵ��� ������ ü���� �ſ� ���� ���̸���ũ ���ǵ��� Ŭ����",
            "<size=45>��� ����</size>\n\n�������� ������ ª�� �ð� �ӵ��� �������ϴ�.", "<color=blue>�Ϲ���</color> ������ �ִ�ӷ��� �ö󰩴ϴ�.\n<color=red>����</color>");


    }

}
