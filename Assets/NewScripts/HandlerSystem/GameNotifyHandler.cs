using UnityEngine;
using System.Collections.Generic;
using Clicker.Scrypts;
using UnityEngine.UI;
using Clicker.Models;
using UnityEngine.EventSystems;

namespace Clicker.HandlerSystem
{
    class TestGameScene : GameScene
    {
        public Text Click_rate;
        public Text Frame_rate;

        public TestGameScene() : base()
        {
            Click_rate = GameObject.Find("Click_rate").GetComponent<Text>();
            Frame_rate = GameObject.Find("Frame_rate").GetComponent<Text>();
        }
        public override void update(){
            base.update();
            try
            {
                Click_rate.text = Values.data.clickPerSecond.ToString();
                Frame_rate.text = (1f/Time.deltaTime).ToString("0.0");
            }
            catch { }
        }
    }
    class GameScene : GameNotifyVal
    {
        public InfoPanelContentIniter shopPanel;
        public InfoPanelContentIniter lvlPanel;
        public BulletContainer shooter;
        public BannerController banner;
        public ADPublish clickAd;

        public GameScene() : base()
        {
            clickAd = GameObject.Find("clickable_ad").GetComponent<ADPublish>();
            banner = GameObject.FindGameObjectWithTag("Banner").GetComponent<BannerController>();
            lvlPanel = GameObject.Find("LvlPanel").GetComponent<InfoPanelContentIniter>();
            shopPanel = GameObject.Find("ShopPanel").GetComponent<InfoPanelContentIniter>();
            shooter = GameObject.Find("ScoreBored").GetComponent<BulletContainer>();
            GameObject.Find("MainBG").GetComponent<Image>().sprite = 
                    Resources.Load<Sprite>($"BGpic/{Values.data.pack}.{Values.data.lvl}");
            //нужен путь к панеле с предложением баффа
            scene = 1;
        }
        public void Localize()
        {
            banner.Localize();
            clickAd.rePublish();
        } 
        public override void update()
        {
        }
    }
    class ExtraScene : GameNotifyVal
    {
        public int clicks;
        protected string time;
        protected string valute;
        public Text Timer;
        public Text ScoreBored;
        public BlueLoad reloader;
        public Animation ShatterPanel;
        private bool isGame;
        private XXLNum scorePerClick;
        //время для генерации жизни экрана
        public float TimeOnScene = 7;

        public ExtraScene() : base()
        {
            time = "Time to reboot:\n";
            valute = MyUtile.JsonWorker.JsonParser.getLocaliz("ScoreBored");
            TimeOnScene = Random.Range(TimeOnScene, TimeOnScene + 5);
            scene = 2;
            clicks = 0;
            isGame = true;
            Timer = GameObject.Find("Timer").GetComponent<Text>();
            ScoreBored = GameObject.Find("ScoreBored").GetComponent<Text>();
            reloader = GameObject.Find("Reloading").GetComponent<BlueLoad>();
            reloader.gameObject.SetActive(false);
            ShatterPanel = GameObject.Find("ShatterPanel").GetComponent<Animation>();
            scorePerClick = Values.profile.ScorePerClick * 15;
            GameObject.Find("Panel").GetComponent<AudioSource>().volume = Values.settings.volume;
            GameObject.Find("BG").GetComponent<Image>().sprite = Resources.Load<Sprite>($"BGpic/{Values.data.pack}.{Values.data.lvl}");
            GameObject.FindGameObjectWithTag("Controller").GetComponent<AudioSource>().Pause();
            //GameObject.Find("Panel").GetComponent<AudioSource>().time = 3;
        }

        public void AssignBaff()
        {
            int x = 2;
            if (Random.value >= 0.95)
                x = 4;
            else if (Random.value >= 0.7)
                x = 3;
            if (Random.value > 0.5f)
                ValuesNotifyHandle.putNotify(new TakeBuff(BuffType.perSecond, x, Random.Range(30, 60)));
            else
                ValuesNotifyHandle.putNotify(new TakeBuff(BuffType.perClick, x, Random.Range(30, 60)));
        }
        public void PlayAnim()
        {
            if (!ShatterPanel.isPlaying)
                ShatterPanel.Play();
        }

        public override void update()
        {
            if (TimeOnScene > 0)
            {
                ScoreBored.text = valute.Replace("%s", (scorePerClick * clicks).ToString());
                Timer.text = time + TimeOnScene.ToString("0.00");
                TimeOnScene -= Time.deltaTime;
            }
            else if(isGame)
            {
                isGame = false;
                Timer.text = time + "0.00";
                AssignBaff();
                reloader.init(Values.profile.ScorePerSecond * (15 * clicks));
                GameObject.Find("Panel").GetComponent<AudioSource>().time = 42f;
            }
        }
    }
    class WelcomeScene : GameNotifyVal
    {
        public ControllWelcomeInit welcomeController;
        public WelcomeScene()
        {
            scene = 0;
            welcomeController = GameObject.FindObjectOfType<ControllWelcomeInit>();
        }

        public override void update()
        { 

        }
    }
    class EditorScene : GameNotifyVal
    {
        public InitEditor editor;
        public EditorScene()
        {
            scene = 3;
            editor = GameObject.Find("ad_prefab").GetComponent<InitEditor>();
            GameObject.Find("BG").GetComponent<Image>().sprite =
                    Resources.Load<Sprite>($"BGpic/{Values.data.pack}.{Values.data.lvl}");
        }
        public override void update()
        {

        }
    }
    abstract class GameNotifyVal
    {
        public AsyncOperation operation;
        public int scene;
        public bool isAllowPut;
        public GameNotifyVal()
        {
            operation = null;
            isAllowPut = true;
        }
        public abstract void update();
    }
    /// <summary>
    /// загружает уровень в фоне;
    /// запускает загруженный уровень;
    /// выводит окнo баффа с рекламой;
    /// </summary>
    class GameNotifyHandler
    {
        private static List<GameNotify> notifies;
        private static GameNotifyVal val;
        public GameNotifyHandler()
        {
            InitVal();
            notifies = new List<GameNotify>();
        }
        public void update()
        {
            if (notifies.Count != 0)
            {
                try
                {
                    if (notifies[0].isValid(val))
                    {
                        notifies[0].init(ref val);
                    }
                    notifies.RemoveAt(0);
                }
                catch (System.Exception e)
                {
                    notifies.RemoveAt(0);
                    Debug.LogError(e);
                }
            }
            val.update();
        }
        public static void InitVal()
        {
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
            {
                case 0:
                    val = new WelcomeScene();
                    break;

                case 1:
                    if (Values.data.isTest)
                        val = new TestGameScene();
                    else
                        val = new GameScene();
                    break;

                case 2:
                    val = new ExtraScene();
                    break;
                case 3:
                    val = new EditorScene();
                    break;
                default:
                    val = null;
                    break;
            }
        }
        public static void putNotify(GameNotify notify, bool isNes = false)
        {
            if (val.isAllowPut || isNes)
                notifies.Add(notify);
        }
    }
}
