using UnityEngine;

public class LandGenerator : MonoBehaviour
{
    public Gradient Color;
    public GameObject Tile;
    public GameObject[] Props;
    public float NoiseScale;
    [Range(0, 1)]
    public float FillPercent;
    public int size;

    private void Start()
    {
        for (int x = 0; x <= size; x++){
            for (int y = 0; y <= size; y++){
                float a = Mathf.PerlinNoise((float)x / NoiseScale, (float)y / NoiseScale);
                float h = a > 1 - (float)FillPercent ? 0 : -0.1f;
                GameObject g = Instantiate(Tile, new Vector3(x, h, y), Quaternion.identity);
                g.GetComponent<MeshRenderer>().material.color = a > 1 - (float)FillPercent ? Color.Evaluate(a + 0.3f) : Color.Evaluate(a);
                g.transform.parent = transform;
                g.transform.localScale = a > 1 - (float)FillPercent ? new Vector3(1, 0.5f, 1) : new Vector3(1, 0.3f, 1);
                GameObject l = Random.value > 0.9f && a > 1 - (float)FillPercent ?
                    Instantiate(Props[Random.Range(0, Props.Length)], new Vector3(x, 0.2f, y), Quaternion.Euler(-90, Random.Range(0, 360), 0)) : null;

            }
        }
    }
}
