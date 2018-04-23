using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatDisplay : MonoBehaviour
{
    [Header("Game Objects")]
    public Image[] beats;
    public RectTransform[] beatSizes;

    [Header("Colors")]
    public Color barColor;
    public Color eigthColor;
    public Color forthColor;

    #region PrivateVariables
    private int CurrentBeat;
    private Color barColorOff;
    private Color eigthColorOff;
    private Color forthColorOff;
    #endregion

    void Start()
    {
        CurrentBeat = 0;
        float alphaFactor = 0.5f;
        barColorOff = barColor;
        barColorOff.a = alphaFactor;
        eigthColorOff = eigthColor;
        eigthColorOff.a = alphaFactor;
        forthColorOff = forthColor;
        forthColorOff.a = alphaFactor;
        for (int i = 0; i < beats.Length; i++)
        {
            beats[i].fillCenter = false;
            if (i == 0) beats[i].color = barColorOff;
            else if (i % 2 == 1) beats[i].color = eigthColorOff;
            else beats[i].color = forthColorOff;
        }
    }

    public void ChangeBeat(int beat)
    {
        beatSizes[CurrentBeat].localScale = Vector3.one;
        beats[CurrentBeat].fillCenter = false;
        if (CurrentBeat == 0) beats[CurrentBeat].color = barColorOff;
        else if (CurrentBeat % 2 == 1) beats[CurrentBeat].color = eigthColorOff;
        else beats[CurrentBeat].color = forthColorOff;

        CurrentBeat = beat;

        beatSizes[CurrentBeat].localScale = Vector3.one * 1.05f;
        beats[CurrentBeat].fillCenter = true;
        if (CurrentBeat == 0) beats[CurrentBeat].color = barColor;
        else if (CurrentBeat % 2 == 1) beats[CurrentBeat].color = eigthColor;
        else beats[CurrentBeat].color = forthColor;
    }
}
