using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public class IslandTextureGeneration : MonoBehaviour
{
    public int TextureSize;
    public float NoiseScale, IslandSize;
    [Range(1, 20)] public int NoiseOctaves;
    [Range(0, 99999999)] public int Seed;

    // Privates

    private Color[] col;
    private Texture2D tex;

    public Gradient ColorGradient;

    private void Start()
    {
        tex = new Texture2D(TextureSize, TextureSize);
        col = new Color[tex.height * tex.width];

        Renderer rend = GetComponent<MeshRenderer>();
        rend.sharedMaterial.mainTexture = tex;

        Vector2 Org = new Vector2(Mathf.Sqrt(Seed), Mathf.Sqrt(Seed));

        for (int x = 0, i = 0; x < TextureSize; x++){
            for (int y = 0; y < TextureSize; y++, i++){
                float a = Noisefunction(x, y, Org);
                col[i] = ColorGradient.Evaluate(Noisefunction((float)x, (float)y, Org));
            }
        }
        tex.SetPixels(col);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
    }

    private float Noisefunction(float x, float y, Vector2 Origin)
    {
        float a = 0, noisesize = NoiseScale, opacity = 1;

        for (int octaves = 0; octaves < NoiseOctaves; octaves++)
        {
            float xVal = (x / (noisesize * TextureSize)) + Origin.x;
            float yVal = (y / (noisesize * TextureSize)) - Origin.y;
            float z = noise.snoise(new float2(xVal, yVal));
            a += Mathf.InverseLerp(0, 1, z) / opacity;

            noisesize /= 2f;
            opacity *= 2f;
        }

        return a -= FallOffMap(x, y, TextureSize, IslandSize);
    }

    private float FallOffMap(float x, float y, int size, float islandSize)
    {
        float gradient = 1;

        gradient /= (x * y) / (size * size) * (1 - (x / size)) * (1 - (y / size));
        gradient -= 16;
        gradient /= islandSize;


        return gradient;
    }
}
