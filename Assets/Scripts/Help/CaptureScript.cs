using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CaptureScript : MonoBehaviour
{
    public Camera _camera;
    int i =0;
    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            Capture();
        }
    }

    private void Capture()
    {
        string path = "C:\\Users\\KGA_024\\Desktop\\에셋" + "/capture"+i.ToString()+".png";
        i++;
        StartCoroutine(CoCapture(path));
    }

    private IEnumerator CoCapture(string path)
    {
        if (path == null)
        {
            yield break;
        }

        // ReadPixels을 하기 위해서 쉬어줌
        yield return new WaitForEndOfFrame();

        Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
        Texture2D texture = Capture(Camera.main, rect);

        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    private Texture2D Capture(Camera camera, Rect pRect)
    {
        Texture2D capture;
        CameraClearFlags preClearFlags = camera.clearFlags;
        Color preBackgroundColor = camera.backgroundColor;
        {
            camera.clearFlags = CameraClearFlags.SolidColor;

            camera.backgroundColor = Color.black;
            camera.Render();
            Texture2D blackBackgroundCapture = CaptureView(pRect);

            camera.backgroundColor = Color.white;
            camera.Render();
            Texture2D whiteBackgroundCapture = CaptureView(pRect);

            for (int x = 0; x < whiteBackgroundCapture.width; ++x)
            {
                for (int y = 0; y < whiteBackgroundCapture.height; ++y)
                {
                    Color black = blackBackgroundCapture.GetPixel(x, y);
                    Color white = whiteBackgroundCapture.GetPixel(x, y);
                    if (black != Color.clear)
                    {
                        whiteBackgroundCapture.SetPixel(x, y, GetColor(black, white));
                    }
                }
            }

            whiteBackgroundCapture.Apply();
            capture = whiteBackgroundCapture;
            Object.DestroyImmediate(blackBackgroundCapture);
        }
        camera.backgroundColor = preBackgroundColor;
        camera.clearFlags = preClearFlags;
        return capture;
    }

    private Color GetColor(Color black, Color white)
    {
        float alpha = GetAlpha(black.r, white.r);
        return new Color(
            black.r / alpha,
            black.g / alpha,
            black.b / alpha,
            alpha);
    }

    private float GetAlpha(float black, float white)
    {
        return 1 + black - white;
    }

    private Texture2D CaptureView(Rect rect)
    {
        Texture2D captureView = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
        captureView.ReadPixels(rect, 0, 0, false);
        return captureView;
    }
}


