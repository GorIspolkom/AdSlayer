using Clicker.GameSystem;
using System.Linq;
using UnityEngine;

namespace Clicker.Models
{
    //все игровые структуры данных игрового процесса
    //-------------------------------------------------------------------
    //player's progress data 
    [System.Serializable]
    public class ProfileData
    {
        public enum ProfilicChart
        {
            Score = 0,
            ScorePerSecond = 1,
            BlueScreenCount = 2,
            EverScore = 3,
            Click = 4,
            ScorePerClick = 5
        }
        //все время в игре(переменная, в которую каждый игровой тик добавляется 1)
        public long time;
        //текущей скор
        public XXLNum Score;
        //весь скор(за все уровни)
        public XXLNum EverScore;
        //количество кликов
        public XXLNum Click;
        public XXLNum prevClick;
        //Количество скора за клик и секунду, причем ScorePerClick ~ ScorePerSecond
        public XXLNum ScorePerClick;
        public XXLNum ScorePerSecond;
        public int coins;
        public int prestige;

        //Количество выпадания синего экрана(для других паков что-то свое)
        public long BlueScreenCount;
        //Грейды, первый массив - уровни, второй - номер грейда
        private int[][] upgrades;
        public int[] getGrades() => upgrades[Values.data.lvl];
        public int getGrade(int i) => upgrades[Values.data.lvl][i];
        //Система ачивок, хранит полученные ачивки и проверяет условие получения
        private ArchivmentSystem archivments;
        public bool IsDoneArchiv() => archivments.IsDoneArchiv();
        public Archivka GetDoneArchiv() => archivments.GetDonArchiv();
        public void PopArchiv(Archivka arch) => archivments.archivmentDone(arch);
        public int[] GetAllProgress() => archivments.GetAllProgress();
        public int GetArchivProgress(int id) => archivments.getArchivProgress((Archivments)id);
        public void openLvl() => archivments.OpenNewLvl();
        public int lastLvl { get => archivments.LastLvl();}

        //система баффов, которая хранит баффы, обрабатывает их и удаляет инвалидные
        private BuffSystem TimerBuff; //для баффа за секуннду
        private ClickBuffSystem ClickBuff; //для баффа за клик
        //добавление новых баффов
        public void AddClickBuff(int buffVal, long timer) => ClickBuff.Add(buffVal, timer);
        public void AddTimerBuff(int buffVal, long timer) => TimerBuff.Add(buffVal, timer);
        //можно ли добавить бафф
        public bool isTimerAbleTake() => TimerBuff.isAbleTake(); 
        public bool isClickAbleTake() => ClickBuff.isAbleTake(); 
        //получения баффа 
        public int GetTimerBuff() => TimerBuff.GetBuff();
        public int GetClickBuff() => ClickBuff.GetBuff();
        //получения скора за действие с учетом баффов
        public XXLNum GetScorePerSecondInBuff() => ScorePerSecond * TimerBuff.GetPrestigedBuff();
        public XXLNum GetScorePerClickInBuff() => ScorePerClick * ClickBuff.GetPrestigedBuff();
        //множитель ScorePerClick в зависимости от количества кликов за секунду 
        public void BuffByClick(int delta) => ClickBuff.DeltaCounter(delta);
        private BustSystem bustSystem;
        public void PutBust(Buster buster) => bustSystem.AddBust(buster);

        public ProfileData()
        {
            Score = XXLNum.zero;
            EverScore = XXLNum.zero;
            Click = XXLNum.zero;
            ScorePerSecond = XXLNum.zero;
            ScorePerClick = XXLNum.calibrate(1);
            coins = 0;
            prestige = 0;
            TimerBuff = new BuffSystem();
            ClickBuff = new ClickBuffSystem();
            archivments = new ArchivmentSystem();
            bustSystem = new BustSystem();

            int size = MyUtile.JsonWorker.JsonParser.getLvlCount(MyUtile.JsonWorker.JsonParser.lvlType.level);
            upgrades = new int[size][];
            for (int i = 0; i != size; i++)
            {
                int length = MyUtile.JsonWorker.JsonParser.getPurchasesSize(i);
                upgrades[i] = new int[length];
                for (int j = 0; j != length; j++)
                    upgrades[i][j] = 0;
            }
            time = 0;
            BlueScreenCount = 0;
        }
        //math module
        //-------------------------------------------------------
        public void AddScore(XXLNum add)
        {
            if (add > 0)
            {
                Score = Score + add;
                EverScore += add;
            }
            else if(Score > add)
                Score += add;
        }
        public void AddScorePerSecond(XXLNum add) {
            ScorePerSecond = ScorePerSecond + add;
            ScorePerClick = SPCfromSPS();
        }
        public void ClickAction()
        {
            Click++;
            AddScore(GetScorePerClickInBuff());
        }
        private XXLNum SPCfromSPS() => ScorePerSecond > 37.5f ? ScorePerSecond / 37.5f : new XXLNum(1, 0);
        //output module
        //-------------------------------------------------------
        public string outputNeeded(ProfilicChart chart)
        {
            switch (chart)
            {
                case ProfilicChart.Score:
                    return Score.ToString();

                case ProfilicChart.EverScore:
                    return EverScore.ToString();

                case ProfilicChart.ScorePerSecond:
                    return ScorePerSecond.ToString();

                case ProfilicChart.ScorePerClick:
                    return ScorePerClick.ToString();

                case ProfilicChart.Click:
                    return Click.ToString();

                case ProfilicChart.BlueScreenCount:
                    return BlueScreenCount.ToString();

                default:
                    return "UNKNOWN CATEGORY!";
            }
        }

