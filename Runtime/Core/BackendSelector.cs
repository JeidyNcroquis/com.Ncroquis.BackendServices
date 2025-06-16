using System.Collections.Generic;
using VContainer;



namespace Ncroquis.Backend
{

    // 다중 백엔드 구현체 를 위한 컨테이너 추상 클래스

    public class BackendContainer
    {
        public Dictionary<string, IBackendProvider> Providers { get; } = new(); // Changed to string
        public Dictionary<string, IBackendAuth> Auths { get; } = new();         // Changed to string
        public Dictionary<string, IBackendAnalytics> Analytics { get; } = new(); // Changed to string
        public Dictionary<string, IBackendData> Datas { get; } = new();         // Changed to string
    }



    public class BackendSelector
    {
        public IBackendProvider Provider { get; }
        public IBackendAuth Auth { get; }
        public IBackendAnalytics Analytics { get; }
        public IBackendData Data { get; }

        private const string FallbackKey = "NONE";

        [Inject]
        public BackendSelector(BackendContainer container, string selectedKey) // Changed BackendType to string [5]
        {
            var fallbackKey = FallbackKey;
            Provider = TryGet(container.Providers, selectedKey, fallbackKey);
            Auth = TryGet(container.Auths, selectedKey, fallbackKey);
            Analytics = TryGet(container.Analytics, selectedKey, fallbackKey);
            Data = TryGet(container.Datas, selectedKey, fallbackKey);
        }

        private T TryGet<T>(Dictionary<string, T> map, string key, string fallbackKey) // Changed BackendType to string [6]
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