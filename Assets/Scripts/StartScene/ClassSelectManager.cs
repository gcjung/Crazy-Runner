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
    public string skillDescript = "<size=45>스킬 - </size>";
    public string synergyCounterDescript = "<size=45><color=blue>S</color>nergy&<color=red>C</color>ounter</size>\n\n";

    // 프로그래스바  
    /* 최대속도
    // 최저속도
    // 가속도
    // 체력 */

    // 캐릭설명 
    // 스킬 
    // 시너지 및 카운터 캐릭
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

    private ClassDescription[] classDescriptions = new ClassDescription[10];    // 일단 10개만 만들어놓음.. 그중 3개사용중

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

    public void ChangeDescriptPageButton(int changePage)    // 현재 
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
        // 일반인
        classDescriptions[0].SetStat(60, 60, 50);
        classDescriptions[0].SetDescriptText("일반인", "기본스탯이 좋은 밸런스형 클래스",
            "<size=45>빠른 기상</size>\n\n방해효과가 빠르게 풀립니다.", "<color=blue>육상선수</color> 서로의 최대속력이 올라갑니다.\n<color=red>테러리스트</color>에 의한 피해를 더 입습니다");
        // 테러리스트
        classDescriptions[1].SetStat(80, 50, 30);
        classDescriptions[1].SetDescriptText("테러리스트", "체력이 높은 공격형 클래스",
            "<size=45>폭탄 설치</size>\n\n폭탄을 설치하여 피해를 입힙니다.", "<color=blue>일반인</color>은 폭탄설치의 피해를 더 입습니다.\n<color=red>경찰</color>이 있으면 최대속력이 내려갑니다.");
        // 육상선수
        classDescriptions[2].SetStat(25, 90, 70);
        classDescriptions[2].SetDescriptText("육상선수", "최대속도와 가속도가 높지만 체력이 매우 낮은 하이리스크 스피드형 클래스",
            "<size=45>고속 질주</size>\n\n아이템을 먹으면 짧은 시간 속도가 빨라집니다.", "<color=blue>일반인</color> 서로의 최대속력이 올라갑니다.\n<color=red>없음</color>");


    }

}
