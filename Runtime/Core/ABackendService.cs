using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;


namespace Ncroquis.Backend
{
    public abstract class ABackendService
    {

        public abstract IBackendProvider Provider(string key = null);
        public abstract IBackendAuth Auth(string key = null);
        public abstract IBackendAnalytics Analytics(string key = null);
        public abstract IBackendData Data(string key = null);
        public abstract IBackendAds Ads(string key = null);




        protected readonly Dictionary<string, IBackendProvider> _providers;
        protected readonly Dictionary<string, IBackendAuth> _auths;
        protected readonly Dictionary<string, IBackendAnalytics> _analytics;
        protected readonly Dictionary<string, IBackendData> _datas;
        protected readonly Dictionary<string, IBackendAds> _ads;

        [Inject]
        protected ABackendService(
            IEnumerable<IBackendProvider> providers,
            IEnumerable<IBackendAuth> auths,
            IEnumerable<IBackendAnalytics> analytics,
            IEnumerable<IBackendData> datas,
            IEnumerable<IBackendAds> ads)
        {
            _providers = providers.ToDictionary(p => p.ProviderName);
            _auths = auths.ToDictionary(p => p.ProviderName);
            _analytics = analytics.ToDictionary(p => p.ProviderName);
            _datas = datas.ToDictionary(p => p.ProviderName);
            _ads = ads.ToDictionary(p => p.ProviderName);
        }

        private string DefaultKey => _providers.Keys.FirstOrDefault();

        protected T Get<T>(Dictionary<string, T> dict, string key, string label) where T : class
        {
            var actualKey = key ?? DefaultKey;
            if (actualKey == null)
            {
                Debug.LogWarning($"[BackendService] {label} default key is null");
                return null;
            }

            if (dict.TryGetValue(actualKey, out var instance))
                return instance;

            Debug.LogWarning($"[BackendService] {label} key '{actualKey}' not found");
            return null;
        }
        
    }
}
