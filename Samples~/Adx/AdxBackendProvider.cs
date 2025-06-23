using System.Threading;
using System.Threading.Tasks;
using VContainer;
using R3;

namespace Ncroquis.Backend
{
    public class AdxBackendProvider : IBackendProvider
    {

        [Inject] private readonly ILogger _logger;

        public ProviderKey providerKey => ProviderKey.ADX;

#if UNITY_ANDROID || UNITY_EDITOR
        private readonly string _adxAppId = "61ee18cecb8c670001000023"; //TEST
#elif UNITY_IPHONE
        private readonly string _adxAppId = "6200fea42a918d0001000001"; //TEST
#endif
        private readonly GdprType _gdprType = GdprType.POPUP_LOCATION;

        private readonly ReactiveProperty<bool> _isInitialized = new(false);
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized.ToReadOnlyReactiveProperty();

        private TaskCompletionSource<bool> _initializeTcs;


        

        public Task InitializeAsync(CancellationToken cancellation = default)
        {
            if (_isInitialized.Value)
            {
                _logger.Warning("[ADX] ADX SDK는 이미 초기화되었습니다.");
                return Task.CompletedTask;
            }

            if (_initializeTcs != null && !_initializeTcs.Task.IsCompleted)
            {
                _logger.Warning("[ADX] ADX SDK 초기화가 이미 진행 중입니다.");
                return _initializeTcs.Task;
            }

            cancellation.ThrowIfCancellationRequested();

            _initializeTcs = new TaskCompletionSource<bool>();

            cancellation.Register(() =>
            {
                _initializeTcs.TrySetCanceled(cancellation);
            });

            // UnityEditor 모드에서는 초기화를 생략하고 바로 완료로 처리
#if UNITY_EDITOR
            _logger.Log("[ADX] Editor모드에서는 ADX 초기화가 안돼서 생략합니다.");
            _isInitialized.Value = true;
            _initializeTcs.TrySetResult(true);
            return _initializeTcs.Task;
#else
            // 실제 디바이스에서는 기존 초기화 로직 수행
            AdxSDK.SetLogEnable(true);

            var adxConfiguration = new ADXConfiguration.Builder()
                .SetAppId(_adxAppId)
                .SetGdprType(_gdprType)
                .Build();

            AdxSDK.Initialize(adxConfiguration, OnADXConsentCompleted);

            return _initializeTcs.Task;
#endif
        }

        private void OnADXConsentCompleted(string s)
        {
            _logger.Log($"[ADX] ADX 동의 완료: {s}");

            _isInitialized.Value = true;
            _initializeTcs?.TrySetResult(true);
        }
    }
}