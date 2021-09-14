using Clicker.GameSystem;
using Clicker.Models;
using Clicker.Scrypts;
using UnityEngine;

namespace Clicker.HandlerSystem
{
    public enum BuffType
    {
        perClick,
        perSecond
    }
    //Notify classes
    //Уведомления без подтверждения или выбора, то есть их задача изменить Values
    public class Notify
    {
        public delegate void doAction(ref ProfileData profile);
        protected event doAction action;
        protected string message;
        public Notify() => action += ((ref ProfileData profile) => getLogMessage());
        public void complite(ref ProfileData profile)
        {
            action?.Invoke(ref profile);
        }
        public void getLogMessage()
        {
            if(message != null)
                Debug.Log(message);
        }
        public static Notify operator +(Notify not1, Notify not2)
        { 
            not1.action += not2.action;
            return not1;
        }
    }
    public class AddToProfile : Notify
    {
        public AddToProfile(XXLNum add, ProfileData.ProfilicChart chart)
        {
            switch (chart)
            {
                case ProfileData.ProfilicChart.Score:
                    message = $"Игрок получил {add} Score";
                    action += delegate (ref ProfileData profile)
                    {
                        profile.AddScore(add);
                    };
                    break;
                case ProfileData.ProfilicChart.ScorePerSecond:
                    message = $"Игрок получил {add} ScorePerSecond";
                    action += delegate (ref ProfileData profile)
                    {
                        profile.AddScorePerSecond(add);
                    };
                    break;
            }
        }
    }
    public class TakeScore : Notify
    {
        public TakeScore(XXLNum add)
        {
            message = $"Игрок получил {add} Score";
            action += delegate (ref ProfileData profile)
            {
                profile.AddScore(add);
            };
        }
    }
    public class ClickAction : Notify
    {
        public ClickAction()
        {
            message = null;
            action += delegate (ref ProfileData profile)
            {
                XXLNum add = profile.GetScorePerClickInBuff();
                profile.Click++;
                profile.AddScore(add);
                GameNotifyHandler.putNotify(new ShootScore(add));
            };
        }
    }
    public class TakeBuff : Notify
    {
        public TakeBuff(BuffType type, int buff, long time)
        {
            switch (type)
            {
                case BuffType.perClick:
                    message = $"Получен бафф x{buff} на скор за |-КЛИК-| на {time}c";
                    action += delegate (ref ProfileData profile)
                    {
                        profile.AddClickBuff(buff, time);
                    };
                    break;
                case BuffType.perSecond:
                    message = $"Получен бафф x{buff} на скор за |-СЕКУНДУ-| на {time}c";
                    action += delegate (ref ProfileData profile)
                    {
                        profile.AddTimerBuff(buff, time);
                    };
                    break;
            }
        }
    }
    public class GoToExtraScene : Notify
    {
        public GoToExtraScene()
        {
            message = $"Выпал синий экран!";
            action += delegate (ref ProfileData profile)
            {
                profile.BlueScreenCount++;
                GameNotifyHandler.putNotify(new LoadScene(2));
                GameNotifyHandler.putNotify(new ActivateScene());
            };
        }
    }
    public class Purchase : Notify
    {
        public Purchase(XXLNum cost, XXLNum perAdd, int id, int add)
        {
            message = $"Приобретен апгрейд {id}";
            action += delegate (ref ProfileData profile)
            {
                if (profile.Score > cost)
                {
                    profile.Grade(id, add);
                    profile.AddScorePerSecond(perAdd);
                    profile.AddScore(-cost);
                    GameNotifyHandler.putNotify(new ActivateScene());
                }
            };
        }
    }
    public class ArchivDone : Notify
    {
        public ArchivDone(Archivka archiv)
        {
            message = $"Ачивка категории {archiv.archType}";
            action += delegate (ref ProfileData profile)
            {
                profile.PopArchiv(archiv);
            };
        }
    }
    public class LevelPurch : Notify
    {
        public LevelPurch()
        {
            message = $"Открыт новый уровень: {Values.profile.lastLvl + 1}";
            action += delegate (ref ProfileData profile)
            {
                profile.openLvl();
            };
        }
    }
    public class VoidNotify : Notify
    {
        public VoidNotify()
        {
            message = null;
        }
    } 
    public class ResetValues : Notify
    {
        public ResetValues()
        {
            action += delegate (ref ProfileData profile) { profile = new ProfileData(); };
            message = "ОБНУЛЕНИЕ!!!";
        }
    }
    public class Timer : Notify
    {
        public Timer()
        {
            action += delegate (ref ProfileData profile) {
                profile.Timer();
                SaveSystem.SaveSystem2.OldSaveValues(false);
            };
            message = null;
        }
    }
    public class Save : Notify
    {
        public Save(bool isEthernet, bool isEnd)
        {
            action += delegate (ref ProfileData profile) {
                SaveSystem.SaveSystem2.OldSaveValues(isEthernet, isEnd);
            };
            message = "Save is Done";
        }
    }
    public class TakeCoins : Notify
    {
        public TakeCoins(int coin)
        {
            action += delegate (ref ProfileData profile) { profile.coins += coin; };
            message = $"получено {coin} коинов";
        }
    }
    public class TakeBuster : Notify
    {
        public TakeBuster(Buster bust, int cost)
        {
            action += delegate (ref ProfileData profile)
            {
                profile.PutBust(bust);
                profile.coins -= cost;
            };
            message = $"получен буст {bust.GetType()}";
        }
    }
}

