using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextScroller : MonoBehaviour
{
    public string Text;
    public Text textObject;


    private void Start()
    {
        StartCoroutine(ShowText(""));
    }

    private IEnumerator ShowText(string s)
    {
        foreach (char c in Text){
            s += c.ToString();
            textObject.text = s;
            yield return new WaitForSeconds(0.05f); 
        }
    }
}
