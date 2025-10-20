using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;

public class veryAnnoyingScript : MonoBehaviour {

    public KMSelectable[] buttons = new KMSelectable[16];
    public TextMesh[] texts = new TextMesh[16];
    public MeshRenderer[] buttonMesh = new MeshRenderer[16];
    public GameObject[] buttonObjects = new GameObject[16];
    public TextMesh color;
    public KMBombModule Module;
    public MeshRenderer moduleMesh;
    public KMAudio Audio;
    public AudioClip[] clips = new AudioClip[3];

    private int[] points = new int[16];
    private int correctPressed = 0;
    private bool secondStage = false;
    private bool solved = false;

    private int[] red = new int[16];
    private int[] green = new int[16];
    private int[] blue = new int[16];
    private string[] colors = new string[16];

    static int ModuleIdCounter = 0;
    int ModuleId;

    int max(int a, int b) { return a > b ? a : b; }
    int min(int a, int b) { return a < b ? a : b; }

    int[] generateFirstStage()
    {
        int[] numbers = new int[16];
        int[] ans = new int[16];
        string log = "Button points (stage 1):\n";
        for (int i=0; i<16; i++)
        {
            numbers[i] = Random.Range(0, 65536);
            ans[i] = 0;
            int mask = 1;
            for (int j=0; j < 16; j++)
            {
                if ((numbers[i] & mask) > 0) ans[i]++;
                mask <<= 1;
            }
            texts[i].text = numbers[i].ToString();
            log += ans[i].ToString() + " ";
            if (i % 4 == 3) log += "\n";
        }
        Debug.Log(log);
        return ans;
    }

    bool checkMin(int[] array, int index)
    {
        for (int i=0; i<array.Length; i++){if (array[i] < array[index]) return false;}
        return true;
    }
    int mod(int a, int b)
    {
        while (a < 0) a += b;
        while (a >= b) a -= b;
        return a;
    }

    int rgbToHsvSum(int r, int g, int b)
    {
        int maxVal = max(max(r, g), b);
        int minVal = min(min(r, g), b);
        int delta = maxVal - minVal;

        int h = 0;
        int s = 0;
        if (delta != 0)
        {
            if (maxVal == r) h = 60 * (g - b) / delta;
            else if (maxVal == g) h = 60 * (b - r) / delta +120;
            else h = 60 * (r - g) / delta +240;
            h = mod(h, 360);
        }
        if (maxVal != 0) s = 255 * delta / maxVal;
        return h + s + maxVal;
    }
    
    string dectohex(int num)
    {
        string hex = "0123456789ABCDEF";
        return hex[num / 16].ToString() + hex[num % 16].ToString();
    }

    void nextStage()
    {
        if (secondStage)
        {
            Module.HandlePass();
            string final = "YOUDIDITGOODJOB!";
            for (int i=0; i<16; i++) {
                texts[i].text = final[i].ToString();
                buttonMesh[i].material.color = new Color(0, 178 / 255f, 0);
                buttonObjects[i].SetActive(true);
            }
            moduleMesh.material.color = new Color(94/255f, 1, 94 / 255f);
            secondStage = false;
            solved = true;
        }
        else
        {
            secondStage = true;
            correctPressed = 0;
            points = generateSecondStage();
        }
    }

    int[] generateSecondStage()
    {
        int[] ans = new int[16];
        string log = "Button points (stage 2):\n";
        for (int i=0; i<16; i++)
        {
            red[i] = Random.Range(0, 256);
            green[i] = Random.Range(0, 256);
            blue[i] = Random.Range(0, 256);
            texts[i].text = "";
            buttonMesh[i].material.color = new Color(red[i] / 255f, green[i] / 255f, blue[i] / 255f);
            colors[i] = "#"+ dectohex(red[i]) + dectohex(green[i]) + dectohex(blue[i]);
            ans[i] = rgbToHsvSum(255-red[i], 255 - green[i], 255 - blue[i]);
            log += ans[i].ToString() + " ";
            if (i % 4 == 3) log += "\n";
        }
        Debug.Log(log);
        return ans;
    }

    bool Press(int index)
    {
        if (!solved)
        {
            if (points[index] != 999)
            {
                if (checkMin(points, index))
                {
                    correctPressed++;
                    texts[index].text = "";
                    points[index] = 999;
                    if (secondStage) { buttonObjects[index].SetActive(false); }


                    if (correctPressed == 16)
                    {
                        nextStage();
                        Audio.PlaySoundAtTransform(clips[1].name, transform);
                    }
                    else Audio.PlaySoundAtTransform(clips[0].name, transform);
                }
                else
                {
                    Module.HandleStrike();
                    correctPressed = 0;
                    for (int i=0; i<16; i++) buttonObjects[i].SetActive(true);
                    points = secondStage ? generateSecondStage() : generateFirstStage();
                    Audio.PlaySoundAtTransform(clips[2].name, transform);
                }
            }
        }
        return true;
    }



