using System.Threading;
using System.Threading.Tasks;
using Firebase;
using R3;
using UnityEngine;


namespace Ncroquis.Backend
{
    public class FirebaseBackendProvider : IBackendProvider
    {
        public ProviderKey providerKey => ProviderKey.FIREBASE;

        private readonly ReactiveProperty<bool> _isInitialized = new(false);
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized.ToReadOnlyReactiveProperty();


        public async Task InitializeAsync(CancellationToken cancellation = default)
        {
            if (_isInitialized.Value)
            {
                Debug.Log($"[{providerKey}] 이미 초기화 됨");
                return;
            }

            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log($"[{providerKey}] 초기화 성공");
                _isInitialized.Value = true;
            }
            else
            {
                Debug.LogError($"[{providerKey}] 초기화 실패: {dependencyStatus}");
            }
        }
    }
}
