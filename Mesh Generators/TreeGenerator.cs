using System.Collections.Generic;
using UnityEngine;
using B83.MeshHelper;

public class TreeGenerator : MonoBehaviour
{
    public float height, LeavesHeight;
    [Range(1, 10)] public int HeightSegments;

    public Color bark, Leaves;

    List<Vector3> verts = new List<Vector3>();
    List<int> Triangles = new List<int>();

    Color[] Colors;

    private void Start()
    {
        // Slow

        Vector3 pos = transform.position;

        for (int i = 0; i < HeightSegments; i++){
            float x = 1 - BiasFunction((float)i / (float)HeightSegments, .2f);
            AddSquarePoints(x, Random.Range(-25, 25), new Vector3(pos.x, i * height + pos.y, pos.z), i);
        }

        float BiasFunction(float x, float Bias) => x * Mathf.Pow(1 - Bias, 3) / (x * Mathf.Pow(1 - Bias, 3) - x + 1);

        // Fast

        for (int i = 0; i < HeightSegments - 1; i++)
        {
            addquad(verts[i * 4],     verts[i * 4 + 1], verts[(i + 1) * 4 + 1], verts[(i + 1) * 4]);
            addquad(verts[i * 4 + 1], verts[i * 4 + 2], verts[(i + 1) * 4 + 2], verts[(i + 1) * 4 + 1]);
            addquad(verts[i * 4 + 2], verts[i * 4 + 3], verts[(i + 1) * 4 + 3], verts[(i + 1) * 4 + 2]);
            addquad(verts[i * 4 + 3], verts[i * 4],     verts[(i + 1) * 4],     verts[(i + 1) * 4 + 3]);
        }

        // Slow

        int s = (HeightSegments - 1) * 4;

        addquad(verts[s], verts[s + 1], verts[s + 2], verts[s + 3]);

        // Fast

        float d = verts[(int)(verts.Count / 2) + (int)(verts.Count / 4)].y;
        AddTreeSquare(3, Random.Range(-5, 5), new Vector3(pos.x, pos.y + LeavesHeight + (d - (d / 2)), pos.z), true);
        AddTreeSquare(2, Random.Range(-5, 5), new Vector3(pos.x, pos.y + LeavesHeight + (d  + (height) * 3), pos.z), false);

        // Slow

        int b = verts.Count - 8;

        // Fast

        addquad(verts[b], verts[b + 1], verts[b + 5], verts[b + 4]);
        addquad(verts[b + 1], verts[b + 2], verts[b + 6], verts[b + 5]);
        addquad(verts[b + 2], verts[b + 3], verts[b + 7], verts[b + 6]);
        addquad(verts[b], verts[b + 4], verts[b + 7], verts[b + 3]);

        // Slow

        GetComponent<MeshFilter>().mesh = new Mesh() {
            vertices = verts.ToArray(),
            triangles = Triangles.ToArray()
        };

        // For End

        var welder = new MeshWelder(GetComponent<MeshFilter>().mesh);

        welder.Weld();

        // Slow

        GetComponent<MeshFilter>().mesh.RecalculateNormals();

        Colors = new Color[GetComponent<MeshFilter>().mesh.vertices.Length];

        for (int i = 0; i < GetComponent<MeshFilter>().mesh.vertices.Length; i++)
        {
            if (i < GetComponent<MeshFilter>().mesh.vertices.Length - 8)
                Colors[i] = bark;
            else
                Colors[i] = Leaves;
        }

        GetComponent<MeshFilter>().mesh.colors = Colors;

    }


    // Slow

    void addquad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int l = verts.Count;
        Triangles.AddRange(new List<int>() { l, l + 1, l + 2, l, l + 2, l + 3 });
        verts.AddRange(new List<Vector3>() { a, b, c, d });
    }



    float Cos(float x) => Mathf.Cos(x); float Sin(float x) => Mathf.Sin(x);




    public void AddSquarePoints(float scale, float Rotation, Vector3 pos, int verticalposition)
    {
        scale = scale * 5;

        // Fast

        Vector3 a = new Vector3(pos.x - scale, pos.y, pos.z + scale)
              , b = new Vector3(pos.x + scale, pos.y, pos.z + scale)
              , c = new Vector3(pos.x + scale, pos.y, pos.z - scale)
              , d = new Vector3(pos.x - scale, pos.y, pos.z - scale);

        // Slow

        int l = verts.Count;

        if (verticalposition == 0) {Triangles.AddRange(new List<int>() { l + 2, l + 1, l, l + 3, l + 2, l });}

        verts.AddRange(new List<Vector3>() { rot(pos, a, Rotation), rot(pos, b, Rotation), rot(pos, c, Rotation), rot(pos, d, Rotation) });

        Vector3 rot(Vector3 center, Vector3 point, float Degrees)
        {
            Vector3 FinalPosition = Vector3.zero;

            float angle = (-Degrees) * (Mathf.PI / 180);
            FinalPosition.x = Cos(angle) * (point.x - center.x) - Sin(angle) * (point.z - center.z) + center.x;
            FinalPosition.z = Sin(angle) * (point.x - center.x) + Cos(angle) * (point.z - center.z) + center.z;

            return new Vector3(FinalPosition.x, pos.y, FinalPosition.z);
        }

    }

    public void AddTreeSquare(float scale, float Rotation, Vector3 pos, bool upordown)
    {
        scale = scale * 5;

        // Fast

        Vector3 a = new Vector3(pos.x - scale, pos.y, pos.z + scale)
              , b = new Vector3(pos.x + scale, pos.y, pos.z + scale)
              , c = new Vector3(pos.x + scale, pos.y, pos.z - scale)
              , d = new Vector3(pos.x - scale, pos.y, pos.z - scale);

        // Slow

        int l = verts.Count;

        if (upordown)
        {
            Triangles.AddRange(new List<int>() { l + 2, l + 1, l, l + 3, l + 2, l });
        }
        else
        {
            Triangles.AddRange(new List<int>() { l, l + 1, l + 2, l, l + 2, l + 3 });
        }

        // Fast

        verts.AddRange(new List<Vector3>() { rot(pos, a, Rotation), rot(pos, b, Rotation), rot(pos, c, Rotation), rot(pos, d, Rotation) });


        Vector3 rot(Vector3 center, Vector3 point, float Degrees)
        {
            Vector3 FinalPosition = Vector3.zero;

            float angle = (-Degrees) * (Mathf.PI / 180);
            FinalPosition.x = Cos(angle) * (point.x - center.x) - Sin(angle) * (point.z - center.z) + center.x;
            FinalPosition.z = Sin(angle) * (point.x - center.x) + Cos(angle) * (point.z - center.z) + center.z;

            return new Vector3(FinalPosition.x, pos.y, FinalPosition.z);
        }
    }
}
