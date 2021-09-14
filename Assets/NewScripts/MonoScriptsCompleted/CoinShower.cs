using Clicker.DetachedScrypts;
using Clicker.Models;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class CoinShower : Localizate
    {
        public Text score;
        //обновляет значения 
        private void Update( )
        {
            score.text = Values.profile.coins.ToString();
        }
        //вызывается при переходе на другой язык
        public override void Translate()
        {

        }
    }
}
