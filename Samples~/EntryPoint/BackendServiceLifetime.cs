
using VContainer;
using VContainer.Unity;
using UnityEngine;


namespace Ncroquis.Backend
{
    
    // 백엔드 서비스들을 등록 하고 종속성을 주입하는 클래스 입니다.
    public class BackendServiceLifetime : LifetimeScope
    {

        [Space(20)]
        [Tooltip("출력할 로그 레벨을 설정합니다.\n(Info:전체, Error:에러만)")]
        [SerializeField] LogLevel LogLevel = LogLevel.Info;


        protected override void Configure(IContainerBuilder builder)
        {
            // // [선택] FIREBASE 등록
            // builder.Register<FirebaseBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>();
            // builder.Register<FirebaseBackendAuth>(Lifetime.Singleton).AsSelf().As<IBackendAuth>();
            // builder.Register<FirebaseBackendAnalytics>(Lifetime.Singleton).AsSelf().As<IBackendAnalytics>();
            // builder.Register<FirebaseBackendData>(Lifetime.Singleton).AsSelf().As<IBackendData>();

            // // [선택] ADX 등록
            // builder.Register<AdxBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>();
            // builder.Register<AdxBackendAds>(Lifetime.Singleton).AsSelf().As<IBackendAds>();


            // [필수] 등록
            builder.Register<ILogger>(_ => new UnityLogger(LogLevel), Lifetime.Singleton);
            builder.Register<BackendService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BackendServiceInitializer>(Lifetime.Singleton);
        }
    }

}