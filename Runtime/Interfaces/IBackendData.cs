using System.Threading.Tasks;

namespace Ncroquis.Backend
{
    /// <summary>
    /// 백엔드 데이터 저장소와의 연동을 위한 범용 인터페이스입니다.
    /// collectionName: 컬렉션(또는 테이블) 이름
    /// documentId: 문서(또는 레코드) 고유 식별자
    /// data: 저장할 데이터 객체
    ///
    /// 사용 예시:    
    /// await backend.SaveAsync("Users", "userID", new User { Name = "홍길동", Age = 30 });
    /// await backend.DeleteAsync("Users", "userID");
    /// </summary>
    public interface IBackendDataStore : IBackendIdentifiable
    {
        /// <summary>
        /// 지정된 컬렉션(테이블)과 문서 ID로 데이터를 비동기적으로 읽어옵니다.
        /// 사용 예시:
        /// var user = await LoadAsync<User>("Users", "userID");
        /// </summary>
        Task<T> LoadAsync<T>(string collectionName, string documentId);

        /// <summary>
        /// 지정된 컬렉션(테이블)과 문서 ID로 데이터를 비동기적으로 저장(덮어쓰기)합니다.
        /// 사용 예시:
        /// await SaveAsync("Users", "userID", new User { Name = "홍길동", Age = 30 });
        /// </summary>
        Task<bool> SaveAsync<T>(string collectionName, string documentId, T data);

        /// <summary>
        /// 지정된 컬렉션(테이블)과 문서 ID로 데이터를 비동기적으로 삭제합니다.
        /// 사용 예시:
        /// await DeleteAsync("Users", "userID");
        /// </summary>
        Task<bool> DeleteAsync(string collectionName, string documentId);
    }
}
