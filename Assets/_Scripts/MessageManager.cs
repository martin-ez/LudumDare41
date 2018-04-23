using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    [Header("Colors")]
    public Color perfectColor;
    public Color goodColor;
    public Color regularColor;
    public Color badColor;
    public Color transparent;
    public Color panelColor;
    public Color barColor;
    public Color eigthColor;
    public Color forthColor;

    [Header("Panel")]
    public Image mainPanel;
    public Image levelPanel;

    [Header("Message")]
    public Image messagePanel;
    public Text messageText;

    [Header("Level")]
    public Image levelBg;
    public Image levelBar;

    [Header("Info")]
    public Image thumbnail;

    public Image health;
    public Text healthText;

    public Image attack;
    public Text attackText;

    [Header("Sequence")]
    public RectTransform tracker;
    public RectTransform bar1Seq;
    public Image[] bar1Hits;
    public Image[] bar1Dots;

    public RectTransform bar2Seq;
    public Image[] bar2Hits;
    public Image[] bar2Dots;

    public RectTransform bar4Seq;
    public Image[] bar4Hits;
    public Image[] bar4Dots;

    #region PrivateVariables
    private Castle.PlayerState state = Castle.PlayerState.Idle;
    private Engine engine;
    private int bars;
    private bool[] sequence;
    private int currentStep;
    private int unitHealth;
    private int unitAttack;
    private Sprite unitSprite;
    private Color barColorOff;
    private Color eigthColorOff;
    private Color forthColorOff;
    #endregion

    #region SequencerVariables
    private Image[] hits;
    private Image[] dots;
    #endregion

    #region Animations
    IEnumerator panelAnimation;
    IEnumerator msgAnimation;
    #endregion

    void Awake()
    {
        engine = FindObjectOfType<Engine>();
    }

    void Start()
    {
        float alphaFactor = 0.2f;
        barColorOff = barColor;
        barColorOff.a = alphaFactor;
        eigthColorOff = eigthColor;
        eigthColorOff.a = alphaFactor;
        forthColorOff = forthColor;
        forthColorOff.a = alphaFactor;

        mainPanel.color = transparent;
        levelPanel.color = transparent;

        messagePanel.color = transparent;
        messageText.color = transparent;

        levelBg.color = transparent;
        levelBar.fillAmount = 0;

        thumbnail.color = transparent;
        health.color = transparent;
        healthText.color = transparent;
        attack.color = transparent;
        attackText.color = transparent;

        bar1Seq.gameObject.SetActive(false);
        bar2Seq.gameObject.SetActive(false);
        bar4Seq.gameObject.SetActive(false);
        tracker.gameObject.SetActive(false);
    }

    void Update()
    {
        if (state == Castle.PlayerState.Attempt || state == Castle.PlayerState.Invoking)
        {
            float beat = engine.GetPreciseBeat();

            if (state == Castle.PlayerState.Attempt && beat > 5) beat -= 8;
            else if (state == Castle.PlayerState.Invoking) beat += Mathf.FloorToInt(currentStep / 8) * 8;


            float t = beat - Mathf.FloorToInt(beat);
            float position = 0;
            float start = 0;
            float step = 0;
            switch (bars)
            {
                case 1:
                    start = -105;
                    step = 30;
                    break;
                case 2:
                    start = -150;
                    step = 20;
                    break;
                case 4:
                    start = -217;
                    step = 14;
                    break;
            }
            position = Mathf.LerpUnclamped(start + (step * Mathf.FloorToInt(beat)), start + (step * Mathf.FloorToInt(beat + 1)), t);
            tracker.localPosition = new Vector3(position, 0, 0);
        }
    }

    public void Attempt(bool[] s, int h, int a, Sprite spr)
    {
        unitHealth = h;
        unitAttack = a;
        sequence = s;
        bars = sequence.Length / 8;
        if (panelAnimation != null) StopCoroutine(panelAnimation);
        panelAnimation = ShowPanel(spr);
        StartCoroutine(panelAnimation);
    }

    public void StopAttempt()
    {
        if (panelAnimation != null) StopCoroutine(panelAnimation);
        panelAnimation = HidePanel(false, badColor);
        StartCoroutine(panelAnimation);
    }

    public void WrongAttempt()
    {
        if (msgAnimation != null) StopCoroutine(msgAnimation);
        msgAnimation = ShowMessage("Wrong", badColor, true);
        StartCoroutine(msgAnimation);
    }

    public void Hit(Castle.HitType type, int step, float level, bool off)
    {
        state = Castle.PlayerState.Invoking;
        UpdateLevel(level);
        currentStep = step;
        if (msgAnimation != null) StopCoroutine(msgAnimation);
        Color dotColor = badColor;
        switch (type)
        {
            case Castle.HitType.Perfect:
                dotColor = perfectColor;
                msgAnimation = ShowMessage("Perfect", perfectColor, false);
                break;
            case Castle.HitType.Good:
                dotColor = goodColor;
                msgAnimation = ShowMessage("Good", goodColor, false);
                break;
            case Castle.HitType.TooLate:
                dotColor = regularColor;
                msgAnimation = ShowMessage("Too Late", regularColor, false);
                break;
            case Castle.HitType.TooSoon:
                dotColor = regularColor;
                msgAnimation = ShowMessage("Too Soon", regularColor, false);
                break;
            case Castle.HitType.Wrong:
                dotColor = badColor;
                msgAnimation = ShowMessage("Wrong", badColor, false);
                break;
            case Castle.HitType.Miss:
                dotColor = badColor;
                msgAnimation = ShowMessage("Miss", badColor, false);
                break;
        }

        if (!off)
        {
            dots[currentStep].color = dotColor;
            if (currentStep % 8 == 0) hits[currentStep].color = barColor;
            else if (currentStep % 2 == 1) hits[currentStep].color = eigthColor;
            else hits[currentStep].color = forthColor;
            StartCoroutine(msgAnimation);
        }

    }

    public void End(Castle.HitType type, float level, bool success)
    {
        Color levelColor = UpdateLevel(level);

        StopAllCoroutines();
        panelAnimation = HidePanel(true, levelColor);
        StartCoroutine(panelAnimation);
        if (success) msgAnimation = ShowMessage("Success", goodColor, true);
        else msgAnimation = ShowMessage("Failure", badColor, true);
        StartCoroutine(msgAnimation);
    }

    public void Failure()
    {
        StopAllCoroutines();
        panelAnimation = HidePanel(false, badColor);
        StartCoroutine(panelAnimation);
        msgAnimation = ShowMessage("Failed", badColor, true);
        StartCoroutine(msgAnimation);
    }

    private Color UpdateLevel(float level)
    {
        levelBar.fillAmount = level;

        Color levelColor = badColor;
        if (level >= 0.87) levelColor = perfectColor;
        else if (level >= 0.74) levelColor = goodColor;
        else if (level >= 0.6) levelColor = regularColor;

        if (level >= 0.6)
        {
            attackText.text = "" + Mathf.FloorToInt(unitAttack * level);
            healthText.text = "" + Mathf.FloorToInt(unitHealth * level);
        }

        levelBar.color = levelColor;
        return levelColor;
    }

    private void DisplaySequence()
    {
        bar1Seq.gameObject.SetActive(false);
        bar2Seq.gameObject.SetActive(false);
        bar4Seq.gameObject.SetActive(false);
        tracker.gameObject.SetActive(true);

        switch (bars)
        {
            case 1:
                bar1Seq.gameObject.SetActive(true);
                hits = bar1Hits;
                dots = bar1Dots;
                break;
            case 2:
                bar2Seq.gameObject.SetActive(true);
                hits = bar2Hits;
                dots = bar2Dots;
                break;
            case 4:
                bar4Seq.gameObject.SetActive(true);
                hits = bar4Hits;
                dots = bar4Dots;
                break;
        }

        for (int i = 0; i < hits.Length; i++)
        {
            dots[i].color = transparent;
            if (sequence[i])
            {
                if (i % 8 == 0) hits[i].color = barColorOff;
                else if (i % 2 == 1) hits[i].color = eigthColorOff;
                else hits[i].color = forthColorOff;
            }
            else
            {
                hits[i].color = transparent;
            }
        }
        state = Castle.PlayerState.Attempt;
    }

    private IEnumerator ShowPanel(Sprite spr)
    {
        float t = 0;
        float time = 0;
        float duration = 0.25f;

        mainPanel.color = transparent;
        levelPanel.color = transparent;

        levelBg.color = transparent;
        levelBar.fillAmount = 0;

        thumbnail.sprite = spr;
        thumbnail.color = transparent;
        health.color = transparent;
        healthText.color = transparent;
        healthText.text = "00";
        attack.color = transparent;
        attackText.color = transparent;
        attackText.text = "00";

        yield return null;

        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.Clamp01(time / duration);

            Color transitionColorWhite = Color.Lerp(transparent, Color.white, t);
            Color transitionColorBlack = Color.Lerp(transparent, Color.black, t);

            mainPanel.color = Color.Lerp(transparent, panelColor, t);
            levelPanel.color = Color.Lerp(transparent, panelColor, t);

            levelBg.color = transitionColorWhite;
            thumbnail.color = transitionColorWhite;
            health.color = transitionColorWhite;
            healthText.color = transitionColorBlack;
            attack.color = transitionColorWhite;
            attackText.color = transitionColorBlack;
            yield return null;
        }

        mainPanel.color = panelColor;
        levelPanel.color = panelColor;

        levelBg.color = Color.white;
        thumbnail.color = Color.white;
        health.color = Color.white;
        healthText.color = Color.black;
        attack.color = Color.white;
        attackText.color = Color.black;
        DisplaySequence();
    }

    private IEnumerator HidePanel(bool delay, Color levelColor)
    {
        float t = 0;
        float time = 0;
        float duration = 0.25f;

        state = Castle.PlayerState.Idle;
        bar1Seq.gameObject.SetActive(false);
        bar2Seq.gameObject.SetActive(false);
        bar4Seq.gameObject.SetActive(false);
        tracker.gameObject.SetActive(false);
        if (delay) yield return new WaitForSeconds(duration * 4);
        else yield return null;

        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.Clamp01(time / duration);

            Color transtionColorLevel = Color.Lerp(levelColor, transparent, t);
            Color transitionColorWhite = Color.Lerp(Color.white, transparent, t);
            Color transitionColorBlack = Color.Lerp(Color.black, transparent, t);

            mainPanel.color = Color.Lerp(panelColor, transparent, t);
            levelPanel.color = Color.Lerp(panelColor, transparent, t);

            levelBg.color = transitionColorWhite;
            levelBar.color = transtionColorLevel;
            thumbnail.color = transitionColorWhite;
            health.color = transitionColorWhite;
            healthText.color = transitionColorBlack;
            attack.color = transitionColorWhite;
            attackText.color = transitionColorBlack;
            yield return null;
        }

        mainPanel.color = transparent;
        levelPanel.color = transparent;

        levelBg.color = transparent;
        levelBar.color = transparent;
        thumbnail.color = transparent;
        health.color = transparent;
        healthText.color = transparent;
        attack.color = transparent;
        attackText.color = transparent;
    }

    private IEnumerator ShowMessage(string msg, Color color, bool doubleTime)
    {
        float t = 0;
        float time = 0;
        float duration = engine.GetTimeBetweenBeats() / 4.0f;

        if (doubleTime) duration *= 2;
        messagePanel.color = color;
        messageText.color = Color.black;
        messageText.text = msg;
        yield return new WaitForSeconds(duration * 3);

        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.Clamp01(time / duration);

            Color transitionColor = Color.Lerp(color, transparent, t);
            Color transitionColorBlack = Color.Lerp(Color.black, transparent, t);

            messagePanel.color = transitionColor;
            messageText.color = transitionColorBlack;
            yield return null;
        }

        messagePanel.color = transparent;
        messageText.color = transparent;
    }
}
