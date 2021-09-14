using Clicker.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;

namespace MyUtile.JsonWorker
{
    public static class JsonParser
    {
        //загруженные json файлы
        public static JObject localization = JObject.Parse(getRoot("localization", false));
        public static JObject prefabs;
        //дает весь текст json файла
        private static string ReadFile(string filePath)
        {
            WWW www = new WWW(filePath);
            while (!www.isDone) { }
            return www.text;
        }
        public static string getRoot(string name, bool isEditable)
        {
            string json;
            //если это версия для тестов, то он сохраняет копию для редактирования
            if (isEditable)
            {
                //грузит из файлов игры файл по имени
                string tempPath = Application.persistentDataPath + "/Docs/" + name + ".json";
                if (File.Exists(tempPath))
                {
                    json = File.ReadAllText(tempPath);
                }
                //грузит файл и делает копию в директорию с data 
                else
                {
                    string filePath = Application.streamingAssetsPath + "/Docs/" + name + ".json";
                    json = ReadFile(filePath);
                    if (!Directory.Exists(Application.persistentDataPath + "/Docs"))
                        Directory.CreateDirectory(Application.persistentDataPath + "/Docs");
                    File.WriteAllText(tempPath, json);
                }
            }
            //достает файл из корня игры
            else
            {
                json = ReadFile(Application.streamingAssetsPath + "/Docs/" + name + ".json");
            }
            return json;
        }
        //грузит всю активную рекламу пака и добавляем им тег активности
        private static void AddModule(ref JArray ads, JArray pack)
        {
            JArray PackOfAd = new JArray(); 
            for (int j = 0; j < pack.Count; j++)
            {
                JArray level = (JArray)pack[j]["ads"];
                for (int k = 0; k < level.Count; k++)
                    ((JObject)level[k]).Add("isActive", true);
                PackOfAd.Add(level);
            }
            ads.Add(PackOfAd);
        }
        //Проверяет используется ли изображение в одной из реклам
        public static bool isIn(JArray ads, string picture)
        {
            for(int i = 0; i != ads.Count; i++)
            {
                if (ads[i]["picture"].Value<string>().Equals(picture))
                    return true;
            }
            return false;
        }
        //создает json массив рекламы пака
        public static JArray GetAdModuleAndSync(int pack)
        {
            //грузит достанные ранее рекламы пака
            string tempPath = Application.persistentDataPath + "/Docs/AdModuls.json";
            if (File.Exists(tempPath))
            {
                //дает всю рекламу из сохраненного файла
                string json = File.ReadAllText(tempPath);
                //загружает файл с префабами игры
                JArray ads = JArray.Parse(json);
                string filePath = Application.streamingAssetsPath + "/Docs/adPrefabs.json";
                string packsText = ReadFile(filePath);

                //достает из файла с префабами всю рекламу пака
                JArray packs = JArray.Parse(packsText);
                for (int i = 0; i != packs.Count; i++)
                {
                    //проверка все ли паки на месте
                    if(i > ads.Count-1)
                    {
                        Debug.Log($"New pack {i}");
                        AddModule(ref ads, (JArray)packs[i]["PlayLevels"]);
                        continue;
                    }
                    //init ad lvl and prefabs lvl
                    JArray levelsAds = (JArray)ads[i];
                    JArray levels = (JArray)packs[i]["PlayLevels"];
                    for (int j = 0; j != levels.Count - 1; j++)
                    {
                        //check if is all lvl
                        if(j > levelsAds.Count - 1)
                        {
                           Debug.Log("New lvl in pack " + i + ", lvl " + j);
                           JArray Level = (JArray)levels[j];
                            for (int k = 0; k != Level.Count; k++)
                                ((JObject)Level[k]).Add("isActive", true);
                            ((JArray)ads[i]).Add(Level);
                            continue;
                        }
                        //init module ads and prefab ads
                        JArray levelAds = (JArray)levelsAds[j];
                        JArray level = (JArray)levels[j]["ads"];
                        for (int k = 0; k != level.Count; k++)
                        {
                            //check if is all ads 
                            if (!isIn(levelAds, level[k]["picture"].Value<string>()))
                            {
                                JObject ad = (JObject)level[k];
                                Debug.Log($"New Ad in pack {i}, lvl {j}, name {ad.Value<string>("picture")}");
                                ad.Add("isActive", true);
                                ((JArray)ads[i][j]).Add(ad);
                            }
                        }
                    }
                    SaveAds(ads.ToString());
                }
                return (JArray)ads[pack];
            }
            else
            {
                //пробегается по всей рекламе и добавляет ее, если она существует
                JArray Ads = new JArray();
                JArray packs = (JArray)prefabs.Root;
                for (int i = 0; i < packs.Count; i++)
                {
                    AddModule(ref Ads, (JArray)packs[i]["PlayLevels"]);
                }

                //сохраняет полученный массив
                if (!Directory.Exists(Application.persistentDataPath + "/Docs"))
                    Directory.CreateDirectory(Application.persistentDataPath + "/Docs");
                File.WriteAllText(tempPath, Ads.ToString());
                return (JArray)Ads[pack];
            }
        }
        //get text for lore 
        public static string getLore(int lvl)
        {
            string lore = "";
            JArray lores = (JArray)prefabs["Archivment"]["Levels"][lvl]["Lore"][Values.settings.lang];
            for(int i = 0; i < lores.Count; i++)
            {
                lore += lores[i].Value<string>() + " ";
            }
            return lore;
        }

