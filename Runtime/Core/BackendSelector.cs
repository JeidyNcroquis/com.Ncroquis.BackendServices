using System.Collections.Generic;
using VContainer;



namespace Ncroquis.Backend
{

    // 다중 백엔드 구현체 를 위한 컨테이너 클래스


    public class BackendContainer
    {
        public Dictionary<BackendType, IBackendProvider> Providers { get; } = new();
        public Dictionary<BackendType, IBackendAuth> Auths { get; } = new();
        public Dictionary<BackendType, IBackendAnalytics> Analytics { get; } = new();
        public Dictionary<BackendType, IBackendData> Datas { get; } = new();
    }



    public class BackendSelector
    {
        public IBackendProvider Provider { get; }
        public IBackendAuth Auth { get; }
        public IBackendAnalytics Analytics { get; }
        public IBackendData Data { get; }

        private const BackendType FallbackKey = BackendType.FIREBASE;

        [Inject]
        public BackendSelector(BackendContainer container, BackendType selectedKey)
        {
            var fallbackKey = FallbackKey;

            Provider = TryGet(container.Providers, selectedKey, fallbackKey);
            Auth = TryGet(container.Auths, selectedKey, fallbackKey);
            Analytics = TryGet(container.Analytics, selectedKey, fallbackKey);
            Data = TryGet(container.Datas, selectedKey, fallbackKey);
        }

        private T TryGet<T>(Dictionary<BackendType, T> map, BackendType key, BackendType fallbackKey)
        {
            if (map.TryGetValue(key, out var value))
                return value;

            if (map.TryGetValue(fallbackKey, out var fallback))
            {
                UnityEngine.Debug.LogWarning($"[BackendSelector] '{key}' 항목이 없어 '{fallbackKey}'로 대체됩니다.");
                return fallback;
            }

            UnityEngine.Debug.LogError($"[BackendSelector] '{key}'와 기본값 '{fallbackKey}' 모두 등록되지 않았습니다.");
            return default;
        }
    }

}