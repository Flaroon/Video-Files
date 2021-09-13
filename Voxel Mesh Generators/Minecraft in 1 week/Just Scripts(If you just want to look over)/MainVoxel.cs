using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System.Linq;

using System;


public class MainVoxel : MonoBehaviour
{
    public GameObject grassLayer, WaterLayer;
    public ChunkVariables NoiseVariables;
    [HideInInspector]public int[] Voxels;

    Vector3[] VertPos = new Vector3[8]{
        new Vector3(-1, 1, -1), new Vector3(-1, 1, 1),
        new Vector3(1, 1, 1), new Vector3(1, 1, -1),
        new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
        new Vector3(1, -1, 1), new Vector3(1, -1, -1),
    };

    int[,] Faces = new int[6, 8]{
        {0, 1, 2, 3, 0, 1, 0, 0},     //top
        {7, 6, 5, 4, 0, -1, 0, 2},   //bottom
        {2, 1, 5, 6, 0, 0, 1, 1},     //right
        {0, 3, 7, 4, 0, 0, -1, 1},   //left
        {3, 2, 6, 7, 1, 0, 0, 1},    //front
        {1, 0, 4, 5, -1, 0, 0, 1}    //back
    };

    public void GenerateChunk(ChunkVariables nv)
    {
        NoiseVariables = nv;

        Voxels = new int[NoiseVariables.Dimentions.x * NoiseVariables.Dimentions.y * NoiseVariables.Dimentions.z];

        GenerateNoise();


        GenerateMesh();
    }

    public void UpdateChunk(int3[] positiontoedit, int[] ChangeTo)
    {
        for (int i = 0; i < positiontoedit.Length; i++)
        {
            if (Voxels[convert(positiontoedit[i].x, positiontoedit[i].y, positiontoedit[i].z)] != 32)
            {
                Voxels[convert(positiontoedit[i].x, positiontoedit[i].y, positiontoedit[i].z)] = ChangeTo[i];
            }
        }

        GenerateMesh();
    }

    private void GenerateNoise()
    {
        // Schedule all of the different jobs
        NativeArray<int> NoiseData = new NativeArray<int>(NoiseVariables.Dimentions.x * NoiseVariables.Dimentions.y * NoiseVariables.Dimentions.z, Allocator.TempJob);

        CalculateNoiseValues NoiseJob = new CalculateNoiseValues()
        {
            NoiseArr = NoiseData,
            NoiseData = NoiseVariables,
            WorldPosition = transform.position
        };

        JobHandle NoiseJobHandle = NoiseJob.Schedule(NoiseVariables.Dimentions.x * NoiseVariables.Dimentions.y * NoiseVariables.Dimentions.z, 100);
        NoiseJobHandle.Complete();

        AddFoliage FoliageJob = new AddFoliage()
        {
            NoiseArr = NoiseData,
            NoiseData = NoiseVariables,
            WorldPosition = transform.position
        };

        JobHandle FoliageJobHandle = FoliageJob.Schedule(NoiseVariables.Dimentions.x * NoiseVariables.Dimentions.z, 100);
        FoliageJobHandle.Complete();

        // copy the noise data to the voxel array
        NoiseData.CopyTo(Voxels);

        NoiseData.Dispose();

    }

