using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{

    public enum GameState
    {
        Start,
        Ongoing,
        Pause,
        Finish
    };

    [Header("Game Objects")]
    public Grid grid;
    public BeatDisplay beatDisplay;

    [Header("Settings")]
    public int Tempo = 120;

    #region PrivateVariables
    public GameState State { get; private set; }
    private float timeBetweenBeats;
    private float lastBeatTime;
    private float nextBeatTime;
    private int beat;
    private float preciseBeat;
    #endregion

    void Awake()
    {
        State = GameState.Start;
        beat = 0;
        preciseBeat = 0.0f;
        timeBetweenBeats = 60.0f / (Tempo * 2);
        lastBeatTime = Time.time;
        nextBeatTime = lastBeatTime + timeBetweenBeats;
    }

    void Start()
    {
        beatDisplay.ChangeBeat(beat);
    }

    void Update()
    {
        float currentTime = Time.time;
        if (currentTime > nextBeatTime)
        {
            beat++;
            if (beat == 8) beat = 0;
            beatDisplay.ChangeBeat(beat);
            lastBeatTime = nextBeatTime;
            nextBeatTime = lastBeatTime + timeBetweenBeats;
        }

        preciseBeat = beat + Mathf.InverseLerp(lastBeatTime, nextBeatTime, currentTime);

    }

    public int GetBeat()
    {
        return beat;
    }

    public float GetPreciseBeat()
    {
        return preciseBeat;
    }

    public float GetTimeBetweenBeats()
    {
        return timeBetweenBeats;
    }

    public void DeployTroop(int player, int troop, float level, Vector2 position)
    {
        //TODO
    }
}
