using System;
using System.Threading;
using System.Threading.Tasks;
using AdxUnityPlugin;
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

        
        // OnAdShown 이벤트를 위한 Subject
        private readonly Subject<Unit> _adShownSubject = new();
        // OnAdClosed 이벤트를 위한 Subject
        private readonly Subject<Unit> _adClosedSubject = new();
        // Zip 구독 관리를 위한 Disposable
        private IDisposable _completionHandlerDisposable;
        

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
            if (_interstitialAd == null)
            {
                _interstitialAd = new AdxInterstitialAd(_adUnitId);

                // 이벤트 핸들러를 별도 메소드로 연결
                _interstitialAd.OnAdShown += OnAdShown;
                _interstitialAd.OnAdClosed += OnAdClosed;
                _interstitialAd.OnPaidEvent += OnAdPaid;
            }

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

        // ShowInterstitialAdAsync 수정: R3 Zip 로직 추가
        public async Task ShowInterstitialAdAsync(Action onCompleted)
        {
            if (_isDisposed || !_parent.IsInitialized)
            {
                _logger.Warning("[ADX] 광고 표시 시도 실패: 객체가 파괴되었거나 SDK가 초기화되지 않음");
                OnAdError?.Invoke();
                return;
            }

            _pendingCallback = onCompleted;

            // 이전 구독 정리
            _completionHandlerDisposable?.Dispose();

            // OnAdShown과 OnAdClosed가 모두 발생했을 때 완료 처리 설정
            _completionHandlerDisposable = _adShownSubject
                .Zip(_adClosedSubject, (shown, closed) => Unit.Default)
                .Take(1) // 한 번만 실행되도록 설정
                .Subscribe(async _ =>
                {
                    _logger.Log($"[ADX] 전면 광고 시청 완료 (Shown & Closed 모두 발생) [{DateTime.Now:HH:mm:ss.fff}]");

                    // 완료 콜백 실행
                    _pendingCallback?.Invoke();
                    _pendingCallback = null;

                    // 광고 시청 완료 후 자동 재로드
                    _logger.Log("[ADX] 전면 광고 재로드 시작");
                    await LoadInterstitialAsync();
                });

            if (IsInterstitialAdReady())
            {
                _logger.Log($"[ADX] 전면 광고 표시 요청 [{DateTime.Now:HH:mm:ss.fff}]");
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
                    // 로드 실패 시 구독 해제
                    _completionHandlerDisposable?.Dispose();
                    OnAdError?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"[ADX] 전면 광고 로드 실패: {ex.Message}");
                // 로드 실패 시 구독 해제
                _completionHandlerDisposable?.Dispose();
                OnAdError?.Invoke();
            }
        }


        // Dispose 수정: R3 Subject 및 Disposable 정리 추가
        public void Dispose()
        {
            if (_isDisposed) return;

            _cts.Cancel();
            _cts.Dispose();

            // R3 리소스 정리
            _completionHandlerDisposable?.Dispose();
            _adShownSubject?.Dispose();
            _adClosedSubject?.Dispose();

            if (_interstitialAd != null)
            {
                // 이벤트 핸들러 해제
                _interstitialAd.OnAdShown -= OnAdShown;
                _interstitialAd.OnAdClosed -= OnAdClosed;
                _interstitialAd.OnPaidEvent -= OnAdPaid;

                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            _isDisposed = true;
        }


        // 
        // 이벤트 핸들러
        // 

        private void OnAdShown()
        {
            // OnAdShown 발생 시 Subject에 알림
            _adShownSubject.OnNext(Unit.Default);
        }

        private void OnAdClosed()
        {
            _logger.Log($"[ADX] 전면 광고 닫힘 (Closed) [{DateTime.Now:HH:mm:ss.fff}]");
            // OnAdClosed 발생 시 Subject에 알림
            _adClosedSubject.OnNext(Unit.Default);
            // 참고: 재로드는 ShowInterstitialAdAsync의 Subscribe 내부에서 처리됩니다.
        }

        private void OnAdPaid(double ecpm)
        {
            // 수익 이벤트만 처리합니다. 완료 콜백(_pendingCallback)은 여기서 호출하지 않습니다.
            OnAdRevenue?.Invoke(_adUnitId, ecpm / 1000.0);
        }
    }
}