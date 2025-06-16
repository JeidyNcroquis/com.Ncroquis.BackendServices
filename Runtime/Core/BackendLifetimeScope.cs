
using VContainer;
using VContainer.Unity;

namespace Ncroquis.Backend
{

    // 사용 할 모든 백엔드 서비스 들을 등록

    public enum BackendType
    {
        FIREBASE,
        ADX
    }


    public class BackendLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 인스턴스 직접 생성
            var backendContainer = new BackendContainer();

            ConfigBackends(backendContainer);

            // 한 번에 등록
            builder.RegisterInstance(backendContainer);

            // BackendService 등록
            builder.Register<BackendSelector>(Lifetime.Singleton).AsSelf();
            builder.Register<BackendService>(Lifetime.Singleton).AsSelf();

            // 초기화 EntryPoint 등록
            builder.RegisterEntryPoint<BackendEntryPoint>();
        }


        protected void ConfigBackends(BackendContainer backendContainer)
        {
            // FIREBASE 관련 등록
            backendContainer.Providers[BackendType.FIREBASE] = new FirebaseBackendProvider();
            backendContainer.Auths[BackendType.FIREBASE] = new FirebaseBackendAuth();
            backendContainer.Analytics[BackendType.FIREBASE] = new FirebaseBackendAnalytics();
            backendContainer.Datas[BackendType.FIREBASE] = new FirebaseBackendData();

            // ADX 관련 등록
            backendContainer.Providers[BackendType.ADX] = new NullBackendProvider();
        }

    }

}