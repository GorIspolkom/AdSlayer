using Clicker.DetachedScrypts;
using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    class InfoPanelContentIniter : InteractionSctipt
    {
        public abstract class AbstractPanelState : Localizate
        {
            private void Awake()
            {
                foreach (Transform child in transform)
                    Destroy(child.gameObject);
            }
            public void Start()
            {
                init();
                (transform as RectTransform).anchoredPosition = new Vector2((transform as RectTransform).anchoredPosition.x, 0);
            }
            public initCategory GetCategory()
            {
                switch (GetType().Name)
                {
                    case "ShopState":
                        return initCategory.shop;
                    case "BusterState":
                        return initCategory.buster;
                    case "LevelState":
                        return initCategory.levels;
                    case "PackState":
                        return initCategory.packs;
                    case "EditState":
                        return initCategory.testing;
                    default:
                        return initCategory.buster;
                }
            }
            public abstract void init();
        }
        //состояние инициализации контента панели
        public class ShopState : AbstractPanelState
        {
            private struct btn_model
            {
                public XXLNum base_cost;
                public XXLNum cost;
                private XXLNum base_add;
                public XXLNum add;
                public int progress;

                public btn_model(XXLNum base_cost, XXLNum add, int i)
                {
                    this.cost = XXLNum.zero;
                    this.add = XXLNum.zero;
                    this.base_cost = base_cost;
                    this.base_add = add;
                    progress = Values.profile.getGrade(i);
                }
                public XXLNum getCost(int prog_add, bool isGrade)
                {
                    if (isGrade) progress += prog_add;
                    cost = XXLNum.zero;
                    add = XXLNum.zero;
                    for (int i = 1; i <= prog_add; i++)
                    {
                        cost += getCurCost(i);
                        add += GetAdd(i);
                    }
                    return cost;
                }
                public int getMaxGrade(XXLNum score, int grade = 0)
                {
                    progress += grade;
                    int i = 1;
                    cost = getCurCost(i);
                    add = GetAdd(i);
                    XXLNum curCost = getCurCost(2);
                    while (cost + curCost <= score)
                    {
                        i++;
                        cost += curCost;
                        add += GetAdd(i);
                        curCost = getCurCost(i + 1);
                    }
                    return i;
                }
                public void update(int newAdd)
                {
                    cost += getCurCost(newAdd);
                    add += GetAdd(newAdd);
                }
                public XXLNum GetAdd(int addProgress)
                {
                    return base_add + base_add * ((progress + addProgress) / 10) * Values.data.k_shop;
                }
                public XXLNum getCurCost(int add)
                {
                    float k = (Mathf.Pow(progress + add, 4f / 3f) / 5) + 1;
                    return base_cost * k;
                }
            }
            btn_model[] purch_model;
            int[] add;
            public ShopState()
            {
                add = new int[1];
                add[0] = 1;
                purch_model = initBtnModels();
            }

            public void changeAdd(Text switcher)
            {
                if (add[0] == 1)
                {
                    add[0] = 10;
                    for (int i = 0; i < purch_model.Length; i++)
                        initBtn(transform.GetChild(i), i);
                    switcher.text = "+10";
                }
                else if (add[0] == 10)
                {
                    add[0] = 100;
                    for (int i = 0; i < purch_model.Length; i++)
                        initBtn(transform.GetChild(i), i);
                    switcher.text = "+100";
                }
                else if (add[0] == 100)
                {
                    add = new int[purch_model.Length + 1];
                    add[0] = 0;
                    for (int i = 0; i < purch_model.Length; i++)
                    {
                        add[i + 1] = purch_model[i].getMaxGrade(Values.profile.Score);
                        //purch_model[i].getCost(1, false);
                        initMaxBtn(transform.GetChild(i), i);
                    }
                    switcher.text = "max";
                }
                else
                {
                    add = new int[1];
                    add[0] = 1;
                    for (int i = 0; i < purch_model.Length; i++)
                        initBtn(transform.GetChild(i), i);
                    switcher.text = "+1";
                }
            }
            private void initBtn(Transform transform, int id, bool isGrade = false)
            {
                XXLNum cost = purch_model[id].getCost(add[0], isGrade);
                reInitPurch(transform, id);
                transform.Find("add").GetComponent<Text>().text =
                    JsonParser.getLocaliz("PerSecond").Replace("%s", (purch_model[id].add).ToString());
                transform.GetComponent<Button>().onClick.RemoveAllListeners();
                transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (Values.profile.Score >= cost)
                    {
                        ValuesNotifyHandle.putNotify(new Purchase(cost, purch_model[id].add, id, add[0]));
                        initBtn(transform, id, true);
                    }
                });
            }
            private void initMaxBtn(Transform transform, int id)
            {
                XXLNum cost = purch_model[id].cost;
                reInitPurch(transform, id);
                transform.Find("add").GetComponent<Text>().text =
                    JsonParser.getLocaliz("PerSecond").Replace("%s", (purch_model[id].add).ToString());
                transform.GetComponent<Button>().onClick.RemoveAllListeners();
                transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (Values.profile.Score >= cost)
                    {
                        XXLNum score = Values.profile.Score;
                        ValuesNotifyHandle.putNotify(new Purchase(cost, purch_model[id].add, id, add[id + 1]));
                        while (score <= Values.profile.Score) { }
                        for (int i = 0; i < purch_model.Length; i++)
                        {
                            add[i + 1] = purch_model[i].getMaxGrade(Values.profile.Score - cost, add[i + 1]);
                            initMaxBtn(transform.GetChild(i), i);
                        }
                    }
                });
            }
            private void reInitPurch(Transform purch, int id)
            {
                purch.Find("cost").GetComponent<Text>().text = JsonParser.getLocaliz("Cost").Replace("%s", purch_model[id].cost.ToString());
                if (add[0] == 0)
                    purch.Find("cost").GetComponent<Text>().text += $"(+{add[id + 1]})";
                purch.Find("Level").GetComponent<Text>().text = "lvl: " + purch_model[id].progress;
            }
            private btn_model[] initBtnModels()
            {
                int lvl = Values.data.lvl;
                int length = JsonParser.getPurchasesSize(Values.data.lvl);
                btn_model[] btn_val = new btn_model[length];
                for (int i = 0; i != length; i++)
                    btn_val[i] = new btn_model(JsonParser.getCost(lvl, i), JsonParser.getAdd(lvl, i), i);
                return btn_val;
            }
            public override void init()
            {
                int lvl = Values.data.lvl;
                string path = "ShopIcon/" + Values.data.pack + "." + Values.data.lvl + ".";
                //создает объекты покупок и их инит через соответствующие модели
                GameObject purchPrefab = Resources.Load<GameObject>("Prefabs/btnInContent/Shop_btn");
                for (int i = 0; i != purch_model.Length; i++)
                {
                    var purch = GameObject.Instantiate(purchPrefab, transform).transform;
                    purch.Find("name").GetComponent<Text>().text = JsonParser.getPurchName(i);
                    purch.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(path + i);
                    purch.Find("add").GetComponent<Text>().text = JsonParser.getLocaliz("PerSecond").Replace("%s", purch_model[i].add.ToString());
                    initBtn(purch, i);
                }
            }
            public override void Translate()
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    child.Find("name").GetComponent<Text>().text = JsonParser.getPurchName(i);
                    child.Find("add").GetComponent<Text>().text = purch_model[i].add.ToString();
                    reInitPurch(child, i);
                }
            }

            public void Update()
            {
                if (add[0] == 0)
                {
                    for (int i = 0; i < purch_model.Length; i++)
                    {
                        if (Values.profile.Score > purch_model[i].cost)
                            if (Values.profile.Score > purch_model[i].cost + purch_model[i].getCurCost(add[i + 1] + 1))
                            {
                                ++add[i + 1];
                                purch_model[i].update(add[i + 1]);
                                initMaxBtn(transform.GetChild(i), i);
                            }
                    }
                }
            }
        }
        public class BusterState : AbstractPanelState
        {
            public override void init()
            {
                int size = JsonParser.GetTimeBusterCount();
                GameObject bustBtn = Resources.Load<GameObject>("Prefabs/btnInContent/bust_btn");
                for (int i = 0; i < size; i++)
                {
                    var bustPurch = GameObject.Instantiate(bustBtn, transform).transform;
                    int bustVal = JsonParser.GetTimeBusterVal(i);
                    int cost = JsonParser.GetTimeBusterCost(i);
                    bustPurch.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"BustIcons/timer{Values.data.pack}.{bustVal}");
                    bustPurch.Find("cost").GetComponent<Text>().text = JsonParser.getLocaliz("ContractCost").Replace("%s", cost.ToString());
                    bustPurch.Find("descript").GetComponent<Text>().text = JsonParser.GetTimeBusterDesc(i);
                    bustPurch.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (Values.profile.coins >= cost)
                            ValuesNotifyHandle.putNotify(new TakeBuster(new GetMomentalScore(bustVal * 3600), cost));
                    });
                }
                size = JsonParser.GetClickBusterCount();
                for (int i = 0; i < size; i++)
                {
                    var bustPurch = GameObject.Instantiate(bustBtn, transform).transform;
                    int bustVal = JsonParser.GetClickBusterVal(i);
                    int cost = JsonParser.GetClickBusterCost(i);
                    bustPurch.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"BustIcons/click{Values.data.pack}.{bustVal / 10}");
                    bustPurch.Find("cost").GetComponent<Text>().text = JsonParser.getLocaliz("ContractCost").Replace("%s", cost.ToString());
                    bustPurch.Find("descript").GetComponent<Text>().text = JsonParser.GetClickBusterDesc(i);
                    bustPurch.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (Values.profile.coins >= cost)
                            ValuesNotifyHandle.putNotify(new TakeBuster(new AutoClicker(bustVal * 60, 20), cost));
                    });
                }
            }
            public override void Translate()
            {
                int size = JsonParser.GetTimeBusterCount();
                for (int i = 0; i < size; i++)
                {
                    var bustPurch = transform.GetChild(i);
                    int cost = JsonParser.GetTimeBusterCost(i);
                    bustPurch.Find("cost").GetComponent<Text>().text = JsonParser.getLocaliz("ContractCost").Replace("%s", cost.ToString());
                    bustPurch.Find("descript").GetComponent<Text>().text = JsonParser.GetTimeBusterDesc(i);
                }
                int size_loc = JsonParser.GetClickBusterCount();
                for (int i = 0; i < size_loc; i++)
                {
                    var bustPurch = transform.GetChild(i + size);
                    int cost = JsonParser.GetClickBusterCost(i);
                    bustPurch.Find("cost").GetComponent<Text>().text = JsonParser.getLocaliz("ContractCost").Replace("%s", cost.ToString());
                    bustPurch.Find("descript").GetComponent<Text>().text = JsonParser.GetClickBusterDesc(i);
                }
            }
            public void Update()
            {

            }
        }
        public class LevelState : AbstractPanelState
        { 
            //инит панели под все уровни
            private void InitLvlBtn(GameObject InstanceLvlBtn, JsonParser.lvlType type, int lvl)
            {
                InstanceLvlBtn.GetComponentInChildren<Text>().text = JsonParser.getLvlName(type, lvl, Values.data.pack);
                Sprite sprite = Resources.Load<Sprite>("BGpic/" + Values.data.pack + "." + lvl);
                InstanceLvlBtn.GetComponentInChildren<Image>().sprite = sprite;
                InstanceLvlBtn.transform.Find("Image").GetComponentInChildren<Image>().sprite = sprite;
                //верно, если этот уровень не редактор рекламы
                switch (type)
                {
                    case JsonParser.lvlType.level:
                        //если куплен
                        if (Values.profile.lastLvl > lvl)
                        {
                            InstanceLvlBtn.transform.Find("isOpen").gameObject.SetActive(false);
                            InstanceLvlBtn.GetComponent<Button>().onClick.AddListener(() =>
                            {
                                if (Values.data.lvl != lvl)
                                    GameNotifyHandler.putNotify(new LoadLevel(lvl));
                            });
                        }
                        //если не куплен уровень
                        else
                        {
                            XXLNum cost = JsonParser.getLvlCost(lvl);
                            InstanceLvlBtn.transform.Find("isOpen").GetComponent<Button>().onClick.AddListener(() =>
                            {
                                if (Values.profile.Score > cost)
                                {
                                    GameNotifyHandler.putNotify(new TalkLore(lvl));
                                }
                            });
                            InstanceLvlBtn.transform.Find("isOpen").Find("LvlCost").GetComponent<Text>().text = 
                                    JsonParser.getLocaliz("LvlCost").Replace("%s", cost.ToString());
                        }
                        break;
                    case JsonParser.lvlType.scene:
                        InstanceLvlBtn.transform.Find("isOpen").gameObject.SetActive(false);
                        InstanceLvlBtn.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GameNotifyHandler.putNotify(new LoadScene(JsonParser.getSceneCount(lvl)));
                            GameNotifyHandler.putNotify(new ActivateScene());
                        });
                        break;
                }
            }
            public override void init()
            {
                int size = JsonParser.getLvlCount(JsonParser.lvlType.level);
                GameObject LvlBtn = Resources.Load<GameObject>("Prefabs/btnInContent/LvlBtn");
                GameObject InstanceLvlBtn;
                for (int i = 0; i != size; i++)
                {
                    InstanceLvlBtn = GameObject.Instantiate(LvlBtn, transform);
                    InitLvlBtn(InstanceLvlBtn, JsonParser.lvlType.level, i);
                }
                //editors of ads
                InstanceLvlBtn = GameObject.Instantiate(LvlBtn, transform);
                InitLvlBtn(InstanceLvlBtn, JsonParser.lvlType.scene, 0);
            } 
            public override void Translate()
            {
                int size = JsonParser.getLvlCount(JsonParser.lvlType.level);
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (i < size)
                    {
                        transform.GetChild(i).GetComponentInChildren<Text>().text =
                            JsonParser.getLvlName(JsonParser.lvlType.level, i, Values.data.pack);
                        if (Values.profile.lastLvl <= i)
                        {
                            Text lvlCost = transform.GetChild(i).Find("isOpen").Find("LvlCost").GetComponent<Text>();
                            Debug.Log(lvlCost.text);
                            lvlCost.text = JsonParser.getLocaliz("LvlCost").Replace("%s", JsonParser.getLvlCost(i).ToString());
                        }
                    }
                    else
                        transform.GetChild(i).GetComponentInChildren<Text>().text =
                            JsonParser.getLvlName(JsonParser.lvlType.scene, i - size, Values.data.pack);
                }
            }

            public void Update()
            {

            }
        }
        public class PackState : AbstractPanelState
        {
            public override void init()
            {
                Font font = Resources.Load<Font>("Fonts/New/PTSansCaption-Regular");
                GameObject PackBtn = Resources.Load<GameObject>("Prefabs/btnInContent/LvlBtn");
                int size = JsonParser.getPackCount();

                GameObject InstanceLvlBtn = GameObject.Instantiate(PackBtn, transform);
                Destroy(InstanceLvlBtn.transform.Find("isOpen").gameObject);
                Text text = InstanceLvlBtn.GetComponentInChildren<Text>();
                text.text = JsonParser.getPackNames(0);
                text.fontSize = 40;
                text.font = font;
                InstanceLvlBtn.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(@"BGpic/" + 0 + ".0");

                InstanceLvlBtn = GameObject.Instantiate(PackBtn, transform);
                text = InstanceLvlBtn.transform.Find("isOpen").GetComponentInChildren<Text>();
                text.text = JsonParser.getLocaliz("Soon");
                text.fontSize = 70;
                text.font = font;
                InstanceLvlBtn.GetComponentInChildren<Text>().text = JsonParser.getPackNames(1);
                Destroy(InstanceLvlBtn.transform.Find("Image").gameObject);
            } 

            public override void Translate()
            {
                for(int i = 0; i < transform.childCount; i++)
                    transform.GetChild(i).GetComponentInChildren<Text>().text = JsonParser.getPackNames(i);
            }

            public void Update()
            {

            }
        }
        public class ExLevelState : AbstractPanelState
        {
            int pack;
            SaveSystem.SaveData data;
            public ExLevelState(int pack)
            {
                data = SaveSystem.SaveSytem.LoadValues(pack);
                this.pack = pack;
            }
            //инит панели под все уровни
            private void InitLvlBtn(GameObject InstanceLvlBtn, JsonParser.lvlType type, int lvl)
            {
                InstanceLvlBtn.GetComponentInChildren<Text>().text = JsonParser.getLvlName(type, lvl, pack);
                Sprite sprite = Resources.Load<Sprite>("BGpic/" + pack + "." + lvl);
                InstanceLvlBtn.GetComponentInChildren<Image>().sprite = sprite;
                InstanceLvlBtn.transform.Find("Image").GetComponentInChildren<Image>().sprite = sprite;
                //верно, если этот уровень не редактор рекламы
                switch (type)
                {
                    case JsonParser.lvlType.level:
                        //если не куплен уровень
                        if (data.val.lastLvl > lvl || data.val.lastLvl == 0)
                        {
                            InstanceLvlBtn.transform.Find("isOpen").gameObject.SetActive(false);
                            InstanceLvlBtn.GetComponent<Button>().onClick.AddListener(() =>
                            {

                            });
                        }
                        //если куплен
                        else
                        {
                            XXLNum cost = JsonParser.getLvlCost(lvl);
                            InstanceLvlBtn.transform.Find("isOpen").GetComponent<Button>().onClick.AddListener(() =>
                            {

                            });
                            Text LvlCost = InstanceLvlBtn.transform.Find("isOpen").Find("LvlCost").GetComponent<Text>();
                            InstanceLvlBtn.transform.Find("isOpen").Find("LvlCost").GetComponent<Text>().text =
                                                    JsonParser.getLocaliz("LvlCost").Replace("%s", cost.ToString());
                        }
                        break;
                    case JsonParser.lvlType.scene:
                        InstanceLvlBtn.transform.Find("isOpen").gameObject.SetActive(false);
                        InstanceLvlBtn.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            GameNotifyHandler.putNotify(new LoadLevel(lvl));
                        });
                        break;
                }
            }

            public override void init()
            { 
                int size = JsonParser.getLvlCount(JsonParser.lvlType.level);
                GameObject InstanceLvlBtn;
                GameObject LvlBtn = Resources.Load<GameObject>("Prefabs/btnInContent/LvlBtn");
                for (int i = 0; i != size; i++)
                {
                    InstanceLvlBtn = GameObject.Instantiate(LvlBtn, transform);
                    InitLvlBtn(InstanceLvlBtn, JsonParser.lvlType.level, i);
                }
            } 

            public override void Translate()
            {
                int size = JsonParser.getLvlCount(JsonParser.lvlType.level);
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).GetComponentInChildren<Text>().text =
                        JsonParser.getLvlName(JsonParser.lvlType.level, i, pack);
                }
            }

            public void Update()
            {

            }
        }
        public class EditState : AbstractPanelState
        {  
            //инит моделей редактирования покупок
            private void InitEditbalancer(GameObject purch, string path, int id)
            {
                purch.transform.Find("name").GetComponent<Text>().text = JsonParser.getPurchName(id);
                purch.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
                purch.transform.Find("cost").GetComponent<Text>().text = "Cost:\n";
                purch.transform.Find("add").GetComponent<Text>().text = "Add:\n";
                purch.transform.Find("AddValue").GetComponent<InputField>().onEndEdit.AddListener((string val) =>
                {
                    int space = val.IndexOf(' ');
                    int add = int.Parse(val.Remove(space));
                    int power = int.Parse(val.Remove(0, space + 1));
                    Debug.Log("Add:" + add + "   Power:" + power);
                    JsonParser.setAdd(add, power, id);
                });
                purch.transform.Find("CostValue").GetComponent<InputField>().onEndEdit.AddListener((string val) =>
                {
                    int space = val.IndexOf(' ');
                    int cost = int.Parse(val.Remove(space));
                    int power = int.Parse(val.Remove(0, space + 1));
                    Debug.Log("Cost:" + cost + "   Power:" + power);
                    JsonParser.setCost(cost, power, id);
                });
            }
            public override void init()
            {
                int lvl = Values.data.lvl;
                int length = JsonParser.getPurchasesSize(lvl);

                string path = "ShopIcon/" + Values.data.pack + "." + Values.data.lvl + ".";
                GameObject EditBtn = Resources.Load<GameObject>("Prefabs/btnInContent/edit_btn");
                GameObject lvlTranslate = Resources.Load<GameObject>("Prefabs/btnInContent/PerehodBalance");
                GameObject LvlBtn = Resources.Load<GameObject>("Prefabs/btnInContent/LvlBtn");
                for (int i = 0; i != length; i++)
                {
                    var purch = Instantiate(EditBtn, transform);
                    InitEditbalancer(purch, path + i, i);
                }
                int size = JsonParser.getArchivCount("Levels");
                for (int i = 1; i != size; i++)
                {
                    var trans = Instantiate(lvlTranslate, transform);
                    trans.GetComponentInChildren<Text>().text += (i + 1);
                    int id = i;
                    trans.GetComponentInChildren<InputField>().onEndEdit.AddListener((string val) =>
                    {
                        int space = val.IndexOf(' ');
                        int mean = int.Parse(val.Remove(space));
                        int power = int.Parse(val.Remove(0, space + 1));
                        Debug.Log("Add:" + mean + "   Power:" + power);
                        JsonParser.setTranslateLvl(mean, power, id);
                    });
                }
                //смена rate кликов автокликера за секунда
                var ClickPerSecond = Instantiate(lvlTranslate, transform);
                ClickPerSecond.GetComponentInChildren<Text>().text = "Изменить количество кликов за секнду:";
                ClickPerSecond.GetComponentInChildren<InputField>().onEndEdit.AddListener((string val) =>
                {
                    int mean = int.Parse(val);
                    GameData.data.deltaClick = mean;
                });
                //смена кефа при покупках(достижении 10 лвл)
                var KForShop = Instantiate(lvlTranslate, transform);
                KForShop.GetComponentInChildren<Text>().text = "Смена кефа при покупках(достижении 10 лвл)";
                KForShop.GetComponentInChildren<InputField>().onEndEdit.AddListener((string val) =>
                {
                    float mean = float.Parse(val);
                    GameData.data.k_shop = mean;
                });

                //вкл\выкл автокликера
                var stop = Instantiate(LvlBtn, transform);
                stop.transform.Find("isOpen").gameObject.SetActive(false);
                stop.GetComponent<Button>().onClick.AddListener(() => GameData.data.isAutoClick = !Values.data.isAutoClick);
                stop.GetComponentInChildren<Text>().text = "Остановка/пуск автоклкика";

                //обнуление прогресса
                var goDown = Instantiate(LvlBtn, transform);
                goDown.transform.Find("isOpen").gameObject.SetActive(false);
                goDown.GetComponent<Button>().onClick.AddListener(() => ValuesNotifyHandle.putNotify(new ResetValues()));
                goDown.GetComponentInChildren<Text>().text = "Сбросить значния до дефолтных";

                var extraSceneLoader = Instantiate(LvlBtn, transform);
                extraSceneLoader.transform.Find("isOpen").gameObject.SetActive(false);
                extraSceneLoader.GetComponent<Button>().onClick.AddListener(() => ValuesNotifyHandle.putNotify(new GoToExtraScene()));
                extraSceneLoader.GetComponentInChildren<Text>().text = "Загрузить синий экран";
            }

            public override void Translate()
            {

            }

            public void Update()
            {

            }
        } 

        private RectTransform rect;

        //состояние панели поднята/опущена
        private Transform curContent { get => rect.Find("Purchases").Find("ContentKeeper").Find("Content"); }
        //1 если она слева
        int side;
        
        public enum initCategory
        {
            shop = 1,
            buster = 2,
            levels = 3,
            packs = 4,
            testing = 5
        } 
        private string NameByCategory(initCategory category)
        {
            switch (category)
            {
                case initCategory.shop:
                    return "Shop";
                case initCategory.buster:
                    return "Busters";
                case initCategory.levels:
                    return "Levels";
                case initCategory.packs:
                    return "Packs";
                case initCategory.testing:
                    return "Testing";
            }
            return "";
        }

        private void Start()
        {
            rect = transform as RectTransform;
            side = rect.pivot.x == 1 ? 1 : -1;
            Vector2 size = new Vector2(rect.rect.width, 0);
            Debug.Log(size);
            if (side == -1)
                rect.anchorMin = new Vector2(1, 0);
            else
                rect.anchorMax = new Vector2(0, 1);
            rect.sizeDelta = size;
            LeanTween.moveX(rect, size.x * side, 0);
            LeanTween.moveX(rect, 0, 0.8f).setEaseInExpo();
            Transform categoryTypes = transform.Find("Categories");
            GameObject shopCategory = Resources.Load<GameObject>("Prefabs/btnInContent/ShopCategory");
            foreach (Transform child in categoryTypes)
                Destroy(child.gameObject);

            if (side == 1)
            {
                curContent.gameObject.AddComponent<ShopState>();

                Transform categories = transform.Find("Categories");
                foreach (Transform categoryBtn in categories)
                    if (categoryBtn.name.Equals(NameByCategory(initCategory.shop)))
                        categoryBtn.GetComponent<Button>().interactable = false;

                GameObject section = Instantiate(shopCategory, categoryTypes);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Busters");
                section.name = NameByCategory(initCategory.buster);
                section.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SetState(initCategory.buster);
                });

                section = Instantiate(shopCategory, categoryTypes);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Grades");
                section.name = NameByCategory(initCategory.shop);
                section.GetComponent<Button>().interactable = false;
                section.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SetState(initCategory.shop);
                });
            }
            else
            {
                curContent.gameObject.AddComponent<LevelState>();
                Transform categories = transform.Find("Categories");
                foreach (Transform categoryBtn in categories)
                    if (categoryBtn.name.Equals(NameByCategory(initCategory.levels)))
                        categoryBtn.GetComponent<Button>().interactable = false;

                GameObject section = Instantiate(shopCategory, categoryTypes);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Levels");
                section.name = NameByCategory(initCategory.levels);
                section.GetComponent<Button>().interactable = false;
                section.GetComponent<Button>().onClick.AddListener(() => SetState(initCategory.levels));

                section = Instantiate(shopCategory, categoryTypes);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Pack");
                section.name = NameByCategory(initCategory.packs);
                section.GetComponent<Button>().onClick.AddListener(() => SetState(initCategory.packs));

                if (Values.data.isTest)
                {
                    section = Instantiate(shopCategory, categoryTypes);
                    section.GetComponentInChildren<Text>().text = "TestModule";
                    section.name = NameByCategory(initCategory.testing);
                    section.GetComponent<Button>().onClick.AddListener(() => SetState(initCategory.testing));
                } 
            }
        }
        public void reinit()
        {
            Transform content = curContent;
            foreach (Transform child in content)
                Destroy(child.gameObject);
            content.GetComponent<AbstractPanelState>().init();
        }
        //анимация скрытия панели
        public void down(bool isDown = true)
        { 
            StartCoroutine(Animate(isDown));
        }

        IEnumerator Animate(bool isLeft)
        { 
            float width = rect.rect.width;
            float timer = 0;               
            float velocity = 50;
            float x_finite = isLeft ? 0 : width;
            int sign = isLeft ? -1 : 1;
            x_finite *= side; sign *= side;

            float k, x;
            while (rect.anchoredPosition.x != x_finite)
            {
                timer += Time.deltaTime;
                k = Mathf.Clamp(velocity * Mathf.Cos(Mathf.PI * timer), 20, velocity);
                x = Mathf.Clamp(rect.anchoredPosition.x + sign * k, 0, width);
                rect.anchoredPosition = new Vector2(x, 0);
                yield return null;
            }
        }
        void SetState(initCategory category)
        {
            initCategory thisCategory = curContent.GetComponent<AbstractPanelState>().GetCategory();
            if (thisCategory == category) return;
            RectTransform swapRect = rect.Find("Purchases").Find("ContentKeeper") as RectTransform;
            int vsign = thisCategory > category ? -1 : 1;
            int contentCount = (category - thisCategory) * vsign;
            float width = swapRect.rect.width;

            GameObject contentPref = Resources.Load<GameObject>("Prefabs/AllUse/Content");
            for (int i = 1; i <= contentCount; i++)
            {
                GameObject content = GameObject.Instantiate(contentPref, swapRect);
                initContentPanel(content, (initCategory)(i*vsign+(int)thisCategory));
                 
                (content.transform as RectTransform).anchoredPosition = new Vector2((width + 5) * i * -vsign * side, 0);
            }
            
            LeanTween.moveX(swapRect, width * contentCount * vsign * side, 0.5f).setEaseOutExpo().setOnComplete(() =>
            {
                swapRect.GetChild(contentCount).name = "Content";
                swapRect.anchoredPosition = new Vector2(0, 0);
                (swapRect.GetChild(contentCount).transform as RectTransform).anchoredPosition = new Vector2(0, 0);

                for (int i = 0; i < contentCount; i++)
                    Destroy(swapRect.GetChild(0).gameObject);

                Transform categories = swapRect.parent.parent.Find("Categories");
                foreach(Transform categoryBtn in categories)
                {
                    if (categoryBtn.name.Equals(NameByCategory(thisCategory)))
                        categoryBtn.GetComponent<Button>().interactable = true;
                    else if (categoryBtn.name.Equals(NameByCategory(category)))
                        categoryBtn.GetComponent<Button>().interactable = false;
                }
            });  
        }
        private void initContentPanel(GameObject content, initCategory category)
        {
            switch (category)
            {
                case initCategory.shop:
                    content.AddComponent<ShopState>();
                    break;
                case initCategory.buster:
                    content.AddComponent<BusterState>(); 
                    break;
                case initCategory.levels:
                    content.AddComponent<LevelState>();
                    break;
                case initCategory.packs:
                    content.AddComponent<PackState>(); 
                    break;
                case initCategory.testing:
                    content.AddComponent<EditState>(); 
                    break;
            }
        }
        private SwapState CreateSwap(Transform parent, initCategory category)
        {
            GameObject content = Resources.Load<GameObject>("Prefabs/AllUse/Content");
            RectTransform leftContent = GameObject.Instantiate(content, parent).GetComponent<RectTransform>();
            leftContent.name = "leftContent";
            RectTransform rightContent = GameObject.Instantiate(content, parent).GetComponent<RectTransform>();
            rightContent.name = "rightContent";

            float shift = (parent as RectTransform).rect.width + 5;
            leftContent.anchoredPosition = new Vector2(-shift, 0);
            rightContent.anchoredPosition = new Vector2(shift, 0);
             
            switch (category)
            {
                case initCategory.shop:
                    leftContent.gameObject.AddComponent<BusterState>();
                    rightContent.gameObject.AddComponent<BusterState>();
                    break;
                case initCategory.buster:
                    leftContent.gameObject.AddComponent<ShopState>();
                    rightContent.gameObject.AddComponent<ShopState>();
                    break;
                case initCategory.levels:
                    if (Values.data.isTest)
                        leftContent.gameObject.AddComponent<EditState>();
                    else
                        leftContent.gameObject.AddComponent<PackState>();
                    rightContent.gameObject.AddComponent<PackState>();
                    break;
                case initCategory.packs:
                    leftContent.gameObject.AddComponent<LevelState>();
                    if (Values.data.isTest)
                        rightContent.gameObject.AddComponent<EditState>();
                    else
                        rightContent.gameObject.AddComponent<LevelState>();
                    break;
                case initCategory.testing:
                    leftContent.gameObject.AddComponent<PackState>();
                    rightContent.gameObject.AddComponent<LevelState>();
                    break;
            }
            return new SwapState(parent, leftContent, rightContent, NameByCategory(category), 9);
        } 
        public override void OnBeginDrag(PointerEventData eventData)
        {
            initCategory category = curContent.GetComponent<AbstractPanelState>().GetCategory();
            if (Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y))
                actionState = new ScrollState(curContent);
            else
            {
                if ((category == initCategory.levels || category == initCategory.shop) && eventData.delta.x * side < 0)
                    actionState = new MovePanelStateX(rect.parent, side);
                else
                {
                    actionState = CreateSwap(rect.Find("Purchases").Find("ContentKeeper"), category);
                }
            }
            actionState.Update(eventData.delta);
        }
    }
} 
