using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    [Header("Settings")]
    public int health;
    public int damage;
    public Sprite sprite;
    public Image panelP1;
    public Image panelP2;
    public Sequence[] sequences;

    #region PrivateVariables
    private bool[] currentSequence;
    private bool offCooldownP1;
    private bool offCooldownP2;
    private bool onP1;
    private bool onP2;
    #endregion

    public void ChangeSequence()
    {
        currentSequence = sequences[Random.Range(0, sequences.Length)].sequence;
    }

    public bool[] GetSequence()
    {
        return currentSequence;
    }

    public bool OffCooldown(int player)
    {
        if (player == 1)
        {
            return offCooldownP1;
        }
        else
        {
            return offCooldownP2;
        }
    }

    public bool On(int player)
    {
        if (player == 1)
        {
            return onP1;
        }
        else
        {
            return onP2;
        }
    }

    public void StartCooldown(int player, float time)
    {
        IEnumerator coroutine = CooldownCycle(time, player);
        StartCoroutine(coroutine);
    }

    public void Toggle(bool state, int player)
    {
        if (player == 1)
        {
            onP1 = state;
        }
        else
        {
            onP2 = state;
        }
    }

    private IEnumerator CooldownCycle(float duration, int player)
    {
        if (player == 1)
        {
            offCooldownP1 = false;
            panelP1.fillAmount = 1;
        }
        else
        {
            offCooldownP2 = false;
            panelP2.fillAmount = 1;
        };

        float t = 0.0f;
        float currentTime = 0.0f;
        yield return null;

        while (t < 1)
        {
            currentTime += Time.deltaTime;
            t = Mathf.Clamp01(currentTime / duration);
            if (player == 1) panelP1.fillAmount = 1 - t;
            else panelP2.fillAmount = 1 - t;
            yield return null;
        }

        if (player == 1)
        {
            offCooldownP1 = true;
            panelP1.fillAmount = 0;
        }
        else
        {
            offCooldownP2 = true;
            panelP2.fillAmount = 0;
        };
    }
}

[System.Serializable]
public struct Sequence
{
    public bool[] sequence;
}