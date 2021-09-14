using Clicker.DetachedScrypts;
using Clicker.Models;
using MyUtile.JsonWorker;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class PerShow : Localizate
    {
        public Text bust;
        public Text score;

        private string bustRich;
        private int buff;

        private void Start()
        {
            buff = -1;
            bustRich = $"<color=#%c>x%s</color>";
        }
        //обновляет значения 
        private void Update()
        {
            score.text = "+" + Values.profile.GetScorePerSecondInBuff().ToString() + " ADBan/s";
            if (buff != Values.profile.GetTimerBuff())
            {
                buff = Values.profile.GetTimerBuff();

                Color color = Color.Lerp(new Color(255, 209, 129, 1), new Color(255, 141, 216, 1), buff / 15f);
                string hex = $"{((int)color.r).ToString("x")}{((int)color.g).ToString("x")}{((int)color.b).ToString("x")}FF";
                bust.text = bustRich.Replace("%c", hex).Replace("%s", buff.ToString());
            }
        }
        //вызывается при переходе на другой язык
        public override void Translate()
        {

        }
    }
}
