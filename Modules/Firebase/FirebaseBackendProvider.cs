
using System.Threading.Tasks;
using Firebase;
using UnityEngine;


namespace Ncroquis.Backend
{
    public class FirebaseBackendProvider : IBackendProvider
    {
        public string ProviderName => "Firebase";
        public bool IsInitialized { get; private set; } = false;

        public async Task<bool> InitializeAsync()
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            IsInitialized = dependencyStatus == DependencyStatus.Available;

            Debug.Log($"Firebase Backend Provider Initialization: {IsInitialized}");

            return dependencyStatus == DependencyStatus.Available;
        }
    }
}