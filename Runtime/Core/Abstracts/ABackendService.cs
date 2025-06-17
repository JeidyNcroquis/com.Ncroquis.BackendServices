using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Ncroquis.Backend
{

    public abstract class ABackendService
    {

        [Inject] protected readonly BackendContainer _container;


        // 키를 명시하지 않으면 기본값으로 첫 번째 등록된 백엔드 타입을 사용
        public abstract IBackendProvider Provider(string key = null);
        public abstract IBackendAuth Auth(string key = null);
        public abstract IBackendAnalytics Analytics(string key = null);
        public abstract IBackendData Data(string key = null);
        public abstract IBackendAds Ads(string key = null);


        // 명시하지 않으면 기본값으로 첫 번째 등록된 백엔드 타입을 사용
        private string _defaultkey => _container.Providers.Keys.FirstOrDefault(); // Returns string [9]

        // 공통된 로직을 사용하여 각 서비스 타입에 대해 인스턴스를 가져옴
        protected T Get<T>(Dictionary<string, T> dict, string key, string label) where T : class // Changed BackendType? to string [9]
        {
            var actualKey = key ?? _defaultkey;
            if (actualKey == null) // Add null check for actualKey
            {
                UnityEngine.Debug.LogWarning($"[BackendService] {label}에 대해 기본 키가 설정되지 않아 null 반환");
                return null;
            }

            if (dict.TryGetValue(actualKey, out var instance))
                return instance;
                
            UnityEngine.Debug.LogWarning($"[BackendService] {label}에 대해 '{actualKey}'가 등록되지 않아 null 반환");
            return null;
        }

    }

}