using UnityEngine;
using Unity.Mathematics;

public class Noisefunction
{
    public static Color FinalNoise(float x, float y, int Octaves, Vector2 Origin, int Size, float NoiseScale, float IslandSize, Texture2D BiomeTex)
    {
        // Get rainfall and temperature noise
        float a = WarpedNoise(x, y, Octaves, Origin, Size, NoiseScale);
        float t = TempNoise(x + 100, y - 100, Octaves, Origin, Size, NoiseScale, IslandSize);
        float r = RainNoise(x - 100, y + 100, Octaves, Origin, Size, NoiseScale, IslandSize);
        // Biome Texture is 50 x 50 pixels so to calculate the pixel
        // correspondence we multiply the 0 to 1 nosie values to be between 0 and 50 and an int

        t = Mathf.RoundToInt(t * 50);
        r = Mathf.RoundToInt(r * 50);

        // Return color returned from the texture
        return terraincol((int)t, (int)r, BiomeTex);  
    }

    static float TempNoise(float x, float y, int Octaves, Vector2 Origin, int Size, float NoiseScale, float IslandSize)
    {
        float a = WarpedNoise(x, y, Octaves, Origin, Size, NoiseScale);

        return a - FalloffMap(x - 100, y + 100, Size, IslandSize);
    }
    static float RainNoise(float x, float y, int Octaves, Vector2 Origin, int Size, float NoiseScale, float IslandSize)
    {
        float a = WarpedNoise(x, y, Octaves, Origin, Size, NoiseScale);

        return a - FalloffMap(x + 100, y - 100, Size, IslandSize);
    }
    public static float WarpedNoise(float x, float y, int Octaves, Vector2 Origin, int Size, float NoiseScale)
    {
        // warp the nosie by generating multiple instances of noise
        var q  = Noise(x + 5.3f, y + 0.8f, Octaves, Origin, Size, NoiseScale);

        return Noise(x + 80.0f * q, y + 80.0f * q, Octaves, Origin, Size, NoiseScale);
    }

    public static float Noise(float x, float y, int Octaves, Vector2 Origin, int Size, float NoiseScale)
    {
        // Just generate the noise map 

        float a = 0, Opacity = 1, MaxValue = 0;

        // Loop for octaves
        for (int octaves = 0; octaves < Octaves; octaves++)
        {
            // find sample position on xy axis
            float xVal = (x / (Size / NoiseScale)) + Origin.x;
            float yVal = (y / (Size / NoiseScale)) + Origin.y;
            float z = (noise.snoise(new float2(xVal, yVal)) + 1) * 0.5f;
            a += (z / Opacity);
            MaxValue += 1f / Opacity;

            // Change opacity and scale
            NoiseScale *= 2f;
            Opacity *= 2f;
        }

        // divide by max value to normalize the noise value between 0 and 1
        a /= MaxValue;

        return a;
    }

    private static float FalloffMap(float x, float y, float size, float islandSize)
    { 
        // Generate a radial gradient to make island
        return (1 / ((x * y) / (size * size) * (1 - (x / size)) * (1 - (y / size))) - 16) / islandSize;
    }

    public static Color terraincol(int temperaturemap, int rainfallmap, Texture2D biomeTexture )
    {
        // use the temp and rainfall maps to sample pixel color from biome texture
        return (biomeTexture.GetPixel(temperaturemap, rainfallmap));
    }
}
