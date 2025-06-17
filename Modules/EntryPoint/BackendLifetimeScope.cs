
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Ncroquis.Backend;



// 사용 할 모든 백엔드 서비스 들을 등록
public static class Backend
{
    public const string NONE = "NONE";
    public const string FIREBASE = "FIREBASE";
    public const string ADX = "ADX";
}



//사용법 : 빈 GameObject 에 붙여주세요

public class BackendLifetimeScope : ABackendLifetimeScope
{

    [Header("ADX Settings")]
    [SerializeField] string adxAppID = "";
    [SerializeField] GdprType gdprType = GdprType.POPUP_LOCATION;


    protected override void OnCustomConfigs(BackendContainer backendContainer)
    {
        // 여기에 백엔드 서비스에 대한 커스텀 설정을 추가합니다.

        // 예) FIREBASE 관련 등록
        // backendContainer.Providers[Backend.FIREBASE] = new FirebaseBackendProvider();
        // backendContainer.Auths[Backend.FIREBASE] = new FirebaseBackendAuth();
        // backendContainer.Analytics[Backend.FIREBASE] = new FirebaseBackendAnalytics();
        // backendContainer.Datas[Backend.FIREBASE] = new FirebaseBackendData();


        // 예) ADX 관련 등록
        // 유니티 에디터에서 GDPR 동의 팝업을 테스트하기 위해 GdprType.POPUP_DEBUG를 설정합니다       
        backendContainer.Providers[Backend.ADX] = new AdxBackendProvider(adxAppID, gdprType);
        backendContainer.Ads[Backend.ADX] = new AdxBackendAds();
    }

    protected override void OnCustomEntryPoint(IContainerBuilder builder)
    {
        // 여기에 초기화 EntryPoint를 등록합니다.
        builder.RegisterEntryPoint<BackendEntryPoint>();
    }
}
