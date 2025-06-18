
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using AdxUnityPlugin;
using R3;
using VContainer;


namespace Ncroquis.Backend
{

    public class AdxBackendProvider : IBackendProvider
    {
        public string ProviderName => BackendKeys.ADX;

        private readonly string _adxAppId;
        private readonly GdprType _gdprType;

        private readonly ReactiveProperty<bool> _isInitialized = new(false);
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized.ToReadOnlyReactiveProperty();

        private TaskCompletionSource<bool> _initializeTcs;


        
        /// <param name="adxAppId">ADX에서 발급받은 App ID</param>
        /// <param name="gdprType">GDPR 설정 방식</param>
        public AdxBackendProvider(string adxAppId, GdprType gdprType = GdprType.POPUP_DEBUG)
        {
            _adxAppId = adxAppId;
            _gdprType = gdprType;
        }

        public Task InitializeAsync(CancellationToken cancellation = default)
        {
            if (_isInitialized.Value)
            {
                Debug.LogWarning("[ADX Backend Provider] ADX SDK는 이미 초기화되었습니다.");
                return Task.CompletedTask;
            }

            if (_initializeTcs != null && !_initializeTcs.Task.IsCompleted)
            {
                Debug.LogWarning("[ADX Backend Provider] ADX SDK 초기화가 이미 진행 중입니다.");
                return _initializeTcs.Task;
            }

            cancellation.ThrowIfCancellationRequested();

            _initializeTcs = new TaskCompletionSource<bool>();

            cancellation.Register(() =>
            {
                _initializeTcs.TrySetCanceled(cancellation);
            });

            AdxSDK.SetLogEnable(true);

            var adxConfiguration = new ADXConfiguration.Builder()
                .SetAppId(_adxAppId)
                .SetGdprType(_gdprType)
                .Build();

            AdxSDK.Initialize(adxConfiguration, OnADXConsentCompleted);

            return _initializeTcs.Task;
        }

        private void OnADXConsentCompleted(string s)
        {
            Debug.LogFormat("[ADX Backend Provider] ADX 동의 완료: {0}", s);

            _isInitialized.Value = true;
            _initializeTcs?.TrySetResult(true);
        }
    }

}