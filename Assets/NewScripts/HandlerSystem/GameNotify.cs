using Clicker.DetachedScrypts;
using Clicker.Models;
using Clicker.Scrypts;
using MyUtile.JsonWorker;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clicker.HandlerSystem
{
    interface GameNotify
    {
        void init(ref GameNotifyVal val);
        bool isValid(GameNotifyVal val);
    }
    //Detached scripts
    class LoadScene : GameNotify
    {
        int lvl;
        public LoadScene(int lvl)
        {
            this.lvl = lvl;
        }

        public void init(ref GameNotifyVal val)
        {
            val.isAllowPut = false;
            val.operation = SceneManager.LoadSceneAsync(lvl, LoadSceneMode.Single);
            val.operation.allowSceneActivation = false;
            val.operation.completed += delegate (AsyncOperation oper) 
            {
                GameNotifyHandler.InitVal();
            };
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.operation == null;
        }
    }
    class ActivateScene : GameNotify
    {
        public void init(ref GameNotifyVal val)
        {
            Debug.Log("Разрешена загрузка!");
            val.scene = -1;
            val.operation.allowSceneActivation = true;
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.operation != null;
        }
    }
    class LoadPanel : GameNotify
    {
        bool anotherConds;
        string name;
        public LoadPanel(string name, bool anotherConds = true)
        {
            this.name = name;
            this.anotherConds = anotherConds;
        }
        public void init(ref GameNotifyVal val)
        {
            GameObject panel = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Panels/" + name), GameObject.Find("Canvas").transform);
            panel.name = name;
        }

        public bool isValid(GameNotifyVal val)
        {
            return GameObject.Find(name) == null && anotherConds;
        }
    }
    class ScoreBuffPanel : GameNotify
    {
        private XXLNum Score;
        private int buff;
        private float ver;

        public ScoreBuffPanel(XXLNum Score, int buff = 0, float ver = 0f)
        {
            this.Score = Score;
            this.ver = ver;
        }
        public void init(ref GameNotifyVal val)
        {
            GameObject buffCondition = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Panels/BuffPanel"), GameObject.FindGameObjectWithTag("MainCanvas").transform);
            buffCondition.name = "BuffPanel";
            float to = buffCondition.transform.localPosition.y;
            buffCondition.transform.localPosition = new Vector2(0, -700);
            LeanTween.moveY(buffCondition.transform as RectTransform, -40, 0.3f).setEaseOutBack();
            buffCondition.GetComponent<BuffPanelInit>().InitBuffScore(Score, buff, ver);
        }

        public bool isValid(GameNotifyVal val)
        {
            //условие вывода окна баффа
            return GameObject.Find("BuffPanel") == null && val.scene == 1 && !Score.isZero()
                && Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
    class TakeBuffPanel : GameNotify
    {
        private BuffType type;
        private int buff;
        private long time;
        private float ver;

        public TakeBuffPanel(BuffType type, long time, int buff = 0, float ver = 0f)
        {
            this.type = type;
            this.time = time;
            this.buff = buff;
            this.ver = ver;
        }
        public void init(ref GameNotifyVal val)
        {
            GameObject buffCondition = GameObject.
                Instantiate(Resources.Load<GameObject>("Prefabs/Panels/BuffPanel"), GameObject.Find("Canvas").transform);
            buffCondition.name = "BuffPanel";
            float to = buffCondition.transform.localPosition.y;
            buffCondition.transform.localPosition = new Vector2(0, -700);
            LeanTween.moveY(buffCondition.transform as RectTransform, -40, 0.3f).setEaseOutBack();
            buffCondition.GetComponent<BuffPanelInit>().InitBuff(type, buff, time);
        }

        public bool isValid(GameNotifyVal val)
        {
            //условие вывода окна баффа
            return GameObject.Find("BuffPanel") == null && val.scene == 1 
                && Application.internetReachability != NetworkReachability.NotReachable;
        }
    }
    class ResetScene : GameNotify
    {
        public void init(ref GameNotifyVal val)
        {
            ((GameScene)val).banner.SetDefault();
            ((GameScene)val).shopPanel.down();
            ((GameScene)val).lvlPanel.down();
            ((GameScene)val).shopPanel.reinit();
            ((GameScene)val).lvlPanel.reinit();
            ((GameScene)val).clickAd.PublishNewAd();
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 1;
        }
    }
    class LoadLevel : GameNotify
    { 
        public LoadLevel(int lvl, int pack = -1)
        {
            GameData.data.lvl = lvl;
            if(pack != -1)
                GameData.data.pack = pack;
            GameNotifyHandler.putNotify(new ResetScene());
        }
        public void init(ref GameNotifyVal val)
        {
            JsonIniter.InitClickers(GameData.data.lvl);
            GameObject.Find("MainBG").GetComponent<UnityEngine.UI.Image>().sprite = Resources.Load<Sprite>($"BGpic/{GameData.data.pack}.{GameData.data.lvl}");
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 1;
        }
    }
    class ShootScore : GameNotify
    {
        private XXLNum add;
        public ShootScore(XXLNum add)
        {
            this.add = add;
        }
        public void init(ref GameNotifyVal val)
        {
            ((GameScene)val).shooter.ShootScore(add);
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 1;
        }
    }
    class BuffShowUpdate : GameNotify
    {
        public void init(ref GameNotifyVal val)
        {
            GameObject.Find("ScoreShower").GetComponent<ScoreShower>().ReCalcAngels();
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 1;
        }
    }
    class TakeContractsPanel : GameNotify
    {
        private int contracts;

        public TakeContractsPanel(int contracts)
        {
            this.contracts = contracts;
        }
        public void init(ref GameNotifyVal val)
        {
            GameObject buffCondition = GameObject.
                Instantiate(Resources.Load<GameObject>("Prefabs/Panels/BuffPanel"), GameObject.Find("Canvas").transform);
            buffCondition.name = "BuffPanel";
            buffCondition.GetComponent<BuffPanelInit>().InitContractOffer(contracts);
        }

        public bool isValid(GameNotifyVal val)
        {
            //условие вывода окна баффа
            return GameObject.Find("BuffPanel") == null && val.scene == 1;
        }
    }
    class Localize : GameNotify
    {
        public void init(ref GameNotifyVal val)
        {
            XXLNum.localizePrefix();
            JsonIniter.reinitTexts();
            ((GameScene)val).Localize(); 
            foreach(Localizate local in GameObject.FindObjectsOfType<Localizate>())
            {
                local.Translate();
            }
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 1;
        }
    }
    class TalkLore : GameNotify
    {
        int lvl;
        bool isNew;

        public TalkLore(int lvl, bool isNew = true)
        {
            this.lvl = lvl;
            this.isNew = isNew;
        }
        public void init(ref GameNotifyVal val)
        {
            ((GameScene)val).shopPanel.down();
            ((GameScene)val).lvlPanel.down();
            ((GameScene)val).banner.TalkLore(lvl, isNew);
        }

        public bool isValid(GameNotifyVal val)
        {
            if (val.scene == 1)
                if (!((GameScene)val).banner.isLoreTalk())
                    return true;
            return false;
        }
    }
    class BrokeBanner : GameNotify
    {
        public void init(ref GameNotifyVal val)
        {
            ((GameScene)val).banner.SetDefault();
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 1;
        }
    } 
    class ReInitInfoPanel : GameNotify
    {

        public void init(ref GameNotifyVal val)
        {
            ((GameScene)val).shopPanel.reinit();
            ((GameScene)val).lvlPanel.reinit(); 
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 1;
        }
    }
    class DestroyAd : GameNotify
    {
        public void init(ref GameNotifyVal val)
        {
            ((ExtraScene)val).clicks++;
            ((ExtraScene)val).PlayAnim();
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 2;
        }
    }
    class VolumeSet : GameNotify
    {
        float value;
        public VolumeSet(float value) => this.value = value;
        public void init(ref GameNotifyVal val)
        {
            GameObject.FindGameObjectWithTag("Controller").GetComponent<AudioSource>().volume = value;
            Values.settings.volume = value;
        }

        public bool isValid(GameNotifyVal val)
        {
            return true;
        }
    }
    class SoundSet : GameNotify
    {
        float value;
        public SoundSet(float value) => this.value = value;
        public void init(ref GameNotifyVal val)
        {
            GameObject.Find("clickable_ad").GetComponent<AudioSource>().volume = value;
            Values.settings.sound = value;
        }

        public bool isValid(GameNotifyVal val)
        {
            return true;
        }
    }
    class ClosePanel : GameNotify
    {
        GameObject panel;
        public ClosePanel(GameObject panel)
        {
            this.panel = panel;
        }
        public void init(ref GameNotifyVal val)
        { 
            GameObject.Destroy(panel);
        }

        public bool isValid(GameNotifyVal val)
        {
            return true;
        }
    }
    class CloseAllPanels : GameNotify
    { 
        public void init(ref GameNotifyVal val)
        {
            foreach (GameObject panel in GameObject.FindGameObjectsWithTag("Panel")) 
                GameObject.Destroy(panel);
            if (val.scene == 1)
            {
                ((GameScene)val).shopPanel.reinit();
                ((GameScene)val).lvlPanel.reinit();
            }
        }

        public bool isValid(GameNotifyVal val)
        {
            return true;
        }
    }
    class WelcomePlayer : GameNotify
    {
        long time;
        public WelcomePlayer(long time)
        {
            this.time = time;
        }

        public void init(ref GameNotifyVal val)
        {
            ((WelcomeScene)val).welcomeController.init(time);
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 0;
        }
    } 
    class UrinoPause : GameNotify
    { 
        public void init(ref GameNotifyVal val)
        { 
            Pause(); 
        }
        async void Pause()
        { 
            long time = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            await System.Threading.Tasks.Task.Run(() => { }); 

            time = DateTime.Now.Ticks / TimeSpan.TicksPerSecond - time;
            Debug.Log($"Time on pause: {time}");
            if (time > 7200)
                GameNotifyHandler.putNotify(new ScoreBuffPanel(Values.profile.ScorePerSecond * 7200));
            else if (time > 5)
                GameNotifyHandler.putNotify(new ScoreBuffPanel(Values.profile.ScorePerSecond * time));
        }
        public bool isValid(GameNotifyVal val)
        {
            if (val.scene == 1)
                return !Advertisment.Advert.isAdShown;
            return false;
        }
    }
    class SaveEditor : GameNotify
    {
        public void init(ref GameNotifyVal val)
        {
            ((EditorScene)val).editor.Save();
        }

        public bool isValid(GameNotifyVal val)
        {
            return val.scene == 3;
        }
    }
}