        public string printTime()
        {
            int h = (int)time / 3600;
            int m = (int)(time - h * 3600) / 60;
            int s = (int)(time - h * 3600 - m * 60);
            return $"{h}:{m}:{s}";
        }
        //update module
        //-------------------------------------------------------
        //покупка
        public void Grade(int id, int add) => upgrades[Values.data.lvl][id] += add;
        //Метод обработки игрового тика
        public void Timer()
        {
            int delta = (Click - prevClick).ToInt();
            GameData.data.clickPerSecond = delta; 
            prevClick = Click;
            BuffByClick(delta);
            TimerBuff.CheckValidBuff();
            ClickBuff.CheckValidBuff();
            bustSystem.Update();
            AddScore(ScorePerSecond * GetTimerBuff());
            archivments.WaitForArchivment(this);
            time++;
        }

        public bool Equals(ProfileData data) => data != null &&
                   time == data.time &&
                   Score == data.Score &&
                   EverScore == data.EverScore &&
                   Click == data.Click &&
                   ScorePerClick == data.ScorePerClick &&
                   ScorePerSecond == data.ScorePerSecond &&
                   BlueScreenCount == data.BlueScreenCount &&
                   upgrades.SequenceEqual(data.upgrades);
    }
    //settings values
    [System.Serializable]
    public class SettingData
    {
        public static SettingData settings;
        //language
        public string lang;
        public float volume;
        public float sound;
        public SettingData(bool isTest, bool isAutoClick, string lang, float volume, float sound)
        {
            this.lang = lang;
            this.volume = volume;
            this.sound = sound;
        }
        public SettingData()
        {
            lang = "Eng";
            volume = 0.5f;
            sound = 0.5f;
        }

        public override bool Equals(object obj)
        {
            var data = obj as SettingData;
            return lang.Equals(data.lang) &&
                   volume == data.volume &&
                   sound == data.sound;
        }
    }
    //in-game data
    [System.Serializable]
    public struct GameData
    {
        public static GameData data;
        //текущий уровень
        public int lvl;
        //пак данного сейва
        public int pack;
        //test module
        public bool isTest;
        public bool isAutoClick;
        public int deltaClick;
        public int clickPerSecond;
        public float k_shop;
        public GameData(int pack, int lvl, bool isTest)
        {
            this.pack = pack;
            this.lvl = lvl;
            this.isTest = isTest;
            this.isAutoClick = false;
            k_shop = 0.8f;
            deltaClick = 0;
            clickPerSecond = 0;
        }
        public GameData(int pack)
        {
            this.pack = pack;
            lvl = 0;
            isTest = false;
            isAutoClick = false;
            k_shop = 0.8f;
            deltaClick = 0;
            clickPerSecond = 0;
        }
    }

    //описанние работы класса Values
    //-------------------------------------------------------------------
    public class Values
    {
        private static ProfileData profileData;
        public static ProfileData profile{ get => profileData; }
        public ref ProfileData profileRef { get => ref profileData; }

        public static SettingData settings { get => SettingData.settings; }
        public static GameData data { get => GameData.data; }
        public static void init(ProfileData save, SettingData settings, GameData gameData)
        {
            profileData = save;
            SettingData.settings = settings;
            GameData.data = gameData; 
        }
        public static void init(int pack)
        {
            profileData = new ProfileData();
            SettingData.settings = new SettingData();
            GameData.data = new GameData(pack);
        }
        public static void initGameData(SettingData settings, GameData gameData)
        {
            SettingData.settings = settings;
            GameData.data = gameData;
        }
        public static void initProfile(ProfileData save)
        {
            profileData = save;
        }
        public static bool Equals(ProfileData _profile, SettingData _settings, GameData _gameData)
        {
            return profile.Equals(_profile) && settings.Equals(_settings) && data.Equals(_gameData);
        }
    }
}