        //save json to persistant path
        public static void SavePrefabsForTest()
        {
            File.WriteAllText(Application.persistentDataPath + "/Docs/adPrefabs.json", prefabs.Root.ToString());
        }
        public static void SaveAds(string ads)
        {
            File.WriteAllText(Application.persistentDataPath + "/Docs/AdModuls.json", ads);
        }
        //select pack and pack file name
        public static void SelectPack()
        {
            prefabs = (JObject)JArray.Parse(getRoot("adPrefabs", Values.data.isTest))[Values.data.pack];
            GetAdModuleAndSync(Values.data.pack);
        }
        //Purchases data
        public static XXLNum getCost(int lvl, int id)
        {
            int power = prefabs["PlayLevels"][lvl]["purchases"]["grades"][id]["CostPower"].Value<int>();
            float cost = prefabs["PlayLevels"][lvl]["purchases"]["grades"][id]["cost"].Value<int>();
            return XXLNum.calibrate(cost, power);
        }
        //add to per second score
        public static XXLNum getAdd(int lvl, int id)
        {
            int power = prefabs["PlayLevels"][lvl]["purchases"]["grades"][id]["AddPower"].Value<int>();
            float add = prefabs["PlayLevels"][lvl]["purchases"]["grades"][id]["add"].Value<float>();
            return XXLNum.calibrate(add, power);
        }
        //name of purch 
        public static string getPurchName(int id)
        {
            return prefabs["PlayLevels"][Values.data.lvl]["purchases"]["names"][Values.settings.lang][id].Value<string>();
        }
        //size of purch in lvl
        public static int getPurchasesSize(int lvl)
        {
            try
            {
                return ((JArray)prefabs["PlayLevels"][lvl]["purchases"]["grades"]).Count;
            }
            catch
            {
                return 0;
            }
        }
        //purch testing
        public static void setAdd(int add, int power, int id)
        {
            prefabs["PlayLevels"][Values.data.lvl]["purchases"]["grades"][id]["add"] = add;
            prefabs["PlayLevels"][Values.data.lvl]["purchases"]["grades"][id]["AddPower"] = power;
            SavePrefabsForTest();
        }
        public static void setCost(int cost, int power, int id)
        {
            prefabs["PlayLevels"][Values.data.lvl]["purchases"]["grades"][id]["cost"] = cost;
            prefabs["PlayLevels"][Values.data.lvl]["purchases"]["grades"][id]["CostPower"] = power;
            SavePrefabsForTest();
        }
        public static void setTranslateLvl(int val, int power, int id)
        {
            prefabs["PlayLevels"][id]["Cost"]["Value"] = val;
            prefabs["PlayLevels"][id]["Cost"]["Power"] = power;
            SavePrefabsForTest();
        }
        //localization
        public static string getLocaliz(string name)
        {
            return localization[name][Values.settings.lang].Value<string>();
        }
        public static string[] getAllValName()
        {
            JArray powNames = localization.Value<JArray>("Mantise");
            string[] names = new string[powNames.Count];
            for (int i = 0; i < powNames.Count; i++)
            {
                names[i] = powNames[i].Value<string>(Values.settings.lang);
            }
            return names;
        }
        //archivments 
        public static XXLNum getLvlCost(int lvl)
        {
            return getArchivValue("Levels", lvl);
        }
        //count of archivs in category
        public static int getArchivCount(string category)
        {
            return ((JArray)prefabs["Archivment"][category]).Count;
        }
        //get text on out put in banner
        public static string getArchivText(string category, int id)
        {
            return prefabs["Archivment"][category][id]["Text"][Values.settings.lang].Value<string>();
        }
        //get value needed to reach for archiv
        public static XXLNum getArchivValue(string category, int id)
        {
            int m = 1, p = 0;
            try { m = prefabs["Archivment"][category][id]["Value"].Value<int>(); }
            catch { }
            try { p = prefabs["Archivment"][category][id]["Power"].Value<int>(); }
            catch { }
            return XXLNum.calibrate(m, p);
        } 
        //how much types of archivs
        public static int getArchivmentsCount()
        {
            return ((JObject)prefabs["Archivment"]).Count;
        }
        //count of all arch
        public static int getAllArchCount()
        {
            int all = 0;
            var archivCategory = prefabs["Archivment"].First.First;
            while (archivCategory != null)
            {
                all += ((JArray)archivCategory).Count;
                try { archivCategory = archivCategory.Parent.Next.First; }
                catch { archivCategory = null; }
            }
            return all;
        }
        //news
        public static int getNewsCount()
        {
            return ((JArray)prefabs["News"]).Count;
        }
        public static string[] getNews()
        {
            int size = getNewsCount();
            string[] news = new string[size];
            for (int i = 0; i != size; i++)
                news[i] = prefabs["News"][i][Values.settings.lang].Value<string>();
            return news;
        }
        //For Lvl List
        public enum lvlType
        {
            level,
            scene
        }
        private static string getLvlTypeName(lvlType type)
        {
            switch (type)
            {
                case lvlType.level:
                    return "PlayLevels";
                case lvlType.scene:
                    return "PlayScene";
                default:
                    return "PlayLevels";
            }
        }
        public static int getLvlCount(lvlType type)
        {
            return ((JArray)prefabs[getLvlTypeName(type)]).Count;
        }
        public static string getLvlName(lvlType type, int lvl, int pack = -1)
        {
            pack = pack == -1 ? Values.data.pack : pack;
            return prefabs[getLvlTypeName(type)][lvl]["Name"][Values.settings.lang].Value<string>();
        }
        public static int getSceneCount(int num, int pack = -1)
        {
            pack = pack == -1 ? Values.data.pack : pack;
            return prefabs["PlayScene"][num]["Scene"].Value<int>();
        }
        //for packs
        public static int getPackCount()
        {
            return ((JArray)prefabs.Root).Count;
        }
        public static string getPackPicture(int pack)
        {
            return prefabs.Root[pack]["Path"].Value<string>();
        }
        public static string getPackNames(int pack)
        {
            return prefabs.Root[pack]["Name"][Values.settings.lang].Value<string>();
        }
        public static string GetPackSavePath(int pack)
        {
            return prefabs.Root[pack]["Name"]["Eng"].Value<string>().Replace(" ", "_");
        }
        public static int GetClickBusterCount()
        {
            return ((JArray)prefabs["Busters"]["Click"]).Count;
        }
        public static int GetTimeBusterCount()
        {
            return ((JArray)prefabs["Busters"]["Time"]).Count;
        }
        public static int GetTimeBusterVal(int id)
        {
            return prefabs["Busters"]["Time"][id]["Value"].Value<int>();
        }
        public static int GetClickBusterVal(int id)
        {
            return prefabs["Busters"]["Click"][id]["Value"].Value<int>();
        }
        public static int GetTimeBusterCost(int id)
        {
            return prefabs["Busters"]["Time"][id]["Cost"].Value<int>();
        }
        public static int GetClickBusterCost(int id)
        {
            return prefabs["Busters"]["Click"][id]["Cost"].Value<int>();
        }
        public static string GetClickBusterDesc(int id)
        {
            return prefabs["Busters"]["Click"][id]["Text"][Values.settings.lang].Value<string>();
        }
        public static string GetTimeBusterDesc(int id)
        {
            return prefabs["Busters"]["Time"][id]["Text"][Values.settings.lang].Value<string>();
        }

    }
}