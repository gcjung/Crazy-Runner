using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class RenderImageToPNG : MonoBehaviour
{
    public RenderTexture rt;
    private int i = 0;
    
    public Transform[] characterSkin;
    private void Awake()
    {
        
        
    }
    void Start()
    {
        //StartCoroutine(nameof(Co_toTexture2D));
        
    }

    IEnumerator Co_toTexture2D()
    {
        for (int i = 0; i < 1; i++)
        {
            yield return new WaitForSeconds(0.5f);
            //if (i == 0) continue;

            //characterSkin[i-1].gameObject.SetActive(false);
            characterSkin[i].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.3f);

            SaveRenderTextureToPng(Application.dataPath + "/Sprites/Characters/" + i.ToString()+ ". " +characterSkin[i].name + ".png", rt);
            
        }
    }

    public static void SaveRenderTextureToPng(string path, RenderTexture rt)
    {
        // 렌더 텍스쳐를 바이트화 시킵니다.
        var bytes = toTexture2D(rt).EncodeToPNG();
        File.WriteAllBytes(path, bytes);

    }

    private static Texture2D toTexture2D(RenderTexture rTex)
    {
        // 렌더 텍스쳐의 가로 X 세로 사이즈로, RGBA32 포맷을 가진 텍스쳐2D를 만듭니다.
        var tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
        RenderTexture.active = rTex;

        // 렌더 텍스쳐 가로X세로 사이즈로 읽을 수 있는 픽셀을 처리합니다.
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);

        //적용
        tex.Apply();   
        return tex;
    }
}
