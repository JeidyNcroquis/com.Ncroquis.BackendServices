using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Ncroquis.Backend
{
    public class PointpubBackendOfferwall : IBackendOfferwall
    {
        [Inject] readonly ILogger _logger;
        

        public ProviderKey providerKey => ProviderKey.POINTPUB;

        public bool IsFocused { get; set; } = false;

        public void StartOfferwall(string userId)
        {
#if UNITY_EDITOR
            _logger.Log("[POINTPUB OFFERWALL] Unity Editor에서 StartOfferwall 호출됨.");
#elif UNITY_ANDROID
            if (string.IsNullOrEmpty(userId))
            {
                _logger.Warning($"[POINTPUB OFFERWALL] StartOfferwall 실패: UserId : {userId}가 유효하지 않습니다.");
                return;
            }

            PointPubUnityPlugin.Android.PointPubSdkClient.Instance.StartOfferwall(userId);
            IsFocused = true;
#elif UNITY_IOS || UNITY_IPHONE
            _logger.Warning("[POINTPUB OFFERWALL] iOS는 아직 지원되지 않습니다.");
#endif
        }

        public async Task GetRewardsAsync(string userId, Action<int, string> callback)
        {
#if UNITY_EDITOR
            await UniTask.SwitchToMainThread();
            _logger.Log("[POINTPUB OFFERWALL] Unity Editor에서는 Participation 조회가 동작하지 않습니다.");
            callback?.Invoke(-1, "Editor mode");
            return;
#elif UNITY_ANDROID
            if (!IsFocused)
            {
                await UniTask.SwitchToMainThread();
                _logger.Log("[POINTPUB OFFERWALL] GetRewardsAsync 무시됨: 사용자가 Offerwall에서 복귀하지 않았습니다.");
                callback?.Invoke(-1, "[POINTPUB OFFERWALL] Offerwall 복귀 상태 아님");
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                _logger.Warning("[POINTPUB OFFERWALL] GetRewardsAsync 실패: Provider 나 UserId 가 null입니다.");
                await UniTask.SwitchToMainThread();
                callback?.Invoke(-1, "[POINTPUB OFFERWALL] GetRewardsAsync 실패: Provider 나 UserId 가 null입니다.");
                return;
            }

            try
            {
                var result = await GetParticipationAsync(userId);
                await UniTask.SwitchToMainThread();
                callback?.Invoke(result.Item1, result.Item2);

                IsFocused = false;
            }
            catch (Exception e)
            {
                _logger.Warning($"[POINTPUB OFFERWALL] GetRewardsAsync 실패: {e}");
                await UniTask.SwitchToMainThread();
                callback?.Invoke(-1, $"Exception: {e.Message}");
            }
#elif UNITY_IOS || UNITY_IPHONE
            await UniTask.SwitchToMainThread();
            _logger.Warning("[POINTPUB OFFERWALL] iOS는 아직 지원되지 않습니다.");
            callback?.Invoke(-1, "iOS not supported");
#endif
        }

#if UNITY_ANDROID
        private UniTask<(int, string)> GetParticipationAsync(string userId)
        {
            var tcs = new UniTaskCompletionSource<(int, string)>();

            PointPubUnityPlugin.Android.PointPubSdkClient.Instance.GetParticipation(userId, (code, data) =>
            {
                tcs.TrySetResult((code, data));
            });

            return tcs.Task;
        }
#endif
    }
}
