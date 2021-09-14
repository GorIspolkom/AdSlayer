using Clicker.HandlerSystem;
using UnityEngine;

namespace Clicker.DetachedScrypts
{
    class PanelSpawner : MonoBehaviour
    {
        public void LoadPanel(string name) => GameNotifyHandler.putNotify(new LoadPanel(name));
        public void ClosePanel(string name) => ClosePanel(GameObject.Find(name));
        public void ClosePanel(GameObject panel) => GameNotifyHandler.putNotify(new ClosePanel(panel));

        public void LoadScene(int scene) {
            GameNotifyHandler.putNotify(new LoadScene(scene));
            GameNotifyHandler.putNotify(new ActivateScene());
        }
        public void SaveEditor()
        {
            GameNotifyHandler.putNotify(new SaveEditor());
        }
    }
}
