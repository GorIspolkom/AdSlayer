using Clicker.Models;
using UnityEngine;

//скрипт который испускает частицу с количеством добавленного скора после клика
namespace Clicker.Scrypts
{
    public class BulletContainer : MonoBehaviour
    {
        //частица
        public GameObject click_prefab;
        //Родитель всех частиц
        public RectTransform bulletParent;
        //метод спавна
        public void ShootScore(XXLNum ScoreAdd)
        {
            //инит частицы
            GameObject clickscores = Instantiate(click_prefab, GetComponent<RectTransform>().position, Quaternion.identity, bulletParent);
            //конвертирует, устанавливает значение и спавнит
            clickscores.GetComponent<FlyScore>().setAdd(ScoreAdd.ToString());
            //уничтожение через 1 секунду
            Destroy(clickscores, 1f);
        }
    }
}