    private void GenerateMesh()
    {

        List<int> Triangles = new List<int>();
        List<Vector3> Verticies = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();

        List<int> WaterTriangles = new List<int>();
        List<Vector3> WaterVerticies = new List<Vector3>();

        for (int x = 1; x < NoiseVariables.Dimentions.x - 1; x++)
        {
            for (int y = 1; y < NoiseVariables.Dimentions.y - 1; y++)
            {
                for (int z = 1; z < NoiseVariables.Dimentions.z - 1; z++)
                {
                    float heightrange = NoiseVariables.WaterHeight * (NoiseVariables.Dimentions.y - 1);

                    // explained in the last video so if you havent seen that already check it out

                    if (Voxels[convert(x, y, z)] > 0) // if solid voxel
                    {
                        for (int o = 0; o < 6; o++)
                        {
                            if (Voxels[convert(x + Faces[o, 4], y + Faces[o, 5], z + Faces[o, 6])] == 0 || Voxels[convert(x + Faces[o, 4], y + Faces[o, 5], z + Faces[o, 6])] == 19 || Voxels[convert(x + Faces[o, 4], y + Faces[o, 5], z + Faces[o, 6])] == -1 || Voxels[convert(x + Faces[o, 4], y + Faces[o, 5], z + Faces[o, 6])] == 7)
                            {
                                int v = Verticies.Count;
                                for (int i = 0; i < 4; i++) Verticies.Add(new Vector3(x, y, z) + VertPos[Faces[o, i]] / 2f);
                                Triangles.AddRange(new List<int>() { v, v + 1, v + 2, v, v + 2, v + 3 });

                                // Under Grass Dirt. this just checks is the voxel above is a grass voxel. if so then set the current voxel to be a solid dirt voxel
                                if (Voxels[convert(x, y + 1, z)] <= 8 && Voxels[convert(x, y, z)] <= 8 && Voxels[convert(x, y + 1, z)] > 0 && Voxels[convert(x, y, z)] > 0)
                                {
                                    AddUV(Voxels[convert(x, y, z)], 1);
                                }
                                else AddUV(Voxels[convert(x, y, z)], o);
                            }
                        }
                    }
                    // if block type is a water block
                    else if (Voxels[convert(x, y, z)] <= 0 && y < heightrange + 1 && y > heightrange - 3) // if the block type is less that 0 or a water voxel
                    {
                        // Generate a water voxel
                        if (Voxels[convert(x + Faces[0, 4], y + Faces[0, 5], z + Faces[0, 6])] == 0 || Voxels[convert(x + Faces[0, 4], y + Faces[0, 5], z + Faces[0, 6])] == 19)
                        {
                            int v = WaterVerticies.Count;
                            for (int i = 0; i < 4; i++) WaterVerticies.Add(new Vector3(x, y, z) + VertPos[Faces[0, i]] / 2f);
                            WaterTriangles.AddRange(new List<int>() { v, v + 1, v + 2, v, v + 2, v + 3 });
                        }
                    }

                    void AddUV(int BI, int FaceIndex = 0)
                    {

                        int BlockIndex = BI;
                        BlockIndex -= 1;

                        if (BlockIndex + 1 <= 16) // the first 16 blocks are blocks with 3 different faces.
                        {                         //to calculate what texture to apply we need to multiply the face orientation in the face array by 16.
                            BlockIndex += Faces[FaceIndex, 7] * 16;
                        }
                        else { BlockIndex += 32; } // if the block is non specific then just add 32 and set all of the block faces to 1 texture.


                        int blockyindex = BlockIndex / NoiseVariables.TexturesSections;
                        int blockxindex = BlockIndex % NoiseVariables.TexturesSections;
                        float step = 1f / NoiseVariables.TexturesSections;


                        Vector2 bottomLeft = new Vector2((float)blockxindex * step, (float)blockyindex * step);

                        uv.AddRange(new List<Vector2>() { bottomLeft + new Vector2(step, step), bottomLeft + new Vector2(0, step), bottomLeft, bottomLeft + new Vector2(step, 0) });

                    }
                }
            }
        }

        // set terrain mesh

        Mesh TerrainMesh = new Mesh()
        {
            vertices = Verticies.ToArray(),
            uv = uv.ToArray(),
            triangles = Triangles.ToArray()
        };


        TerrainMesh.RecalculateNormals();


        GetComponent<MeshFilter>().sharedMesh = TerrainMesh;
        GetComponent<MeshCollider>().sharedMesh = TerrainMesh;

        // set water mesh without collider

        Mesh WaterMesh = new Mesh()
        {
            vertices = WaterVerticies.ToArray(),
            triangles = WaterTriangles.ToArray()
        };

        WaterLayer.GetComponent<MeshFilter>().mesh = WaterMesh;

        // Grass-flowers-seagrass-kelp

        List<int> TrianglesGrass = new List<int>();
        List<Vector3> VerticiesGrass = new List<Vector3>();
        List<Vector2> uvGRass = new List<Vector2>();

        UnityEngine.Random.InitState(0); // make sure that the random numbers are always the same seed

        // loop though each column
        for (int x = 1; x < NoiseVariables.Dimentions.x - 1; x++)
        {
            for (int z = 1; z < NoiseVariables.Dimentions.z - 1; z++)
            {
                // find surface level of the cirrent column
                int SurfaceLevel = NoiseVariables.Dimentions.y - 1;
                // while the block we are on is still air or another type of block we keep going down until the block is solid.
                while (Voxels[convert(x, SurfaceLevel, z)] == 0 || Voxels[convert(x, SurfaceLevel, z)] == -1 || Voxels[convert(x, SurfaceLevel, z)] == 19 || Voxels[convert(x, SurfaceLevel, z)] == 7) SurfaceLevel--;

                // if the surface block is above water level
                if (UnderBlock(x, SurfaceLevel, z, 1) || UnderBlock(x, SurfaceLevel, z, 3) || UnderBlock(x, SurfaceLevel, z, 4))
                {

                    if (UnityEngine.Random.Range(0, 100) < 85)
                    {
                        addGrass(0); // add grass
                    }
                    else
                    {
                        addGrass(1); // add flower
                    }
                } // if surface block is under water
                else if (UnderBlock(x, SurfaceLevel, z, 26) || UnderBlock(x, SurfaceLevel, z, 17) && SurfaceLevel < 33)
                {
                    if (UnityEngine.Random.Range(0, 100) < 85) // add sea grass
                        addGrass(2);
                    else // create kelp
                    {
                        int tempsurf = SurfaceLevel; // the underwater surface is equal to the actual surface
                        int maxkelpHeight = 0; // kelp height to loop though

                        while (tempsurf < 35) // find how tall the kelp shouold be
                        {
                            tempsurf++;
                            maxkelpHeight++;
                        }

                        for (int i = 0; i < maxkelpHeight + UnityEngine.Random.Range(-3, 5); i++) // create kelp column with some height variability
                        {
                            addGrass(3, i);
                        }
                    }

                }

                void addGrass(int type, int kelpheightindex = 0)
                {
                    // tri array for the squares
                    int[] Tris = new int[6]{
                    3, 2, 0, 0, 1, 3
                    };

                    // type 0 ==> Normal Grass, 1 ==> Normal Flower, 2 ==> Sea Grass, 3 ==> Kelp

                    // somtimes we need 2 squares. that is where i need to create 2 arrays of verticies.
                    // the uv is added to the grass uvs when i create each square at different rotations.
                    // different indicies in the addUv function represent different sample locations from the texture atlas.
                    // the different types of grass and flowers work on the same principals so it should be pretty simple to understand.

                    if (type == 0)
                        {
                            // Positions 1
                            Vector3[] verts1 = new Vector3[4]
                            {
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(Vector3.zero, Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(0, 1, 0), Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 0, -1), Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 1, -1), Vector3.zero, new Vector3(0, 0, 0)),
                            };
                            int len = VerticiesGrass.Count;

                            VerticiesGrass.AddRange(verts1);

                            for (int i = 0; i < 6; i++)
                            {
                                TrianglesGrass.Add(Tris[i] + len);
                            }


                            AddUV(20);

                            Vector3[] verts2 = new Vector3[4]
                            {
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(Vector3.zero, Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(0, 1, 0), Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 0, -1), Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 1, -1), Vector3.zero, new Vector3(0, 90, 0)),
                            };

                            int len1 = VerticiesGrass.Count;

                            VerticiesGrass.AddRange(verts2);

                            for (int i = 0; i < 6; i++)
                            {
                                TrianglesGrass.Add(Tris[i] + len1);
                            }

                            AddUV(20);
                        }
                    else if (type == 1)
                        {

                            // Add Flower
                            Vector3[] verts1 = new Vector3[4]
                            {
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + new Vector3(0, 0, 0),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + new Vector3(0, 1, 0),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + new Vector3(1, 0, -1),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + new Vector3(1, 1, -1),
                            };
                            int len = VerticiesGrass.Count;

                            VerticiesGrass.AddRange(verts1);

                            foreach (int i in Tris)
                            {
                                TrianglesGrass.Add(i + len);
                            }
                            AddUV(21 + UnityEngine.Random.Range(0, 3));
                        }
                    else if (type == 2)
                        {
                            // Positions 1
                            Vector3[] verts1 = new Vector3[4]
                            {
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(Vector3.zero, Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(0, 2, 0), Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 0, -1), Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 2, -1), Vector3.zero, new Vector3(0, 0, 0)),
                            };
                            int len = VerticiesGrass.Count;

                            VerticiesGrass.AddRange(verts1);

                            for (int i = 0; i < 6; i++)
                            {
                                TrianglesGrass.Add(Tris[i] + len);
                            }


                            AddUV(27);

                            Vector3[] verts2 = new Vector3[4]
                            {
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(Vector3.zero, Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(0, 1, 0), Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 0, -1), Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f, z + .5f) + Rotate(new Vector3(1, 1, -1), Vector3.zero, new Vector3(0, 90, 0)),
                            };

                            int len1 = VerticiesGrass.Count;

                            VerticiesGrass.AddRange(verts2);

                            for (int i = 0; i < 6; i++)
                            {
                                TrianglesGrass.Add(Tris[i] + len1);
                            }

                            AddUV(27);
                        }
                    else if (type == 3)
                        {
                            // Positions 1
                            Vector3[] verts1 = new Vector3[4]
                            {
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(Vector3.zero, Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(new Vector3(0, 2, 0), Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(new Vector3(1, 0, -1), Vector3.zero, new Vector3(0, 0, 0)),
                                new Vector3(x - 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(new Vector3(1, 2, -1), Vector3.zero, new Vector3(0, 0, 0)),
                            };
                            int len = VerticiesGrass.Count;

                            VerticiesGrass.AddRange(verts1);

                            for (int i = 0; i < 6; i++)
                            {
                                TrianglesGrass.Add(Tris[i] + len);
                            }


                            AddUV(28);

                            Vector3[] verts2 = new Vector3[4]
                            {
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(Vector3.zero, Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(new Vector3(0, 1, 0), Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(new Vector3(1, 0, -1), Vector3.zero, new Vector3(0, 90, 0)),
                                new Vector3(x + 0.5f, SurfaceLevel + 0.5f + kelpheightindex, z + .5f) + Rotate(new Vector3(1, 1, -1), Vector3.zero, new Vector3(0, 90, 0)),
                            };

                            int len1 = VerticiesGrass.Count;

                            VerticiesGrass.AddRange(verts2);

                            for (int i = 0; i < 6; i++)
                            {
                                TrianglesGrass.Add(Tris[i] + len1);
                            }

                            AddUV(28);
                        }
                }

                void AddUV(int BI, int FaceIndex = 0)
                {
                    int BlockIndex = BI;
                    BlockIndex -= 1;

                    if (BlockIndex + 1 <= 16)
                    {
                        BlockIndex += Faces[FaceIndex, 7] * 16;
                    }
                    else { BlockIndex += 32; }


                    int blockyindex = BlockIndex / NoiseVariables.TexturesSections;
                    int blockxindex = BlockIndex % NoiseVariables.TexturesSections;
                    float step = 1f / NoiseVariables.TexturesSections;


                    Vector2 bottomLeft = new Vector2((float)blockxindex * step, (float)blockyindex * step);

                    uvGRass.AddRange(new List<Vector2>() { bottomLeft, bottomLeft + new Vector2(0, step), bottomLeft + new Vector2(step, 0), bottomLeft + new Vector2(step, step) });
                  }

                // function to rotate point around pivot
                Vector3 Rotate(Vector3 point, Vector3 pivot, Vector3 angles) => Quaternion.Euler(angles) * (point - pivot) + pivot;
            }
        }

        Mesh GrassMesh = new Mesh()
        {
            vertices = VerticiesGrass.ToArray(),
            uv = uvGRass.ToArray(),
            triangles = TrianglesGrass.ToArray()
        };

        GrassMesh.RecalculateNormals();
        grassLayer.GetComponent<MeshFilter>().sharedMesh = GrassMesh;

    }

    bool UnderBlock(int x, int y, int z, int blocktype) {
        if (Voxels[convert(x, y, z)] == blocktype && noise.snoise(new float2(x, z)) > 0)
        {
            return true;
        }
        return false;
    }
    int convert(int x, int y, int z) => x + NoiseVariables.Dimentions.x * (y + NoiseVariables.Dimentions.y * z);
}

