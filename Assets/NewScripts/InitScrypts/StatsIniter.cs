using Clicker.DetachedScrypts;
using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.NewScripts.InitScrypts
{
    class StatsIniter : MonoBehaviour
    {
        Text EverScore;
        Text Time;
        void Start()
        {
            transform.Find("Music").Find("Volume").GetComponent<Slider>().value = Values.settings.volume;
            transform.Find("Music").Find("Sound").GetComponent<Slider>().value = Values.settings.sound;
            Dropdown droper = transform.GetComponentInChildren<Dropdown>();
            if (Values.settings.lang.Equals("Rus"))
                droper.value = 0;
            else if (Values.settings.lang.Equals("Eng"))
                droper.value = 1;
             
            Transform stats = transform.Find("Stats");
            EverScore = stats.Find("EverScore").GetComponent<Text>();
            Time = stats.Find("Time").GetComponent<Text>();
            stats.Find("Clicks").GetComponent<Text>().text = JsonParser.getLocaliz("Clicks").Replace("%s", Values.profile.Click.ToString());

            RectTransform rect = transform as RectTransform;
            rect.anchoredPosition = new Vector2(0, rect.rect.height); 
            LeanTween.moveY(rect, 0, 0.3f).setEaseInOutBack();
        }

        private void Update()
        {
            EverScore.text = JsonParser.getLocaliz("EverScore").Replace("%s", Values.profile.EverScore.ToString());
            Time.text = JsonParser.getLocaliz("TimeUngame").Replace("%s", TimeSpan.FromSeconds(Values.profile.time).ToString(@"hh\:mm\:ss"));
        }

        public void Localize(Dropdown droper)
        {
            switch (droper.value)
            {
                case 0:
                    if (Values.settings.lang.Equals("Rus"))
                        return;
                    Values.settings.lang = "Rus";
                    break;
                case 1:
                    if (Values.settings.lang.Equals("Eng"))
                        return;
                    Values.settings.lang = "Eng";
                    break;
            }
            transform.Find("Stats").Find("Clicks").GetComponent<Text>().text = 
                    JsonParser.getLocaliz("Clicks").Replace("%s", Values.profile.Click.ToString());

            GameNotifyHandler.putNotify(new Localize());
        }
        public void SetVolume(Slider slider) => GameNotifyHandler.putNotify(new VolumeSet(slider.value)); 
        public void SetSound(Slider slider) => GameNotifyHandler.putNotify(new SoundSet(slider.value)); 
    }
}
