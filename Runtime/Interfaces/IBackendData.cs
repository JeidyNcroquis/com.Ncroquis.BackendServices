using System.Threading.Tasks;
using UnityEngine;



namespace Ncroquis.Backend
{

    // 데이터베이스 연동 (예: Firestore)

    public interface IBackendData : IBackendIdentifiable
    {
        Task<T> GetDocumentAsync<T>(string collection, string documentId);
        Task<bool> SetDocumentAsync<T>(string collection, string documentId, T data);
        Task<bool> DeleteDocumentAsync(string collection, string documentId);
    }


}