public struct AddFoliage : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeArray<int> NoiseArr;
    public ChunkVariables NoiseData;
    public float3 WorldPosition;
    public void Execute(int ThreadIndex)
    {
        // only need to loop though colums so 2d conversion is fine. y component is dynamic inside the foliage loops
        int x = ThreadIndex / NoiseData.Dimentions.x;
        int z = ThreadIndex % NoiseData.Dimentions.z;

        // Find Surface Value
        int Surface = NoiseData.Dimentions.y - 1;

        while (NoiseArr[Convert(x, Surface, z)] == 0) Surface -= 1;

        var rand = Unity.Mathematics.Random.CreateFromIndex((uint)ThreadIndex);

        if (x < 2 || z < 2 || x >= NoiseData.Dimentions.x - 2 || z >= NoiseData.Dimentions.z - 2) { }
        else
        {
            // Spawn Cacti
            if (noise.snoise(new float2(x, z) * 1000f) > 0.72f && NoiseArr[Convert(x, Surface, z)] == 17) // random number and Block Below check
            {
                for (int i = 0; i < 3 + Mathf.RoundToInt(noise.snoise(new float2(x, z) * 50f)); i++) // random number for height
                {
                    NoiseArr[Convert(x, Surface + i, z)] = 25;
                }
            }

            // Spawn Plains Trees

            int stumptype = rand.NextFloat(0f, 1f) > .5f ? 15 : 16;

            if (rand.NextFloat(0f, 1f) > .98f && NoiseArr[Convert(x, Surface, z)] == 1) // random number and Block Below check
            {
                for (int i = 0; i < 4 + Mathf.RoundToInt(noise.snoise(new float2(x, z) * 50f)); i++) // create 4 high stump
                {
                    NoiseArr[Convert(x, Surface + i, z)] = stumptype; // tree stump can have either oak or birch stump
                }

                // leaf block loop
                for (int xt = -1; xt <= 1; xt++)
                {
                    for (int yt = -1; yt <= 2; yt++)
                    {
                        for (int zt = -1; zt <= 1; zt++)
                        {
                            if (rand.NextFloat(0f, 1f) > .3f) // remove random leafs
                                NoiseArr[Convert(x + xt, Surface + 4 + yt, z + zt)] = 19;
                        }
                    }
                }
            }

            // Forest Trees

            if (rand.NextFloat(0f, 1f) > .98f && NoiseArr[Convert(x, Surface, z)] == 3) // random number and Block Below check
            {
                for (int i = 0; i < 12 + Mathf.RoundToInt(noise.snoise(new float2(x, z) * 50f)); i++) // create tree stump of height 12
                {
                    NoiseArr[Convert(x, Surface + i, z)] = 15;
                }
                for (int y = 0; y < 5; y++) // loop to go through the 5 rings going up
                {
                    for (int xt = -2 + Mathf.FloorToInt(y / 2f); xt <= 2 - Mathf.FloorToInt(y / 2f); xt++) // x component of the ring
                    {
                        for (int zt = -2 + Mathf.FloorToInt(y / 2f); zt <= 2 - Mathf.FloorToInt(y / 2f); zt++) // y component of the ring
                        {
                            if (rand.NextFloat(0f, 1f) < .97f) // if random number is less than .97f then create a leaf
                                NoiseArr[Convert(x + xt, Surface + y * 2 + 5, z + zt)] = 19;
                        }
                    }
                }
            }

            // Snow Trees

            if (rand.NextFloat(0f, 1f) > .98f && NoiseArr[Convert(x, Surface, z)] == 2) // random number and Block Below check
            {
                for (int i = 0; i < 4 + Mathf.RoundToInt(noise.snoise(new float2(x, z) * 50f)); i++) // create stump
                {
                    NoiseArr[Convert(x, Surface + i, z)] = 14;
                }

                // same loop and the oak trees but the leaf block is a snow block

                for (int xt = -1; xt <= 1; xt++)
                {
                    for (int yt = -1; yt <= 2; yt++)
                    {
                        for (int zt = -1; zt <= 1; zt++)
                        {
                            if (rand.NextFloat(0f, 1f) > .3f)
                                NoiseArr[Convert(x + xt, Surface + 4 + yt, z + zt)] = 7;
                        }
                    }
                }
            }

            // Pumpkins

            if (rand.NextFloat(0f, 1f) > .98f && NoiseArr[Convert(x, Surface, z)] == 6) // random number and Block Below check (woodland dirt)
            {
                NoiseArr[Convert(x, Surface + 1, z)] = 8; // set block to be a pumpkin
            }
        }
    }
    // convert 3d index back into a 1d index (for finding blocks above and below)
    int Convert(int x, int y, int z) => x + NoiseData.Dimentions.x * (y + NoiseData.Dimentions.y * z);

}

