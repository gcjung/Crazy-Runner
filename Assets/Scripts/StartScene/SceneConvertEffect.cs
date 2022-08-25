using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneConvertEffect : MonoBehaviour
{
    Image image;                        //판넬 이미지
    private bool checkbool = false;     //투명도 조절 논리형 변수

    void Awake()
    {
        image = GetComponent<Image>();    //판넬오브젝트에 이미지 참조
    }

    public void StartFadeInOut(float time)
    {
        StartCoroutine(nameof(FadeInOut), time);
    }
    public void StartFadeOut(float time)
    {
        StartCoroutine(nameof(FadeOut), time);
    }

    private IEnumerator FadeInOut(float time)
    {
        image.raycastTarget = true;
        float fadeValue = 0;
        while (fadeValue < 1.0f)
        {
            fadeValue += 0.02f;
            yield return new WaitForSeconds(0.01f);
            image.color = new Color(0, 0, 0, fadeValue);
        }

        yield return new WaitForSeconds(time);

        fadeValue = 1;
        while (fadeValue > 0f)
        {
            fadeValue -= 0.02f;
            yield return new WaitForSeconds(0.01f);
            image.color = new Color(0, 0, 0, fadeValue);
        }
        image.raycastTarget = false;
    }


    private IEnumerator FadeOut(float time)
    {
        image.raycastTarget = true;
        image.color = new Color(0, 0, 0, 1);
        yield return new WaitForSeconds(time);

        float fadeValue = 1;
        while (fadeValue > 0f)
        {
            fadeValue -= 0.015f;
            yield return new WaitForSeconds(0.01f);
            image.color = new Color(0, 0, 0, fadeValue);
        }
        image.raycastTarget = false;
    }

    //private IEnumerator FadeIn(float time)
    //{
    //    image.raycastTarget = true;
    //    float fadeValue = 0;
    //    fadeValue = 1;
    //    while (fadeValue > 0f)
    //    {
    //        fadeValue -= 0.02f;
    //        yield return new WaitForSeconds(0.01f);
    //        image.color = new Color(0, 0, 0, fadeValue);
    //    }
    //    image.raycastTarget = false;
    //}
}