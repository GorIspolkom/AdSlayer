using Clicker.HandlerSystem;
using System;
using System.Collections.Generic;

namespace Clicker.GameSystem
{
    /// <summary>
    /// хранит в себе все баффы
    /// обрабатывает их каждый игровой тик
    /// </summary>
    //общий вид системы баффов
    [Serializable]
    public class BuffSystem
    {
        //представление баффа
        [Serializable]
        public class buff
        {
            //значение баффа
            public int buffVal;
            //время существования в секундах
            public long timer;
            //инит
            public buff(int buffVal, long timer)
            {
                this.buffVal = buffVal;
                this.timer = timer;
            }
            //вычет одной секунды
            public void Tick() => timer--; 
        }
        //лист всех баффов
        public List<buff> buffs;
        public float prestigeBuff;
        //первая инициализация
        public BuffSystem()
        {
            buffs = new List<buff>();
            prestigeBuff = 0;
        }
        //добавления баффа
        public void Add(int buffVal, long timer)
        {
            buffs.Add(new buff(buffVal, timer));
            GameNotifyHandler.putNotify(new BuffShowUpdate());
        }
        //возвращает общий множитель от всех баффов
        public virtual int GetBuff()
        {
            int _buff = 1;
            if (buffs.Count != 0)
                foreach (buff b in buffs)
                    _buff += b.buffVal;
            return _buff < 15 ? _buff : 15;
        }
        //обработка времени каждый игровой тик
        public void MinusTime(long seconds)
        {
            if (buffs.Count != 0)
                foreach (buff b in buffs.ToArray())
                {
                    b.timer -= (int)seconds;
                    if (b.timer <= seconds)
                        buffs.Remove(b);
                }
        }
        //проверка на валидатность баффа. Удаляется если timer < 0
        public void CheckValidBuff()
        {
            if (buffs.Count != 0)
                foreach (buff b in buffs.ToArray())
                {
                    b.Tick();
                    if (b.timer <= 0)
                    {
                        GameNotifyHandler.putNotify(new BuffShowUpdate());
                        buffs.Remove(b);
                    }
                }
        }
        public float GetPrestigedBuff() => GetBuff() + prestigeBuff;
        public void PrestigedBuff(int prestige) => prestigeBuff = UnityEngine.Mathf.Log(prestige + 1)/3f;
        public bool isAbleTake() => GetBuff() <= 10;
    }
    //рпсширение бафф системы для клика
    [Serializable]
    public class ClickBuffSystem : BuffSystem
    {
        //бафф, который зависит от количества кликов в секунду
        public int BuffOfClickRate;
        //по сути это количество игровых тиков, в которых превосходится число delta
        //метод описан в DeltaCounter
        public int counter;
        //инит системы
        public ClickBuffSystem() : base() { BuffOfClickRate = 1; }
        //общий множитель системы
        public override int GetBuff()
        {
            int _buff = BuffOfClickRate;
            if (buffs.Count != 0)
                foreach (buff b in buffs)
                    _buff += b.buffVal;
            return _buff < 15 ? _buff : 15;
        }
        //рассчитывает counter от кликов за прошедший игровой тик
        public void DeltaCounter(int delta)
        {
            //Debug.Log("Delta = " + delta);
            //условия повышения количества тиков
            switch (BuffOfClickRate)
            {
                case 1:
                    if (delta >= 3)
                        counter++;
                    else if (counter > 0)
                        counter--;
                    break;
                case 2:
                    if (delta >= 8)
                        counter++;
                    else
                        counter--;
                    break;
                case 3:
                    if (delta >= 14)
                        counter++;
                    else
                        counter--;
                    break;
                case 4:
                    if (delta >= 20)
                        counter++;
                    else
                        counter--;
                    break;
                case 5:
                    if (delta >= 30)
                        counter++;
                    else
                        counter--;
                    break;
            }
            if (delta == 0 && counter > 1)
                counter -= 2;
            //определение BuffOfClickRate от counter
            BuffByCounter();
        }
        //change buff by counter
        private void BuffByCounter()
        {
            if (counter >= 9)
            {
                BuffOfClickRate = 5;
                counter = counter >= 11 ? 11 : counter;
            }
            else if (counter >= 7)
                BuffOfClickRate = 4;
            else if (counter >= 4)
                BuffOfClickRate = 3;
            else if (counter >= 2)
                BuffOfClickRate = 2;
            else
                BuffOfClickRate = 1;
        }
        //обнулние баффов, зависящих от кликов
        public void ResetBuff()
        {
            BuffOfClickRate = 1;
            counter = 0;
        }
        //дает количество баффов
        public int GetBuffCount() => buffs.Count;
    }
}
