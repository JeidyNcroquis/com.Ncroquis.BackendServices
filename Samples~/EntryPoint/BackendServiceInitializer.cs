using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using VContainer;
using UnityEngine;


namespace Ncroquis.Backend
{
    /// <summary>
    /// 백엔드 서비스들을 초기화한 뒤, 추가적인 기능을 처리하기 위한 클래스입니다.
    /// 모든 백엔드 프로바이더가 준비되면 필요한 후속 작업을 수행합니다.
    /// </summary>
    public class BackendServiceInitializer : AProvidersInitializerBase
    {
        private readonly ILogger logger;
        private readonly BackendService service;
        private readonly IObjectResolver resolver;

        [Inject]
        public BackendServiceInitializer(IEnumerable<IBackendProvider> providers, ILogger logger, BackendService service, IObjectResolver resolver)
            : base(providers, logger)
        {
            this.logger = logger;
            this.service = service;
            this.resolver = resolver;
        }

        /// <summary>
        /// 모든 프로바이더 초기화가 완료된 후 실행되는 메서드입니다.
        /// 백엔드 서비스 초기화 이후 필요한 추가 작업을 이곳에서 처리합니다.
        /// </summary>
        protected override async Task OnAfterAllInitialized(CancellationToken cancellation)
        {
            // AppWatcher GameObject 생성
            // var appwatcher = new GameObject("BackendServiceAppWatcher (DontDestroy)").AddComponent<BackendServiceAppWatcher>();
            // Object.DontDestroyOnLoad(appwatcher.gameObject);
            // resolver.Inject(appwatcher);


            logger.Log("[BackendServiceInitializer] 모든 백엔드 서비스가 초기화되었습니다.");


            await Task.CompletedTask;
        }

    }
}
