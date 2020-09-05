using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;


public class TellMeWhenScript : MonoBehaviour {

    public KMSelectable Button;
    public KMBombModule Module;
    public KMAudio Audio;
    private int TimeIThink;
    public KMBombInfo Bomb;
    static int _moduleIdCounter = 1;
    int _moduleID = 0;
    private int SolveCheck = 0;
    private bool TimerOn;
    public TextMesh Timer;
    private float time = 1;
    private bool Begin;
    private bool Solved;


    void Awake()
    {
        _moduleID = _moduleIdCounter++;
    }

    // Baba booey
    void Start() {
        FindTime();
        StartCoroutine(Flicker());
        Button.OnInteract += delegate
        {
            StartCoroutine(AnimButton());
            Button.AddInteractionPunch();
            ButtonPress();
            Audio.PlaySoundAtTransform("press", Button.transform);
            return false;
        };
    }

    // Lemme tell you something, lemme tell you something
    void Update() {
        
        if (!Solved)
        {
            if (SolveCheck != Bomb.GetSolvedModuleNames().Count)
            {
                FindTime();
                SolveCheck = Bomb.GetSolvedModuleNames().Count();
            }
        }
        if (Begin)
        {
            time += Time.deltaTime; Timer.text = ((int)time).ToString("00");
        }
        if (Timer.text == 31.ToString())
        {
            Begin = false;
            TimerOn = false;
            time = 1;
            Audio.PlaySoundAtTransform("buzzer", Button.transform);
            Module.HandleStrike();
            Timer.text = "--";
        }

    }
    void FindTime() {
        if (Bomb.GetBatteryCount() >= 7 && Bomb.IsIndicatorOn(Indicator.BOB) && Bomb.GetPortCount() == 0)
        {
            TimeIThink = 5;
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #1 applies, making the time 05.", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if (Bomb.IsIndicatorOn(Indicator.BOB))
        {
            if (Bomb.GetBatteryCount() == 0)
            {
                TimeIThink = 7;
                Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #2 applies, making the time 07.", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
            }
            else
            {
                TimeIThink = (Bomb.GetBatteryCount() % 30 + 1);
                Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #2 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
            }
        }
        else if (Bomb.GetSolvedModuleNames().Count() % 10 == Bomb.GetSerialNumberNumbers().Last())
        {
            TimeIThink = (Bomb.GetSerialNumberNumbers().First() + Bomb.GetSerialNumberNumbers().Last() + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #3 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if (Bomb.GetBatteryCount() == Bomb.GetIndicators().Count() && Bomb.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U'))
        {
            TimeIThink = (Bomb.GetModuleNames().Count() % 30 + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #4 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if (Bomb.GetBatteryCount() >= 2 && Bomb.IsPortPresent(Port.Parallel) && Bomb.GetStrikes() != 0)
        {
            TimeIThink = (Bomb.GetBatteryHolderCount() % 30 + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #5 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if ((Bomb.IsPortPresent(Port.HDMI) && Bomb.IsPortPresent(Port.AC)) || (Bomb.IsIndicatorOn(Indicator.CLR) && Bomb.IsPortPresent(Port.StereoRCA)))
        {
            TimeIThink = (Bomb.GetSolvedModuleNames().Count() % 30 + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #6 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if (Bomb.GetSerialNumberNumbers().Any(x => x == (Bomb.GetPortPlateCount() % 10)))
        {
            TimeIThink = ((Bomb.GetSerialNumberNumbers().Last() + 1) * (Bomb.GetPortCount() + 1) % 30) + 1;
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #7 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if (Bomb.GetTwoFactorCounts() != 0 || Bomb.IsIndicatorOff(Indicator.MSA))
        {
            TimeIThink = (31 - (Bomb.GetIndicators().Count() * 6 + Bomb.GetSerialNumberLetters().Count()) % 30 + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #8 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if (Bomb.GetSerialNumberNumbers().Any(x => x == (Bomb.GetSolvedModuleNames().Count() % 10)))
        {
            TimeIThink = (DigitalRoot(Bomb.GetSerialNumberNumbers().Sum()) * 2 + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #9 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else if (Bomb.GetSerialNumberNumbers().Last() % 2 == 0)
        {
            TimeIThink = ((Bomb.GetBatteryCount() + 1) * (Bomb.GetSerialNumberNumbers().Last() / 2 + 1) % 30 + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #10 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
        else
        {
            TimeIThink = (((Bomb.GetBatteryCount() + 1) * (Bomb.GetSolvedModuleNames().Count() + 1) + Bomb.GetPortCount(Port.VGA)) % 30 + 1);
            Debug.LogFormat("[Tell Me When #{0}] For {1} solve(s) and {2} strikes, rule #11 applies, making the time " + TimeIThink.ToString("00") + ".", _moduleID, Bomb.GetSolvedModuleNames().Count(), Bomb.GetStrikes());
        }
    }
    private static int DigitalRoot(int n)
    {
        var root = 0;
        while (n > 0 || root > 9)
        {
            if (n == 0)
            {
                n = root;
                root = 0;
            }

            root += n % 10;
            n /= 10;
        }
        return root;
    }

    private IEnumerator AnimButton()
    {
        for (int i = 0; i < 3; i++)
        {
            Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y - 0.002f, Button.transform.localPosition.z);
            yield return new WaitForSeconds(0.02f);
        }
        for (int i = 0; i < 6; i++)
        {
            Button.transform.localPosition = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y + 0.001f, Button.transform.localPosition.z);
            yield return new WaitForSeconds(0.02f);
        }
    }

    private IEnumerator Flicker() {

        while (true)
        {
            Timer.color = new Color(1f, 0f, 0f, Rnd.Range(0f, 1f));
            yield return new WaitForSeconds(0.05f);
        }
    }

    void ButtonPress() {

        if (!Solved)
        {
            if (!TimerOn)
            {
                Begin = true;
                Debug.LogFormat("[Tell Me When #{0}] You took a shot at solving the module.", _moduleID);
                TimerOn = true;
            }
            else
            {
                CheckAnswer();
                TimerOn = false;
            }
        }
    }

    void CheckAnswer()
    {
        Begin = false;
        time = 1;
        if (Timer.text == TimeIThink.ToString("00"))
        {
            Module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, Button.transform);
            Debug.LogFormat("[Tell Me When #{0}] {1} was indeed the correct answer. Poggers!", _moduleID, Timer.text);
            Solved = true;
            Timer.text = "GG";
        }
        else
        {
            Debug.LogFormat("[Tell Me When #{0}] {1} was not correct. Strike!", _moduleID, Timer.text);
            Audio.PlaySoundAtTransform("buzzer", Button.transform);
            Module.HandleStrike();
            FindTime();
            Timer.text = "--";
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} 07' to press the button, then press it again when the timer displays 07. Note that single digit numbers need to be preceded with a 0.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string validcmds = "0123456789";
        string cmdsbelowthirty = "3456789";

        if (command.Length != 2)
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
        for (int i = 0; i < 2; i++)
        {
            if (!validcmds.Contains(command[i]))
            {
                yield return "sendtochaterror Invalid command.";
                yield break;
            }
        }
        if ((cmdsbelowthirty.Contains(command[0]) && command != "30") || command == "00")
        {
            yield return "sendtochaterror Invalid command. Make sure your command is between 01 and 30.";
            yield break;
        }
        yield return null;
        Button.OnInteract();

        while (command != Timer.text)
        {
            yield return "trycancel Oh no, the second button press for Tell Me When has been cancelled.";
        }

        yield return null;
        Button.OnInteract();
    }
    IEnumerator TwitchHandleForcedSolve() {
        yield return null;
        Button.OnInteract();

        while (TimeIThink.ToString("00") != Timer.text)
        {
            yield return null;
        }

        yield return null;
        Button.OnInteract();
    }
    // It's a thicc button, I know
}