using Clicker.Models;
using Clicker.SaveSystem;
using MyUtile.JsonWorker;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Clicker.HandlerSystem
{
    public class SaverLoader : MonoBehaviour
    {
        //true если на игровой сцене 
        public bool isTest;
        private ValuesNotifyHandle valHandler;
        private GameNotifyHandler objHandler;
        public AudioSource source;
        public bool isEthernet { get =>
            Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork ||
                       Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork; }
        //музыка
        public AudioClip[] musics;
        //текущий трек
        int musicID;
        //инитит Values и объект пака из json и запускает корутины
        //на время тестов Controller есть на каждой сцене, поэтому он удаляется если существует
        void Awake()
        {
            if (Values.settings == null)
            {
                //проверка необходимая для тестов
                //статический(глобальный) объект
                DontDestroyOnLoad(gameObject);
                //загрузка
                Application.targetFrameRate = 120;
                long unGameTime = SaveSystem2.LoadGameData(isTest, isEthernet);
                SaveSystem2.LoadLastSession();

                //JsonParser.SelectPack(0, isTest);
                //SaveData data = SaveSytem.LoadValues();
                //Values.init(data.val, data.settings, data.data);

                JsonIniter.InitClickers(Values.data.lvl);
                XXLNum.localizePrefix();
                source.volume = Values.settings.volume;

                musicID = 0;
                source.clip = musics[musicID];
                source.Play();

                valHandler = new ValuesNotifyHandle();
                objHandler = new GameNotifyHandler();
                
                GameNotifyHandler.putNotify(new WelcomePlayer(unGameTime));

                //запускаем корутины сохранения и прибавления
                StartCoroutine(TimerProcces()); 
                
                Thread handle = new Thread(Handler);
                handle.Start();
            }
            else
                Destroy(gameObject);
        }

        private void Handler()
        {
            while (true) {
                valHandler.update();
                Thread.Sleep(16);
            }
        }
        private void Update()
        {
            objHandler.update();
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.X))
            {
                GameData.data.deltaClick = 60;
                GameData.data.isAutoClick = !GameData.data.isAutoClick;
            }
            else if (Input.GetKeyDown(KeyCode.Z))
                GameNotifyHandler.putNotify(new LoadLevel(0));
            else if (Input.GetKeyDown(KeyCode.W))
                ValuesNotifyHandle.putNotify(new TakeCoins(10));
            else if (Input.GetKeyDown(KeyCode.C))
                ValuesNotifyHandle.putNotify(new GoToExtraScene());
            else if (Input.GetKeyDown(KeyCode.V))
                GameNotifyHandler.putNotify(new BrokeBanner());
            else if (Input.GetKeyUp(KeyCode.F))
            {
                UnityEditor.EditorUtility.RevealInFinder(Application.persistentDataPath);
                Debug.Log(Application.persistentDataPath);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                ValuesNotifyHandle.putNotify(new TakeBuster(new GetMomentalScore(10 * 3600), 0));
                ValuesNotifyHandle.putNotify(new AddToProfile(Values.profile.Score / 100000 + 1, ProfileData.ProfilicChart.ScorePerSecond));
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                ValuesNotifyHandle.putNotify(new TakeBuster(new GetMomentalScore(10 * 3600), 0));
                ValuesNotifyHandle.putNotify(new AddToProfile(Values.profile.Score / 1000 + 1, ProfileData.ProfilicChart.ScorePerSecond));
            }

#elif UNITY_ANDROID && !UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameNotifyHandler.putNotify(new CloseAllPanels());
            }
#endif
        }
        //прибавка каждую секунду
        private IEnumerator TimerProcces()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(1);
                //производим прибавление ScorePerSecond
                //ставит музыку
                SetMusic();
                ValuesNotifyHandle.putNotify(new Timer());

                float x = UnityEngine.Random.value;
                if (x < Mathf.Log10(Values.data.clickPerSecond + 1) / 950)
                    ValuesNotifyHandle.putNotify(new GoToExtraScene());
            }
        }
        //работа с музыкой
        private void SetMusic()
        {
            //если музыка кончилась
            if (source.time == 0)
            {
                musicID++;
                if (musicID == musics.Length)
                    musicID = 0;
                source.clip = musics[musicID];
                source.Play();
            }
        }

        //сейв при выходе из игры
#if UNITY_ANDROID
        private void OnApplicationPause(bool pause)
        {
            if (pause)
                ValuesNotifyHandle.putNotify(new Save(isEthernet, true));

            GameNotifyHandler.putNotify(new UrinoPause());
        }
#endif
        
        private void OnApplicationQuit()
        {
            ValuesNotifyHandle.putNotify(new Save(isEthernet, true));
        } 
    }
}
