using MyUtile.JsonWorker;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class AdIniter : MonoBehaviour
    {
        private void Start()
        {
            int x = Random.Range(0, JsonIniter.GetSize());
            Publish(x);
        }

        public void Publish(int rr)
        {
            transform.Find("Image").GetComponent<Image>().sprite = JsonIniter.GetSprite(rr);
            if (JsonIniter.isText)
            {
                string text = JsonIniter.GetText(rr);
                Text message = GetComponentInChildren<Text>();
                message.text = text;
                if (text.Equals(""))
                    transform.Find("TextTable").GetComponent<Image>().color = new Color(0, 0, 0, 0);
                else
                    transform.Find("TextTable").GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            }
        }
    }
}
