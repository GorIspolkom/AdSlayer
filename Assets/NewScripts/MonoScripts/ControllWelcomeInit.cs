using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    /// <summary>
    /// скрипт, который грузит сейв и выводит все изменения за время отсутствие
    /// выводит лого и вызывает предложение увеличить полученный 
    /// </summary>
    public class ControllWelcomeInit : MonoBehaviour
    {
        //показывает время отсутствия
        public GameObject LastVisit;
        //показывает сколько получено за это время
        public GameObject MuchAdd;
        //кнопка перехода в игру
        public Button GoIn;
        //анимированное лого
        public Animation logo;
        //Максимальное время, которое учитывается при добавлении скора
        //если вермя отсутствия больше, то берется это
        public long MaxMinutes;
        //переменная запуска игровой сцены, меняется при нажатии GoIn
        //если нажата кнопка, то грузится сцена игры и предложение увеличить скор
        private bool isReady;

        public void init(long UnGameTime)
        {
            GameNotifyHandler.putNotify(new LoadScene(1));
            //подсчет время отсутствия 
            if (!Values.profile.ScorePerSecond.isZero())
            {
                string format;
                long seconds = UnGameTime <= MaxMinutes * 60 ? UnGameTime : MaxMinutes * 60;
                if (UnGameTime / (24 * 3600) >= 1)
                    format = @"d'd : 'h'h'";
                else if (UnGameTime / 3600 >= 1)
                    format = @"h'h : 'm'm'";
                else
                    format = @"m'm : 's's'";

                //подсчет заработанного скора
                XXLNum AddWhileLeave = Values.profile.ScorePerSecond * seconds / UnityEngine.Random.Range(0.95f, 1.1f);
                //инит всех панелей с информацией 
                LastVisit.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("LastVisit") + TimeSpan.FromSeconds(UnGameTime).ToString(format);
                MuchAdd.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("MuchAdd") + AddWhileLeave.ToString();
                GoIn.onClick.AddListener(() =>
                {
                    GameNotifyHandler.putNotify(new ActivateScene(), true);
                    GameNotifyHandler.putNotify(new ScoreBuffPanel(AddWhileLeave), true);
                });
                //запуск корутины анимаций
                StartCoroutine(PlayAnim());
            }
            else
                StartCoroutine(StayLogo());
        }
        IEnumerator StayLogo()
        {
            yield return new WaitForSeconds(2f);
            GameNotifyHandler.putNotify(new ActivateScene(), true);
        }
        //вывод лого и панелей затем
        IEnumerator PlayAnim()
        {
            yield return new WaitForSeconds(2f);
            logo.Play();
            yield return null;
            while (logo.isPlaying) yield return null;
            LastVisit.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(0.15f);
            MuchAdd.GetComponent<Animation>().Play();
        }
    }
}