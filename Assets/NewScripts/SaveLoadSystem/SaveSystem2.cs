using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Android;

namespace Clicker.SaveSystem
{
    class SaveSystem2
    {
        [System.Serializable]
        protected class SaveGameData
        {
            public GameData data;
            public SettingData settings;
            public long leaveTime;
            public SaveGameData(bool isEthernet)
            {
                data = Values.data;
                settings = Values.settings;
                leaveTime = GetNetworkTime(isEthernet).Ticks / TimeSpan.TicksPerSecond;
            }
            public long UngameTime(bool isEthernet)
            {  
                return (GetNetworkTime(isEthernet).Ticks / TimeSpan.TicksPerSecond - leaveTime);
            }
            //get time from internet
            public static DateTime GetNetworkTime(bool isEthernet)
            {
                if (isEthernet)
                {
                    try {
                        const string ntpServer = "time.windows.com";
                        var ntpData = new byte[48];
                        ntpData[0] = 0x1B;

                        var addresses = Dns.GetHostEntry(ntpServer).AddressList;
                        var ipEndPoint = new IPEndPoint(addresses[0], 123);

                        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                        {
                            socket.Connect(ipEndPoint);
                            socket.Send(ntpData);
                            socket.Receive(ntpData);
                            socket.Close();
                        }

                        var intPart = (ulong)ntpData[40] << 24 | (ulong)ntpData[41] << 16 | (ulong)ntpData[42] << 8 | ntpData[43];
                        var fractPart = (ulong)ntpData[44] << 24 | (ulong)ntpData[45] << 16 | (ulong)ntpData[46] << 8 | ntpData[47];

                        var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
                        var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

                        return networkDateTime.ToLocalTime();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                        return DateTime.Now;
                    }
                }
                else
                {
                    return DateTime.Now;
                }
            }
        }
        public static string pack_name;
        public static string persist_path = Application.persistentDataPath;
        public static string path = Application.persistentDataPath + "/LastSession.save";
        public static bool OldSaveValues(bool isEthernet, bool isEnd = false)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(persist_path + "/test.save", FileMode.Create);
            formatter.Serialize(stream, Values.profile);
            stream.Close();
            SaveGameData saveData = new SaveGameData(isEthernet);
            File.WriteAllText(persist_path + "/GameData.data", JsonUtility.ToJson(saveData));
            if (isEnd)
            {
                if (isSaveCorrect())
                {
                    byte[] gameData = File.ReadAllBytes(persist_path + "/test.save");
                    File.WriteAllBytes(path, gameData);
                        
                    File.Delete(persist_path + "/test.save");

                    string name = "/" + JsonParser.GetPackSavePath(Values.data.pack) + ".save";
                    stream = new FileStream(persist_path + name, FileMode.Create);
                    formatter.Serialize(stream, Values.profile);
                    stream.Close();
                    return true;
                }
            }
            else
            {
                byte[] gameData = File.ReadAllBytes(persist_path + "/test.save");
                File.WriteAllBytes(path, gameData);
                return true;
            }
            return false;
        }
        //check if save was done correct
        public static bool isSaveCorrect()
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(persist_path + "/test.save", FileMode.Open);
                SaveData lval = formatter.Deserialize(stream) as SaveData;
                stream.Close();
                return Values.Equals(lval.val, lval.settings, lval.data);
            }
            catch
            {
                return false;
            }
        }
        //load values from save file
        public static ProfileData LoadValues(int pack)
        {
            ProfileData val = null;
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);

            string name = "/" + JsonParser.GetPackSavePath(Values.data.pack) + ".save";
            if (File.Exists(name))
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(persist_path + name, FileMode.Open);
                    val = formatter.Deserialize(stream) as ProfileData;
                    stream.Close();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            else
            { 
                while (!File.Exists(path))
                    try { File.Create(path); }
                    catch { Debug.LogError("Файл не создан!"); }
                val = new ProfileData();
            }
            return val;
        }
        public static void LoadLastSession()
        {
            ProfileData val = null;
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            try
            { 
                if (File.Exists(path))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(path, FileMode.Open);
                    val = formatter.Deserialize(stream) as ProfileData;
                    stream.Close();
                }
                else
                {
                    val = new ProfileData();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            Values.initProfile(val);
        }
        public static long LoadGameData(bool isTest, bool isEthernet)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);

            long unGameTime;
            if (File.Exists(persist_path + "/GameData.data"))
            {
                SaveGameData saveData = JsonUtility.FromJson<SaveGameData>(File.ReadAllText(persist_path + "/GameData.data"));
                Values.initGameData(saveData.settings, saveData.data);
                unGameTime = saveData.UngameTime(isEthernet);
            }
            else
            {
                Values.initGameData(new SettingData(), new GameData(0));
                unGameTime = 0;
            }

            GameData.data.isTest = isTest;
            JsonParser.SelectPack();
            return unGameTime;
        }
        public static ProfileData LoadProfile(int pack)
        {
            string load_pack_name = JsonParser.GetPackSavePath(pack);
            return JsonUtility.FromJson<ProfileData>(PlayerPrefs.GetString(load_pack_name));
        }
        //Load save & return unGameTime
        public static long LoadLastSession(bool isTest)
        {
            long unGameTime = LoadGameData(isTest, false);
            pack_name = JsonParser.GetPackSavePath(Values.data.pack);
            Debug.Log(JsonUtility.FromJson<ProfileData>(PlayerPrefs.GetString(pack_name)));
            if (PlayerPrefs.HasKey(pack_name))
            {
                ProfileData profile = JsonUtility.FromJson<ProfileData>(PlayerPrefs.GetString(pack_name));
                Values.initProfile(profile);
            }
            else
                Values.initProfile(new ProfileData());
            return unGameTime;
        }
    }
}
