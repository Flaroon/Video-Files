using System;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public class TerrainMeshGenerator : MonoBehaviour
{
    public static TerrainMeshGenerator Instance;
    
    public MeshFilter meshFilter;
    public TerrainMeshVariables meshVariables;
    public TerrainHeightmapVariables heightmapVariables;
    public Gradient heightmapGradient;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GenerateMap()
    {
        NativeArray<Color> _gradientColorArray = new NativeArray<Color>(100, Allocator.Persistent);
        
        for (int i = 0; i < 100; i++) {
            _gradientColorArray[i] = heightmapGradient.Evaluate(i / 100f);
        }
        
        HeightMapGenerator heightmapGenerator = new HeightMapGenerator(meshVariables, heightmapVariables, _gradientColorArray);
        heightmapGenerator.Schedule(meshVariables.TotalVerts, 10000).Complete();
        Maps _maps = heightmapGenerator.ReturnAndDispose();

        MeshGenerator meshGenerator = new MeshGenerator(meshVariables, _maps);
        meshGenerator.Schedule(meshVariables.terrainMeshDetail * meshVariables.terrainMeshDetail, 10000).Complete();
        
        meshFilter.mesh = meshGenerator.DisposeAndGetMesh();
        
        _gradientColorArray.Dispose();
    }
}

[BurstCompile]
public struct HeightMapGenerator : IJobParallelFor {

    [NativeDisableParallelForRestriction,] private NativeArray<float> _heightMap;
    [NativeDisableParallelForRestriction] private NativeArray<Color> _colMap; 
    private readonly TerrainMeshVariables _meshVariables;
    private readonly TerrainHeightmapVariables _heightmapVariables;
    [NativeDisableParallelForRestriction] private NativeArray<Color> _gradient;


    public HeightMapGenerator(TerrainMeshVariables mv, TerrainHeightmapVariables hv, NativeArray<Color> grad)
    {
        _meshVariables = mv;
        _heightmapVariables = hv;
        _colMap = new NativeArray<Color>(_meshVariables.TotalVerts, Allocator.TempJob);
        _heightMap = new NativeArray<float>(_meshVariables.TotalVerts, Allocator.TempJob);
        _gradient = grad;
    }
    
    public void Execute(int threadIndex)
    {
        float x = threadIndex / /*(float)*/(_meshVariables.terrainMeshDetail+1);
        float y = threadIndex % (_meshVariables.terrainMeshDetail+1);
        float2 pos = new float2(x, y);

        float h = Mathf.Clamp((OctavedSimplexNoise(pos) + OctavedRidgeNoise(pos))/2f * FalloffMap(pos) * _meshVariables.height, _heightmapVariables.waterLevel, 1000);

        _heightMap[threadIndex] = h / _meshVariables.TileEdgeLength;
        _colMap[threadIndex] = _gradient[Mathf.Clamp(Mathf.RoundToInt(h), 0, 99)];
    }

    public Maps ReturnAndDispose() => new Maps(_heightMap, _colMap);

    float OctavedRidgeNoise(float2 pos)
    {
        float noiseVal = 0, amplitude = 1, freq = _heightmapVariables.noiseScale, weight = 1;

        for (int o = 0; o < _heightmapVariables.octaves; o++)
        {
            float v = 1 - Mathf.Abs(noise.snoise(pos / freq / _meshVariables.terrainMeshDetail));
            v *= v;
            v *= weight;
            weight = Mathf.Clamp01(v * _heightmapVariables.weight);
            noiseVal += v * amplitude;
            
            freq /= _heightmapVariables.frequency;
            amplitude /= _heightmapVariables.lacunarity;
        }

        return noiseVal;
    }
    float OctavedSimplexNoise(float2 pos)
    {
        float noiseVal = 0, amplitude = 1, freq = _heightmapVariables.noiseScale;

        for (int o = 0; o < _heightmapVariables.octaves; o++)
        {
            float v = (noise.snoise(pos / freq / _meshVariables.terrainMeshDetail) + 1) / 2f;
            noiseVal += v * amplitude;
            
            freq /= _heightmapVariables.frequency;
            amplitude /= _heightmapVariables.lacunarity;
        }

        return noiseVal;
    }
    float FalloffMap(float2 pos)
    { 
        float x = (pos.x / (_meshVariables.terrainMeshDetail+1)) * 2 - 1;
        float y = (pos.y / (_meshVariables.terrainMeshDetail+1)) * 2 - 1;

        float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

        float a = _heightmapVariables.falloffSteepness;
        float b = _heightmapVariables.falloffOffset;
        
        return 1 - (Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow((b - b * value), a)));
    }
}

