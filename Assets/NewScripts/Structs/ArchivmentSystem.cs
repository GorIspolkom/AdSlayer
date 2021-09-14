using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Clicker.GameSystem
{
    /// <summary>
    /// система ачивок хранит достигнутые ачивки
    /// ачивки храняться последовательно, по возрастанию в одном типе ачивок
    /// поэтому в словаре хранится числа прогресса типа ачивки
    /// Также каждый игровой тик обрабатывает условие выполнение следующей ачивки
    /// </summary>

    [System.Serializable]
    public struct Archivka
    {
        public Archivments archType;
        public int progLvl;
        public string category;
        public string text;
        public Archivka(Archivments archType, int progLvl)
        {
            this.archType = archType;
            this.progLvl = progLvl;
            this.category = ArchivmentSystem.GetCategoryArchivName(archType);
            this.text = JsonParser.getArchivText(category, progLvl);
        }
        public string LocalizeCategory() => JsonParser.getLocaliz(category);
        public override bool Equals(object obj)
        {
            var arch = (Archivka)obj;
            return arch.archType == archType && arch.progLvl == progLvl;
        }
    }
    //Archivments type
    public enum Archivments
    {
        Score = 0,
        PerSecond = 1,
        Time = 2,
        Wasted = 3,
        BlueScreen = 4,
        Archivs = 5
    }
    [System.Serializable]
    public class ArchivmentSystem
    {
        [System.Serializable]
        protected abstract class LevelCondition
        {
            public int lvl;
            public bool isHandle;
            public LevelCondition(int lvl)
            {
                this.lvl = lvl;
                this.isHandle = false;
            }
            public abstract bool Condition();
        }
        [System.Serializable]
        protected class FirstLevel : LevelCondition
        {
            public FirstLevel() : base(0)
            {

            }

            public override bool Condition()
            {
                return true;
            }
        }
        [System.Serializable]
        protected class NoneCondition : LevelCondition
        {
            public NoneCondition(int lvl) : base(lvl)
            {
            }

            public override bool Condition()
            {
                return false;
            }
        }
        [System.Serializable]
        protected class LastLevel : LevelCondition
        {
            XXLNum condition;
            public LastLevel(int lvl) : base(lvl)
            {
                condition = JsonParser.getLvlCost(lvl);
            }
            public override bool Condition()
            {
                return Values.profile.Score > condition;
            }
        } 
        //archivments progress by type
        private Dictionary<Archivments, int> archivProg;
        private Dictionary<Archivments, XXLNum> archivMax;
        
        LevelCondition lvlCond; 
        int progressSum;
        public List<Archivka> doneArchiv;
        public bool IsDoneArchiv() => doneArchiv.Count != 0;
        public Archivka GetDonArchiv() => doneArchiv[0];
        public int[] GetAllProgress()
        {
            int[] progresses = new int[7];
            foreach (Archivments archiv in Enum.GetValues(typeof(Archivments)))
            {
                progresses[(int)archiv] = archivProg[archiv];
            }
            progresses[6] = LastLvl();
            return progresses;
        }
        //archivments dictionary init
        public ArchivmentSystem()
        {
            archivProg = new Dictionary<Archivments, int>();
            archivMax = new Dictionary<Archivments, XXLNum>();
            lvlCond = new FirstLevel(); 
            progressSum = 0;
            doneArchiv = new List<Archivka>();
            foreach (Archivments archiv in Enum.GetValues(typeof(Archivments)))
            {
                archivProg.Add(archiv, 0);
                archivMax[archiv] = JsonParser.getArchivValue(GetCategoryArchivName(archiv), 0);
            }
        }
        //complete archiv and return previous progress
        public void archivmentDone(Archivka arch)
        {
            doneArchiv.Remove(arch);
        }
        //get last get archivment
        public int getArchivProgress(Archivments archiv) => archivProg[archiv]; 
        //метод проверки условий достижения следующей ачивки в категории
        public bool isProgress(Archivments archiv, ProfileData profile)
        {
            //имя категории
            string category = GetCategoryArchivName(archiv);
            //условия, что есть неоткрытые ачивки
            if (archivProg[archiv] < JsonParser.getArchivCount(category))
                switch (archiv)
                {
                    //ачивки достижения скора(учитывается скор за все время) и так далее
                    case Archivments.Score:
                        return profile.EverScore >= archivMax[archiv];

                    case Archivments.PerSecond:
                        return profile.ScorePerSecond >= archivMax[archiv];

                    case Archivments.Time:
                        return profile.time / 3600 >= archivMax[archiv].ToLong();

                    case Archivments.Wasted:
                        return profile.EverScore - profile.Score >= archivMax[archiv];

                    case Archivments.BlueScreen:
                        return profile.BlueScreenCount >= archivMax[archiv].ToLong();

                    case Archivments.Archivs:
                        return progressSum / JsonParser.getAllArchCount() >= archivMax[archiv].ToLong();
                }
            return false;
        }
        public void WaitForArchivment(ProfileData profile)
        {
            if (lvlCond.Condition() && !lvlCond.isHandle)
            {
                Debug.Log(lvlCond.lvl);
                lvlCond.isHandle = true;
                GameNotifyHandler.putNotify(new TalkLore(lvlCond.lvl));
            }
            foreach (Archivments archiv in Enum.GetValues(typeof(Archivments)))
                if (isProgress(archiv, profile))
                {
                    Debug.Log(archiv);
                    doneArchiv.Add(new Archivka(archiv, archivProg[archiv]));
                    archivProg[archiv]++;
                    archivMax[archiv] = JsonParser.getArchivValue(GetCategoryArchivName(archiv), archivProg[archiv]);
                    progressSum++;
                }
        }
        //Открытие нового уровня
        //Оно происходит после вывода текста лора 
        public int LastLvl() => lvlCond.lvl;
        public void OpenNewLvl()
        {
            int openedLvl = lvlCond.lvl + 1;
            if (openedLvl == JsonParser.getLvlCount(JsonParser.lvlType.level) - 1)
                lvlCond = new LastLevel(openedLvl);
            else
                lvlCond = new NoneCondition(openedLvl);
        }
        //дает имя категории по ее ID
        public static string GetCategoryArchivName(int arch)
        {
            if (arch == 6)
                return "Levels";
            return GetCategoryArchivName((Archivments)arch);
        }
        public static string GetCategoryArchivName(Archivments arch)
        {
            switch (arch)
            {
                case Archivments.Score:
                    return "Score";

                case Archivments.PerSecond:
                    return "Second";

                case Archivments.Time:
                    return "Time";

                case Archivments.Wasted:
                    return "Wasted";

                case Archivments.BlueScreen:
                    return "BlueScreen";

                case Archivments.Archivs:
                    return "Archivs";
                default:
                    return "Levels";
            }
        }
    }
} 
