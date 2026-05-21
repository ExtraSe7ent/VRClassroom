using UnityEngine;
using Photon.Pun;

public class Whiteboard : MonoBehaviourPun
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);

    private Texture2D texture2D;
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        
        texture2D = new Texture2D((int)textureSize.x, (int)textureSize.y);
        
        Color[] pixels = new Color[(int)textureSize.x * (int)textureSize.y];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture2D.SetPixels(pixels);
        texture2D.Apply();

        _renderer.material.mainTexture = texture2D;
    }

    [PunRPC]
    public void DrawRPC(float x, float y, int penSize, float[] colorArr)
    {
        Color color = new Color(colorArr[0], colorArr[1], colorArr[2]);

        int xPos = (int)(x * textureSize.x);
        int yPos = (int)(y * textureSize.y);

        for (int i = -penSize; i < penSize; i++)
        {
            for (int j = -penSize; j < penSize; j++)
            {
                if (xPos + i >= 0 && xPos + i < textureSize.x && yPos + j >= 0 && yPos + j < textureSize.y)
                {
                    texture2D.SetPixel(xPos + i, yPos + j, color);
                }
            }
        }
        texture2D.Apply();
    }
}