
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Ncroquis.Backend;




public class BackendLifetimeScope : LifetimeScope
{
    // [Header("ADX Settings")]
    // [SerializeField] string adxAppId = "<ADX_APP_ID>";
    // [SerializeField] GdprType gdprType = GdprType.POPUP_LOCATION;

    protected override void Configure(IContainerBuilder builder)
    {

        // // [선택] FIREBASE 관련
        //builder.Register<FirebaseBackendProvider>(Lifetime.Singleton).As<IBackendProvider>().As<IBackendIdentifiable>();
        // builder.Register<FirebaseBackendAuth>(Lifetime.Singleton).As<IBackendAuth>().As<IBackendIdentifiable>();
        // builder.Register<FirebaseBackendAnalytics>(Lifetime.Singleton).As<IBackendAnalytics>().As<IBackendIdentifiable>();
        // builder.Register<FirebaseBackendData>(Lifetime.Singleton).As<IBackendData>().As<IBackendIdentifiable>();

        // // [선택] ADX 관련
        // var adxProvider = new AdxBackendProvider(adxAppId, gdprType);
        // builder.RegisterInstance(adxProvider).As<AdxBackendProvider>().As<IBackendProvider>().As<IBackendIdentifiable>();
        // builder.Register<AdxBackendAds>(Lifetime.Singleton).As<IBackendAds>().AsSelf();

        //builder.Register<NullBackendProvider>(Lifetime.Singleton).As<IBackendProvider>().As<IBackendIdentifiable>();        

        // [필수] 등록해야 작동
        builder.Register<BackendService>(Lifetime.Singleton).As<IBackendService>().AsSelf();
        builder.RegisterEntryPoint<BackendInitializer>(Lifetime.Singleton);
    }
}


