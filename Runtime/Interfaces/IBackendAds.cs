using System;


namespace Ncroquis.Backend
{

    /// ADX 라이브러리가 지원하는 광고 형식에 기반한 광고 기능 인터페이스입니다.

    public interface IBackendAds
    {

        /// ADX SDK를 초기화합니다.
        void InitializeAdSDK();



        /// 배너 광고를 로드합니다.
        /// <param name="adUnitId">광고 단위 ID</param>        
        void LoadBannerAd(string adUnitId);

        /// 로드된 배너 광고를 표시합니다.
        void ShowBannerAd();

        /// 표시된 배너 광고를 숨깁니다.
        void HideBannerAd();



        /// 전면 광고를 로드합니다.
        /// <param name="adUnitId">광고 단위 ID</param>        
        void LoadInterstitialAd(string adUnitId);

        /// 로드된 전면 광고를 표시합니다.
        void ShowInterstitialAd();

        /// 전면 광고가 로드되어 표시 준비가 되었는지 확인합니다.        
        bool IsInterstitialAdReady();



        /// 보상형 광고를 로드합니다.
        /// <param name="adUnitId">광고 단위 ID</param>
        void LoadRewardedAd(string adUnitId);

        /// 로드된 보상형 광고를 표시합니다.
        void ShowRewardedAd();

        /// 보상형 광고가 로드되어 표시 준비가 되었는지 확인합니다.        
        /// <returns>광고 준비 여부</returns>
        bool IsRewardedAdReady();


        /// 네이티브 광고를 로드합니다. (주로 Android 및 iOS에서 지원)
        /// 네이티브 광고의 표시는 구현체에서 UI 통합 방식에 따라 달라질 수 있습니다.        
        /// <param name="adUnitId">광고 단위 ID</param>        
        void LoadNativeAd(string adUnitId);


        /// 광고 로드 또는 표시 중 오류가 발생할 때 발생하는 이벤트입니다.        
        event Action<string> OnAdError;


        /// 광고로부터 수익이 발생했을 때 발생하는 이벤트입니다.        
        /// <param name="adUnitId">수익이 발생한 광고 단위 ID</param>
        /// <param name="revenueAmount">발생한 수익 금액</param>        
        event Action<string, double> OnAdRevenue;
    }
}