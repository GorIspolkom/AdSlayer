using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class FlyScore : MonoBehaviour
    {
        public float dict;
        Vector2 dictionary;
        void Start()
        {
            GetComponent<Text>().color = new Color(Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f), Random.Range(0.3f, 0.9f), 1);
            dictionary = new Vector2(Random.Range(-dict, dict), Random.Range(0, dict));
        }
        public void setAdd(string add)
        {
            GetComponent<Text>().text = "+" + add;
        }

        void Update()
        {
            transform.Translate(dictionary * Time.deltaTime);
        }
    }
}