using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public int size, FallOff;
    public float scale;
    public Renderer rend;
    Color[] col;
    Texture2D tex;

    private void Start()
    {
        col = new Color[size * size];
        tex = new Texture2D(size, size);
        rend.material.mainTexture = tex;
        int rand = UnityEngine.Random.Range(0, 100000);
        float y = 0.0f;
        while(y < size)
        {
            float x = 0.0f;
            while(x < size)
            {
                float xcoord = (x / size * scale) + rand;
                float ycoord = (y / size * scale) + rand;
                float sample = (Mathf.PerlinNoise(xcoord, ycoord) + Mathf.PerlinNoise(xcoord * 3f, ycoord * 3f) / 2
                    + Mathf.PerlinNoise(xcoord * 6f, ycoord * 6f) / 5)
                    - Vector2.Distance(new Vector2(y, x), new Vector2(size / 2, size / 2)) / FallOff;

                col[(int)y * size + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }
        tex.SetPixels(col);
        tex.Apply();
    }
}
