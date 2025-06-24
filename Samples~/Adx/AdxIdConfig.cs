using UnityEngine;

[CreateAssetMenu(fileName = "AdxIdConfig", menuName = "BackendServices/AdxIdConfig")]
public class AdxIdConfig : ScriptableObject
{
    [Header("App ID")]
    public string androidAppId;
    public string iosAppId;

    [Header("광고 유닛 ID - Android")]
    public string androidBannerId;
    public string androidInterstitialId;
    public string androidRewardedId;

    [Header("광고 유닛 ID - iOS")]
    public string iosBannerId;
    public string iosInterstitialId;
    public string iosRewardedId;

    public string GetAppId()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        return androidAppId;
#elif UNITY_IPHONE
        return iosAppId;
#endif
    }

    public string GetBannerId()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        return androidBannerId;
#elif UNITY_IPHONE
        return iosBannerId;
#endif
    }

    public string GetInterstitialId()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        return androidInterstitialId;
#elif UNITY_IPHONE
        return iosInterstitialId;
#endif
    }

    public string GetRewardedId()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        return androidRewardedId;
#elif UNITY_IPHONE
        return iosRewardedId;
#endif
    }
}
