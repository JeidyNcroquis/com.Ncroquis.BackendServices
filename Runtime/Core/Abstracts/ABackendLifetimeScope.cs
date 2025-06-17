
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

            // 커스텀 Backend 들 컨테이너에 등록
            OnRegisterBefore(backendContainer);

            // 한 번에 등록
            builder.RegisterInstance(backendContainer);

            // BackendService 등록
            builder.Register<BackendSelector>(Lifetime.Singleton).AsSelf();
            
            // BackendService, EntryPoint 등록
            OnRegisterAfter(builder);
        }

        // 필요한 백엔드들을 등록
        protected abstract void OnRegisterBefore(BackendContainer backendContainer);

        // BackendService , EntryPoint 등록
        protected abstract void OnRegisterAfter(IContainerBuilder builder);
    }

}