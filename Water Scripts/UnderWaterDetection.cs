using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UnderWaterDetection : MonoBehaviour
{
    public GameObject BoundingBox, Player;
    public Volume Post;
    public Color UnderWaterColor;
    public bool UnderWater;

    // Effects

    private Vignette VG;
    private DepthOfField DOF;
    private ColorAdjustments CA;

    private void Start()
    {
        Post.profile.TryGet(out DOF);
        Post.profile.TryGet(out CA);
        Post.profile.TryGet(out VG);
    }

    private void FixedUpdate()
    {
        if (BoundingBox.GetComponent<BoxCollider>().bounds.Contains(Player.transform.position)) UnderWater = false;
        else UnderWater = true;

        if (!UnderWater)
        {
            VG.intensity.value = 0.35f;
            DOF.focusDistance.value = 0.1f;
            CA.colorFilter.value = UnderWaterColor;
        }
        else
        {
            VG.intensity.value = 0.292f;
            DOF.focusDistance.value = 5f;
            CA.colorFilter.value = Color.white;
        }
    }
}
