using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// скрипт, который пускает анимацию как слайд-шоу
/// не нашел инструментов в unity для этого
/// </summary>
namespace Clicker.DetachedScrypts
{
    public class SpriteChanger : MonoBehaviour
    {
        //кадры анимации
        public Sprite[] frames;
        //время между слайдами
        public float TimePerFrame;
        //время с создания
        private float ExistTime;

        void Start()
        {
            ExistTime = 0;
            TimePerFrame /= 10;
        }

        void Update()
        {
            ExistTime += Time.deltaTime;
            int index = (int)(ExistTime / TimePerFrame) % frames.Length;
            GetComponent<Image>().sprite = frames[index];
        }
    }
}