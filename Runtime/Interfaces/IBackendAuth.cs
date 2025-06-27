using System.Threading.Tasks;


namespace Ncroquis.Backend
{
    public enum ExternalAuthProvider
    {
        GooglePlay,
        AppleGameCenter
    }

    public interface IBackendAuth : IBackendIdentifiable
    {
        bool IsSignedIn { get; }
        string UserId { get; }

        // 익명 로그인
        Task<bool> SignInAnonymouslyAsync();

        // 이메일 로그인 및 회원가입
        Task<bool> SignInWithEmailAsync(string email, string password);
        Task<bool> SignUpWithEmailAsync(string email, string password);

        // 외부 인증(Google Play, Apple Game Center 등) 로그인
        Task<bool> SignInWithExternalProviderAsync(ExternalAuthProvider provider, string idToken = null, string accessToken = null);

        // 로그아웃
        Task SignOutAsync();

        // 인증 상태 리스너 등록
        void AddAuthStateChangedListener(System.Action<AuthStateEventArgs> listener);
        void RemoveAuthStateChangedListener(System.Action<AuthStateEventArgs> listener);
    }

    public class AuthStateEventArgs
    {
        public string UserId { get; set; }
        public bool IsSignedIn { get; set; }
    }
}
