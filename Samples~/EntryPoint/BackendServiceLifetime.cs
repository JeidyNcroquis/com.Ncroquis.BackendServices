using UnityEngine;
using VContainer;
using VContainer.Unity;
using MessagePipe;



namespace Ncroquis.Backend
{

    public class BackendServiceLifetime : LifetimeScope
    {
        [Header("기본 설정")]
        [Tooltip("출력할 로그 레벨을 설정합니다.\n(Info:전체, Error:에러만)")]
        [SerializeField] private LogLevel LogLevel = LogLevel.Info;


        [Header("ADX 설정 (없으면 TEST용 자동 설정)")]
        [SerializeField] private AdxIdConfig adxIdConfig;

        [Header("POINTPUB 설정 (없으면 실행 안함)")]
        [SerializeField] private PointpubIdConfig pointpubIdConfig;


        protected override void Configure(IContainerBuilder builder)
        {

            // [필수1] MessagePipe 설정
            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));

            // [필수2] Publisher/Subscriber 등록
            builder.RegisterMessageBroker<FocusChangedMessage>(options);
            builder.RegisterMessageBroker<PauseChangedMessage>(options);

            // [필수3] 등록
            builder.Register<ILogger>(_ => new UnityLogger(LogLevel), Lifetime.Singleton);
            builder.Register<BackendService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BackendServiceInitializer>(Lifetime.Singleton);





            // [예] FIREBASE 등록
            // builder.Register<FirebaseBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>();
            // builder.Register<FirebaseBackendAuth>(Lifetime.Singleton).AsSelf().As<IBackendAuth>();
            // builder.Register<FirebaseBackendAnalytics>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces().As<IBackendAnalytics>();
            // builder.Register<FirebaseBackendDataStore>(Lifetime.Singleton).AsSelf().As<IBackendDataStore>();



            // [예] ADX 등록
            builder.Register<AdxBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>()
                .WithParameter("adxAppId", adxIdConfig?.GetAppId() ?? default);

            // ISubscriber<FocusChangedMessage>, ISubscriber<PauseChangedMessage>는 자동 주입됨
            builder.Register<AdxBackendAds>(Lifetime.Singleton).AsSelf().As<IBackendAds>()
                .WithParameter("bannerAdUnitId", adxIdConfig?.GetBannerId() ?? default)
                .WithParameter("interstitialAdUnitId", adxIdConfig?.GetInterstitialId() ?? default)
                .WithParameter("rewardedAdUnitId", adxIdConfig?.GetRewardedId() ?? default);



            // [예] POINTPUB 등록
            builder.Register<PointpubBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>()
                .WithParameter("offerwallAppId", "" ?? default);

            builder.Register<PointpubBackendOfferwall>(Lifetime.Singleton).AsSelf().As<IBackendOfferwall>();

        }
    }
    
}