	void Start () {

        ModuleId = ModuleIdCounter++;
        ModuleId++;
        color.text = "";
        points = generateFirstStage();

        for (int i = 0; i < 16; i++) buttonMesh[i].material.color = new Color(104 / 255f, 46 / 255f, 46 / 255f);
        moduleMesh.material.color = new Color(178 / 255f, 65 / 255f, 65 / 255f);

        buttons[0].OnInteract += delegate () { return Press(0); };
        buttons[1].OnInteract += delegate () { return Press(1); };
        buttons[2].OnInteract += delegate () { return Press(2); };
        buttons[3].OnInteract += delegate () { return Press(3); };
        buttons[4].OnInteract += delegate () { return Press(4); };
        buttons[5].OnInteract += delegate () { return Press(5); };
        buttons[6].OnInteract += delegate () { return Press(6); };
        buttons[7].OnInteract += delegate () { return Press(7); };
        buttons[8].OnInteract += delegate () { return Press(8); };
        buttons[9].OnInteract += delegate () { return Press(9); };
        buttons[10].OnInteract += delegate () { return Press(10); };
        buttons[11].OnInteract += delegate () { return Press(11); };
        buttons[12].OnInteract += delegate () { return Press(12); };
        buttons[13].OnInteract += delegate () { return Press(13); };
        buttons[14].OnInteract += delegate () { return Press(14); };
        buttons[15].OnInteract += delegate () { return Press(15); };

        buttons[ 0].OnHighlight+=delegate(){color.text = (secondStage && points[ 0] != 999) ? colors[ 0] : ""; };
        buttons[ 1].OnHighlight+=delegate(){color.text = (secondStage && points[ 1] != 999) ? colors[ 1] : ""; };
        buttons[ 2].OnHighlight+=delegate(){color.text = (secondStage && points[ 2] != 999) ? colors[ 2] : ""; };
        buttons[ 3].OnHighlight+=delegate(){color.text = (secondStage && points[ 3] != 999) ? colors[ 3] : ""; };
        buttons[ 4].OnHighlight+=delegate(){color.text = (secondStage && points[ 4] != 999) ? colors[ 4] : ""; };
        buttons[ 5].OnHighlight+=delegate(){color.text = (secondStage && points[ 5] != 999) ? colors[ 5] : ""; };
        buttons[ 6].OnHighlight+=delegate(){color.text = (secondStage && points[ 6] != 999) ? colors[ 6] : ""; };
        buttons[ 7].OnHighlight+=delegate(){color.text = (secondStage && points[ 7] != 999) ? colors[ 7] : ""; };
        buttons[ 8].OnHighlight+=delegate(){color.text = (secondStage && points[ 8] != 999) ? colors[ 8] : ""; };
        buttons[ 9].OnHighlight+=delegate(){color.text = (secondStage && points[ 9] != 999) ? colors[ 9] : ""; };
        buttons[10].OnHighlight+=delegate(){color.text = (secondStage && points[10] != 999) ? colors[10] : ""; };
        buttons[11].OnHighlight+=delegate(){color.text = (secondStage && points[11] != 999) ? colors[11] : ""; };
        buttons[12].OnHighlight+=delegate(){color.text = (secondStage && points[12] != 999) ? colors[12] : ""; };
        buttons[13].OnHighlight+=delegate(){color.text = (secondStage && points[13] != 999) ? colors[13] : ""; };
        buttons[14].OnHighlight+=delegate(){color.text = (secondStage && points[14] != 999) ? colors[14] : ""; };
        buttons[15].OnHighlight+=delegate(){color.text = (secondStage && points[15] != 999) ? colors[15] : ""; };

        buttons[ 0].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 1].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 2].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 3].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 4].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 5].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 6].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 7].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 8].OnHighlightEnded += delegate () { color.text = "";};
        buttons[ 9].OnHighlightEnded += delegate () { color.text = "";};
        buttons[10].OnHighlightEnded += delegate () { color.text = "";};
        buttons[11].OnHighlightEnded += delegate () { color.text = "";};
        buttons[12].OnHighlightEnded += delegate () { color.text = "";};
        buttons[13].OnHighlightEnded += delegate () { color.text = "";};
        buttons[14].OnHighlightEnded += delegate () { color.text = "";};
        buttons[15].OnHighlightEnded += delegate () { color.text = "";};
        
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage =
        @"Use !{0} press # to press button on this position. Buttons are numbered 0-15 in reading order.";
    private bool TwitchPlaysActive = false;
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        Command = Command.ToLower();
        if (!Command.RegexMatch("press ([0-9])+")) yield return "sendtochaterror Invalid command!";
        else
        {
            int? num = Command.Substring(6).TryParseInt();
            if (num == null || num < 0 || num > 15) yield return "sendtochaterror Invalid command!";
            else Press((int)num);
        }
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        Module.HandlePass();
        string final = "YOUDIDITGOODJOB!";
        for (int i = 0; i < 16; i++)
        {
            texts[i].text = final[i].ToString();
            buttonMesh[i].material.color = new Color(0, 178 / 255f, 0);
            buttonObjects[i].SetActive(true);
        }
        moduleMesh.material.color = new Color(94 / 255f, 1, 94 / 255f);
        secondStage = false;
        solved = true;
    }
}
