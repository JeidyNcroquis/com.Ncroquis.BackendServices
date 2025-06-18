
using System.Collections.Generic;
using VContainer;


namespace Ncroquis.Backend
{
    public class BackendService : ABackendService, IBackendService
    {
        [Inject]
        public BackendService(
            IEnumerable<IBackendProvider> providers,
            IEnumerable<IBackendAuth> auths,
            IEnumerable<IBackendAnalytics> analytics,
            IEnumerable<IBackendData> datas,
            IEnumerable<IBackendAds> ads
        ) : base(providers, auths, analytics, datas, ads) { }

        public override IBackendProvider Provider(string key = null) => Get(_providers, key, nameof(IBackendProvider));
        public override IBackendAuth Auth(string key = null) => Get(_auths, key, nameof(IBackendAuth));
        public override IBackendAnalytics Analytics(string key = null) => Get(_analytics, key, nameof(IBackendAnalytics));
        public override IBackendData Data(string key = null) => Get(_datas, key, nameof(IBackendData));
        public override IBackendAds Ads(string key = null) => Get(_ads, key, nameof(IBackendAds));
    }
}
