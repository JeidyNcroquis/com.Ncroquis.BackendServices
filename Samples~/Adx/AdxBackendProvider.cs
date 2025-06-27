using System.Threading;
using System.Threading.Tasks;
using VContainer;
using R3;

namespace Ncroquis.Backend
{
    public class AdxBackendProvider : IBackendProvider
    {

        public ProviderKey providerKey => ProviderKey.ADX;

        private readonly string _adxAppId = "";
        private readonly GdprType _gdprType = GdprType.POPUP_LOCATION;


        private readonly ILogger _logger;

        private readonly ReactiveProperty<bool> _isInitialized = new(false);
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized.ToReadOnlyReactiveProperty();

        private TaskCompletionSource<bool> _initializeTcs;


        [Inject]
        public AdxBackendProvider(ILogger logger, string adxAppId = "")
        {
            _logger = logger;


            if (string.IsNullOrEmpty(adxAppId))
                _logger.Warning("[ADX PROVIDER] adxAppId가 비어 있습니다. 테스트용 ID가 사용됩니다.");
            

            // 값이 없거나 빈 문자열이면 플랫폼별 테스트용 사용            
#if UNITY_ANDROID
            _adxAppId = string.IsNullOrEmpty(adxAppId) ? "61ee18cecb8c670001000023" : adxAppId;
#elif UNITY_IPHONE
            _adxAppId = string.IsNullOrEmpty(adxAppId) ? "6200fea42a918d0001000001" : adxAppId;
#endif
        }



        public Task InitializeAsync(CancellationToken cancellation = default)
        {
            if (_isInitialized.Value)
            {
                _logger.Warning("[ADX PROVIDER] ADX SDK는 이미 초기화되었습니다.");
                return Task.CompletedTask;
            }

            if (_initializeTcs != null && !_initializeTcs.Task.IsCompleted)
            {
                _logger.Warning("[ADX PROVIDER] ADX SDK 초기화가 이미 진행 중입니다.");
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
            _logger.Log("[ADX PROVIDER] Editor모드에서는 ADX 초기화가 안돼서 생략합니다.");
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

            AdxSDK.Initialize(adxConfiguration, (s) => 
            {
                _logger.Log($"[ADX PROVIDER] ADX 동의 완료: {s}");

                _isInitialized.Value = true;
                _initializeTcs?.TrySetResult(true);
            });

            return _initializeTcs.Task;
#endif
        }

    }
}