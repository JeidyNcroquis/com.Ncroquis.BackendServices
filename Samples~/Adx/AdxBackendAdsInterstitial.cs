using System;
using System.Threading;
using System.Threading.Tasks;
using AdxUnityPlugin;
using Cysharp.Threading.Tasks;
using R3;

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
        private readonly CompositeDisposable _disposables = new();

        // R3 Subjects for event streams
        private readonly Subject<Unit> _adShownSubject = new();
        private readonly Subject<Unit> _adClosedSubject = new();

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsInterstitial(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _adUnitId = adUnitId ?? throw new ArgumentNullException(nameof(adUnitId));
        }

        public bool IsInterstitialAdReady() => _interstitialAd?.IsLoaded() == true;

        public async Task LoadInterstitialAsync(CancellationToken externalToken = default)
        {
            if (_isDisposed || !_parent.IsInitialized)
            {
                _logger.Warning("[ADX] 광고 로드를 시도했지만 객체가 파괴되었거나 SDK가 초기화되지 않음");
                OnAdError?.Invoke();
                return;
            }

            if (_isLoading)
            {
                _logger.Log("[ADX] 전면 광고 이미 로딩 중입니다.");
                return;
            }

            _isLoading = true;

            try
            {
                // 광고 인스턴스 초기화
                if (_interstitialAd == null)
                {
                    InitializeInterstitialAd();
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, externalToken);
                var token = linkedCts.Token;
                var tcs = new TaskCompletionSource<bool>();

                void OnLoaded() => tcs.TrySetResult(true);
                void OnFailed(int error) => tcs.TrySetException(new Exception($"Ad load failed with error: {error}"));

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
                finally
                {
                    _interstitialAd.OnAdLoaded -= OnLoaded;
                    _interstitialAd.OnAdFailedToLoad -= OnFailed;
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
                _isLoading = false;
            }
        }

        public async Task ShowInterstitialAdAsync(Action onCompleted)
        {
            if (_isDisposed || !_parent.IsInitialized)
            {
                _logger.Warning("[ADX] SDK가 초기화되지 않았거나 객체가 파괴됨");
                OnAdError?.Invoke();
                return;
            }

            if (_pendingCallback != null)
            {
                _logger.Warning("[ADX] 이미 전면 광고가 진행 중입니다.");
                return;
            }

            _pendingCallback = onCompleted;

            // 광고 표시 전 완료 처리 구독 설정
            // CombineLatest를 사용하여 두 이벤트가 모두 발생했을 때 완료 처리
            Observable.CombineLatest(
                    _adShownSubject.Take(1),            // 광고 표시 이벤트 (첫 번째만)
                    _adClosedSubject.Take(1),           // 광고 닫힘 이벤트 (첫 번째만)
                    (shown, closed) => Unit.Default     // 두 이벤트를 Unit으로 결합
                )
                .Take(1) // 두 조건이 모두 충족되었을 때 한 번만 실행
                .Subscribe(_ =>
                {
                    UniTask.Post(() =>
                    {
                        _logger.Log($"[ADX] 전면 광고 표시 & 닫힘 완료 [{DateTime.Now:HH:mm.ss}]");

                        // 완료 콜백 실행
                        var callback = _pendingCallback;
                        _pendingCallback = null;
                        callback?.Invoke();

                        // 광고 재로드 (Fire and Forget 방식)
                        ReloadAdAsync().Forget();
                    });
                })
                .AddTo(_disposables);


            if (IsInterstitialAdReady())
            {
                _logger.Log($"[ADX] 전면 광고 표시 요청 [{DateTime.Now:HH:mm.ss}]");
                _interstitialAd.Show();
                return;
            }


            try
            {
                _logger.Log("[ADX] 전면 광고 준비되지 않음. 로드 후 표시 시도");
                await LoadInterstitialAsync(_cts.Token);

                if (IsInterstitialAdReady())
                {
                    _logger.Log("[ADX] 전면 광고 로드 성공. 즉시 표시");
                    _interstitialAd.Show();
                }
                else
                {
                    _logger.Warning("[ADX] 광고 로드 후에도 준비되지 않음");
                    ResetPendingCallback();
                    OnAdError?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 전면 광고 로드 실패: {ex.Message}");
                ResetPendingCallback();
                OnAdError?.Invoke();
            }
        }


        /// <summary>
        /// 광고 재로드를 Fire and Forget 방식으로 처리하는 UniTask 메서드
        /// </summary>
        private async UniTask ReloadAdAsync()
        {
            try
            {
                await LoadInterstitialAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Log("[ADX] 광고 재로드 취소됨");
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 광고 재로드 실패: {ex.Message}");
                OnAdError?.Invoke();
            }
        }

        /// <summary>
        /// 전면 광고 인스턴스 초기화
        /// </summary>
        private void InitializeInterstitialAd()
        {
            _interstitialAd = new AdxInterstitialAd(_adUnitId);
            _interstitialAd.OnAdShown += OnAdShown;
            _interstitialAd.OnAdClosed += OnAdClosed;
            _interstitialAd.OnPaidEvent += OnAdPaid;
        }

        /// <summary>
        /// 진행 중인 완료 콜백 초기화
        /// </summary>
        private void ResetPendingCallback()
        {
            _pendingCallback = null;
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _cts.Cancel();
            _cts.Dispose();

            _disposables?.Dispose();
            _adShownSubject?.Dispose();
            _adClosedSubject?.Dispose();

            if (_interstitialAd != null)
            {
                _interstitialAd.OnAdShown -= OnAdShown;
                _interstitialAd.OnAdClosed -= OnAdClosed;
                _interstitialAd.OnPaidEvent -= OnAdPaid;

                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            _pendingCallback = null;
            _isDisposed = true;
        }

        // 
        // 이벤트 핸들러
        // 

        private void OnAdShown()
        {
            UniTask.Post(() =>_adShownSubject.OnNext(Unit.Default));
        }

        private void OnAdClosed()
        {
            UniTask.Post(() =>_adClosedSubject.OnNext(Unit.Default));
        }

        private void OnAdPaid(double ecpm)
        {
            UniTask.Post(() =>OnAdRevenue?.Invoke(_adUnitId, ecpm / 1000.0));
        }

    }
}