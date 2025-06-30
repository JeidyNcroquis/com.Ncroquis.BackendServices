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
        private bool _isDisposed;
        private Action _pendingCallback;

        private CancellationTokenSource _cts = new();

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsInterstitial(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public bool IsInterstitialAdReady() => _interstitialAd?.IsLoaded() == true;

        public async Task LoadInterstitialAsync(CancellationToken externalToken = default)
        {
            if (_isDisposed || !_parent.IsInitialized)
            {
                _logger.Error("[ADX] 광고 로드를 시도했지만, 객체가 파괴되었거나 SDK가 초기화되지 않음");
                OnAdError?.Invoke();
                return;
            }

            if (_isLoading)
            {
                _logger.Log("[ADX] 전면 광고 이미 로딩 중입니다.");
                return;
            }

            _isLoading = true;
            InitializeAdIfNeeded();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, externalToken);
            var token = linkedCts.Token;
            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded() => tcs.TrySetResult(true);
            void OnFailed(int error) => tcs.TrySetException(new Exception(error.ToString()));

            _interstitialAd.OnAdLoaded += OnLoaded;
            _interstitialAd.OnAdFailedToLoad += OnFailed;

            try
            {
                _logger.Log("[ADX] 전면 광고 로드 요청");
                _interstitialAd.Load();

                using (token.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task;
                    _logger.Log("[ADX] 전면 광고 로드 완료");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("[ADX] 전면 광고 로드 취소됨");
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 전면 광고 로드 실패: {ex.Message}");
                OnAdError?.Invoke();
            }
            finally
            {
                if (_interstitialAd != null)
                {
                    _interstitialAd.OnAdLoaded -= OnLoaded;
                    _interstitialAd.OnAdFailedToLoad -= OnFailed;
                }

                _isLoading = false;
            }
        }

        public async Task ShowInterstitialAdAsync(Action onShown)
        {
            if (_isDisposed || !_parent.IsInitialized)
            {
                _logger.Warning("[ADX] 광고 표시 시도 실패: 객체가 파괴되었거나 SDK가 초기화되지 않음");
                OnAdError?.Invoke();
                return;
            }

            _pendingCallback = onShown;

            if (IsInterstitialAdReady())
            {
                _logger.Log("[ADX] 전면 광고 표시 요청");
                _interstitialAd.Show();
                return;
            }

            try
            {
                _logger.Log("[ADX] 광고 준비 안됨 → 로드 후 표시 시도");
                await LoadInterstitialAsync();

                if (IsInterstitialAdReady())
                {
                    _logger.Log("[ADX] 전면 광고 로드 성공 → 즉시 표시");
                    _interstitialAd.Show();
                }
                else
                {
                    _logger.Warning("[ADX] 광고 로드 후에도 준비되지 않음");
                    OnAdError?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 전면 광고 로드 실패: {ex.Message}");
                OnAdError?.Invoke();
            }
        }

        private void InitializeAdIfNeeded()
        {
            if (_interstitialAd == null)
            {
                _interstitialAd = new AdxInterstitialAd(_adUnitId);

                _interstitialAd.OnAdClosed += () =>
                {
                    _logger.Log("[ADX] 전면 광고 닫힘. 재로드 시도");
                    _ = LoadInterstitialAsync(); // fire-and-forget
                };

                _interstitialAd.OnPaidEvent += (ecpm) =>
                {
                    _pendingCallback?.Invoke();
                    _pendingCallback = null;
                    OnAdRevenue?.Invoke(_adUnitId, ecpm / 1000.0);
                };
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _cts.Cancel();
            _cts.Dispose();

            _interstitialAd?.Destroy();
            _interstitialAd = null;

            _isDisposed = true;
        }
    }
}
