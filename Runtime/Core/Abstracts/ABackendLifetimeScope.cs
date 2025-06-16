
using VContainer;
using VContainer.Unity;

namespace Ncroquis.Backend
{

    // 사용 할 모든 백엔드 서비스 들을 등록
    public abstract class ABackendLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 인스턴스 직접 생성
            var backendContainer = new BackendContainer();

            OnCustomConfigs(backendContainer);

            // 한 번에 등록
            builder.RegisterInstance(backendContainer);

            // BackendService 등록
            builder.Register<BackendSelector>(Lifetime.Singleton).AsSelf();
            builder.Register<BackendService>(Lifetime.Singleton).AsSelf();

            // 초기화 EntryPoint 등록
            OnCustomEntryPoint(builder);
        }

        protected abstract void OnCustomConfigs(BackendContainer backendContainer);
        protected abstract void OnCustomEntryPoint(IContainerBuilder builder);
    }

}