using System.Threading.Tasks;
using UnityEngine;



namespace Ncroquis.Backend
{

    // 데이터베이스 연동 (예: Firestore)

    public interface IBackendData
    {
        Task<T> GetDocumentAsync<T>(string collection, string documentId);
        Task<bool> SetDocumentAsync<T>(string collection, string documentId, T data);
        Task<bool> DeleteDocumentAsync(string collection, string documentId);
    }



    // NULL - 스텁 클래스
    public class NullBackendData : IBackendData
    {
        public Task<T> GetDocumentAsync<T>(string collection, string documentId)
        {
            Debug.LogWarning("IBackendData] 구현체가 없습니다");
            return Task.FromResult(default(T));
        }

        public Task<bool> SetDocumentAsync<T>(string collection, string documentId, T data)
        {
            Debug.LogWarning("IBackendData] 구현체가 없습니다");
            return Task.FromResult(false);
        }

        public Task<bool> DeleteDocumentAsync(string collection, string documentId)
        {
            Debug.LogWarning("IBackendData] 구현체가 없습니다");
            return Task.FromResult(false);
        }
    }
}