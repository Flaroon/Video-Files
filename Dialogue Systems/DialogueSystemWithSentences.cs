using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class DialogueSystemTutorial : MonoBehaviour
{
    public TextMeshProUGUI TextObj;
    public TextVars[] text;

    void Start()
    {
        StartCoroutine(ShowText());
    }

    private IEnumerator ShowText()
    {
        foreach (TextVars t in text)
        {
            string v = "";
            foreach (char c in t.Sentence)
            {
                v += c.ToString();
                TextObj.text = v;
                TextObj.fontSize = t.TextSize;
                TextObj.color = t.TextColor;
                yield return new WaitForSeconds(t.Speed / 60);
            }
            yield return new WaitForSeconds(t.EndDelay);
        }
    }
}

[Serializable]
public struct TextVars {
    [TextArea(3, 20)]
    public string Sentence;
    public float Speed, EndDelay, TextSize;
    public Color TextColor;

}

