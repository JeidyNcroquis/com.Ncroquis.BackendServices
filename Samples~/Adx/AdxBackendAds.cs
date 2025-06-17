using System;
using UnityEngine;
using VContainer; // ADX SDK 관련 클래스를 사용하기 위해 필요합니다. [1]
using AdxUnityPlugin;


namespace Ncroquis.Backend
{
    /// <summary>
    /// ADX 라이브러리의 광고 기능 인터페이스(IBackendAds) 구현체입니다.
    /// ADX SDK를 초기화하고, 배너, 전면, 보상형 광고의 로드 및 표시를 관리합니다.
    /// </summary>
    public class AdxBackendAds : IBackendAds
    {

        [Inject] protected readonly AdxBackendProvider _adxprovider;
                

        // ADX 광고 인스턴스
        private AdxBannerAd _bannerAd;
        private AdxInterstitialAd _interstitialAd;
        private AdxRewardedAd _rewardedAd;

        // IBackendAds 인터페이스 이벤트 [7, 8]
        public event Action OnAdError;
        // OnAdRevenue 이벤트는 IBackendAds 정의상 Action 파라미터가 없으나,
        // 소스 설명(광고 단위 ID, 수익 금액 [8]) 및 ADX OnPaidEvent 콜백 [9-11]의 유용성을 고려하여
        // Action<string, double> (광고 이름, 수익 금액)으로 구현했습니다.
        public event Action<string, double> OnAdRevenue;


        /// <summary>
        /// AdxSDK.Initialize 호출 후 ADX 동의 절차가 완료될 때 호출되는 콜백 메서드입니다. 
        /// 이 콜백이 호출된 후, 광고 관련 로직을 진행해야 합니다. 
        /// </summary>
        private void OnADXConsentCompleted(string s)
        {
            Debug.LogFormat("[ADX Backend Ads] ADX 동의 완료: {0}", s);
            
        }

        /// <summary>
        /// 배너 광고를 로드합니다.
        /// ADX 배너 광고는 크기와 위치를 생성자에 명시해야 합니다.
        /// IBackendAds 인터페이스는 이러한 파라미터를 제공하지 않으므로, 기본값(320x50, 상단)을 사용합니다.
        /// </summary>
        /// <param name="adUnitId">광고 단위 ID</param>
        public void LoadBannerAd(string adUnitId)
        {
            if (!_adxprovider.IsInitialized)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 배너 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke(); // 오류 이벤트 트리거
                return;
            }

            // 기존 배너 광고가 있다면 파괴하고 다시 생성합니다.
            if (_bannerAd != null)
            {
                _bannerAd.Destroy();
                _bannerAd = null;
                Debug.Log("[ADX Backend Ads] 기존 배너 광고를 파괴하고 다시 로드합니다.");
            }

            // 배너 광고 인스턴스 생성 (기본 크기 및 위치 사용)
            _bannerAd = new AdxBannerAd(adUnitId, AdxBannerAd.AD_SIZE_320x50, AdxBannerAd.POSITION_TOP);

            // 이벤트 핸들러 등록
            _bannerAd.OnAdLoaded += () =>
            {
                Debug.Log("[ADX Backend Ads] 배너 광고 로드 성공.");
            };
            _bannerAd.OnAdFailedToLoad += (error) =>
            {
                Debug.LogError($"[ADX Backend Ads] 배너 광고 로드 실패: {error}");
                OnAdError?.Invoke(); // 오류 이벤트 트리거
            };
            _bannerAd.OnAdClicked += () =>
            {
                Debug.Log("[ADX Backend Ads] 배너 광고 클릭됨.");
            };
            _bannerAd.OnPaidEvent += (ecpm) =>
            {
                // 예상 광고 수익 계산
                double revenue = ecpm / 1000f;
                Debug.Log($"[ADX Backend Ads] 배너 광고 수익 발생: {revenue} USD (eCPM: {ecpm})");
                OnAdRevenue?.Invoke("ADX Banner Ad", revenue); // 수익 이벤트 트리거
            };

            _bannerAd.Load(); // 광고 로드 요청. ADX 배너 광고는 Load() 시 자동으로 표시됩니다.
            Debug.Log($"[ADX Backend Ads] 배너 광고 로드 요청: {adUnitId}");
        }

