using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

namespace Ncroquis.Backend
{
    public class FirebaseBackendAuth : IBackendAuth
    {
        public string ProviderName => BackendKeys.FIREBASE;

        private readonly FirebaseAuth auth;
        private FirebaseUser currentUser;

        public bool IsSignedIn => currentUser != null;
        public string UserId => currentUser?.UserId;

        public event Action<AuthStateEventArgs> AuthStateChanged;

        public FirebaseBackendAuth()
        {
            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += (_, _) => NotifyAuthStateChanged();
        }

        public Task<bool> SignInAnonymouslyAsync() =>
            SignIn(auth.SignInAnonymouslyAsync(), "익명 로그인");

        public Task<bool> SignInWithEmailAsync(string email, string password) =>
            SignIn(auth.SignInWithEmailAndPasswordAsync(email, password), "이메일 로그인");

        public Task<bool> SignUpWithEmailAsync(string email, string password) =>
            SignIn(auth.CreateUserWithEmailAndPasswordAsync(email, password), "회원가입");

        public Task SignOutAsync()
        {
            auth.SignOut();
            Debug.Log("[Firebase Auth] 로그아웃 완료");
            NotifyAuthStateChanged();
            return Task.CompletedTask;
        }

        public void AddAuthStateChangedListener(Action<AuthStateEventArgs> listener) =>
            AuthStateChanged += listener;

        public void RemoveAuthStateChangedListener(Action<AuthStateEventArgs> listener) =>
            AuthStateChanged -= listener;

        private async Task<bool> SignIn(Task<AuthResult> signInTask, string action)
        {
            try
            {
                await signInTask;
                NotifyAuthStateChanged();
                Debug.Log($"[Firebase Auth] {action} 성공 - UserId: {currentUser?.UserId}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Firebase Auth] {action} 실패 - {e.Message}");
                return false;
            }
        }

        private void NotifyAuthStateChanged()
        {
            currentUser = auth.CurrentUser;
            Debug.Log($"[Firebase Auth] 상태 변경됨 - 로그인됨: {IsSignedIn}");
            AuthStateChanged?.Invoke(new AuthStateEventArgs
            {
                UserId = currentUser?.UserId,
                IsSignedIn = currentUser != null
            });
        }
    }
}
