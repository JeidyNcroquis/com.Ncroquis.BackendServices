using System.Threading.Tasks;


namespace Ncroquis.Backend
{

    /// <summary>
    /// 백엔드 인증 인터페이스
    /// </summary>
    public interface IBackendAuth : IBackendIdentifiable
    {
        /// <summary> 현재 로그인 상태 여부 </summary>
        bool IsSignedIn { get; }

        /// <summary> 로그인된 사용자 ID </summary>
        string UserId { get; }


        /// <summary> Firebase 익명 로그인 </summary>
        Task<bool> SignInAnonymouslyAsync();

        /// <summary> 이메일/비밀번호 로그인 </summary>
        Task<bool> SignInWithEmailAsync(string email, string password);

        /// <summary> 이메일/비밀번호 회원가입 </summary>
        Task<bool> SignUpWithEmailAsync(string email, string password);


        /// <summary> Google Play Games 인증 (authCode 기반) </summary>
        Task<bool> SignInWithGooglePlayAsync(string serverAuthCode);

        /// <summary> Apple Game Center 인증 (idToken 기반) </summary>
        Task<bool> SignInWithAppleGameCenterAsync(string identityToken);


        /// <summary> 로그아웃 </summary>
        Task SignOutAsync();

        /// <summary> 인증 상태 변경 리스너 등록 </summary>
        void AddAuthStateChangedListener(System.Action<AuthStateEventArgs> listener);

        /// <summary> 인증 상태 변경 리스너 해제 </summary>
        void RemoveAuthStateChangedListener(System.Action<AuthStateEventArgs> listener);
    }

    public class AuthStateEventArgs
    {
        public string UserId { get; set; }
        public bool IsSignedIn { get; set; }
    }
}
