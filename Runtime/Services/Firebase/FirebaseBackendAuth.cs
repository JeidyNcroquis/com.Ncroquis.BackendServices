using System;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;


namespace Ncroquis.Backend
{

    public class FirebaseBackendAuth : IBackendAuth
    {
        private FirebaseAuth auth;
        private FirebaseUser currentUser;

        public bool IsSignedIn => currentUser != null;
        public string UserId => currentUser?.UserId;

        public FirebaseBackendAuth()
        {
            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += OnStateChanged;
        }

        private void OnStateChanged(object sender, System.EventArgs e)
        {
            currentUser = auth.CurrentUser;
            AuthStateChanged?.Invoke(new AuthStateEventArgs
            {
                UserId = currentUser?.UserId,
                IsSignedIn = currentUser != null
            });
        }

        public event Action<AuthStateEventArgs> AuthStateChanged;

        public Task<bool> SignInAnonymouslyAsync() => HandleSignIn(auth.SignInAnonymouslyAsync());
        public Task<bool> SignInWithEmailAsync(string email, string password) => HandleSignIn(auth.SignInWithEmailAndPasswordAsync(email, password));
        public Task<bool> SignUpWithEmailAsync(string email, string password) => HandleSignIn(auth.CreateUserWithEmailAndPasswordAsync(email, password));


        private async Task<bool> HandleSignIn(Task<AuthResult> task)
        {
            try
            {
                await task;
                currentUser = auth.CurrentUser;
                Debug.Log($"[Firebase Auth] 로그인 성공: {currentUser?.UserId}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Firebase Auth] Error: {ex.Message}");
                return false;
            }
        }

        public Task SignOutAsync()
        {
            auth.SignOut();
            return Task.CompletedTask;
        }

        public void AddAuthStateChangedListener(Action<AuthStateEventArgs> listener) => AuthStateChanged += listener;
        public void RemoveAuthStateChangedListener(Action<AuthStateEventArgs> listener) => AuthStateChanged -= listener;
    }
}