[BurstCompile]
public struct CalculateNoiseValues : IJobParallelFor
{
    [NativeDisableParallelForRestriction] public NativeArray<int> NoiseArr;
    public ChunkVariables NoiseData;
    public float3 WorldPosition;
    Unity.Mathematics.Random rand;

    public void Execute(int ThreadIndex)
    {
        //random number generator for job systems
        rand = Unity.Mathematics.Random.CreateFromIndex((uint)ThreadIndex);

        // convert the single thread index into a 3d array refrence;
        int x = ThreadIndex % NoiseData.Dimentions.x;
        int y = (ThreadIndex / NoiseData.Dimentions.x) % NoiseData.Dimentions.y;
        int z = ThreadIndex / (NoiseData.Dimentions.y * NoiseData.Dimentions.x);

        // assign block type
        NoiseArr[ThreadIndex] = FindBlockType(new float3(x, y, z));

    }

    int Convert(int3 pos) => pos.x + NoiseData.Dimentions.x * (pos.y + NoiseData.Dimentions.y * pos.z);

    #region noise Functions

    float Noise2D(float2 pos, bool Biome = false, bool Mountain = false)
    {
        float a = 0, max = 0, opac = 1, freq = NoiseData.Noisescale;

        if (Biome) freq *= 0.3f;

        for (int i = 0; i <= NoiseData.Octaves; i++)
        {
            float2 org = new float2(WorldPosition.x, WorldPosition.z) / freq;
            a += noise.snoise(pos / freq + org) * opac;
            max += 1 / i;
            opac *= 0.5f;
            freq *= 0.5f;
        }
        if (Mountain)
            return 1 - Mathf.Abs(a / max);
        else
            return a / max;
    }

