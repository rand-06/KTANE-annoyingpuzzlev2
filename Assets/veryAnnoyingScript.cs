using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class veryAnnoyingScript : MonoBehaviour {

    public KMSelectable[] buttons;
    public TextMesh[] texts;
    public MeshRenderer[] buttonMesh;
    public GameObject[] buttonObjects;
    public TextMesh color;
    public KMBombModule Module;
    public MeshRenderer moduleMesh;
    public KMAudio Audio;
    public AudioClip[] clips = new AudioClip[3];

    private int[] points = new int[9];
    private int correctPressed ;
    private bool secondStage;
    private bool solved;

    private int[] red = new int[9];
    private int[] green = new int[9];
    private int[] blue = new int[9];
    private string[] colors = new string[9];

    static int ModuleIdCounter;
    int ModuleId;

    int[] generateFirstStage()
    {
        int[] numbers = Enumerable.Range(0, 9).Select(_ => Random.Range(0, 65536)).ToArray();
        for (int i = 0; i < 9; i++) texts[i].text = numbers[i].ToString();
        int[] ans = Enumerable.Range(0, 9).Select(i => 
            Enumerable.Range(0, 16).Select(x => (numbers[i] & 1 << x) > 0 ? 1 : 0).Sum()
            ).ToArray();
        Debug.Log("[Very Annoying Puzzle #" + ModuleId + "] Button points (stage 1): "
                  + ans.Aggregate("", (a, b) => a + ", " + b));
        return ans;
    }
    int rgbToHsvSum(int r, int g, int b)
    {
        int maxVal = new[] { r, g, b }.Max();
        int minVal = new[] { r, g, b }.Min();
        int delta = maxVal - minVal;
        
        int h = 0;
        int s = 0;
        if (delta != 0)
        {
            if (maxVal == r) h = 60 * (g - b) / delta;
            else if (maxVal == g) h = 60 * (b - r) / delta +120;
            else h = 60 * (r - g) / delta +240;
            h = (h % 360 + 360) % 360;
        }
        if (maxVal != 0) s = 255 * delta / maxVal;
        return h + s + maxVal;
    }

    void nextStage()
    {
        if (secondStage)
        {
            Module.HandlePass();
            string final = "VAPSOLVED";
            for (int i=0; i<9; i++) {
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
        int[] temp = Enumerable.Range(0, 9).Select(_ => Random.Range(0, 256*256*256)).ToArray();
        red = temp.Select(x => x/256/256).ToArray();
        green = temp.Select(x => x/256%256).ToArray();
        blue = temp.Select(x => x%256).ToArray();
        colors = temp.Select(x => "#"+x.ToString("X")).ToArray();
        int[] ans = Enumerable.Range(0, 9).Select(i => rgbToHsvSum(255-red[i], 255 - green[i], 255 - blue[i])).ToArray();
        Debug.Log("[Very Annoying Puzzle #" + ModuleId + "] Button points (stage 2): " + ans.Select(x=>x.ToString()).Aggregate((a, b) => a + ", " + b));
        for (int i=0; i<9; i++)
        {
            texts[i].text = "";
            buttonMesh[i].material.color = new Color(red[i] / 255f, green[i] / 255f, blue[i] / 255f);
        }
        return ans;
    }

    void Press(int index)
    {
        if (!solved)
        {
            if (points[index] != 999)
            {
                if (!Enumerable.Range(0, points.Length).Any(x=>points[x] < points[index]))
                {
                    correctPressed++;
                    texts[index].text = "";
                    points[index] = 999;
                    if (secondStage) { buttonObjects[index].SetActive(false); }


                    if (correctPressed == 9)
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
                    for (int i=0; i<9; i++) buttonObjects[i].SetActive(true);
                    points = secondStage ? generateSecondStage() : generateFirstStage();
                    Audio.PlaySoundAtTransform(clips[2].name, transform);
                }
            }
        }
    }

	void Start () {

        ModuleId = ModuleIdCounter++;
        ModuleId++;
        color.text = "";
        points = generateFirstStage();
        for (int i = 0; i < 9; i++) buttonMesh[i].material.color = new Color(104 / 255f, 46 / 255f, 46 / 255f);
        moduleMesh.material.color = new Color(178 / 255f, 65 / 255f, 65 / 255f);
        for (int i1 = 0; i1 < 9; i1++)
        {
            int i = i1;
            buttons[i].OnInteract += delegate{Press(i); return false;};
            buttons[i].OnHighlight+=delegate{color.text = secondStage && points[i] != 999 ? colors[i] : ""; };
            buttons[i].OnHighlightEnded += delegate{ color.text = "";};
        }
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage =
        "Use !{0} press # to press button on this position. Buttons are numbered 0-8 in reading order. Use !{0} hover # to hover the button.";
    private bool TwitchPlaysActive = false;
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
        Command = Command.ToLower();
        if (Command.RegexMatch("press ([0-9])+"))
        {
            color.text = "";
            int? num = Command.Substring(6).TryParseInt();
            if (num == null || num < 0 || num > 8) yield return "sendtochaterror Invalid command!";
            else Press((int)num);
        }
        else if (Command.RegexMatch("hover ([0-9])+"))
        {
            int? num = Command.Substring(6).TryParseInt();
            if (num == null || num < 0 || num > 8) yield return "sendtochaterror Invalid command!";
            else buttons[(int)num].OnHighlight();
        }
        else yield return "sendtochaterror Invalid command!";
        
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        Module.HandlePass();
        string final = "VAPSOLVED";
        for (int i = 0; i < 9; i++)
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
