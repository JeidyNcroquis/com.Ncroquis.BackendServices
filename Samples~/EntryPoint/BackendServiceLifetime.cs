using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace Ncroquis.Backend
{
    public class BackendServiceLifetime : LifetimeScope
    {
        [Header("기본 설정")]
        [Tooltip("출력할 로그 레벨을 설정합니다.\n(Info:전체, Error:에러만)")]
        [SerializeField] private LogLevel LogLevel = LogLevel.Info;


        // [Header("ADX 설정")]
        // [SerializeField] private AdxIdConfig adxIdConfig;


        protected override void Configure(IContainerBuilder builder)
        {
            // [예] FIREBASE 등록
            // builder.Register<FirebaseBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>();
            // builder.Register<FirebaseBackendAuth>(Lifetime.Singleton).AsSelf().As<IBackendAuth>();
            // builder.Register<FirebaseBackendAnalytics>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces().As<IBackendAnalytics>();
            // builder.Register<FirebaseBackendData>(Lifetime.Singleton).AsSelf().As<IBackendData>();



            // [예] ADX 등록
            // builder.Register<AdxBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>()
            //     .WithParameter("adxAppId", adxIdConfig.GetAppId());

            // builder.Register<AdxBackendAds>(Lifetime.Singleton).AsSelf().As<IBackendAds>()
            //     .WithParameter("bannerAdUnitId", adxIdConfig.GetBannerId())
            //     .WithParameter("interstitialAdUnitId", adxIdConfig.GetInterstitialId())
            //     .WithParameter("rewardedAdUnitId", adxIdConfig.GetRewardedId());


            // [필수] 등록
            builder.Register<ILogger>(_ => new UnityLogger(LogLevel), Lifetime.Singleton);
            builder.Register<BackendService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BackendServiceInitializer>(Lifetime.Singleton);
        }
    }
}
