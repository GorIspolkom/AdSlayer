using Clicker.Models;
using MyUtile.JsonWorker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScoreShower : MonoBehaviour
{ 
    [SerializeField] Transform lArrow;
    [SerializeField] Transform rArrow;
    [SerializeField] Text scoreBust;
    [SerializeField] Text secondBust;
    [SerializeField] Text score;
    [SerializeField] Text perSecond;

    string bustRich;
    string valute;

    void Start()
    { 
        bustRich = $"<color=#%c>x%s</color>";
        valute = JsonParser.getLocaliz("ScoreBored");
        ReCalcAngels();
    }
    public void ReCalcAngels()
    {
        int buff = Values.profile.GetClickBuff();
        lArrow.eulerAngles = new Vector3(0, 0, -180 * (buff-1) / 14f);

        Color color = Color.Lerp(new Color(255, 209, 129, 1), new Color(255, 141, 216, 1), buff / 15f);
        string hex = $"{((int)color.r).ToString("x")}{((int)color.g).ToString("x")}{((int)color.b).ToString("x")}FF";
        scoreBust.text = bustRich.Replace("%c", hex).Replace("%s", buff.ToString()); 

        buff = Values.profile.GetTimerBuff();
        rArrow.eulerAngles = new Vector3(0, 0, -180 * (buff - 1) / 14f);

        color = Color.Lerp(new Color(255, 209, 129, 1), new Color(255, 141, 216, 1), buff / 15f);
        hex = $"{((int)color.r).ToString("x")}{((int)color.g).ToString("x")}{((int)color.b).ToString("x")}FF";
        secondBust.text = bustRich.Replace("%c", hex).Replace("%s", buff.ToString());
    }
    void FixedUpdate()
    {
        score.text = valute.Replace("%s", Values.profile.Score.ToString());
        perSecond.text = "+" + Values.profile.GetScorePerSecondInBuff().ToString() + " ADBan/s";

        if(Values.data.clickPerSecond != 0)
        {
            int buff = Values.profile.GetClickBuff();
            lArrow.eulerAngles = new Vector3(0, 0, -180 * (buff - 1) / 14f);

            Color color = Color.Lerp(new Color(255, 209, 129, 1), new Color(255, 141, 216, 1), buff / 15f);
            string hex = $"{((int)color.r).ToString("x")}{((int)color.g).ToString("x")}{((int)color.b).ToString("x")}FF";
            scoreBust.text = bustRich.Replace("%c", hex).Replace("%s", buff.ToString());
        }

    }
}
