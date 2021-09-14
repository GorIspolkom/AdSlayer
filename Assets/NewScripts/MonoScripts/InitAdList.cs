using MyUtile.JsonWorker;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class InitAdList : MonoBehaviour
    {
        public GameObject ad_prefab;
        public GameObject AdDeleter;
        public void initContent(float scrollbar_pos = 0)
        {
            Init(scrollbar_pos);
        }

        private void creatReabilitationButton(GameObject ad_slot, int ID, Sprite sprite, string text)
        {
            ad_slot.transform.Find("DeleteBtn").GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/reabilit");
            ad_slot.transform.Find("DeleteBtn").GetComponent<Button>().onClick.RemoveAllListeners();
            ad_slot.transform.Find("DeleteBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                JsonIniter.reAbilitation(ID);
                creatDeleteButton(ad_slot, ID, sprite, text);
            });
        }

        private void creatDeleteButton(GameObject ad_slot, int ID, Sprite sprite, string text)
        {
            ad_slot.transform.Find("DeleteBtn").GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/btn_del");
            ad_slot.transform.Find("DeleteBtn").GetComponent<Button>().onClick.RemoveAllListeners();
            ad_slot.transform.Find("DeleteBtn").GetComponent<Button>().onClick.AddListener(() =>
            {
                var deleter = Instantiate(AdDeleter, GameObject.Find("Canvas").transform);
                deleter.transform.Find("DeleteText").Find("DeleteAD").GetComponent<Text>().text = 
                                JsonParser.getLocaliz("DeleteAD").Replace("%s", text);
                deleter.transform.Find("Image").GetComponent<Image>().sprite = sprite;
                deleter.transform.Find("Yes").GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (JsonIniter.deletAd(ID))
                        creatReabilitationButton(ad_slot, ID, sprite, text);
                    else
                        Destroy(ad_slot);
                    Destroy(deleter);
                });
            });
        }

        void Init(float scrollbar_pos)
        {
            foreach (Transform child in gameObject.transform)
            {
                Destroy(child.gameObject);
            }
            int size = JsonIniter.GetLength();
            Sprite[] sprites = JsonIniter.GetSprites();
            string[] Texts = JsonIniter.GetTexts();
            for (int id = 0; id != size; id++)
            {
                var ad_slot = Instantiate(ad_prefab, transform);
                ad_slot.transform.GetComponentInChildren<Text>().text = Texts[id];
                ad_slot.transform.Find("Image").GetComponentInChildren<Image>().sprite = sprites[id];
                int i = id;
                ad_slot.GetComponent<Button>().onClick.AddListener(() => {
                    GameObject.Find("ad_prefab").GetComponent<InitEditor>().initAdPrefab(i, sprites[i], Texts[i]);
                    Destroy(GameObject.Find("Selector"));
                });
                if (!JsonIniter.isActive(i))
                {
                    creatReabilitationButton(ad_slot, i, sprites[i], Texts[i]);
                }
                else
                {
                    creatDeleteButton(ad_slot, i, sprites[i], Texts[i]);
                }
            }
            (transform as RectTransform).anchoredPosition = new Vector2(0, scrollbar_pos);
        }
        private void Start()
        {
            initContent();
        }
    }
}