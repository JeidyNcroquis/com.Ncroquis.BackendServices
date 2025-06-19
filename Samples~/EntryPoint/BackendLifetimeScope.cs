
using VContainer;
using VContainer.Unity;
using Ncroquis.Backend;



public class BackendLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {

        // [선택] FIREBASE 등록
        // builder.Register<FirebaseBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>();
        // builder.Register<FirebaseBackendAuth>(Lifetime.Singleton).AsSelf().As<IBackendAuth>();
        // builder.Register<FirebaseBackendAnalytics>(Lifetime.Singleton).AsSelf().As<IBackendAnalytics>();
        // builder.Register<FirebaseBackendData>(Lifetime.Singleton).AsSelf().As<IBackendData>();

        // [선택] ADX 등록
        // builder.Register<AdxBackendProvider>(Lifetime.Singleton).AsSelf().As<IBackendProvider>();
        // builder.Register<AdxBackendAds>(Lifetime.Singleton).AsSelf().As<IBackendAds>();


        // [필수] 가장 마지막에 등록해야 작동
        builder.Register<BackendService>(Lifetime.Singleton);
        builder.RegisterEntryPoint<BackendInitializer>(Lifetime.Singleton);
    }
}


