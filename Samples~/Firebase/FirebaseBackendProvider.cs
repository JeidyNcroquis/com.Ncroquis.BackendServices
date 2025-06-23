using System.Threading;
using System.Threading.Tasks;
using Firebase;
using VContainer;
using R3;


namespace Ncroquis.Backend
{
    public class FirebaseBackendProvider : IBackendProvider
    {
        [Inject] private readonly ILogger _logger;
        public ProviderKey providerKey => ProviderKey.FIREBASE;

        private readonly ReactiveProperty<bool> _isInitialized = new(false);
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized.ToReadOnlyReactiveProperty();


        public async Task InitializeAsync(CancellationToken cancellation = default)
        {
            if (_isInitialized.Value)
            {
                _logger.Log($"[{providerKey}] 이미 초기화 됨");
                return;
            }

            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                _logger.Log($"[{providerKey}] 초기화 성공");
                _isInitialized.Value = true;
            }
            else
            {
                _logger.Error($"[{providerKey}] 초기화 실패: {dependencyStatus}");
            }
        }
    }
}
