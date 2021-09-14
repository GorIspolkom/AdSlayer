using Clicker.GameSystem;
using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Clicker.InitScrypts
{
    class ArchivIniter : MonoBehaviour
    {
        public GameObject archivment;
        public GameObject categoryBtn;
        public Transform content;
        public Transform categoryTrans;
        
        private int archID;

        private void Start()
        {
            archID = -1;
            ChoicesIniter();
            init(0);

            RectTransform rect = transform as RectTransform;
            rect.anchoredPosition = new Vector2(0, rect.rect.height);
            LeanTween.moveY(rect, 0, 0.3f).setEaseInOutBack();
        }

        public void ChoicesIniter()
        {
            foreach (Transform child in categoryTrans)
            {
                Destroy(child.gameObject);
            }
            int size = JsonParser.getArchivmentsCount();
            for (int i = 0; i != size; i++)
            {
                GameObject btn = Instantiate(categoryBtn, categoryTrans); 
                btn.transform.GetComponentInChildren<Text>().text = JsonParser.getLocaliz(ArchivmentSystem.GetCategoryArchivName(i));
                int id = i;
                btn.GetComponent<Button>().onClick.AddListener(() => init(id));
            }
            categoryTrans.localPosition = new Vector2(0, 0);
        }
        public void init(int id)
        {
            if (archID != id)
            {
                archID = id;
                foreach (Transform child in content)
                {
                    Destroy(child.gameObject);
                }
                int last;
                if (id == 6)
                    last = Values.profile.lastLvl;
                else
                    last = Values.profile.GetArchivProgress(id);
                string category = ArchivmentSystem.GetCategoryArchivName(id);
                int size = JsonParser.getArchivCount(category); 
                for (int i = 0; i != size; i++)
                {
                    GameObject archiv = Instantiate(archivment, content);
                    archiv.transform.Find("Lvl").GetComponent<Text>().text = (i + 1).ToString();
                    string text = JsonParser.getArchivText(category, i);
                    if (i < last)
                    {
                        archiv.transform.Find("Text").GetComponent<Text>().text = text;
                        archiv.transform.Find("isDone").GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/done");
                        if (id == 6)
                        {
                            int lvl = i;
                            archiv.AddComponent<Button>().onClick.AddListener(() =>
                            {
                                GameNotifyHandler.putNotify(new TalkLore(lvl, false));
                                Destroy(GameObject.Find("ArchivsPanel"));
                            });
                        }
                    }
                    else
                    {
                        char[] textChar = text.ToCharArray();
                        for (int j = 0; j != textChar.Length; j++)
                            if (textChar[j] != ' ') textChar[j] = '?';
                        archiv.transform.Find("Text").GetComponent<Text>().text = new string(textChar);
                        archiv.transform.Find("isDone").GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/wait");
                    }
                }
                if (id == 6)
                {
                    GameObject addLevel = Instantiate(archivment, content);
                    addLevel.transform.Find("Lvl").GetComponent<Text>().text = (size + 1).ToString();
                    addLevel.transform.Find("Text").GetComponent<Text>().text = JsonParser.getLocaliz("Soon");
                    addLevel.transform.Find("isDone").GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/wait");
                }
                content.localPosition = new Vector2(0, 0);
            }
        } 
    }
}