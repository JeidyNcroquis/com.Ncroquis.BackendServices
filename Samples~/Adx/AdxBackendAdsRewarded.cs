using System;
using System.Threading;
using System.Threading.Tasks;
using AdxUnityPlugin;

namespace Ncroquis.Backend
{
    public class AdxBackendAdsRewarded : IDisposable
    {
        private readonly AdxBackendAds _parent;
        private readonly ILogger _logger;
        private readonly string _adUnitId;
        private AdxRewardedAd _rewardedAd;
        private bool _isLoading;
        private Action _pendingCallback;

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsRewarded(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public bool IsRewardedAdReady() => _rewardedAd?.IsLoaded() == true;


        public async Task LoadRewardedAdAsync(CancellationToken cancellationToken = default)
        {
            if (_isLoading || !_parent.IsInitialized)
            {
                if (_isLoading)
                {
                    _logger.Log("[ADX] 보상형 광고 이미 로딩 중입니다.");
                }
                else
                {
                    _logger.Warning("[ADX] SDK가 초기화되지 않았습니다.");
                    OnAdError?.Invoke();
                }
                return;
            }

            _isLoading = true;

            if (_rewardedAd == null)
            {
                _rewardedAd = new AdxRewardedAd(_adUnitId);
                _rewardedAd.OnRewardedAdClosed += () =>
                {
                    _logger.Log("[ADX] 보상형 광고 닫힘. 재로드 시도");
                    _ = LoadRewardedAdAsync();
                };
                _rewardedAd.OnPaidEvent += (ecpm) =>
                {
                    _pendingCallback?.Invoke();
                    _pendingCallback = null;
                    OnAdRevenue?.Invoke(_adUnitId, ecpm / 1000.0);
                };
            }

            var tcs = new TaskCompletionSource<bool>();
            void OnLoaded() => tcs.TrySetResult(true);
            void OnFailed(int error) => tcs.TrySetException(new Exception(error.ToString()));

            _rewardedAd.OnRewardedAdLoaded += OnLoaded;
            _rewardedAd.OnRewardedAdFailedToLoad += OnFailed;

            try
            {
                _logger.Log("[ADX] 보상형 광고 로드 요청");
                _rewardedAd.Load();

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task;
                    _logger.Log("[ADX] 보상형 광고 로드 완료");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[ADX] 보상형 광고 로드 실패: {ex.Message}");
                OnAdError?.Invoke();
            }
            finally
            {
                _rewardedAd.OnRewardedAdLoaded -= OnLoaded;
                _rewardedAd.OnRewardedAdFailedToLoad -= OnFailed;
                _isLoading = false;
            }
        }

        public async Task ShowRewardedAdAsync(Action onRewarded)
        {
            if (!_parent.IsInitialized)
            {
                _logger.Warning("[ADX] SDK가 초기화되지 않았습니다.");
                OnAdError?.Invoke();
                return;
            }

            _pendingCallback = onRewarded;

            if (_rewardedAd?.IsLoaded() == true)
            {
                _logger.Log("[ADX] 보상형 광고 표시 요청");
                _rewardedAd.Show();
                return;
            }

            try
            {
                _logger.Log("[ADX] 보상형 광고 준비되지 않음. 로드 후 표시 시도");
                await LoadRewardedAdAsync();

                if (_rewardedAd?.IsLoaded() == true)
                {
                    _logger.Log("[ADX] 보상형 광고 로드 성공. 즉시 표시");
                    _rewardedAd.Show();
                }
                else
                {
                    _logger.Warning("[ADX] 광고 로드 후에도 준비되지 않음");
                    OnAdError?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 보상형 광고 로드 실패: {ex.Message}");
                OnAdError?.Invoke();
            }
        }

        public void Dispose()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;
        }
    }
}
