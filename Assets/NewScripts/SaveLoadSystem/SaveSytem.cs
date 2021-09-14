using Clicker.Models;
using MyUtile.JsonWorker;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Clicker.SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        public ProfileData val;
        public SettingData settings;
        public GameData data;
        public long SavedTime;
        public SaveData(bool isEnd)
        {
            val = Values.profile;
            settings = SettingData.settings;
            data = GameData.data;
            data.clickPerSecond = 0;
            SavedTime = isEnd ? GetNetworkTime().Ticks : DateTime.Now.Ticks;
            SavedTime /= TimeSpan.TicksPerSecond;
        }
        public SaveData()
        {
            val = Values.profile;
            settings = SettingData.settings;
            data = GameData.data;
            SavedTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        }
        public long UngameTime()
        {
            return (GetNetworkTime().Ticks / TimeSpan.TicksPerSecond - SavedTime);
        }
        //get time from internet
        public static DateTime GetNetworkTime()
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
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
            else
            {
                return DateTime.Now;
            }
        }
    }
    public static class SaveSytem
    {

        //save file name
        public static string path = Application.persistentDataPath + "/LastSession.save";
        //save values in save file
        public static bool SaveValues(bool isEnd = false)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(Application.persistentDataPath + "/test.save", FileMode.Create);
            SaveData sData = new SaveData(isEnd);
            formatter.Serialize(stream, sData);
            stream.Close();
            if (isEnd)
            {
                if (isSaveCorrect())
                {
                    byte[] gameData = File.ReadAllBytes(Application.persistentDataPath + "/test.save");
                    File.WriteAllBytes(path, gameData);

                    File.Delete(Application.persistentDataPath + "/test.save");

                    string name = "/" + JsonParser.getPackNames(Values.data.pack).Replace(" ", "_") + ".save";
                    stream = new FileStream(Application.persistentDataPath + name, FileMode.Create);
                    formatter.Serialize(stream, Values.profile);
                    stream.Close();
                    return true;
                }
            }
            else 
            {
                byte[] gameData = File.ReadAllBytes(Application.persistentDataPath + "/test.save");
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
                FileStream stream = new FileStream(Application.persistentDataPath + "/test.save", FileMode.Open);
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
        public static SaveData LoadValues(int pack = -1)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            SaveData val = null;
            if (pack != -1)
            {
                try
                {
                    string name = "/" + JsonParser.getPackNames(pack).Replace(" ", "_") + ".save";
                    FileStream stream = new FileStream(Application.persistentDataPath + name, FileMode.Open);
                    val = formatter.Deserialize(stream) as SaveData;
                    stream.Close();
                }
                catch
                {
                    while (!File.Exists(path))
                        try { File.Create(path); }
                        catch { Debug.LogError("Файл не создан!"); }
                    Values.init(pack);
                    val = new SaveData();
                }
            }
            else
            {
                try
                {
                    FileStream stream = new FileStream(path, FileMode.Open);
                    val = formatter.Deserialize(stream) as SaveData; 
                    stream.Close();
                }
                catch
                {
                    return LoadValues(0);
                }
            }
            return val;
        }

    }
}