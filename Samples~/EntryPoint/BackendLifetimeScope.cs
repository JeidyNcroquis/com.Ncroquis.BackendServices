
using VContainer;
using VContainer.Unity;
using Ncroquis.Backend;
using UnityEngine;
using ILogger = Ncroquis.Backend.ILogger;



public class BackendLifetimeScope : LifetimeScope
{

    [Space(20)]
    [SerializeField] bool LogEnabled = true;


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
        builder.Register<ILogger>(_ => new UnityLogger(LogEnabled), Lifetime.Singleton);
        builder.Register<BackendService>(Lifetime.Singleton);
        builder.RegisterEntryPoint<BackendInitializer>(Lifetime.Singleton);
    }
}