        /// <summary>
        /// 로드된 배너 광고를 표시합니다.
        /// ADX 배너 광고는 Load() 시 자동으로 표시되므로, 이 메서드는 단순히 광고가 로드되었는지 확인하는 역할을 합니다.
        /// </summary>
        public void ShowBannerAd()
        {
            if (!_adxprovider.IsInitialized)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 배너 광고를 표시할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }
            if (_bannerAd != null)
            {
                Debug.Log("[ADX Backend Ads] 배너 광고가 이미 로드되어 있거나 로드 요청되었습니다. (ADX 배너 광고는 Load() 시 표시됩니다.)");
                // ADX Banner Ad는 Load()를 호출하면 자동으로 표시됩니다.
                // 별도의 Show() 메서드가 없으므로, LoadBannerAd가 성공적으로 호출되었다면 이미 표시된 상태로 간주합니다.
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 표시할 배너 광고 인스턴스가 없습니다. LoadBannerAd를 먼저 호출해야 합니다.");
                OnAdError?.Invoke();
            }
        }

        /// <summary>
        /// 표시된 배너 광고를 숨깁니다.
        /// ADX SDK에서는 배너 광고 인스턴스를 파괴함으로써 숨기는 효과를 냅니다.
        /// </summary>
        public void HideBannerAd()
        {
            if (_bannerAd != null)
            {
                _bannerAd.Destroy(); // 광고 인스턴스 파괴
                _bannerAd = null;
                Debug.Log("[ADX Backend Ads] 배너 광고를 숨기고 파괴했습니다.");
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 파괴할 배너 광고 인스턴스가 없습니다.");
            }
        }

        /// <summary>
        /// 전면 광고를 로드합니다.
        /// </summary>
        /// <param name="adUnitId">광고 단위 ID</param>
        public void LoadInterstitialAd(string adUnitId)
        {
            if (!_adxprovider.IsInitialized)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 전면 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke(); // 오류 이벤트 트리거
                return;
            }

            if (_interstitialAd == null)
            {
                _interstitialAd = new AdxInterstitialAd(adUnitId);

                // 이벤트 핸들러 등록
                _interstitialAd.OnAdLoaded += () =>
                {
                    Debug.Log("[ADX Backend Ads] 전면 광고 로드 성공.");
                };
                _interstitialAd.OnAdFailedToLoad += (error) =>
                {
                    Debug.LogError($"[ADX Backend Ads] 전면 광고 로드 실패: {error}");
                    OnAdError?.Invoke(); // 오류 이벤트 트리거
                };
                _interstitialAd.OnAdClicked += () =>
                {
                    Debug.Log("[ADX Backend Ads] 전면 광고 클릭됨.");
                };
                _interstitialAd.OnAdShown += () =>
                {
                    Debug.Log("[ADX Backend Ads] 전면 광고 표시됨.");
                };
                _interstitialAd.OnAdClosed += () =>
                {
                    Debug.Log("[ADX Backend Ads] 전면 광고 닫힘.");
                };
                _interstitialAd.OnAdFailedToShow += () =>
                {
                    Debug.LogError($"[ADX Backend Ads] 전면 광고 표시 실패:");
                    OnAdError?.Invoke(); // 오류 이벤트 트리거
                };
                _interstitialAd.OnPaidEvent += (ecpm) =>
                {
                    // 예상 광고 수익 계산
                    double revenue = ecpm / 1000f;
                    Debug.Log($"[ADX Backend Ads] 전면 광고 수익 발생: {revenue} USD (eCPM: {ecpm})");
                    OnAdRevenue?.Invoke("ADX Interstitial Ad", revenue); // 수익 이벤트 트리거
                };
            }
            _interstitialAd.Load(); // 광고 로드 요청
            Debug.Log($"[ADX Backend Ads] 전면 광고 로드 요청: {adUnitId}");
        }

        /// <summary>
        /// 로드된 전면 광고를 표시합니다.
        /// </summary>
        public void ShowInterstitialAd()
        {
            if (!_adxprovider.IsInitialized)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 전면 광고를 표시할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }
            if (_interstitialAd != null && _interstitialAd.IsLoaded()) // 광고가 로드되어 준비되었는지 확인 [16]
            {
                _interstitialAd.Show(); // 광고 표시
                Debug.Log("[ADX Backend Ads] 전면 광고 표시 요청.");
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 전면 광고가 로드되지 않았거나 아직 준비되지 않았습니다.");
                OnAdError?.Invoke(); // 오류 이벤트 트리거
            }
        }

        /// <summary>
        /// 전면 광고가 로드되어 표시 준비가 되었는지 확인합니다.
        /// </summary>
        /// <returns>광고 준비 여부</returns>
        public bool IsInterstitialAdReady()
        {
            return _interstitialAd != null && _interstitialAd.IsLoaded();
        }

        /// <summary>
        /// 보상형 광고를 로드합니다.
        /// </summary>
        /// <param name="adUnitId">광고 단위 ID</param>
        public void LoadRewardedAd(string adUnitId)
        {
            if (!_adxprovider.IsInitialized)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 보상형 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke(); // 오류 이벤트 트리거
                return;
            }

            if (_rewardedAd == null)
            {
                _rewardedAd = new AdxRewardedAd(adUnitId);

                // 이벤트 핸들러 등록
                _rewardedAd.OnRewardedAdLoaded += () =>
                {
                    Debug.Log("[ADX Backend Ads] 보상형 광고 로드 성공.");
                };
                _rewardedAd.OnRewardedAdFailedToLoad += (error) =>
                {
                    Debug.LogError($"[ADX Backend Ads] 보상형 광고 로드 실패: {error}");
                    OnAdError?.Invoke(); // 오류 이벤트 트리거
                };
                _rewardedAd.OnRewardedAdShown += () =>
                {
                    Debug.Log("[ADX Backend Ads] 보상형 광고 표시됨.");
                };
                _rewardedAd.OnRewardedAdClicked += () =>
                {
                    Debug.Log("[ADX Backend Ads] 보상형 광고 클릭됨.");
                };
                _rewardedAd.OnRewardedAdFailedToShow += () =>
                {
                    Debug.LogError($"[ADX Backend Ads] 보상형 광고 표시 실패:");
                    OnAdError?.Invoke(); // 오류 이벤트 트리거
                };
                _rewardedAd.OnRewardedAdEarnedReward += () =>
                {
                    Debug.Log("[ADX Backend Ads] 보상형 광고 보상 획득 이벤트 발생.");
                };
                _rewardedAd.OnRewardedAdClosed += () =>
                {
                    Debug.Log("[ADX Backend Ads] 보상형 광고 닫힘.");
                };
                _rewardedAd.OnPaidEvent += (ecpm) =>
                {
                    // 예상 광고 수익 계산
                    double revenue = ecpm / 1000f;

                    Debug.Log($"[ADX Backend Ads] 보상형 광고 수익 발생: {revenue} USD (eCPM: {ecpm})");

                    // Adjust 연동 코드 시작
                    // "adx_sdk" 소스를 사용하여 AdjustAdRevenue 객체를 인스턴스화합니다
                    // AdjustAdRevenue adRevenue = new AdjustAdRevenue("adx_sdk");
                    // // 계산된 수익과 "USD" 통화를 사용하여 AdjustAdRevenue 객체의 세부 정보를 입력합니다
                    // adRevenue.SetRevenue(revenue, "USD"); 
                    // // 광고 수익 단위를 "ADX Rewarded Ad"로 설정합니다 [3].
                    // adRevenue.AdRevenueUnit = "ADX Rewarded Ad"; 
                    // // Adjust.TrackAdRevenue 메서드를 호출하여 Adjust로 광고 매출 정보를 전송합니다
                    // Adjust.TrackAdRevenue(adRevenue);
                    // Adjust 연동 코드 끝

                    OnAdRevenue?.Invoke("ADX Rewarded Ad", revenue); // 기존 OnAdRevenue 이벤트 트리거 유지
                };
            }
            _rewardedAd.Load(); // 광고 로드 요청
            Debug.Log($"[ADX Backend Ads] 보상형 광고 로드 요청: {adUnitId}");
        }

        /// <summary>
        /// 로드된 보상형 광고를 표시합니다.
        /// </summary>
        public void ShowRewardedAd()
        {
            if (!_adxprovider.IsInitialized)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 보상형 광고를 표시할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }
            if (_rewardedAd != null && _rewardedAd.IsLoaded()) // 광고가 로드되어 준비되었는지 확인
            {
                _rewardedAd.Show(); // 광고 표시
                Debug.Log("[ADX Backend Ads] 보상형 광고 표시 요청.");
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 보상형 광고가 로드되지 않았거나 아직 준비되지 않았습니다.");
                OnAdError?.Invoke(); // 오류 이벤트 트리거
            }
        }

        /// <summary>
        /// 보상형 광고가 로드되어 표시 준비가 되었는지 확인합니다.
        /// </summary>
        /// <returns>광고 준비 여부</returns>
        public bool IsRewardedAdReady()
        {
            return _rewardedAd != null && _rewardedAd.IsLoaded();
        }

        /// <summary>
        /// 네이티브 광고를 로드합니다.
        /// ADX Unity SDK 문서("Unity" 섹션의 "Ad Formats"를 참조)에는
        /// 네이티브 광고 형식이 나열되어 있지 않습니다.
        /// 따라서 Unity에서 ADX 네이티브 광고를 직접적으로 지원하지 않을 수 있으므로 경고를 로깅합니다.
        /// </summary>
        /// <param name="adUnitId">광고 단위 ID</param>
        public void LoadNativeAd(string adUnitId)
        {
            if (!_adxprovider.IsInitialized)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 네이티브 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }
            Debug.LogWarning($"[ADX Backend Ads] Unity용 ADX SDK는 네이티브 광고를 직접적으로 지원하지 않습니다 (AdUnitId: {adUnitId}).");
            OnAdError?.Invoke(); // 오류 이벤트 트리거
        }
    }
}