    float RidgeNoise(float2 pos)
    {
        float a = 0, opac = 1, freq = NoiseData.Noisescale;

        for (int i = 0; i <= NoiseData.Octaves; i++)
        {
            float2 org = new float2(WorldPosition.x, WorldPosition.z) / freq;
            a += noise.snoise(pos / freq + org) * opac;
            opac *= 0.5f;
            freq *= 0.5f;
        }
        return Mathf.Pow(1 - Mathf.Abs(a), 2) * 15;
    }

    float Noise3D(float3 pos, bool MountainLarge = false)
    {
        float a = 0, max = 0, opac = 1, freq = NoiseData.Noisescale / 2f;
        int Oct = NoiseData.Octaves;


        if (MountainLarge) { freq *= 4; Oct += 4; }

        for (int i = 0; i <= Oct; i++)
        {
            float3 org = new float3(WorldPosition.x, WorldPosition.y, WorldPosition.z) / freq;
            a += noise.snoise(pos / freq + org) * opac;
            max += 1 / i;
            opac *= 0.5f;
            freq *= 0.5f;
        }

        if (MountainLarge)
            return 1 - Mathf.Abs(a / max);
        else
            return a / max;
    }
    float CaveNoise(float3 pos)
    {
        float a = 0, max = 0, opac = 1, freq = NoiseData.Noisescale / 10f;
        int Oct = NoiseData.Octaves;


        for (int i = 0; i <= Oct; i++)
        {
            float3 org = new float3(WorldPosition.x, WorldPosition.y, WorldPosition.z) / freq;
            a += noise.snoise(pos / freq + org) * opac;
            max += 1 / i;
            opac *= 0.5f;
            freq *= 0.5f;
        }

        return a / max;
    }

