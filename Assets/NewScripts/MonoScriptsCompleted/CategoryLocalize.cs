using Clicker.DetachedScrypts;
using MyUtile.JsonWorker;
using UnityEngine;
using UnityEngine.UI;
using static Clicker.Scrypts.InfoPanel;

namespace Clicker.Scrypts
{
    class CategoryLocalize : Localizate
    {
        public override void Translate()
        {
            foreach (Transform category in transform)
            {
                switch (category.name)
                {
                    case "Shop":
                        category.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Grades");
                        break;
                    case "Busters":
                        category.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Busters");
                        break;
                    case "Levels":
                        category.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Levels");
                        break;
                    case "Packs":
                        category.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Pack");
                        break;
                }
            }
        }
    }
}
