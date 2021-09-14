using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using System.Collections.Generic;
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
    public class ArchivSystem
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
            public abstract bool Condition(ProfileData profile);
        }
        [System.Serializable]
        protected class FirstLevel : LevelCondition
        {
            public FirstLevel() : base(0)
            {

            }

            public override bool Condition(ProfileData profile)
            {
                return true;
            }
        }
        [System.Serializable]
        protected class NextLevel : LevelCondition
        {
            public NextLevel(int lvl) : base(lvl)
            {
            }

            public override bool Condition(ProfileData profile)
            {
                return false;
            }
        }
        [System.Serializable]
        protected class StoryEnd : LevelCondition
        {
            XXLNum condition;
            public StoryEnd(XXLNum final) : base(0)
            {
                condition = final;
                lvl = JsonParser.getLvlCount(JsonParser.lvlType.level) + 1;
            }
            public override bool Condition(ProfileData profile)
            {
                return profile.Score > condition;
            }
        }
        [System.Serializable]
        protected class NoneCondition : LevelCondition
        {
            public NoneCondition(int lvl) : base(lvl)
            {

            }
            public override bool Condition(ProfileData profile)
            {
                lvl = 3;
                return false;
            }
        }
        //archivments progress by type
        private Dictionary<Archivments, int> archivProg;
        private Dictionary<Archivments, XXLNum> archivMax;

        private bool[] isOpen;
        LevelCondition lvlCond;
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
        public ArchivSystem()
        {
            archivProg = new Dictionary<Archivments, int>();
            archivMax = new Dictionary<Archivments, XXLNum>();
            lvlCond = new FirstLevel();
            isOpen = new bool[6];
            doneArchiv = new List<Archivka>();
            foreach (Archivments archiv in Enum.GetValues(typeof(Archivments)))
            {
                archivProg.Add(archiv, 0);
                archivMax[archiv] = JsonParser.getArchivValue(GetCategoryArchivName(archiv), 0);
                isOpen[(int)archiv] = true;
            }
        }
        //complete archiv and return previous progress
        public void archivmentDone(Archivka arch)
        { 
            archivProg[arch.archType] += 1;
            archivMax[arch.archType] = JsonParser.getArchivValue(GetCategoryArchivName(arch.archType), archivProg[arch.archType]);

            isOpen[(int)arch.archType] = true;
            doneArchiv.Remove(arch);
        }
        //get last get archivment
        public int getArchivProgress(Archivments archiv) => archivProg[archiv];
        //get sum of all progress
        public int GetSumAllProgress()
        {
            int sum = 0;
            foreach (Archivments archiv in Enum.GetValues(typeof(Archivments)))
                sum += archivProg[archiv];
            return sum;
        }
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
                        return GetSumAllProgress() / JsonParser.getAllArchCount() >= archivMax[archiv].ToLong();
                }
            return false;
        }
        public void WaitForArchivment(ProfileData profile)
        {
            if (lvlCond.Condition(profile))
            {
                GameNotifyHandler.putNotify(new TalkLore(lvlCond.lvl));
            }
            foreach (Archivments archiv in Enum.GetValues(typeof(Archivments)))
                if (isOpen[(int)archiv])
                    if (isProgress(archiv, profile))
                    {
                        doneArchiv.Add(new Archivka(archiv, archivProg[archiv]));
                        isOpen[(int)archiv] = false;
                    }
        }
        //Открытие нового уровня
        //Оно происходит после вывода текста лора
        public void NewLvlOpen()
        {
            if (lvlCond.GetType() == typeof(StoryEnd))
                lvlCond = new NoneCondition(lvlCond.lvl);
            else if (JsonParser.getLvlCount(JsonParser.lvlType.level) - lvlCond.lvl - 1 == 0)
                lvlCond = new StoryEnd(JsonParser.getLvlCost(lvlCond.lvl + 1));
            else
                lvlCond = new NextLevel(lvlCond.lvl + 1);
        }
        public int LastLvl() => lvlCond.lvl;
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