    #endregion

    private int FindBlockType(float3 BlockPosition)
    {
        // different types of noise functions
        float PlainNoise = (Noise2D(new float2(BlockPosition.x, BlockPosition.z), false, true) + Noise2D(new float2(BlockPosition.x, BlockPosition.z), false, false)) * NoiseData.NoiseHeight * Noise2D(new float2(BlockPosition.x, BlockPosition.z), false, true) * 3 + RidgeNoise(new float2(BlockPosition.x, BlockPosition.z));
        float ThreeNoise = Noise3D(BlockPosition) * NoiseData.NoiseHeight + Noise3D(BlockPosition, true) * NoiseData.NoiseHeight;
        // biome maps (temperature and rainfall)
        float Temp = Noise2D(new float2(BlockPosition.x, BlockPosition.z) - new float2(-1000, 1000), true);
        float Rain = Noise2D(new float2(BlockPosition.x, BlockPosition.z) + new float2(-1000, 1000), true);

        float elevation = BlockPosition.y / (float)NoiseData.Dimentions.y;

        // initialize the block type to return
        int BlockType;
        // create base terrain function
        int AirBlock = (PlainNoise - BlockPosition.y + NoiseData.FloorHeight) * NoiseData.FloorWeight + ThreeNoise * NoiseData.NoiseWeight < NoiseData.FillPercent * 0.01f ? 1 : 0;

        // Block
        if (elevation > NoiseData.SnowHeight + noise.snoise((float3)BlockPosition / 50f) * 0.05f && elevation > NoiseData.SnowHeight)
            BlockType = 2; // Snow
        else if (elevation < NoiseData.WaterHeight && elevation > NoiseData.WaterHeight - 0.03f) // set blocks near shore to sand
            BlockType = 17; // sand
        else if (elevation < NoiseData.WaterHeight - 0.03f) // set blocks to gravel
        {
            BlockType = noise.snoise(new float2(BlockPosition.x, BlockPosition.z) / 30f) > 0 ? 26 : 17;
        }
        else // if block is not gravel, sand or snow figure out the biomes
        {
            if (Temp > 0.7f){
                if (Rain > 0.9f)
                {
                    BlockType = 5; // Devil
                }
                else if (Rain > 0.7f)
                {
                    BlockType = 1; // Plains
                }
                else if (Rain > 0.5f)
                {
                    BlockType = 4; // Swamp
                }
                else if (Rain > 0.25f)
                {
                    BlockType = 2; // forest
                }
                else
                {
                    BlockType = 17; // Desert
                }
            }
            else{
                if (Rain > 0.6f)
                {
                    BlockType = 6; // woodland
                }
                else if (Rain > 0.3f)
                {
                    BlockType = 3; // forest
                }
                else
                {
                    BlockType = 1; // Plains
                }
            }
        }

        // set air blocks
        if (AirBlock == 1) BlockType = 0;
        // set water blocks under threshold if block is a air block
        if (elevation < NoiseData.WaterHeight && BlockType == 0)
        {
            BlockType = -1;
        }

        // Create Bedrock layer so people cannot mine down out of the world
        if (BlockPosition.y < 4)
            BlockType = 32;


        return BlockType;
    }
}


// Noise Variables

[Serializable]
public struct ChunkVariables
{
    [Header("Chunk")]
    public int3 Dimentions;
    public int TexturesSections;
    [Header("Noise")]
    public float Noisescale, NoiseHeight, FloorHeight;
    [Range(0, 100)] public float FillPercent;
    [Range(0, 1.5f)]
    public float FloorWeight, NoiseWeight;
    [Range(1, 10)]
    public int Octaves;

    [Range(0, 1)]
    public float SnowHeight, WaterHeight;

    public bool CreateCaves;
}
