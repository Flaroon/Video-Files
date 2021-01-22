using System.Collections.Generic;
using UnityEngine;
public class Circle : MonoBehaviour
{
    public int circle, radius;
    List<Vector3> v = new List<Vector3>();
    List<int> t = new List<int>();

    private void Start()
    {
        for (int i = 0; i < circle; i++){
            float y = radius * Mathf.Sin(Mathf.Deg2Rad * (360 / ((float)circle)*i));
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * (360 / ((float)circle)*i));
            v.Add(new Vector3(x, 0, y));
        }
        for (int l = 0; l < circle; l++){
            this.t.AddRange(new List<int>() {v.Count, v.Count + 1,v.Count + 2 });
            v.AddRange(new List<Vector3>(){v[l],v[l + 1],Vector3.zero});
        }
        Mesh mesh = new Mesh();
        mesh.vertices = v.ToArray();
        mesh.triangles = t.ToArray();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
