using UnityEngine;

public class ChunkLoading : MonoBehaviour
{
    public int ChunkView;
    Vector2 PlayerChunkPos, lastPlayerChunk;
    public GameObject Player, Chunk;
    bool ChunksLoaded;

    private void FixedUpdate()
    {
        PlayerChunkPos = new Vector2(Mathf.Floor((Player.transform.position.x + 5) / 10), Mathf.Floor((Player.transform.position.z + 5) / 10));
        if (lastPlayerChunk != PlayerChunkPos || !ChunksLoaded) LoadChunks();
    }

    private void LoadChunks()
    {
        foreach (GameObject Plane in GameObject.FindGameObjectsWithTag("Plane")) Destroy(Plane);
        lastPlayerChunk = PlayerChunkPos;
        for (int x = -ChunkView; x <= ChunkView; x++)
            for (int y = -ChunkView; y <= ChunkView; y++)
                if (Vector2.Distance(PlayerChunkPos, PlayerChunkPos + new Vector2(x, y)) <= ChunkView / 1.5f)
                    Instantiate(Chunk, new Vector3(x * 10 + PlayerChunkPos.x * 10, 0, y * 10 + PlayerChunkPos.y * 10), Quaternion.identity);

        ChunksLoaded = true;
    }
}
