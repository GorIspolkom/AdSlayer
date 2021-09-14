using Clicker.GameSystem;
using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    class BannerController : MonoBehaviour
    {
        [System.Serializable]
        public struct BannerStruct {
            //Texts: NewsText - RunningString, FullText - Archivments, LoreText - TalkLore
            public Text NewsText;
            public Text FullText;
            public Text LoreText;
            //effects of salute
            public ParticleSystem hlopyshkaL, hlopyshkaR;
            //circle as clickbyte. Spawn when banner clickable
            public GameObject circle;
            //Time for waiting
            public float TimeForNewNews;
            public float TimeForArchivHang;
            //velocity of running string
            public float velocity; 
            //crite Pos for running string
            [System.NonSerialized]
            public float critePos;
            //all new's texts
            [System.NonSerialized]
            public string[] News;
            //ачивки 

            public void init(RectTransform rect)
            {
                velocity *= -1;
                critePos = -rect.rect.width;
                News = JsonParser.getNews();
            }
            public void SetDefault()
            {
                NewsText.text = "";
                FullText.text = "";
                LoreText.text = ""; 
                hlopyshkaL.Stop();
                hlopyshkaR.Stop();
            }
            public void Localize()
            {
                News = JsonParser.getNews();
                SetDefault();
            }
            public string getRandNews() => News[Random.Range(0, News.Length-2)];
        }
        protected abstract class BannerState
        {
            protected BannerStruct bannerVal;

            public BannerState(BannerStruct bannerVal)
            {
                this.bannerVal = bannerVal;
            }
            public void reinit(BannerStruct bannerVal) => this.bannerVal = bannerVal;
            public abstract void init(Transform transform);
            public abstract IEnumerator update();
            public abstract void finish();
            public abstract void broke();
        }
        protected class RunStringState : BannerState
        {
            int k;
            public RunStringState(BannerStruct val) : base(val)
            {
                k = 1;
            }
            public override void init(Transform transform)
            {
                bannerVal.NewsText.text = bannerVal.getRandNews();
                float currentTextWidth = LayoutUtility.GetPreferredWidth(bannerVal.NewsText.rectTransform);
                bannerVal.NewsText.rectTransform.localPosition = new Vector3(currentTextWidth, -bannerVal.NewsText.rectTransform.sizeDelta.y / 2f, 0);
            }
            public override IEnumerator update()
            {
                while (bannerVal.critePos <= bannerVal.NewsText.rectTransform.localPosition.x)
                {
                    bannerVal.NewsText.rectTransform.Translate(bannerVal.velocity * Time.deltaTime * k, 0, 0);
                    yield return null;
                }
            }
            public void goFast() => k = 3;
            public override void finish()
            {
                bannerVal.NewsText.text = "";
            }

            public override void broke()
            {
                bannerVal.NewsText.text = "";
            }
        }
        protected class BuffOfferState : BannerState
        {
            GameObject circle;

            public BuffOfferState(BannerStruct val) : base(val) { }
            public override void init(Transform transform)
            {
                int buff = 2;
                long time = 60;
                circle = Instantiate(bannerVal.circle, bannerVal.NewsText.transform.parent);
                bannerVal.NewsText.text = bannerVal.News[bannerVal.News.Length - 1].Replace("%b", buff.ToString()).Replace("%s", (time/60).ToString());
                bannerVal.hlopyshkaL.Play();
                float currentTextWidth = LayoutUtility.GetPreferredWidth(bannerVal.NewsText.rectTransform);
                bannerVal.NewsText.rectTransform.localPosition = new Vector3(currentTextWidth, -bannerVal.NewsText.rectTransform.sizeDelta.y / 2f, 0);
                    circle.GetComponentInChildren<Button>().onClick.AddListener(() =>
                        GameNotifyHandler.putNotify(new TakeBuffPanel(BuffType.perClick, time, buff)));
            }
            public override IEnumerator update()
            {
                circle.GetComponentInChildren<Animator>().SetBool("isEnd", false);
                yield return new WaitForSecondsRealtime(1);
                while (bannerVal.critePos <= bannerVal.NewsText.rectTransform.localPosition.x)
                {
                    bannerVal.NewsText.rectTransform.Translate(bannerVal.velocity * Time.deltaTime, 0, 0);
                    yield return null;
                }
                circle.GetComponentInChildren<Animator>().SetBool("isEnd", true);
                yield return new WaitForSecondsRealtime(1);
            }
            public override void finish()
            {
                bannerVal.NewsText.text = "";
                Destroy(circle.gameObject);
                bannerVal.hlopyshkaL.Stop();
            }

            public override void broke()
            {
                bannerVal.NewsText.text = "";
                bannerVal.hlopyshkaL.Stop();
                circle.GetComponentInChildren<Animator>().SetBool("isEnd", true);
                Destroy(circle.gameObject, 1); 
            }
        }
        protected class TakeContractsOfferState : BannerState
        {
            GameObject circle;

            public TakeContractsOfferState(BannerStruct val) : base(val) { }
            public override void init(Transform transform)
            { 
                int contracts = Random.Range(2, 6);
                circle = Instantiate(bannerVal.circle, bannerVal.NewsText.transform.parent);
                bannerVal.NewsText.text = bannerVal.News[bannerVal.News.Length - 2].Replace("%s", contracts.ToString());
                bannerVal.hlopyshkaL.Play();
                float currentTextWidth = LayoutUtility.GetPreferredWidth(bannerVal.NewsText.rectTransform);
                bannerVal.NewsText.rectTransform.localPosition = new Vector3(currentTextWidth, -bannerVal.NewsText.rectTransform.sizeDelta.y / 2f, 0);
                if (Random.value < 0.3f)
                    circle.GetComponentInChildren<Button>().onClick.AddListener(() =>
                    {
                        GameNotifyHandler.putNotify(new BrokeBanner());
                        ValuesNotifyHandle.putNotify(new TakeCoins(contracts));
                    });
                else
                    circle.GetComponentInChildren<Button>().onClick.AddListener(() =>
                        GameNotifyHandler.putNotify(new TakeContractsPanel(contracts)));
            }
            public override IEnumerator update()
            {
                circle.GetComponentInChildren<Animator>().SetBool("isEnd", false);
                yield return new WaitForSecondsRealtime(0.5f);
                while (bannerVal.critePos <= bannerVal.NewsText.rectTransform.localPosition.x)
                {
                    bannerVal.NewsText.rectTransform.Translate(bannerVal.velocity * Time.deltaTime, 0, 0);
                    yield return null;
                }
                circle.GetComponentInChildren<Animator>().SetBool("isEnd", true);
                bannerVal.hlopyshkaL.Stop();
                yield return new WaitForSecondsRealtime(0.5f);
            }
            public override void finish()
            {
                bannerVal.NewsText.text = "";
                Destroy(circle);
                bannerVal.hlopyshkaL.Stop();
            }

            public override void broke()
            {
                bannerVal.NewsText.text = "";
                bannerVal.hlopyshkaL.Stop();
                circle.GetComponentInChildren<Animator>().SetBool("isEnd", true);
                Destroy(circle.gameObject, 1);
            }
        }
        protected class LoreTalkState : BannerState
        {
            int lvl;
            string lore;
            bool isNew;
            Animator anim;
            Toggle toggle;

            public LoreTalkState(BannerStruct val, int lvl, bool isNew) : base(val)
            {
                this.lvl = lvl;
                this.isNew = isNew;
            }

            public override void init(Transform transform)
            {
                anim = transform.GetComponent<Animator>();
                toggle = transform.Find("Next").GetComponent<Toggle>();
                lore = JsonParser.getLore(lvl);
                bannerVal.LoreText.text = "";
                anim.SetBool("isLore", true);
                anim.SetBool("isArchiv", false);
                toggle.isOn = false;
            }
            public override IEnumerator update()
            {
                int size = GameObject.Find("MainCanvas").transform.childCount;
                GameObject.Find("Settings").transform.SetSiblingIndex(size);
                yield return new WaitUntil(() => bannerVal.LoreText.IsActive());
                if (isNew)
                {
                    lvl = lvl < JsonParser.getLvlCount(JsonParser.lvlType.level) ? lvl : JsonParser.getLvlCount(JsonParser.lvlType.level)-1;
                    GameNotifyHandler.putNotify(new LoadLevel(lvl));
                    GameNotifyHandler.putNotify(new ReInitInfoPanel());
                }
                for (int i = 0; i != lore.Length; i++)
                {
                    if (Input.GetMouseButtonDown(0))
                        while((!lore[i].Equals('.') && !lore[i].Equals('?')) && i != lore.Length)
                        {
                            bannerVal.LoreText.text += lore[i];
                            i++;
                        }
                    bannerVal.LoreText.text += lore[i];
                    yield return null;
                }
                toggle.gameObject.SetActive(true);
                while(!toggle.isOn)
                    yield return null;
            }
            
            public override void finish()
            {
                if (isNew)
                {
                    ValuesNotifyHandle.putNotify(new LevelPurch());
                    ValuesNotifyHandle.putNotify(new TakeScore(-JsonParser.getLvlCost(lvl)));
                }
                GameObject.Find("Settings").transform.SetSiblingIndex(7);
                toggle.gameObject.SetActive(false);
                anim.SetBool("isLore", false);
                bannerVal.SetDefault();
            }

            public override void broke()
            {
                GameObject.Find("Settings").transform.SetSiblingIndex(7);
                toggle.gameObject.SetActive(false);
                anim.SetBool("isLore", false);
                bannerVal.SetDefault();
            }
        }
        protected class ArchivState : BannerState
        { 
            Animator anim;
            Archivka archiv;
            public ArchivState(BannerStruct val, Archivka archiv) : base(val) => this.archiv = archiv;
            public override void init(Transform transform)
            {
                bannerVal.FullText.text = "<b>" + archiv.LocalizeCategory() + "</b>:\n" + archiv.text;
                anim = transform.GetComponent<Animator>();
                anim.SetBool("isArchiv", true);
            }

            public override IEnumerator update()
            {
                yield return new WaitUntil(() => bannerVal.FullText.IsActive());
                bannerVal.hlopyshkaL.Play();
                bannerVal.hlopyshkaR.Play();
                yield return new WaitForSeconds(bannerVal.TimeForArchivHang);
                anim.SetBool("isArchiv", false);
                bannerVal.hlopyshkaL.Stop();
                bannerVal.hlopyshkaR.Stop();
                yield return new WaitUntil(() =>  anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            }

            public override void finish()
            {
                ValuesNotifyHandle.putNotify(new ArchivDone(archiv));
                bannerVal.SetDefault();
            }

            public override void broke()
            {
                anim.SetBool("isArchiv", false);
                bannerVal.SetDefault();
            }
        }
        private BannerState state;
        public BannerStruct bannerVal;
        bool isNeedWait; 

        private void Start()
        {
            bannerVal.init(GetComponent<RectTransform>());
            state = new RunStringState(bannerVal);
            isNeedWait = true;
            StartCoroutine(StateHandler());
        }
        IEnumerator StateHandler()
        {
            while (true)
            {
                if (state != null)
                {
                    if (isNeedWait)
                        yield return new WaitForSecondsRealtime(bannerVal.TimeForNewNews);
                    state.init(transform);
                    yield return state.update();
                    state.finish();
                    state = null;
                    isNeedWait = true;
                }
                else
                {
                    yield return new WaitForSecondsRealtime(1);
                    if (Values.profile.IsDoneArchiv())
                    {
                        isNeedWait = false;
                        state = new ArchivState(bannerVal, Values.profile.GetDoneArchiv());
                    }
                    else if ((Random.value < 0.025f && Values.profile.isClickAbleTake()) || Input.GetKey(KeyCode.Q))
                        state = new BuffOfferState(bannerVal);
                    else if (Random.value < 0.05f || Input.GetKey(KeyCode.E))
                        state = new TakeContractsOfferState(bannerVal);
                    else
                        state = new RunStringState(bannerVal);
                }
                yield return null;
            }
        } 
        public void TalkLore(int lvl, bool isNew)
        {
            Debug.Log("I am here");
            StopAllCoroutines();
            state?.broke();
            isNeedWait = false;
            state = new LoreTalkState(bannerVal, lvl, isNew);
            StartCoroutine(StateHandler());
        }
        public bool isLoreTalk() => state?.GetType() == typeof(LoreTalkState); 
        private void Update()
        {  
            if (Values.profile.IsDoneArchiv() && state != null)
                if (state.GetType().Equals(typeof(RunStringState)))
                    ((RunStringState)state).goFast();
        }
        public void SetDefault()
        {
            if(state?.GetType() != typeof(LoreTalkState))
            {
                StopAllCoroutines();
                if(state != null)
                    state.broke();
                state = null;
                isNeedWait = false;
                StartCoroutine(StateHandler());
            }
        }
        public void Localize()
        {
            StopAllCoroutines();
            bannerVal.Localize();

            state.reinit(bannerVal);
            StartCoroutine(StateHandler());
        }
    }
}