[BurstCompile]
public struct MeshGenerator : IJobParallelFor
{
    
    [NativeDisableParallelForRestriction] private NativeArray<Vector3> _verticies;
    [NativeDisableParallelForRestriction] private NativeArray<int> _triangleIndicies;
    private TerrainMeshVariables _meshVariables;
    private Maps _maps;

    public MeshGenerator(TerrainMeshVariables mv, Maps m)
    {
        _meshVariables = mv;

        _verticies = new NativeArray<Vector3>(_meshVariables.TotalVerts, Allocator.TempJob);
        _triangleIndicies = new NativeArray<int>(_meshVariables.TotalTriangles, Allocator.TempJob);
        _maps = m;
    }

    public void Execute(int threadIndex)
    {
        int x = threadIndex / _meshVariables.terrainMeshDetail;
        int y = threadIndex % _meshVariables.terrainMeshDetail;
        
        // c - - - - d
        // |         |
        // |         |
        // |         |
        // a - - - - b
        // a is bottom left and the rest of the points are calculated using the index of a
        // we are only looping through each square to calculate the triangle and other bs
        
        int a = threadIndex + Mathf.FloorToInt(threadIndex / (float)_meshVariables.terrainMeshDetail);
        int b = a + 1;
        int c = b + _meshVariables.terrainMeshDetail;
        int d = c + 1;

        _verticies[a] = new Vector3(x + 0, _maps.HeightMap[a], y + 0) * _meshVariables.TileEdgeLength;
        _verticies[b] = new Vector3(x + 0, _maps.HeightMap[b], y + 1) * _meshVariables.TileEdgeLength;
        _verticies[c] = new Vector3(x + 1, _maps.HeightMap[c], y + 0) * _meshVariables.TileEdgeLength;
        _verticies[d] = new Vector3(x + 1, _maps.HeightMap[d], y + 1) * _meshVariables.TileEdgeLength;
        
        _triangleIndicies[threadIndex * 6 + 0] = a;
        _triangleIndicies[threadIndex * 6 + 1] = b;
        _triangleIndicies[threadIndex * 6 + 2] = c;
        _triangleIndicies[threadIndex * 6 + 3] = b;
        _triangleIndicies[threadIndex * 6 + 4] = d;
        _triangleIndicies[threadIndex * 6 + 5] = c;
    }

    public Mesh DisposeAndGetMesh()
    {
        // create and assign values to mesh
        var m = new Mesh();
        
        m.SetVertices(_verticies);
        m.SetColors(_maps.ColorMap);
        m.triangles = _triangleIndicies.ToArray();

        m.RecalculateNormals();

        // Away with the memory hoarding!! (dispose the native arrays from memory)
        _verticies.Dispose();
        _triangleIndicies.Dispose();
        _maps.Dispose();

        return m;
    }
}

[Serializable]
public struct TerrainMeshVariables
{
    [Range(1, 255)]
    public int terrainMeshDetail;
    public float terrainWidth;
    public float height;
    public int TotalVerts => (terrainMeshDetail + 1) * (terrainMeshDetail + 1);
    public int TotalTriangles => terrainMeshDetail * terrainMeshDetail * 6;
    public float TileEdgeLength => terrainWidth / terrainMeshDetail;
}
[Serializable]
public struct TerrainHeightmapVariables
{
    [Header("Noise")]
    public float noiseScale;
    [Range(0, 4)]
    public float frequency, lacunarity;
    [Range(1, 10)]
    public int octaves;
    public float weight;

    public float falloffSteepness, falloffOffset;
    [Header("Extras")] 
    public float waterLevel;
}

public struct Maps
{
    [NativeDisableParallelForRestriction] public NativeArray<float> HeightMap;
    [NativeDisableParallelForRestriction] public NativeArray<Color> ColorMap;

    public Maps(NativeArray<float> h, NativeArray<Color> c)
    {
        HeightMap = h;
        ColorMap = c;
    }
    
    public void Dispose()
    {
        ColorMap.Dispose();
        HeightMap.Dispose();
    }
}