using Clicker.Advertisment;
using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class BuffPanelInit : MonoBehaviour
    {
        public Text buffAdd;
        public Text unBuffAdd;
        private Advert ad;

        private void Start()
        {
            ad = GameObject.FindGameObjectWithTag("Controller").GetComponent<Advert>();
        }

        public int AssignBaff()
        {
            int x = 2;
            if (Random.value >= 0.95)
                x = 4;
            else if (Random.value >= 0.7)
                x = 3;
            return x;
        }
        public void InitBuffScore(XXLNum Score, int buff, float ver)
        {
            if (buff == 0)
                buff = AssignBaff();
            GetComponentInChildren<Text>().text = JsonParser.getLocaliz("BuffPanel").Replace("%s", buff.ToString());
            unBuffAdd.text = "+" + Score.ToString();
            buffAdd.text = "+" + (Score * buff).ToString() + "!";
            transform.Find("Answers").Find("Yes").GetComponent<Button>().onClick.AddListener(() =>
            {
                ad.ShowAdWithReward(Score, Score * buff, "rewardedVideo");
                Destroy(gameObject);
            });
            transform.Find("Answers").Find("No").GetComponent<Button>().onClick.AddListener(() =>
            {
                if (Random.value < ver)
                    ad.ShowAdWithReward(Score, Score, "video");
                else
                    ValuesNotifyHandle.putNotify(new TakeScore(Score));
                LeanTween.moveY(transform as RectTransform, -700, 0.3f).setEaseInBack().setOnComplete(() => Destroy(gameObject));
            });
        }
        public void InitBuff(BuffType type, int buff, long time)
        {
            if (buff == 0)
                buff = AssignBaff();
            GetComponentInChildren<Text>().text = JsonParser.getLocaliz("GetBuffPanel").Replace("%s", (time / 60).ToString()).Replace("%b", buff.ToString());
            unBuffAdd.text = "";
            buffAdd.text = "";
            transform.Find("Answers").Find("Yes").GetComponent<Button>().onClick.AddListener(() =>
            {
                ad.ShowAdWithBuff(type, buff, time, "rewardedVideo");
                GameNotifyHandler.putNotify(new BrokeBanner());
                Destroy(gameObject);
            });
            transform.Find("Answers").Find("No").GetComponent<Button>().onClick.AddListener(() =>
            { 
                LeanTween.moveY(transform as RectTransform, -700, 0.3f).setEaseInBack().setOnComplete(() => Destroy(gameObject));
            });
        }
        public void InitContractOffer(int contracts)
        {
            GetComponentInChildren<Text>().text = JsonParser.getLocaliz("ContractsOffer").Replace("%s", contracts.ToString());
            unBuffAdd.text = "";
            buffAdd.text = "";
            transform.Find("Answers").Find("Yes").GetComponent<Button>().onClick.AddListener(() =>
            {
                ad.ShowAdWithContracts(contracts, "rewardedVideo");
                GameNotifyHandler.putNotify(new BrokeBanner());
                Destroy(gameObject);
            });
            transform.Find("Answers").Find("No").GetComponent<Button>().onClick.AddListener(() =>
            {
                LeanTween.moveY(transform as RectTransform, -700, 0.3f).setEaseInBack().setOnComplete(() => Destroy(gameObject));
            });
        }
    }
}