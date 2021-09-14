using Clicker.HandlerSystem;
using Clicker.Models;
using System.Collections;
using UnityEngine;


namespace Clicker.Scrypts
{
    public class BlueLoad : MonoBehaviour
    {
        public RectTransform loader;
        private float timer;
        private XXLNum Score;

        public void init(XXLNum score)
        {
            timer = 0;
            Score = score;
            gameObject.SetActive(true);
            StartCoroutine(LoadScene());
        }
        private void Update()
        {
            timer += Time.deltaTime;
        }
        private IEnumerator LoadScene()
        {
            GameNotifyHandler.putNotify(new LoadScene(1));
            while (timer < 4f)
            {
                loader.Rotate(new Vector3(0, 0, -3));
                yield return null;
            }
            ValuesNotifyHandle.putNotify(new TakeCoins(Random.Range(2, 6)));
            GameNotifyHandler.putNotify(new ActivateScene(), true);
            GameNotifyHandler.putNotify(new ScoreBuffPanel(Score), true);
        }
        void OnDestroy()
        {
            GameObject.FindGameObjectWithTag("Controller").GetComponent<AudioSource>().UnPause();
        }
    }
}