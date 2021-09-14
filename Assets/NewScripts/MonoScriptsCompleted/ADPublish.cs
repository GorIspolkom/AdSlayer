using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class ADPublish : AdIniter
    { 
        //////test module/////////////////
        IEnumerator AutoClick()
        {
            while (true)
            {
                if (Values.data.isAutoClick)
                { 
                    yield return new WaitForSeconds(1f / Values.data.deltaClick);
                    ClickAction();
                }
                else
                    yield return null;
            }
        }
        /////////////////////////////////
        int randomID;

        public AudioSource click_sound;
        public Animation anima;
        public Image spriter;
        public Text texter;
        public Image TextTable;
        void Start()
        {
            StartCoroutine(AutoClick());
            randomID = Random.Range(0, JsonIniter.GetSize());
            TestPublish(randomID);
            click_sound.volume = Values.settings.sound;
        }
        public void PublishNewAd()
        {
            int rr = Random.Range(0, JsonIniter.GetSize());
            while (rr == randomID)
                rr = Random.Range(0, JsonIniter.GetSize());
            randomID = rr;
            TestPublish(randomID);
        }
        public void rePublish() => Publish(randomID);
        public void ClickAction()
        {
            PublishNewAd();
            anima.Play();
            click_sound.PlayOneShot(click_sound.clip);
            ValuesNotifyHandle.putNotify(new ClickAction()); 
        }

        public void TestPublish(int rr)
        {
            spriter.sprite = JsonIniter.GetSprite(rr);
            if (JsonIniter.isText)
            {
                string text = JsonIniter.GetText(rr);
                texter.text = text;
                if (text.Equals(""))
                    TextTable.color = new Color(0, 0, 0, 0);
                else
                    TextTable.color = new Color(0, 0, 0, 0.6f);
            }
        } 
    }
}