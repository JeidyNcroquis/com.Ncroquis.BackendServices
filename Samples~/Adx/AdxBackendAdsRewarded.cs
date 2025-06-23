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
        private Action<double> _pendingCallback; 

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        
        public AdxBackendAdsRewarded(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public async Task LoadRewardedAsync(CancellationToken cancellationToken = default)
        {
            if (_isLoading)
            {
                _logger.Log("[ADX] 보상형 광고 이미 로딩 중입니다. 무시합니다.");
                return;
            }
            if (!_parent.IsInitialized)
            {
                _logger.Error("[ADX] ADX SDK가 초기화되지 않았습니다. 보상형 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            _isLoading = true;

            if (_rewardedAd == null)
                _rewardedAd = new AdxRewardedAd(_adUnitId); // _adUnitId 사용

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded()
            {
                _logger.Log("[ADX] 보상형 광고 로드 완료");
                tcs.TrySetResult(true);
                _isLoading = false;
            }
            void OnFailed(int error)
            {
                _logger.Error($"[ADX] 보상형 광고 로드 실패: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
                _isLoading = false;
            }

            _rewardedAd.OnRewardedAdLoaded -= OnLoaded;
            _rewardedAd.OnRewardedAdFailedToLoad -= OnFailed;
            _rewardedAd.OnRewardedAdLoaded += OnLoaded;
            _rewardedAd.OnRewardedAdFailedToLoad += OnFailed;

            _rewardedAd.Load();
            _logger.Log("[ADX] 보상형 광고 로드 요청");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    _logger.Log("[ADX] 보상형 광고 로드가 취소되었습니다.");
                    _isLoading = false;
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ADX] 보상형 광고 로드 중 예외 발생: {ex.Message}");
                    OnAdError?.Invoke();
                    _isLoading = false;
                    throw;
                }
            }
        }

        public void ShowRewardedAd(Action<double> onRewarded)
        {
            if (!_parent.IsInitialized)
            {
                _logger.Error("[ADX] ADX SDK가 초기화되지 않았습니다. 보상형 광고를 표시할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            void HandleAdClosed()
            {
                _logger.Log("[ADX] 보상형 광고 닫힘. 재로드 시도");
                _rewardedAd.OnRewardedAdClosed -= HandleAdClosed;
                OnAdRevenue?.Invoke(_adUnitId, 0); // _adUnitId 사용
                _ = LoadRewardedAsync();
            }

            void HandlePaidEvent(double ecpm)
            {
                onRewarded?.Invoke(ecpm / 1000f);
                OnAdRevenue?.Invoke(_adUnitId, ecpm / 1000f); // _adUnitId 사용
            }

            if (_rewardedAd != null && _rewardedAd.IsLoaded())
            {
                _rewardedAd.OnRewardedAdClosed -= HandleAdClosed;
                _rewardedAd.OnPaidEvent -= HandlePaidEvent;
                _rewardedAd.OnRewardedAdClosed += HandleAdClosed;
                _rewardedAd.OnPaidEvent += HandlePaidEvent;
                _rewardedAd.Show();
                _logger.Log("[ADX] 보상형 광고 표시 요청");
            }
            else
            {
                _logger.Log("[ADX] 보상형 광고 준비되지 않음. 로드 시도");
                _pendingCallback = onRewarded; // 실제로는 _pendingCallback으로 사용하세요

                try
                {
                    _ = LoadRewardedAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ADX] 보상형 광고 로드 실패: {ex.Message}");
                    OnAdError?.Invoke();
                }
            }
        }

        public bool IsRewardedAdReady()
        {
            return _rewardedAd != null && _rewardedAd.IsLoaded();
        }

        public void Dispose()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;
        }
    }
}
