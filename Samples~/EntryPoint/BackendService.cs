using System.Collections.Generic;

namespace Ncroquis.Backend
{
    // 외부에서 사용할 BackendService 인터페이스 입니다.
    public class BackendService : ABackendService, IBackendService
    {

        public readonly ILogger Logger;

        public BackendService(
            ILogger logger,
            IEnumerable<IBackendProvider> providers,
            IEnumerable<IBackendAuth> auths,
            IEnumerable<IBackendAnalytics> analytics,
            IEnumerable<IBackendData> datas,
            IEnumerable<IBackendAds> ads
        ) : base(providers, auths, analytics, datas, ads)
        {
            Logger = logger;
        }

        public IBackendProvider Provider(ProviderKey? key = null) => Get(_providers, key, nameof(IBackendProvider));
        public IBackendAuth Auth(ProviderKey? key = null) => Get(_auths, key, nameof(IBackendAuth));
        public IBackendAnalytics Analytics(ProviderKey? key = null) => Get(_analytics, key, nameof(IBackendAnalytics));
        public IBackendData Data(ProviderKey? key = null) => Get(_datas, key, nameof(IBackendData));
        public IBackendAds Ads(ProviderKey? key = null) => Get(_ads, key, nameof(IBackendAds));

    }
}
