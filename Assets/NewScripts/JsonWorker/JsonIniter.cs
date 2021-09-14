using Clicker.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;

namespace MyUtile.JsonWorker
{
    public static class JsonIniter
    {
        static Sprite[] sprites;
        static string[] Texts;
        public static bool isText { get => Texts.Length != 0; }
        static string user_pic_folder = Application.persistentDataPath + "/Pictures/User_Picture_";
        //создает json массив рекламы уровня
        private static JArray GetModule(int lvl)
        {
            JArray ads;
            string tempPath = Application.persistentDataPath + "/Docs/AdModuls.json";
            if (File.Exists(tempPath))
            {
                //дает всю рекламу из сохраненного файла
                string json = File.ReadAllText(tempPath);
                //загружает файл с префабами игры
                ads = (JArray)JArray.Parse(json)[Values.data.pack];
            }
            else
            {
                ads = JsonParser.GetAdModuleAndSync(Values.data.pack);
            }
            return (JArray)ads[lvl];
        }
        //ads length
        public static int GetCount()
        {
            JArray ads = GetModule(Values.data.lvl);
            int size = ads.Count;
            int count = 0;
            for (int i = 0; i != size; i++)
                if (ads[i]["local"].Value<bool>())
                {
                    if (ads[i]["isActive"].Value<bool>())
                        count++;
                }
                else
                    count++;
            return count;
        }
        //get sprite
        public static Sprite GetSprite(int id)
        {
            return sprites[id];
        }
        //get size
        public static int GetLength()
        {
            return GetModule(Values.data.lvl).Count;
        }
        public static int GetSize()
        {
            return sprites.Length;
        }
        public static bool isActive(int id)
        {
            JArray arr = GetModule(Values.data.lvl);
            if (arr[id]["local"].Value<bool>())
                return arr[id]["isActive"].Value<bool>();
            else
                return true;
        }
        //text of ad
        public static string GetText(int id)
        {
            return Texts[id];
        }
        //for editor get all
        public static Sprite[] GetSprites()
        {
            JArray ads = GetModule(Values.data.lvl);
            int size = GetLength();
            Sprite[] sprites = new Sprite[size];
            for (int i = 0; i != size; i++)
            {
                if (ads[i]["local"].Value<bool>())
                {
                    sprites[i] = Resources.Load<Sprite>("Pictures/" + ads[i]["picture"].Value<string>());
                }
                else
                {
                    var texture = new Texture2D(50, 50);
                    texture.LoadImage(File.ReadAllBytes(user_pic_folder + ads[i]["picture"].Value<string>()));
                    sprites[i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            return sprites;
        }
        public static string[] GetTexts()
        {
            JArray ads = GetModule(Values.data.lvl);
            int size = GetLength();
            string[] Texts = new string[size];
            for (int i = 0; i != size; i++)
                Texts[i] = ads[i]["text"][Values.settings.lang].Value<string>();
            return Texts;
        }
        //init array of sprites and texts of lvl + users ad
        public static void InitClickers(int lvl)
        {
            int size = GetCount();
            JArray ads = GetModule(Values.data.lvl);
            sprites = new Sprite[size];
            Texts = new string[size];
            int iterat = 0;
            for (int i = 0; i != ads.Count; i++)
            {
                if (ads[i]["local"].Value<bool>())
                {
                    if (ads[i]["isActive"].Value<bool>())
                    {
                        sprites[iterat] = Resources.Load<Sprite>("Pictures/" + ads[i]["picture"].Value<string>());
                        Texts[iterat] = ads[i]["text"][Values.settings.lang].Value<string>();
                        iterat++;
                    }
                }
                else
                {
                    var texture = new Texture2D(50, 50);
                    texture.LoadImage(File.ReadAllBytes(user_pic_folder + ads[i]["picture"].Value<string>()));
                    sprites[iterat] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                    Texts[iterat] = ads[i]["text"][Values.settings.lang].Value<string>();
                    iterat++;
                }
            }
        }
        //reinit texts of ads(used for change language)
        public static void reinitTexts()
        {
            int count = GetCount();
            string[] Texts = new string[count];
            JArray ads = GetModule(Values.data.lvl);
            for (int i = 0; i != count; i++)
                Texts[i] = ads[i]["text"][Values.settings.lang].Value<string>();
            JsonIniter.Texts = Texts;
        }
        //add and delete ad
        //change value of exists ad or create new object
        public static void saveAd(string text, string path, int id)
        {
            if (path.Equals("") && id == -1)
                return;
            JArray ads = GetModule(Values.data.lvl);
            if (id != -1)
            {
                if (ads[id]["local"].Value<bool>())
                    if (!ads[id]["isActive"].Value<bool>())
                    {
                        ads[id]["isActive"] = true;
                        JsonParser.SaveAds(ads.Root.ToString());
                    }
                ads[id]["text"][Values.settings.lang] = text;
                JsonParser.SaveAds(ads.Root.ToString());
                InitClickers(Values.data.lvl);
            }
            else
                addNewAd(text, path);
        }
        //add sprite to array
        private static void addSprite(string path, string text)
        {
            int size = sprites.Length;
            Sprite[] _sprites = new Sprite[size + 1];
            string[] _Texts = new string[size + 1];
            for (int i = 0; i != size; i++)
            {
                _sprites[i] = sprites[i];
                _Texts[i] = Texts[i];
            }
            var texture = new Texture2D(50, 50);
            texture.LoadImage(File.ReadAllBytes(path));
            _sprites[size] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            _Texts[size] = text;
            sprites = _sprites;
            Texts = _Texts;
        }
        //create new element of ad in array of ads in lvl
        public static void addNewAd(string text, string path)
        {
            JObject newAd = new JObject();
            JObject textObj = new JObject();
            //create text localiz field
            textObj.Add("Rus", "");
            textObj.Add("Eng", "");
            textObj[Values.settings.lang] = text;
            //add text to ad element
            newAd.Add("text", textObj);
            //dictionary with users pictures
            string PicFolder = Application.persistentDataPath + "/Pictures";
            if (!Directory.Exists(PicFolder))
                Directory.CreateDirectory(PicFolder);
            //save picture name
            string id = Directory.GetFiles(PicFolder).Length + path.Remove(0, path.IndexOf('.'));
            string pictureName = "User_Picture_" + id;
            newAd.Add("picture", id);
            PicFolder = PicFolder + "/" + pictureName;

            var texture = new Texture2D(50, 50);
            texture.LoadImage(File.ReadAllBytes(path));
            File.WriteAllBytes(PicFolder, texture.EncodeToJPG(25));
            newAd.Add("local", false);

            //save element of ad
            JArray ads = GetModule(Values.data.lvl);
            ads.Add(newAd);
            JsonParser.SaveAds(ads.Root.ToString());
            //load picture to sprite in sprites
            addSprite(PicFolder, text);
        }
        //delete ad
        public static bool deletAd(int id)
        {
            bool isLocal;
            JArray ads = GetModule(Values.data.lvl);
            if (ads[id]["local"].Value<bool>())
            {
                ads[id]["isActive"] = false;
                isLocal = true;
            }
            else
            {
                File.Delete(user_pic_folder + ads[id]["picture"].Value<string>());
                ads[id].Remove();
                isLocal = false;
            }
            JsonParser.SaveAds(ads.Root.ToString());
            InitClickers(Values.data.lvl);
            return isLocal;
        }
        public static void reAbilitation(int id)
        {
            JArray ads = GetModule(Values.data.lvl);
            ads[id]["isActive"] = true;
            InitClickers(Values.data.lvl);
            JsonParser.SaveAds(ads.Root.ToString());
        }

    }
}