using System.Collections.Generic;
using VContainer;


namespace Ncroquis.Backend
{
    
    // 외부에서 사용하는 백엔드 서비스의 핵심 인터페이스입니다.
    // 다양한 백엔드 기능(인증, 분석, 데이터, 광고 등)을 통합적으로 제공하며,
    // 필요한 기능별로 특정 프로바이더를 선택하여 사용할 수 있습니다.
    public class BackendService : ABackendService, IBackendService
    {

        // 프로바이더를 선택적으로 가져오는 메서드입니다.
        // 예 : Firebase, Adx
        public IBackendProvider Provider(ProviderKey? key = null) => Get(_providers, key, nameof(IBackendProvider));

        // 인증 서비스를 선택적으로 가져오는 메서드입니다.
        // 예 : Firebase 인증
        public IBackendAuth Auth(ProviderKey? key = null) => Get(_auths, key, nameof(IBackendAuth));

        // 분석 서비스를 선택적으로 가져오는 메서드입니다.        
        // 예 : Firebase Analytics
        public IBackendAnalytics Analytics(ProviderKey? key = null) => Get(_analytics, key, nameof(IBackendAnalytics));

        // 데이터 서비스를 선택적으로 가져오는 메서드입니다.
        // 예 : Firestore 등 
        public IBackendDataStore DataStore(ProviderKey? key = null) => Get(_datastores, key, nameof(IBackendDataStore));

        // 광고 서비스를 선택적으로 가져오는 메서드입니다.
        // 예 : Adx 광고
        public IBackendAds Ads(ProviderKey? key = null) => Get(_ads, key, nameof(IBackendAds));



        [Inject]
        public BackendService(
            ILogger logger,
            IEnumerable<IBackendProvider> providers,
            IEnumerable<IBackendAuth> auths,
            IEnumerable<IBackendAnalytics> analytics,
            IEnumerable<IBackendDataStore> datastores,
            IEnumerable<IBackendAds> ads
        ) : base(logger, providers, auths, analytics, datastores, ads)
        {            
        }

    }
}
