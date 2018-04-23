using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Castle : MonoBehaviour
{
    public enum Team
    {
        Funk,
        Metal
    };

    public enum PlayerState
    {
        Idle,
        Attempt,
        Invoking,
        HitCalculation
    };

    public enum HitType
    {
        Perfect,
        Good,
        TooSoon,
        TooLate,
        Miss,
        Wrong
    }

    [Header("Settings")]
    public int player = 1;
    public bool singlePlayer = true;
    public Team team = Team.Funk;
    public float perfectTolerance = 0.05f;
    public float goodTolerance = 0.10f;
    public float badTolerance = 0.20f;

    [Header("Game Objects")]
    public Transform gridSelector;
    public Unit[] units;
    public Image healthBar;
    public Image healthGainBar;
    public Image spellMeter;
    public Image spellGainMeter;
    public MessageManager msg;

    #region PrivateVariables
    private PlayerState state = PlayerState.Idle;
    private Engine engine;
    private Vector3 selectionPosition;
    private bool movementCooldown = false;
    private float moveAnimationTime = 0.15f;
    private IEnumerator animationRoutine;
    #endregion

    #region HealthVariables
    private float health = 1.0f;
    private float healthLost = 0.0f;
    #endregion

    #region InvokingVariables
    private float[] tolerances;
    private int currentInvoking = -1;
    private int invokingStep = 0;
    private float spellLevel = 0.0f;
    private float posibleSpellGain = 0.0f;
    private float invokationLevel = 0.0f;
    #endregion

    void Awake()
    {
        engine = FindObjectOfType<Engine>();
        selectionPosition = new Vector3(player == 1 ? -70 : 70, 1, 30);
    }

    void Start()
    {
        gridSelector.position = selectionPosition;
        CalculateTolerances();

        float timeUnit = engine.GetTimeBetweenBeats();
        for (int i = 0; i < units.Length; i++)
        {
            units[i].ChangeSequence();
            if (i < 3)
            {
                units[i].StartCooldown(player, timeUnit * 8 * (i+1));
            }
            else
            {
                units[i].Toggle(false, player);
            }
        }
    }

    void Update()
    {
        //Movement Input
        if (state == PlayerState.Idle && !movementCooldown)
        {
            if (((player == 1 && Input.GetKeyDown(KeyCode.W)) || (player == 2 && Input.GetKeyDown(KeyCode.UpArrow))) && selectionPosition.z != 30)
            {
                Move('W');
            }
            else if (((player == 1 && Input.GetKeyDown(KeyCode.S)) || (player == 2 && Input.GetKeyDown(KeyCode.DownArrow))) && selectionPosition.z != -30)
            {
                Move('S');
            }
            else if (((player == 1 && Input.GetKeyDown(KeyCode.A)) || (player == 2 && Input.GetKeyDown(KeyCode.LeftArrow))) && selectionPosition.x != -70)
            {
                Move('A');
            }
            else if (((player == 1 && Input.GetKeyDown(KeyCode.D)) || (player == 2 && Input.GetKeyDown(KeyCode.RightArrow))) && selectionPosition.x != 70)
            {
                Move('D');
            }
        }

        //Unit Selection
        bool[] keys = new bool[6];
        keys[0] = (player == 1 && Input.GetKey(KeyCode.Alpha1)) || (player == 2 && Input.GetKey(KeyCode.Alpha7));
        keys[1] = (player == 1 && Input.GetKey(KeyCode.Alpha2)) || (player == 2 && Input.GetKey(KeyCode.Alpha8));
        keys[2] = (player == 1 && Input.GetKey(KeyCode.Alpha3)) || (player == 2 && Input.GetKey(KeyCode.Alpha9));
        keys[3] = (player == 1 && Input.GetKey(KeyCode.F1)) || (player == 2 && Input.GetKey(KeyCode.F7));
        keys[4] = (player == 1 && Input.GetKey(KeyCode.F2)) || (player == 2 && Input.GetKey(KeyCode.F8));
        keys[5] = (player == 1 && Input.GetKey(KeyCode.F3)) || (player == 2 && Input.GetKey(KeyCode.F9));

        int count = 0;
        int selection = -1;
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i])
            {
                count++;
                selection = i;
            }
        }

        if (count == 0 || count > 1)
        {
            if (state == PlayerState.Invoking)
            {
                BreakInvokation();
            }
            else if (state == PlayerState.Attempt)
            {
                BreakAttempt();
            }
        }
        else
        {
            if (state == PlayerState.Idle && ((selection < 3 && units[selection].OffCooldown(player)) || units[selection].On(player)))
            {
                AttemptInvokation(selection);
            }
            else if (state == PlayerState.Invoking && selection != currentInvoking)
            {
                BreakInvokation();
            }
        }

        float beat = engine.GetPreciseBeat();
        int step = invokingStep % 8;
        if (step == 0 && beat > 7)
        {
            beat -= 8;
        }

        //Invoking input
        if (((player == 1 && Input.GetKeyDown(KeyCode.Space)) || (player == 2 && Input.GetKeyDown(KeyCode.RightShift))) && (state == PlayerState.Attempt || state == PlayerState.Invoking))
        {
            bool[] sequence = units[currentInvoking].GetSequence();
            if (sequence[invokingStep])
            {
                if (beat >= step - tolerances[0] && beat <= step + tolerances[0])
                {
                    InvokationHit(HitType.Perfect, false);
                }
                else if (beat >= step - tolerances[1] && beat <= step + tolerances[1])
                {
                    InvokationHit(HitType.Good, false);
                }
                else if (beat >= step - tolerances[2] && beat <= step + tolerances[2])
                {
                    if (beat < step)
                    {
                        InvokationHit(HitType.TooSoon, false);
                    }
                    else
                    {
                        InvokationHit(HitType.TooLate, false);
                    }
                }
                else if (state == PlayerState.Attempt)
                {
                    InvokationHit(HitType.Wrong, false);
                }
            }
            else
            {
                InvokationHit(HitType.Wrong, true);
            }
        }


        //Catching Misses
        else if (state == PlayerState.Invoking)
        {
            bool[] sequence = units[currentInvoking].GetSequence();
            if (sequence[invokingStep] && beat > step + tolerances[2])
            {
                InvokationHit(HitType.Miss, false);
            }
            else if (!sequence[invokingStep] && beat > step + tolerances[0])
            {
                InvokationHit(HitType.Good, true);
            }
        }
    }

    private void Move(char direction)
    {
        Vector3 temp = selectionPosition;
        switch (direction)
        {
            case 'W':
                selectionPosition += Vector3.forward * 20;
                break;
            case 'S':
                selectionPosition += Vector3.forward * -20;
                break;
            case 'A':
                selectionPosition += Vector3.right * -20;
                break;
            case 'D':
                selectionPosition += Vector3.right * 20;
                break;
        }

        animationRoutine = GridAnimation(temp, selectionPosition);
        StartCoroutine(animationRoutine);
    }

    private void CalculateTolerances()
    {
        tolerances = new float[3];
        float timeBetween = engine.GetTimeBetweenBeats();
        tolerances[0] = timeBetween * perfectTolerance;
        tolerances[1] = timeBetween * goodTolerance;
        tolerances[2] = timeBetween * badTolerance;
    }

    private void BreakInvokation()
    {
        state = PlayerState.Idle;
        invokingStep = 0;
        posibleSpellGain = 0;
        invokationLevel = 0;
        UpdateMeters();
        if (currentInvoking != -1) SetCooldowns();
        msg.Failure();
    }

    private void AttemptInvokation(int selection)
    {
        state = PlayerState.Attempt;
        currentInvoking = selection;
        Unit u = units[currentInvoking];
        msg.Attempt(u.GetSequence(), u.health, u.damage, u.sprite);
    }

    private void BreakAttempt()
    {
        state = PlayerState.Idle;
        invokingStep = 0;
        currentInvoking = -1;
        posibleSpellGain = 0;
        invokationLevel = 0;
        UpdateMeters();
        msg.StopAttempt();
    }

    private void InvokationHit(HitType type, bool off)
    {
        if (state == PlayerState.Attempt && type == HitType.Wrong)
        {
            msg.WrongAttempt();
            return;
        }

        state = PlayerState.HitCalculation;

        bool[] sequence = units[currentInvoking].GetSequence();
        float levelGain = 1.0f / sequence.Length;
        float spellGain = currentInvoking < 3 ? 0.0125f : 0.0f;
        switch (type)
        {
            case HitType.Perfect:
                spellGain *= 2.0f;
                break;
            case HitType.Good:
                levelGain *= 0.8f;
                break;
            case HitType.TooLate:
                levelGain *= 0.6f;
                spellGain = 0;
                break;
            case HitType.TooSoon:
                levelGain *= 0.6f;
                spellGain = 0;
                break;
            default:
                levelGain = 0f;
                spellGain = 0;
                break;
        }
        posibleSpellGain += spellGain;
        invokationLevel += levelGain;
        invokingStep++;
        if (invokingStep == sequence.Length)
        {
            
            if (invokationLevel > 0.6f)
            {
                msg.End(type, invokationLevel, true);
                engine.DeployTroop(player, currentInvoking, invokationLevel, PosToCell());
                spellLevel = Mathf.Clamp01(spellLevel + posibleSpellGain);            
            }
            else
            {
                msg.End(type, invokationLevel, false);
            }
            state = PlayerState.Idle;
            posibleSpellGain = 0;
            invokationLevel = 0;
            invokingStep = 0;

            if (currentInvoking != -1) SetCooldowns();
        }
        else
        {
            msg.Hit(type, invokingStep - 1, invokationLevel, off);
        }

        UpdateMeters();
        float timeBetween = engine.GetTimeBetweenBeats();
        float cooldownTime = Mathf.Clamp(timeBetween - (4 * tolerances[2] * timeBetween), 0.01f, timeBetween);
        IEnumerator sCooldown = HitCooldown(cooldownTime);
        StartCoroutine(sCooldown);
    }

    private void SetCooldowns()
    {
        if (currentInvoking < 3)
        {
            units[currentInvoking].StartCooldown(player, (currentInvoking + 1) * engine.GetTimeBetweenBeats() * 8);
        }
        else
        {
            spellLevel = Mathf.Clamp01(spellLevel - 0.333f * (currentInvoking - 2));
        }
        currentInvoking = -1;
    }

    private void UpdateMeters()
    {
        spellMeter.fillAmount = spellLevel;
        spellGainMeter.fillAmount = spellLevel + posibleSpellGain;
        healthBar.fillAmount = health;
        healthGainBar.fillAmount = health + healthLost;

        units[3].Toggle(spellLevel > 0.333f, player);
        units[4].Toggle(spellLevel > 0.666f, player);
        units[5].Toggle(spellLevel > 0.999f, player);
    }

    private Vector2 PosToCell()
    {
        return new Vector2(selectionPosition.x, selectionPosition.z);
    }

    private IEnumerator GridAnimation(Vector3 from, Vector3 to)
    {
        movementCooldown = true;
        float t = 0.0f;
        float current = 0.0f;
        gridSelector.position = from;
        yield return null;

        while (t < 1)
        {
            current += Time.deltaTime;
            t = Mathf.Clamp01(current / moveAnimationTime);
            gridSelector.position = Vector3.Lerp(from, to, t);
            yield return null;
        }

        gridSelector.position = to;
        movementCooldown = false;
    }

    private IEnumerator HitCooldown(float time)
    {
        yield return new WaitForSeconds(time);
        if (state == PlayerState.HitCalculation)
        {
            state = PlayerState.Invoking;
        }
    }
}
