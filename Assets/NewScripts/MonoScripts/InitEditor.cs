using Clicker.Advertisment;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class InitEditor : MonoBehaviour
    {
        public Transform Levels;
        [NonSerialized]
        public string text;
        [NonSerialized]
        public string path;
        [NonSerialized]
        public int id;
        void Start()
        {
            path = "";
            text = "";
            id = -1;
            GameObject.Find("BG").GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/" + Values.data.pack + "." + Values.data.lvl);
            for (int lvl = 0; lvl != Levels.childCount - 1; lvl++)
            {
                string path = Values.data.pack + "." + lvl;
                Levels.GetChild(lvl).GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/" + path);
                int lvlID = lvl;
                if (lvlID < Values.profile.lastLvl)
                    Levels.GetChild(lvl).Find("isOpen").gameObject.SetActive(false);
                else
                    Levels.GetChild(lvl).Find("isOpen").GetComponentInChildren<Text>().text = JsonParser.getLocaliz("isOpen");
                Levels.GetChild(lvl).GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (lvlID != Values.data.lvl)
                        if (lvlID < Values.profile.lastLvl)
                        {
                            GameData.data.lvl = lvlID;
                            JsonIniter.InitClickers(lvlID);
                            GameObject.Find("BG").GetComponent<Image>().sprite = Resources.Load<Sprite>("BGpic/" + path);
                            try { GameObject.Find("Selector").GetComponentInChildren<InitAdList>().initContent(); }
                            catch { }
                        }
                });
            }
            if(JsonParser.getLvlCount(JsonParser.lvlType.level) == 2)
                Levels.GetChild(2).gameObject.SetActive(false);
        }
        public void PickImageFromGallery()
        {
            NativeGallery.Permission permission = NativeGallery.CheckPermission();
            if (permission != NativeGallery.Permission.Granted)
            {
                Debug.Log(permission);
                permission = NativeGallery.RequestPermission();
            }
            permission = NativeGallery.GetImageFromGallery((path) =>
            {
                Debug.Log("Image path: " + path);
                if (path != null)
                {
                    this.path = path;
                    var texture = new Texture2D(50, 50);
                    texture.LoadImage(System.IO.File.ReadAllBytes(path));
                    transform.Find("Image").GetComponent<Image>().sprite = 
                            Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    id = -1;
                }
            }, "Select a Clickbait background", "image/png");
            Debug.Log(permission);
        }
        public void initAdPrefab(int id, Sprite sprite, string text)
        {
            this.id = id;
            path = "";
            transform.Find("Image").GetComponent<Image>().sprite = sprite;
            transform.Find("TextTable").GetComponent<InputField>().text = text;
        }
        public void setTextInEditor(InputField field)
        {
            text = field.text;
        }
        public void Save()
        {
            JsonIniter.saveAd(text, path, id);
            GameObject.FindGameObjectWithTag("Controller").GetComponent<Advert>().ShowAdWithReward(XXLNum.zero, XXLNum.zero, "video");
        }
    }
}