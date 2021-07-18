using UnityEngine;
using System.IO;

public class NoiseGenerationWithOctaves : MonoBehaviour
{
    public int TextureSize;
    public float NoiseScale, IslandSize;
    [Range(1, 20)] public int NoiseOctaves;
    public Texture2D biomeTexture;
    public string Seed;

    private void Awake()
    {
        // Generate textures and apply texture
        Texture2D tex = new Texture2D(TextureSize, TextureSize);
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;

        // Calculate seed
        UnityEngine.Random.InitState(Seed.GetHashCode());
        Vector2 Org = new Vector2(Random.Range(-9999, 9999), Random.Range(-9999, 9999));

        float b = Time.realtimeSinceStartup;

        // Loop through all pixels in texture and assign a color
        for (int x = 0; x < TextureSize; x++)
            for (int y = 0; y < TextureSize; y++)
                tex.SetPixel(x, y, Noisefunction.FinalNoise(x, y, NoiseOctaves, Org, TextureSize, NoiseScale, IslandSize, biomeTexture));
        tex.Apply();

        print("Time since start of game : " + Time.realtimeSinceStartup.ToString()); 
        print("Time for loop to run was : " + (Time.realtimeSinceStartup - b).ToString());
    }

}

