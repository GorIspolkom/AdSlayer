using Clicker.DetachedScrypts;
using Clicker.HandlerSystem;
using Clicker.Models;
using MyUtile.JsonWorker;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Clicker.Scrypts
{
    public class InfoPanel : MonoBehaviour
    {
        [System.Serializable]
        public struct PanelStruct
        {
            //выбор пака
            public GameObject PackBtn;
            //префаб кнопки уровня
            public GameObject LvlBtn;
            //префаб кнопки изменения значений для теста
            public GameObject EditBtn;
            //объект панели для инитов
            public GameObject content;
            //шаблон покупки 
            public GameObject purchas;
            //шаблон буста
            public GameObject bustPurch;
            public GameObject lvlTranslate;
            public GameObject shopCategory;
            public Transform ListCategory;
        }

        //состояние инициализации
        public abstract class PanelState : MonoBehaviour
        {
            protected PanelStruct val;
            public PanelState(PanelStruct val) => this.val = val;
            private void Awake()
            {
                foreach (Transform child in transform)
                    Destroy(child.gameObject);
            } 
            public abstract void localize();
        }
        public class ShopState : PanelState
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
                    if(isGrade) progress += prog_add;
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
                        curCost = getCurCost(i+1);
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
            public ShopState(PanelStruct val) : base(val)
            {
                add = new int[1];
                add[0] = 1;
                purch_model = initBtnModels();
            }

            public void Start()
            {
                int lvl = Values.data.lvl;
                string path = "ShopIcon/" + Values.data.pack + "." + Values.data.lvl + ".";
                //создает объекты покупок и их инит через соответствующие модели
                for (int i = 0; i != purch_model.Length; i++)
                {
                    var purch = GameObject.Instantiate(val.purchas, transform).transform;
                    purch.Find("name").GetComponent<Text>().text = JsonParser.getPurchName(i);
                    purch.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(path + i);
                    purch.Find("add").GetComponent<Text>().text = JsonParser.getLocaliz("PerSecond").Replace("%s", purch_model[i].add.ToString());
                    initBtn(purch, i);
                }
                transform.localPosition = new Vector2(0, 0);
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
            public override void localize()
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
        public class BusterState : PanelState
        {
            public BusterState(PanelStruct val) : base(val)
            {
            }

            public void Start()
            {
                int size = JsonParser.GetTimeBusterCount();
                for (int i = 0; i < size; i++)
                {
                    var bustPurch = GameObject.Instantiate(val.bustPurch, transform).transform;
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
                    var bustPurch = GameObject.Instantiate(val.bustPurch, transform).transform;
                    int bustVal = JsonParser.GetClickBusterVal(i);
                    int cost = JsonParser.GetClickBusterCost(i);
                    bustPurch.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"BustIcons/click{Values.data.pack}.{bustVal/10}");
                    bustPurch.Find("cost").GetComponent<Text>().text = JsonParser.getLocaliz("ContractCost").Replace("%s", cost.ToString());
                    bustPurch.Find("descript").GetComponent<Text>().text = JsonParser.GetClickBusterDesc(i);
                    bustPurch.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        if (Values.profile.coins >= cost)
                            ValuesNotifyHandle.putNotify(new TakeBuster(new AutoClicker(bustVal * 60, 20), cost));
                    });
                }
                transform.localPosition = new Vector2(0, 0);
            }
            public override void localize()
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
        public class LevelState : PanelState
        {
            int lastLvl;
            public LevelState(PanelStruct val, int lastLvl) : base(val) => this.lastLvl = lastLvl;
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
                        if (lastLvl > lvl)
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

            public void Start()
            {
                int size = JsonParser.getLvlCount(JsonParser.lvlType.level);
                GameObject InstanceLvlBtn;
                for (int i = 0; i != size; i++)
                {
                    InstanceLvlBtn = GameObject.Instantiate(val.LvlBtn, transform);
                    InitLvlBtn(InstanceLvlBtn, JsonParser.lvlType.level, i);
                }
#if UNITY_EDITOR
                //editors of ads
                InstanceLvlBtn = GameObject.Instantiate(val.LvlBtn, transform);
                InitLvlBtn(InstanceLvlBtn, JsonParser.lvlType.scene, 0);
#endif
                transform.localPosition = new Vector2(0, 0);
            }

            public override void localize()
            {
                int size = JsonParser.getLvlCount(JsonParser.lvlType.level);
                for (int i = 0; i < transform.childCount; i++)
                {
                    if (i < size)
                        transform.GetChild(i).GetComponentInChildren<Text>().text =
                            JsonParser.getLvlName(JsonParser.lvlType.level, i, Values.data.pack);
                    else
                        transform.GetChild(i).GetComponentInChildren<Text>().text =
                            JsonParser.getLvlName(JsonParser.lvlType.scene, i - size, Values.data.pack);
                }
            }
           
            public void Update()
            {

            }
        }
        public class PackState : PanelState
        {
            public int pack;
            public PackState(PanelStruct val) : base(val)
            {
            }

            public void Start()
            {
                Font font = Resources.Load<Font>("Fonts/New/PTSansCaption-Regular");
                int size = JsonParser.getPackCount();
#if !UNITY_ANDROID
                for (int i = 0; i != size; i++)
                {
                    GameObject InstanceLvlBtn = GameObject.Instantiate(val.PackBtn, val.content);
                    Text text = InstanceLvlBtn.GetComponentInChildren<Text>();
                    text.text = JsonParser.getPackNames(i);
                    text.fontSize = 40;
                    text.font = font;
                    InstanceLvlBtn.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(@"BGpic/" + i + ".0");
                    InstanceLvlBtn.transform.Find("DeleteBtn").gameObject.SetActive(false);
                    int id = i;
                    InstanceLvlBtn.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameNotifyHandler.putNotify(new InfoPanelCategory(initCategory.ex_lvl));
                        pack = id;
                    });
                }
#endif
                GameObject InstanceLvlBtn = GameObject.Instantiate(val.PackBtn, transform);
                Destroy(InstanceLvlBtn.transform.Find("isOpen").gameObject);
                Text text = InstanceLvlBtn.GetComponentInChildren<Text>();
                text.text = JsonParser.getPackNames(0);
                text.fontSize = 40;
                text.font = font;
                InstanceLvlBtn.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>(@"BGpic/" + 0 + ".0");
                
                InstanceLvlBtn = GameObject.Instantiate(val.PackBtn, transform);
                text = InstanceLvlBtn.transform.Find("isOpen").GetComponentInChildren<Text>();
                text.text = JsonParser.getLocaliz("Soon");
                text.fontSize = 70;
                text.font = font;
                InstanceLvlBtn.GetComponentInChildren<Text>().text = JsonParser.getPackNames(1);
                Destroy(InstanceLvlBtn.transform.Find("Image").gameObject);

                transform.localPosition = new Vector2(0, 0);
            }

            public override void localize()
            {

            }

            public void Update()
            {

            }
        }
        public class ExLevelState : PanelState
        {
            int pack;
            SaveSystem.SaveData data;
            public ExLevelState(PanelStruct val, int pack) : base(val)
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

            public void Start()
            {

                int size = JsonParser.getLvlCount(JsonParser.lvlType.level);
                GameObject InstanceLvlBtn;
                for (int i = 0; i != size; i++)
                {
                    InstanceLvlBtn = GameObject.Instantiate(val.LvlBtn, transform);
                    InitLvlBtn(InstanceLvlBtn, JsonParser.lvlType.level, i);
                }
                transform.localPosition = new Vector2(0, 0);
            }

            public override void localize()
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
        public class EditState : PanelState
        {
            public EditState(PanelStruct val) : base(val)
            {
            }

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

            public void Start()
            {

                int lvl = Values.data.lvl;
                int length = JsonParser.getPurchasesSize(lvl);

                string path = "ShopIcon/" + Values.data.pack + "." + Values.data.lvl + ".";
                for (int i = 0; i != length; i++)
                {
                    var purch = Instantiate(val.EditBtn, transform);
                    InitEditbalancer(purch, path + i, i);
                }
                int size = JsonParser.getArchivCount("Levels");
                for (int i = 1; i != size; i++)
                {
                    var trans = Instantiate(val.lvlTranslate, transform);
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
                var ClickPerSecond = Instantiate(val.lvlTranslate, transform);
                ClickPerSecond.GetComponentInChildren<Text>().text = "Изменить количество кликов за секнду:";
                ClickPerSecond.GetComponentInChildren<InputField>().onEndEdit.AddListener((string val) =>
                {
                    int mean = int.Parse(val);
                    GameData.data.deltaClick = mean;
                });
                //смена кефа при покупках(достижении 10 лвл)
                var KForShop = Instantiate(val.lvlTranslate, transform);
                KForShop.GetComponentInChildren<Text>().text = "Смена кефа при покупках(достижении 10 лвл)";
                KForShop.GetComponentInChildren<InputField>().onEndEdit.AddListener((string val) =>
                {
                    float mean = float.Parse(val);
                    GameData.data.k_shop = mean;
                });

                //вкл\выкл автокликера
                var stop = Instantiate(val.LvlBtn, transform);
                stop.transform.Find("isOpen").gameObject.SetActive(false);
                stop.GetComponent<Button>().onClick.AddListener(() => GameData.data.isAutoClick = !Values.data.isAutoClick);
                stop.GetComponentInChildren<Text>().text = "Остановка/пуск автоклкика";

                //обнуление прогресса
                var goDown = Instantiate(val.LvlBtn, transform);
                goDown.transform.Find("isOpen").gameObject.SetActive(false);
                goDown.GetComponent<Button>().onClick.AddListener(() => ValuesNotifyHandle.putNotify(new ResetValues()));
                goDown.GetComponentInChildren<Text>().text = "Сбросить значния до дефолтных";

                var extraSceneLoader = Instantiate(val.LvlBtn, transform);
                extraSceneLoader.transform.Find("isOpen").gameObject.SetActive(false);
                extraSceneLoader.GetComponent<Button>().onClick.AddListener(() => ValuesNotifyHandle.putNotify(new GoToExtraScene()));
                extraSceneLoader.GetComponentInChildren<Text>().text = "Загрузить синий экран";

                transform.localPosition = new Vector2(0, 0);
            }

            public override void localize()
            {

            }

            public void Update()
            {

            }
        }

        public PanelState state;
        public PanelStruct val;

        [System.Serializable]
        public enum initCategory
        {
            shop = 1,
            buster = 2,
            levels = 3,
            ex_lvl = 4,
            packs = 5,
            testing = 6
        }
        private bool isSwitched;
        private initCategory category;

        //контроллер анимации
        private Animator anim;
        //название переменной условия в anim
        private string boolName = "isCommand";

        private void Start()
        {
            SetState(initCategory.shop);
            ListCategoryInit(initCategory.shop);
            anim = GetComponent<Animator>();
        }
        public void ListCategoryInit(initCategory category)
        {
            foreach (Transform cat in val.ListCategory)
                Destroy(cat.gameObject);
            if (category == initCategory.shop)
            {
                GameObject section = Instantiate(val.shopCategory, val.ListCategory);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Grades");
                section.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (this.category != initCategory.shop)
                    {
                        SetState(initCategory.shop);
                        val.ListCategory.Find("Switcher")?.gameObject.SetActive(true);
                    }
                });

                section = Instantiate(val.shopCategory, val.ListCategory);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Busters");
                section.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (this.category != initCategory.buster)
                    {
                        SetState(initCategory.buster);
                        val.ListCategory.Find("Switcher")?.gameObject.SetActive(false);
                    }
                });
