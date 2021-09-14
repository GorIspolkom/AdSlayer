using Clicker.Models;
using System.Collections;
using UnityEngine;

namespace Clicker.Scrypts
{
    class SpawnerBug : MonoBehaviour
    {
        public GameObject adPattern;
        private Transform[] ArrayOfSpawnPoint;
        public float SpawnCoolDownTime;
        public bool isClick;
        void Start()
        {
            ArrayOfSpawnPoint = GameObject.Find("Points").GetComponentsInChildren<Transform>();
            SpawnCoolDownTime /= 100;
            StartCoroutine(BugSpawn());
        }
        public void Update()
        {
            isClick = Values.data.clickPerSecond != 0;
        }
        private IEnumerator BugSpawn()
        {
            while (true)
            {
                if (isClick)
                {
                    yield return new WaitForSeconds(SpawnCoolDownTime);
                    int RandomSpawnPoint = Random.Range(0, ArrayOfSpawnPoint.Length);
                    GameObject newAd = Instantiate(adPattern, ArrayOfSpawnPoint[RandomSpawnPoint].position,
                        Quaternion.identity, transform);
                    Destroy(newAd, 0.5f);
                }
                yield return null;
            }
        }
    }
}
