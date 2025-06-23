using System;
using System.Threading;
using System.Threading.Tasks;
using AdxUnityPlugin;

namespace Ncroquis.Backend
{
    public class AdxBackendAdsInterstitial : IDisposable
    {
        private readonly AdxBackendAds _parent;
        private readonly ILogger _logger;
        private readonly string _adUnitId;
        private AdxInterstitialAd _interstitialAd;
        private bool _isLoading;
        private Action _pendingCallback;

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        // 생성자에서 adUnitId를 받음
        public AdxBackendAdsInterstitial(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public async Task LoadInterstitialAsync(CancellationToken cancellationToken = default)
        {
            if (_isLoading)
            {
                _logger.Log("[ADX] 전면 광고 이미 로딩 중입니다. 무시합니다.");
                return;
            }
            if (!_parent.IsInitialized)
            {
                _logger.Error("[ADX] ADX SDK가 초기화되지 않았습니다. 전면 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            _isLoading = true;

            if (_interstitialAd == null)
                _interstitialAd = new AdxInterstitialAd(_adUnitId); // adUnitId를 사용

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded()
            {
                _logger.Log("[ADX] 전면 광고 로드 완료");
                tcs.TrySetResult(true);
                _isLoading = false;
            }
            void OnFailed(int error)
            {
                _logger.Error($"[ADX] 전면 광고 로드 실패: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
                _isLoading = false;
            }

            _interstitialAd.OnAdLoaded -= OnLoaded;
            _interstitialAd.OnAdFailedToLoad -= OnFailed;
            _interstitialAd.OnAdLoaded += OnLoaded;
            _interstitialAd.OnAdFailedToLoad += OnFailed;

            _interstitialAd.Load();
            _logger.Log("[ADX] 전면 광고 로드 요청");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    _logger.Log("[ADX] 전면 광고 로드가 취소되었습니다.");
                    _isLoading = false;
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ADX] 전면 광고 로드 중 예외 발생: {ex.Message}");
                    OnAdError?.Invoke();
                    _isLoading = false;
                    throw;
                }
            }
        }

        public void ShowInterstitialAd(Action onShown, Action onClose)
        {
            if (!_parent.IsInitialized) // 오타일 수 있음, _parent.IsInitialized로 수정하세요
            {
                _logger.Error("[ADX] ADX SDK가 초기화되지 않았습니다. 전면 광고를 표시할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            void HandleAdShown(double ecpm)
            {
                _logger.Log($"[ADX] 전면 광고 표시됨. 수익: {ecpm / 1000f}");
                _interstitialAd.OnPaidEvent -= HandleAdShown;
                onShown?.Invoke();
                OnAdRevenue?.Invoke(_adUnitId, ecpm / 1000f); // _adUnitId 사용
                _ = LoadInterstitialAsync();
            }

            void HandleAdClosed()
            {
                _logger.Log("[ADX] 전면 광고 닫힘");
                _interstitialAd.OnAdClosed -= HandleAdClosed;
                onClose?.Invoke();
                _ = LoadInterstitialAsync();
            }

            if (_interstitialAd != null && _interstitialAd.IsLoaded())
            {
                _interstitialAd.OnPaidEvent -= HandleAdShown;
                _interstitialAd.OnPaidEvent += HandleAdShown;
                _interstitialAd.OnAdClosed -= HandleAdClosed;
                _interstitialAd.OnAdClosed += HandleAdClosed;
                _interstitialAd.Show();
                _logger.Log("[ADX] 전면 광고 표시 요청");
            }
            else
            {
                _logger.Log("[ADX] 전면 광고 준비되지 않음. 로드 시도");
                _pendingCallback = onShown;

                try
                {
                    _ = LoadInterstitialAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ADX] 전면 광고 로드 실패: {ex.Message}");
                    OnAdError?.Invoke();
                }
            }
        }

        public bool IsInterstitialAdReady()
        {
            return _interstitialAd != null && _interstitialAd.IsLoaded();
        }

        public void Dispose()
        {
            _interstitialAd?.Destroy();
            _interstitialAd = null;
        }
    }
}
