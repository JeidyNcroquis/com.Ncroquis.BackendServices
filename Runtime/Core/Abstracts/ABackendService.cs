using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

namespace Ncroquis.Backend
{

    public abstract class ABackendService
    {

        [Inject] protected readonly BackendContainer _container;


        // 명시하지 않으면 기본값으로 첫 번째 등록된 백엔드 타입을 사용
        private BackendType _defaultkey => _container.Providers.Keys.FirstOrDefault();

        // 공통된 로직을 사용하여 각 서비스 타입에 대해 인스턴스를 가져옴
        protected T Get<T>(Dictionary<BackendType, T> dict, BackendType? key, string label) where T : class
        {
            var actualKey = key ?? _defaultkey;

            if (dict.TryGetValue(actualKey, out var instance))
                return instance;

            Debug.LogWarning($"[BackendService] {label}에 대해 '{actualKey}'가 등록되지 않아 null 반환");
            return null;
        }

    }

}