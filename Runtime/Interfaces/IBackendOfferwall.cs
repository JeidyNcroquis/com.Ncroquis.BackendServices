
using System.Threading.Tasks;


namespace Ncroquis.Backend
{

    /// <summary>
    /// 사용자 리워드 참여형 광고 서비스 (예: Offerwall, Tapjoy 등)
    /// </summary>
    public interface IBackendOfferwall : IBackendIdentifiable
    {

        /// <summary>
        /// 리워드 광고 또는 오퍼월 UI를 시작합니다.
        /// </summary>
        void StartOfferwall(string userId);

        /// <summary>
        /// 사용자의 참여 보상 정보를 가져옵니다.
        /// </summary>
        /// userId , code , data
        Task GetRewardsAsync(string userId, System.Action<int, string> callback);

    }

}