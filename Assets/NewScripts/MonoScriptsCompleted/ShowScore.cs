using Clicker.DetachedScrypts;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class ShowScore : Localizate
    {
        public string valute;
        private string bustRich;
        public Text score;
        public Text bust;
        
        private int buff;

        private void Start()
        {
            valute = JsonParser.getLocaliz(name);
            bustRich = $"<color=#%c>x%s</color>";
            buff = -1;
        }

        private void Update()
        {
            score.text = valute.Replace("%s", Values.profile.Score.ToString());
            if (buff != Values.profile.GetClickBuff())
            {
                buff = Values.profile.GetClickBuff();

                Color color = Color.Lerp(new Color(255, 209, 129, 1), new Color(255, 141, 216, 1), buff / 15f);
                string hex = $"{((int)color.r).ToString("x")}{((int)color.g).ToString("x")}{((int)color.b).ToString("x")}FF";
                bust.text = bustRich.Replace("%c", hex).Replace("%s", buff.ToString());
            }
        }
        public override void Translate()
        {
            valute = JsonParser.getLocaliz(name);
        }
    }
}