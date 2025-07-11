
using System.Collections.Generic;
using System.Linq;


namespace Ncroquis.Backend
{
    public abstract class ABackendService
    {
        protected readonly ILogger _logger;

        protected readonly Dictionary<ProviderKey, IBackendProvider> _providers;
        protected readonly Dictionary<ProviderKey, IBackendAuth> _auths;
        protected readonly Dictionary<ProviderKey, IBackendAnalytics> _analytics;
        protected readonly Dictionary<ProviderKey, IBackendDataStore> _datastores;
        protected readonly Dictionary<ProviderKey, IBackendAds> _ads;
        protected readonly Dictionary<ProviderKey, IBackendOfferwall> _offerwalls;

        protected ABackendService(
            ILogger logger,
            IEnumerable<IBackendProvider> providers,
            IEnumerable<IBackendAuth> auths,
            IEnumerable<IBackendAnalytics> analytics,
            IEnumerable<IBackendDataStore> datastores,
            IEnumerable<IBackendAds> ads,
            IEnumerable<IBackendOfferwall> offerwalls)
        {
            _logger = logger;
            _providers = providers.ToDictionary(p => p.providerKey);
            _auths = auths.ToDictionary(p => p.providerKey);
            _analytics = analytics.ToDictionary(p => p.providerKey);
            _datastores = datastores.ToDictionary(p => p.providerKey);
            _ads = ads.ToDictionary(p => p.providerKey);
            _offerwalls = offerwalls.ToDictionary(p => p.providerKey);
        }

                

        // ProviderKey 로 해당 백엔드를 가져온다.
        protected T Get<T>(Dictionary<ProviderKey, T> dict, ProviderKey? key, string label) where T : class
        {
            if (key != null)
            {
                if (dict.TryGetValue(key.Value, out var instance))
                    return instance;

                _logger.Warning($"[BackendService] {label} 키 '{key}'이(가) 존재하지 않습니다.");
                return null;
            }

            // key가 null일 경우: dict에 등록된 첫 번째 인스턴스를 사용
            if (dict.Count > 0)
                return dict.Values.FirstOrDefault();

            _logger.Warning($"[BackendService] {label} 등록된 인스턴스가 없습니다.");
            return null;
        }

    }
}