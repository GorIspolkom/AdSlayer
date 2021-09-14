using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Clicker.HandlerSystem;

namespace Clicker.Scrypts
{
    public class SpawnerRealAdd : MonoBehaviour
    {
        public GameObject adPattern;
        private Transform[] ArrayOfSpawnPoint;
        public float SpawnCoolDownTime;
        void Start()
        {
            ArrayOfSpawnPoint = GameObject.Find("Points").GetComponentsInChildren<Transform>(); 
            StartCoroutine(SpawnCoolDown());
        }
        private IEnumerator SpawnCoolDown()
        {
            while (true)
            {
                yield return new WaitForSeconds(SpawnCoolDownTime);
                int RandomSpawnPoint = Random.Range(0, ArrayOfSpawnPoint.Length);
                GameObject newAd = Instantiate(adPattern, ArrayOfSpawnPoint[RandomSpawnPoint].position,
                    Quaternion.identity, transform);
                newAd.GetComponent<Button>().onClick.AddListener(() => DestroyAdd(newAd));
                if (Random.value <= 0.2)
                    Destroy(newAd, 0.5f);
            }
        }
        public void DestroyAdd(GameObject add)
        {
            GameNotifyHandler.putNotify(new DestroyAd());
            Destroy(add.gameObject);
        }
    }
}