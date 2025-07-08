using System;
using System.Threading;
using System.Threading.Tasks;
using AdxUnityPlugin;
using R3;

namespace Ncroquis.Backend
{
    public class AdxBackendAdsRewarded : IDisposable
    {
        private readonly AdxBackendAds _parent;
        private readonly ILogger _logger;
        private readonly string _adUnitId;

        private AdxRewardedAd _rewardedAd;
        private bool _isLoading;
        private bool _isDisposed;
        private Action _pendingCallback;

        private CancellationTokenSource _cts = new();
        private readonly CompositeDisposable _disposables = new();

        // R3 Subjects for event streams
        private readonly Subject<Unit> _adClosedSubject = new();
        private readonly Subject<Unit> _rewardEarnedSubject = new();
        private IDisposable _rewardHandlerDisposable;

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsRewarded(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public bool IsRewardedAdReady() => _rewardedAd?.IsLoaded() == true;

        public async Task LoadRewardedAdAsync(CancellationToken externalToken = default)
        {
            if (_isDisposed || !_parent.IsInitialized)
            {
                _logger.Warning("[ADX] 광고 로드를 시도했지만 객체가 파괴되었거나 SDK가 초기화되지 않음");
                OnAdError?.Invoke();
                return;
            }

            if (_isLoading)
            {
                _logger.Log("[ADX] 보상형 광고 이미 로딩 중입니다.");
                return;
            }

            _isLoading = true;

            if (_rewardedAd == null)
            {
                _rewardedAd = new AdxRewardedAd(_adUnitId);

                _rewardedAd.OnRewardedAdClosed += OnAdClosed;
                _rewardedAd.OnPaidEvent += OnAdPaid;
                _rewardedAd.OnRewardedAdEarnedReward += OnAdRewardEarned;
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, externalToken);
            var token = linkedCts.Token;
            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded() => tcs.TrySetResult(true);
            void OnFailed(int error) => tcs.TrySetException(new Exception(error.ToString()));

            _rewardedAd.OnRewardedAdLoaded += OnLoaded;
            _rewardedAd.OnRewardedAdFailedToLoad += OnFailed;

            try
            {
                _logger.Log("[ADX] 보상형 광고 로드 요청");
                _rewardedAd.Load();

                using (token.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task;
                    _logger.Log("[ADX] 보상형 광고 로드 완료");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Log("[ADX] 보상형 광고 로드 취소됨");
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 보상형 광고 로드 실패: {ex.Message}");
                OnAdError?.Invoke();
            }
            finally
            {
                if (_rewardedAd != null)
                {
                    _rewardedAd.OnRewardedAdLoaded -= OnLoaded;
                    _rewardedAd.OnRewardedAdFailedToLoad -= OnFailed;
                }

                _isLoading = false;
            }
        }

        public async Task ShowRewardedAdAsync(Action onRewarded)
        {
            if (_isDisposed || !_parent.IsInitialized)
            {
                _logger.Warning("[ADX] SDK가 초기화되지 않았거나 객체가 파괴됨");
                OnAdError?.Invoke();
                return;
            }

            _pendingCallback = onRewarded;

            // 이전 구독 정리
            _rewardHandlerDisposable?.Dispose();

            // 두 이벤트가 모두 발생했을 때 보상 처리
            _rewardHandlerDisposable = _adClosedSubject
                .Zip(_rewardEarnedSubject, (closed, earned) => Unit.Default)
                .Take(1)
                .Subscribe(async _ =>
                {
                    _logger.Log($"[ADX] 보상형 광고 닫힘 & 보상 획득 완료 [{DateTime.Now:HH:mm:ss.fff}]");
                    _pendingCallback?.Invoke();
                    _pendingCallback = null;

                    // 광고 재로드
                    await LoadRewardedAdAsync();
                });

            if (IsRewardedAdReady())
            {
                _logger.Log($"[ADX] 보상형 광고 표시 요청 [{DateTime.Now:HH:mm:ss.fff}]");
                _rewardedAd.Show();
                return;
            }

            try
            {
                _logger.Log("[ADX] 보상형 광고 준비되지 않음. 로드 후 표시 시도");
                await LoadRewardedAdAsync();

                if (IsRewardedAdReady())
                {
                    _logger.Log("[ADX] 보상형 광고 로드 성공. 즉시 표시");
                    _rewardedAd.Show();
                }
                else
                {
                    _logger.Warning("[ADX] 광고 로드 후에도 준비되지 않음");
                    _rewardHandlerDisposable?.Dispose();
                    OnAdError?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 보상형 광고 로드 실패: {ex.Message}");
                _rewardHandlerDisposable?.Dispose();
                OnAdError?.Invoke();
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _cts.Cancel();
            _cts.Dispose();

            _rewardHandlerDisposable?.Dispose();
            _adClosedSubject?.Dispose();
            _rewardEarnedSubject?.Dispose();
            _disposables?.Dispose();

            if (_rewardedAd != null)
            {
                _rewardedAd.OnRewardedAdClosed -= OnAdClosed;
                _rewardedAd.OnPaidEvent -= OnAdPaid;
                _rewardedAd.OnRewardedAdEarnedReward -= OnAdRewardEarned;

                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            _isDisposed = true;
        }


        // 
        // 이벤트 핸들러
        // 

        private void OnAdClosed()
        {
            _logger.Log($"[ADX] 보상형 광고 닫힘 [{DateTime.Now:HH:mm:ss.fff}]");
            _adClosedSubject.OnNext(Unit.Default);
        }

        private void OnAdPaid(double ecpm)
        {
            OnAdRevenue?.Invoke(_adUnitId, ecpm / 1000.0);
        }

        private void OnAdRewardEarned()
        {            
            _rewardEarnedSubject.OnNext(Unit.Default);
        }
    }
}