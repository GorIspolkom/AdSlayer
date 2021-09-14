using Clicker.HandlerSystem;
using System.Collections.Generic;

namespace Clicker.Models
{
    [System.Serializable]
    public abstract class Buster
    {
        public abstract void init();
        public abstract bool condition();
        public abstract void end();
    }
    [System.Serializable]
    public class GetMomentalScore : Buster
    {
        int val;
        public GetMomentalScore(int val) => this.val = val;
        public override bool condition()
        {
            return true;
        }

        public override void end()
        {

        }

        public override void init()
        {
            ValuesNotifyHandle.putNotify(new TakeScore(Values.profile.ScorePerSecond * val));
        }
    }
    [System.Serializable]
    public class AutoClicker : Buster
    {
        long time;
        int rate;
        public AutoClicker(long time, int rate)
        {
            this.time = time;
            this.rate = rate;
        }
        public override bool condition()
        {
            time--;
            UnityEngine.Debug.Log(time);
            return time > 0;
        }

        public override void end()
        {
            GameData.data.deltaClick = 1;
            GameData.data.isAutoClick = false;
        }

        public override void init()
        {
            GameData.data.deltaClick = rate;
            GameData.data.isAutoClick = true;
        }
    }
    [System.Serializable]
    class BustSystem
    {
        private List<Buster> busters;
        public BustSystem()
        {
            busters = new List<Buster>();
        }
        public void Update()
        {
            if(busters.Count != 0) 
                foreach(Buster bust in busters.ToArray())
                    if (!bust.condition())
                    {
                        bust.end();
                        busters.Remove(bust);
                    } 
        }
        public void AddBust(Buster bust)
        {
            bust.init();
            busters.Add(bust);
        }
    }
}