#if UNITY_EDITOR
                section = Instantiate(Resources.Load<GameObject>("Prefabs/btnInContent/Switcher"), val.ListCategory);
                section.name = "Switcher";
                section.GetComponentInChildren<Text>().text = "+1";
                section.GetComponent<Button>().onClick.AddListener(() => SwitchShopAdd(section.GetComponentInChildren<Text>()));
#endif
            }
            else
            {
                GameObject section = Instantiate(val.shopCategory, val.ListCategory);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Levels");
                section.GetComponent<Button>().onClick.AddListener(() => SetState(initCategory.levels));

                section = Instantiate(val.shopCategory, val.ListCategory);
                section.GetComponentInChildren<Text>().text = JsonParser.getLocaliz("Pack");
                section.GetComponent<Button>().onClick.AddListener(() => SetState(initCategory.packs));

                if (Values.data.isTest)
                {
                    section = Instantiate(val.shopCategory, val.ListCategory);
                    section.GetComponentInChildren<Text>().text = "TestModule";
                    section.GetComponent<Button>().onClick.AddListener(() => SetState(initCategory.testing));
                }
            }

        }
        public void ShopPanelCaller()
        {
            if (category != initCategory.shop && category != initCategory.buster)
            {
                SetState(initCategory.shop);
                ListCategoryInit(initCategory.shop);
                up();
            }
            else
                PlayAnim();
        }
        public void LevelPanelCaller()
        {
            if (category == initCategory.shop || category == initCategory.buster)
            {
                SetState(initCategory.levels);
                ListCategoryInit(initCategory.levels);
                up();
            }
            else
                PlayAnim();
        }
        public void PlayAnim()
        {
            anim.SetBool(boolName, !anim.GetBool(boolName));
        }
        //анимация скрытия панели
        public void down()
        {
            if (anim.GetBool(boolName))
                anim.SetBool(boolName, false);
        }
        public void up()
        {
            if (!anim.GetBool(boolName))
                anim.SetBool(boolName, true);
        }
         
        public void SetState(int stateID) => SetState((initCategory)stateID);
        public void reinit() { }
        public void SetState(initCategory stateID)
        {
            category = stateID;
            switch (stateID)
            {
                case initCategory.shop:
                    state = new ShopState(val);
                    break;
                case initCategory.buster:
                    state = new BusterState(val);
                    break;
                case initCategory.levels:
                    state = new LevelState(val, Values.profile.lastLvl);
                    break;
                case initCategory.packs:
                    state = new PackState(val);
                    break;
                case initCategory.ex_lvl:
                    state = new ExLevelState(val, ((PackState)state).pack);
                    break;
                case initCategory.testing:
                    state = new EditState(val);
                    break;
            } 
        }
        public void Localize()
        {
            ListCategoryInit(category);
            state.localize();
        }
        public void SwitchShopAdd(Text switcher)
        {
            try
            {
                ((ShopState)state).changeAdd(switcher);
            }
            catch { Debug.Log("Нельзя изменить количество покупок, так как другое состояние"); }
        }
        private void Update()
        {
            if (anim.GetBool(boolName))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition, Camera.main))
                    {
                        down();
                    }
                }
            }
        }
    }
}