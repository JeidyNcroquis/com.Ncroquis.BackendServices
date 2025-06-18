using System;
using System.Threading;
using System.Threading.Tasks;


namespace Ncroquis.Backend
{

    /// ADX 라이브러리가 지원하는 광고 형식에 기반한 광고 기능 인터페이스입니다.

    public interface IBackendAds : IBackendIdentifiable
    {

        /// 배너 광고를 로드합니다.      
        Task LoadBannerAsync(string adUnitId, CancellationToken cancellationToken = default);

        /// 로드된 배너 광고를 표시합니다.
        void ShowBannerAd();

        /// 표시된 배너 광고를 숨깁니다.
        void HideBannerAd();



        /// 전면 광고를 로드합니다.
        Task LoadInterstitialAsync(string adUnitId, CancellationToken cancellationToken = default);

        /// 로드된 전면 광고를 표시합니다.
        void ShowInterstitialAd();

        /// 전면 광고가 로드되어 표시 준비가 되었는지 확인합니다.        
        bool IsInterstitialAdReady();



        /// 보상형 광고를 로드합니다.
        /// <param name="adUnitId">광고 단위 ID</param>
        Task LoadRewardedAsync(string adUnitId, CancellationToken cancellationToken = default);

        /// 로드된 보상형 광고를 표시합니다.
        void ShowRewardedAd();

        /// 보상형 광고가 로드되어 표시 준비가 되었는지 확인합니다.        
        /// <returns>광고 준비 여부</returns>
        bool IsRewardedAdReady();




        /// 광고 로드 또는 표시 중 오류가 발생할 때 발생하는 이벤트입니다.                
        event Action OnAdError;

        /// 광고로부터 수익이 발생했을 때 발생하는 이벤트입니다.        
        /// <param name="adUnitId">수익이 발생한 광고 단위 ID</param>
        /// <param name="revenueAmount">발생한 수익 금액</param>        
        event Action<string, double> OnAdRevenue;
    }

}