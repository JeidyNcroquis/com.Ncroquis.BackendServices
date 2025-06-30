using System;
using System.Threading.Tasks;
using Firebase.Auth;
using VContainer;


namespace Ncroquis.Backend
{
    /// <summary>
    /// Firebase 인증을 이용한 백엔드 인증 구현 클래스
    /// 익명, 이메일, 외부(Google/Apple) 인증을 지원
    /// </summary>
    public class FirebaseBackendAuth : IBackendAuth
    {
        public ProviderKey providerKey => ProviderKey.FIREBASE;

        private readonly ILogger _logger;
        private readonly FirebaseAuth auth;
        private FirebaseUser currentUser;

        public bool IsSignedIn => currentUser != null;
        public string UserId => currentUser?.UserId;

        public event Action<AuthStateEventArgs> AuthStateChanged;

        [Inject]
        public FirebaseBackendAuth(ILogger logger)
        {
            _logger = logger;
            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += (_, _) => NotifyAuthStateChanged();
        }


        public Task<bool> SignInAnonymouslyAsync() =>
            SignIn(auth.SignInAnonymouslyAsync(), r => r?.User, "익명 로그인");

        public Task<bool> SignInWithEmailAsync(string email, string password) =>
            SignIn(auth.SignInWithEmailAndPasswordAsync(email, password), r => r?.User, "이메일 로그인");

        public Task<bool> SignUpWithEmailAsync(string email, string password) =>
            SignIn(auth.CreateUserWithEmailAndPasswordAsync(email, password), r => r?.User, "회원가입");

        public Task SignOutAsync()
        {
            auth.SignOut();
            _logger.Log("[Firebase Auth] 로그아웃 완료");
            NotifyAuthStateChanged();
            return Task.CompletedTask;
        }

        public void AddAuthStateChangedListener(Action<AuthStateEventArgs> listener) =>
            AuthStateChanged += listener;

        public void RemoveAuthStateChangedListener(Action<AuthStateEventArgs> listener) =>
            AuthStateChanged -= listener;



        /// <summary>
        /// Google Play Games 로그인 (서버 인증 코드 기반)
        /// </summary>
        public Task<bool> SignInWithGooglePlayAsync(string serverAuthCode)
        {
            if (string.IsNullOrEmpty(serverAuthCode))
            {
                _logger.Warning("[Firebase Auth] GooglePlay 로그인 실패 - authCode 누락");
                return Task.FromResult(false);
            }

            var credential = PlayGamesAuthProvider.GetCredential(serverAuthCode);
            return SignIn(auth.SignInWithCredentialAsync(credential), u => u, "GooglePlay 로그인");
        }

        /// <summary>
        /// Apple Game Center 로그인 (ID 토큰 기반)
        /// </summary>
        public Task<bool> SignInWithAppleGameCenterAsync(string identityToken)
        {
            if (string.IsNullOrEmpty(identityToken))
            {
                _logger.Warning("[Firebase Auth] Apple Game Center 로그인 실패 - identityToken 누락");
                return Task.FromResult(false);
            }

            var credential = OAuthProvider.GetCredential("apple.com", identityToken, null, null);
            return SignIn(auth.SignInWithCredentialAsync(credential), u => u, "Apple Game Center 로그인");
        }

        

        private async Task<bool> SignIn<T>(Task<T> task, Func<T, FirebaseUser> userSelector, string action)
        {
            try
            {
                var result = await task;
                currentUser = userSelector(result);
                NotifyAuthStateChanged();
                _logger.Log($"[Firebase Auth] {action} 성공 - UserId: {currentUser?.UserId}");
                return true;
            }
            catch (Exception e)
            {
                _logger.Warning($"[Firebase Auth] {action} 실패 - {e.Message}");
                return false;
            }
        }

        

        private void NotifyAuthStateChanged()
        {
            currentUser = auth.CurrentUser;
            _logger.Log($"[Firebase Auth] 상태 변경됨 - 로그인: {IsSignedIn} [{currentUser?.UserId}]");

            AuthStateChanged?.Invoke(new AuthStateEventArgs
            {
                UserId = currentUser?.UserId,
                IsSignedIn = currentUser != null
            });
        }
    }
}
