using UnityEngine;
using UnityEngine.Tilemaps;

public class Generator : MonoBehaviour
{
    public Vector2Int XYSize;
    public Tilemap Tilemap;
    public Tile TileObject;

    public int SmoothingSteps, BorderThickness;
    [Range(0, 100)] public float FillPercent;
    private int[,] TileNumbers;

    private void Awake()
    {
        TileNumbers = new int[XYSize.x, XYSize.y];

        for (int x = 0; x < XYSize.x; x++)
            for (int y = 0; y < XYSize.y; y++){
                if (x < BorderThickness || x > XYSize.x - BorderThickness - 1 || y < BorderThickness || y > XYSize.y - BorderThickness - 1) TileNumbers[x, y] = 0;
                else TileNumbers[x, y] = FillPercent > Random.Range(0.0f, 100.0f) ? 1 : 0;
            }

        for (int i = 0; i < SmoothingSteps; i++)
            for (int x = 0; x < XYSize.x; x++)
                for (int y = 0; y < XYSize.y; y++) {
                    if (x < BorderThickness || x > XYSize.x - BorderThickness - 1 || y < BorderThickness || y > XYSize.y - BorderThickness - 1) TileNumbers[x, y] = 0;
                    else {
                        int total = 0;
                        for (int xc = -1; xc <= 1; xc++)
                            for (int yc = -1; yc <= 1; yc++)
                                total += TileNumbers[x + xc, y + yc];
                        if (total >= 5) TileNumbers[x, y] = 1;
                        else if (total < 4) TileNumbers[x, y] = 0;
                    }
                }

        for (int x = 0; x < XYSize.x; x++)
            for (int y = 0; y < XYSize.y; y++)
            {
                Tilemap.SetTile(new Vector3Int(x, y, 0), null);
                if (TileNumbers[x, y] == 1) Tilemap.SetTile(new Vector3Int(Mathf.RoundToInt(x - XYSize.x / 2f), Mathf.RoundToInt(y - XYSize.y / 2f), 0), TileObject);
            }
    }
}
