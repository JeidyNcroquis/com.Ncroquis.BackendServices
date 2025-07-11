using System.Threading;
using System.Threading.Tasks;
using VContainer;
using R3;


namespace Ncroquis.Backend
{
    public class PointpubBackendProvider : IBackendProvider
    {

        public ProviderKey providerKey => ProviderKey.POINTPUB;

        public string UserId => _userId;
        private readonly string _appId = "";

        public string AppId => _appId;
        private readonly string _userId = "";


        private readonly ILogger _logger;

        private readonly ReactiveProperty<bool> _isInitialized = new(false);
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized.ToReadOnlyReactiveProperty();

        private TaskCompletionSource<bool> _initializeTcs;



        [Inject]
        public PointpubBackendProvider(ILogger logger, string offerwallAppId, string offerwallUserId)
        {
            _logger = logger;

            if (string.IsNullOrEmpty(offerwallAppId))
                _logger.Warning("[POINTPUB PROVIDER] AppId가 없습니다.");

            _appId = offerwallAppId;
            _userId = offerwallUserId;            
        }



        public Task InitializeAsync(CancellationToken cancellation = default)
        {
            if (_isInitialized.Value)
            {
                _logger.Warning("[POINTPUB PROVIDER] SDK는 이미 초기화되었습니다.");
                return Task.CompletedTask;
            }

            if (_initializeTcs != null && !_initializeTcs.Task.IsCompleted)
            {
                _logger.Warning("[POINTPUB PROVIDER] SDK 초기화가 이미 진행 중입니다.");
                return _initializeTcs.Task;
            }

            cancellation.ThrowIfCancellationRequested();

            _initializeTcs = new TaskCompletionSource<bool>();

            cancellation.Register(() =>
            {
                _initializeTcs.TrySetCanceled(cancellation);
            });



#if UNITY_EDITOR

#elif UNITY_ANDROID
            PointPubUnityPlugin.Android.PointPubSdkClient.Instance.EnableLogTrace();
            PointPubUnityPlugin.Android.PointPubSdkClient.Instance.SetAppId(_appId);
#elif UNITY_IOS || UNITY_IPHONE

#endif


            _isInitialized.Value = true;
            _initializeTcs?.TrySetResult(true);

            return _initializeTcs.Task;
        }
    }
}