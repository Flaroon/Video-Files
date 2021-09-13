using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class ChunkSpawner : MonoBehaviour
{
    // Variables
    [Header("Game Variables")]
    public int ActiveChunksAroundPlayer;
    public GameObject ChunkEmpty, Player;

    [Header("Chunk Variables")]
    public ChunkVariables ChunkVariables;

    // dictionary of all of the gamojects and their chunk id's
    private Dictionary<Vector2Int, GameObject> Chunks = new Dictionary<Vector2Int, GameObject>();

    // previous and current player chunk location
    private Vector2Int CurrentChunkID, PreviousChunkID;

    public void Start()
    {
        StartCoroutine(generateChunks());

        // spawn chunks every 0.7s so that when moving over chunks very fast the game doesn't lag

        InvokeRepeating("createChunks", 0, 0.7f);


    }

    void createChunks()
    {
        // when we spawn chunks we check if the player has moved from their last chunk. If not then we

        if (CurrentChunkID != PreviousChunkID)
        {
            StartCoroutine(generateChunks());
        }
    }



    public void FixedUpdate()
    {
        CurrentChunkID = new Vector2Int(Mathf.FloorToInt(Player.transform.position.x / (ChunkVariables.Dimentions.x - 2))
                                      , Mathf.FloorToInt(Player.transform.position.z / (ChunkVariables.Dimentions.x - 2)));


        // loop though all of the Chunks and check the distance from the player. If it is too much destroy the object and remove the key from the dictionary
        foreach (var v in Chunks){

            Vector3 chunkPosition = new Vector3(v.Key.x, 0, v.Key.y) * (ChunkVariables.Dimentions.x - 2);

            if (Vector2.Distance(v.Key, CurrentChunkID) > ActiveChunksAroundPlayer){
                Destroy(v.Value);
                Chunks.Remove(v.Key);
                break;
            }

        }

    }

    private IEnumerator generateChunks()
    {
        // Calculate Update Priotoity so that chunks closer to the player spawn first

        Dictionary<Vector2Int, float> Priority = new Dictionary<Vector2Int, float>();

        for (int x = -ActiveChunksAroundPlayer; x < ActiveChunksAroundPlayer; x++)
        {
            for (int y = -ActiveChunksAroundPlayer; y < ActiveChunksAroundPlayer; y++)
            {
                Vector2Int chunktospawn = CurrentChunkID + new Vector2Int(x, y);

                float distancePriority = Vector2.Distance(chunktospawn, CurrentChunkID);

                Priority.Add(chunktospawn, distancePriority);
            }
        }

        // Sort dictionary by value
        var sortedDict = from entry in Priority orderby entry.Value ascending select entry;

        // Go though all chunks and spawn them in order
        foreach (var v in sortedDict)
        {
            // chunk id to spawn
            Vector2Int chunktospawn = v.Key;
            // distance from players current chunk and the chunk to spawn
            float dist = Vector2.Distance(chunktospawn, CurrentChunkID);
            // real world chunk position
            Vector3 chunkPosition = new Vector3(chunktospawn.x, 0, chunktospawn.y) * (ChunkVariables.Dimentions.x - 2);

            // if the chunks dictionary doesn't already have the chunk to spawn already and the chunk is within the players view...
            if (!Chunks.ContainsKey(chunktospawn) && dist < ActiveChunksAroundPlayer)
            {
                // spawn the chunk

                // Instantiate chunk

                GameObject Chunk = Instantiate(ChunkEmpty);

                // position the chunk
                Chunk.transform.position = chunkPosition;

                // generate the chunks mesh
                Chunk.GetComponent<MainVoxel>().GenerateChunk(ChunkVariables);

                // turn the chunk into the managers child
                Chunk.transform.parent = transform;

                // add the spawned chunk to the dictionary
                Chunks.Add(chunktospawn, Chunk);

            }

            // just add some delay to reduce the lag
            yield return new WaitForEndOfFrame();
        }

        // set the PreviousChunkID to the CurrentChunkID to check next when we need to spawn chunks

        PreviousChunkID = CurrentChunkID;
    }
}
