using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using Photon.Realtime;
public class CharacterSelectManager : MonoBehaviour
{
    private const int MAX_CHARACTER_NUM = 101;
    private const int MAX_ANIMATION_NUM = 3;
    private NetworkManager networkManager;

    public GameObject character;
    private SkinnedMeshRenderer[] skin = new SkinnedMeshRenderer[MAX_CHARACTER_NUM];
    private int currentCharacterIndex;

    public Button[] genderButton;

    private Animator characterAnimator;
    private int currentAnimationState;


    private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();

        for (int i = 0; i < MAX_CHARACTER_NUM; i++)
        {
            skin[i] = character.transform.GetChild(0).GetChild(i).GetComponent<SkinnedMeshRenderer>();
        }

        characterAnimator = character.GetComponent<Animator>();
        //for (int i = 0; i < MAX_CHARACTER_NUM; i++) // �׽�Ʈ
        //{
        //    Debug.Log(i.ToString() + "��°, " + skin[i].name);
        //}
        //for (int i = 0; i < genderButton.Length; i++)
        //{
        //    print(i.ToString() + "��°, " + genderButton[i].name);
        //}
        currentCharacterIndex = 0;
        currentAnimationState = 0;
    }
    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Space))
    //    {
    //        Debug.Log("������ ��Ų �̸� : " + skin[currentCharacterIndex].name);
    //        print("����ư ���� : " + genderButton[0].interactable);
    //        print("����ư ���� : " + genderButton[1].interactable);
    //    }
    //}
    public void SelectCharacterButton(int characterIndex)
    {
        skin[currentCharacterIndex].gameObject.SetActive(false);

        currentCharacterIndex = characterIndex;
        skin[characterIndex].gameObject.SetActive(true);

        networkManager.SelectCharacter(characterIndex);

        // characterIndex (0~47: ��ĳ, 48~52: �߼�, 53~100: ��ĳ)
        if (characterIndex < 48)        // ��ĳ
        {
            genderButton[0].interactable = false;
            genderButton[1].interactable = true;
        }
        else if(characterIndex < 53)    // �߼�
        {
            genderButton[0].interactable = false;
            genderButton[1].interactable = false;
        }
        else                            // ��ĳ
        {
            genderButton[0].interactable = true;
            genderButton[1].interactable = false;
        }

       
    }
    public void SelectGenderButton(bool isMan)
    {
        if (isMan)
        {
            SelectCharacterButton(currentCharacterIndex - 53);
        }
        else
        {
            SelectCharacterButton(currentCharacterIndex + 53);
        }
    }


    public void ClickSelectedCharacterButton()
    {
        currentAnimationState = (currentAnimationState + 1) % MAX_ANIMATION_NUM;
        characterAnimator.SetInteger("State", currentAnimationState);
    }
}

/*
    public void SelectApperanceButton(int characterIndex)
    {
        if (currentCharacterIndex != null)
        {
            currentCharacterIndex.SetInteger("State", 0);
            currentCharacterIndex.gameObject.SetActive(false);
        }

        networkManager.SelectCharacter(characterIndex);

        characterAnimator[characterIndex].gameObject.SetActive(true);

        currentCharacterIndex = characterAnimator[characterIndex];
        currentAnimationState = 1;
        characterAnimator[characterIndex].SetInteger("State", currentAnimationState);
    }
 */

/*
     private void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();

        // 
        for (int i = 0; i < MAX_CHARACTER_NUM; i++)
        {
            characterSlot[i] = content.transform.GetChild(i).GetComponent<Button>();
        }

        currentAnimationState = 0;
    }

    public void SelectApperanceButton(int characterIndex)
    {
        if (currentCharacterIndex != null)
        {
            currentCharacterIndex.SetInteger("State", 0);
            currentCharacterIndex.gameObject.SetActive(false);
        }

        networkManager.SelectCharacter(characterIndex);
        
        currentAnimationState = 1;
    }

    public void ClickSelectedCharacterButton()
    {
        currentAnimationState = (currentAnimationState + 1) % MAX_ANIMATION_NUM;
        currentCharacterIndex.SetInteger("State", currentAnimationState);
    }
 */