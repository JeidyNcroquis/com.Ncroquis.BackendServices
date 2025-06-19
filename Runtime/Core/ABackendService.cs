// Ncroquis/Backend/ABackendService.cs

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ncroquis.Backend
{
    public abstract class ABackendService
    {
        
        protected readonly Dictionary<ProviderKey, IBackendProvider> _providers;
        protected readonly Dictionary<ProviderKey, IBackendAuth> _auths;
        protected readonly Dictionary<ProviderKey, IBackendAnalytics> _analytics;
        protected readonly Dictionary<ProviderKey, IBackendData> _datas;
        protected readonly Dictionary<ProviderKey, IBackendAds> _ads;

        protected ABackendService(
            IEnumerable<IBackendProvider> providers,
            IEnumerable<IBackendAuth> auths,
            IEnumerable<IBackendAnalytics> analytics,
            IEnumerable<IBackendData> datas,
            IEnumerable<IBackendAds> ads)
        {
            // 변경된 부분: ProviderName 대신 ProviderKey를 사용
            _providers = providers.ToDictionary(p => p.providerKey);
            _auths = auths.ToDictionary(p => p.providerKey);
            _analytics = analytics.ToDictionary(p => p.providerKey);
            _datas = datas.ToDictionary(p => p.providerKey);
            _ads = ads.ToDictionary(p => p.providerKey);
        }

        
        // 만약 등록된 provider가 없으면 null을 반환하여 기존 로직을 유지합니다.
        private ProviderKey? DefaultKey => _providers.Keys.Any() ? _providers.Keys.First() : (ProviderKey?)null;

        // ProviderKey 로 해당 백엔드를 가져온다.
        protected T Get<T>(Dictionary<ProviderKey, T> dict, ProviderKey? key, string label) where T : class
        {
            // key가 null이면 DefaultKey를 사용합니다.
            var actualKey = key ?? DefaultKey;

            // 실제 키가 null이면 (기본 키도 없고, 명시적 키도 없으면) 경고를 로그하고 null을 반환합니다.
            if (actualKey == null)
            {
                Debug.LogWarning($"[BackendService] {label} default key is null. No providers registered.");
                return null;
            }

            // actualKey는 nullable이므로 .Value를 사용하여 실제 키 값으로 딕셔너리에서 찾습니다.
            if (dict.TryGetValue(actualKey.Value, out var instance))
                return instance;

            Debug.LogWarning($"[BackendService] {label} key '{actualKey}' not found");
            return null;
        }
    }
}