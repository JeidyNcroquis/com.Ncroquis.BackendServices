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

            // Firebase 인증 인스턴스 할당
            auth = FirebaseAuth.DefaultInstance;

            // 인증 상태가 변경될 때마다 이벤트 발생
            auth.StateChanged += (_, _) => NotifyAuthStateChanged();
        }

        /// <summary>
        /// Firebase 익명 로그인
        /// </summary>
        public Task<bool> SignInAnonymouslyAsync() =>
            SignIn(auth.SignInAnonymouslyAsync(), r => r?.User, "익명 로그인");

        /// <summary>
        /// 이메일/비밀번호 로그인
        /// </summary>
        public Task<bool> SignInWithEmailAsync(string email, string password) =>
            SignIn(auth.SignInWithEmailAndPasswordAsync(email, password), r => r?.User, "이메일 로그인");

        /// <summary>
        /// 이메일/비밀번호 회원가입
        /// </summary>
        public Task<bool> SignUpWithEmailAsync(string email, string password) =>
            SignIn(auth.CreateUserWithEmailAndPasswordAsync(email, password), r => r?.User, "회원가입");

        /// <summary>
        /// 로그아웃
        /// </summary>
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
        /// 외부 인증(GooglePlay, Apple GameCenter)으로 로그인
        /// </summary>
        public Task<bool> SignInWithExternalProviderAsync(ExternalAuthProvider provider, string idToken = null, string accessToken = null)
        {
            Credential credential = null;

            // 인증 제공자에 따라 Firebase Credential 생성
            switch (provider)
            {
                case ExternalAuthProvider.GooglePlay:
                    credential = GoogleAuthProvider.GetCredential(idToken, null);
                    break;

                case ExternalAuthProvider.AppleGameCenter:
                    credential = OAuthProvider.GetCredential("apple.com", idToken, accessToken, null);
                    break;

                default:
                    _logger.Warning("[Firebase Auth] 지원되지 않는 인증 제공자");
                    return Task.FromResult(false);
            }

            // FirebaseUser 반환하는 외부 인증용 SignIn 호출
            return SignIn(auth.SignInWithCredentialAsync(credential), u => u, $"{provider} 로그인");
        }

        /// <summary>
        /// 로그인 Task 수행 후 공통 처리 (FirebaseUser 추출 방식 제어)
        /// </summary>
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

        /// <summary>
        /// 인증 상태 변경 이벤트 트리거
        /// </summary>
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
