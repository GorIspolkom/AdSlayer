using Clicker.HandlerSystem;
using Clicker.Models;
using UnityEngine;
using UnityEngine.Advertisements;

namespace Clicker.Advertisment
{
    public class Advert : MonoBehaviour
    {
        private class AdvertReward : IUnityAdsListener
        {
            Notify rewardNotify, nonrewardNotify;
            GameNotify gameNotify;
            public AdvertReward(string gameID, bool isTest)
            {
                Advertisement.AddListener(this);
                Advertisement.Initialize(gameID, isTest); 
            }

            public void ShowAds(Notify nonrewardNotify, Notify rewardNotify, string myPlacementId)
            {
                this.rewardNotify = rewardNotify;
                this.nonrewardNotify = nonrewardNotify;
                Advertisement.Show(myPlacementId);
            }
            public void OnUnityAdsReady(string placementId)
            {
            }

            public void OnUnityAdsDidError(string message)
            {
                isAdShown = false;
                //GameNotifyHandler.putNotify(gameNotify);
            }

            public void OnUnityAdsDidStart(string placementId)
            {
            }

            public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
            {
                isAdShown = false;
                if (showResult == ShowResult.Finished)
                    ValuesNotifyHandle.putNotify(rewardNotify);
                else
                    ValuesNotifyHandle.putNotify(nonrewardNotify);
                //GameNotifyHandler.putNotify(gameNotify);
            }
        }
        //private class GetBuff : IUnityAdsListener
        //{
        //    private BuffType type; 
        //    private int buff;
        //    private long time;
        //    public GetBuff(string gameID, bool isTest)
        //    {
        //        Advertisement.AddListener(this);
        //        Advertisement.Initialize(gameID, isTest);
        //    }

        //    public void ShowAds(BuffType _type, int _buff, long _time, string myPlacementId)
        //    {
        //        type = _type;
        //        buff = _buff;
        //        time = _time;
        //        Advertisement.Show(myPlacementId);
        //    }
        //    public void OnUnityAdsReady(string placementId)
        //    {
        //    }

        //    public void OnUnityAdsDidError(string message)
        //    {
        //        buff = 0;
        //        time = 0;
        //        GameNotifyHandler.putNotify(new SwitchAD(false));
        //    }

        //    public void OnUnityAdsDidStart(string placementId)
        //    {
        //    }

        //    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        //    {
        //        if (showResult == ShowResult.Finished)
        //            ValuesNotifyHandle.putNotify(new TakeBuff(type, buff, time));
        //        buff = 0;
        //        time = 0;
        //        GameNotifyHandler.putNotify(new SwitchAD(false));
        //    }
        //}
        //private class Rewarded : IUnityAdsListener
        //{
        //    private XXLNum Reward;
        //    private XXLNum NonReward;
        //    public Rewarded(string gameID, bool isTest)
        //    {
        //        Advertisement.AddListener(this);
        //        Advertisement.Initialize(gameID, isTest);
        //    }
        //    public void ShowAds(XXLNum _NonReward, XXLNum _Reward, string myPlacementId)
        //    {
        //        Reward = _Reward;
        //        NonReward = _NonReward;
        //        Advertisement.Show(myPlacementId);
        //    }
        //    public void OnUnityAdsReady(string placementId)
        //    {

        //    }

        //    public void OnUnityAdsDidStart(string placementId)
        //    {

        //    }

        //    public void OnUnityAdsDidError(string message)
        //    {
        //        Debug.Log(message);
        //        AddScore(NonReward);
        //        GameNotifyHandler.putNotify(new SwitchAD(false));
        //    }

        //    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
        //    {
        //        switch (showResult)
        //        {
        //            case ShowResult.Finished:
        //                AddScore(Reward);
        //                break;

        //            case ShowResult.Skipped:
        //                AddScore(NonReward);
        //                break;

        //            case ShowResult.Failed:
        //                AddScore(NonReward);
        //                Debug.LogWarning("The ad did not finish due to an error.");
        //                break;
        //        }
        //        GameNotifyHandler.putNotify(new SwitchAD(false));
        //    }
        //    private void AddScore(XXLNum add)
        //    {
        //        if (!add.isZero())
        //        {
        //            ValuesNotifyHandle.putNotify(new TakeScore(add));
        //        }
        //        Reward = XXLNum.zero;
        //        NonReward = XXLNum.zero;
        //    }
        //}
        
        private const string gameId = "3994109";
        public static bool isAdShown;

        private AdvertReward advert;

        private void Start()
        {
            isAdShown = false;
            advert = new AdvertReward(gameId, Values.data.isTest); 
        }
        public void ShowAdWithReward(XXLNum _NonReward, XXLNum _Reward, string _myPlacementId)
        {
            isAdShown = true;
            advert.ShowAds(new TakeScore(_NonReward), new TakeScore(_Reward), _myPlacementId);
        }
        public void ShowAdWithBuff(BuffType type, int _buff, long _time, string _myPlacementId)
        {
            isAdShown = true;
            advert.ShowAds(new VoidNotify(), new TakeBuff(type, _buff, _time), _myPlacementId);
        }
        public void ShowAdWithContracts(int coin, string _myPlacementId)
        {
            isAdShown = true;
            advert.ShowAds(new VoidNotify(), new TakeCoins(coin), _myPlacementId);
        }
    }
}