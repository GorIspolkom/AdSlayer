using MyUtile.JsonWorker;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.DetachedScrypts
{
    public class Localizate : MonoBehaviour
    {
        void Start()
        {
            Translate();
        }
        public virtual void Translate()
        {
            GetComponent<Text>().text = JsonParser.getLocaliz(name);
        }
    }
}