using System.Threading.Tasks;
using System.Collections.Generic;
using Firebase.Firestore;
using VContainer;



namespace Ncroquis.Backend
{

    public class FirebaseBackendData : IBackendData
    {
        private readonly ILogger _logger;
        public ProviderKey providerKey => ProviderKey.FIREBASE;

        private FirebaseFirestore db;

        [Inject]
        public FirebaseBackendData(ILogger logger)
        {
            _logger = logger;
            db = FirebaseFirestore.DefaultInstance;
        }

        public async Task<T> GetDocumentAsync<T>(string collection, string documentId)
        {
            try
            {
                DocumentSnapshot snapshot = await db.Collection(collection).Document(documentId).GetSnapshotAsync();
                if (!snapshot.Exists)
                {
                    _logger.LogWarning($"[FirebaseBackendData] 문서 없음 - Collection: {collection}, DocumentId: {documentId}");
                    return default;
                }

                // Dictionary로 데이터 받기
                var dict = snapshot.ToDictionary();
                var obj = System.Activator.CreateInstance<T>();

                foreach (var prop in typeof(T).GetProperties())
                {
                    if (dict.TryGetValue(prop.Name, out object value))
                    {
                        prop.SetValue(obj, value);
                    }
                }
                return obj;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[FirebaseBackendData] GetDocumentAsync 실패 - Collection: {collection}, DocumentId: {documentId}, Error: {ex.Message}");
                return default;
            }
        }


        public async Task<bool> SetDocumentAsync<T>(string collection, string documentId, T data)
        {
            try
            {
                var dict = new Dictionary<string, object>();
                foreach (var prop in typeof(T).GetProperties())
                    dict[prop.Name] = prop.GetValue(data);
                await db.Collection(collection).Document(documentId).SetAsync(dict);
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"[FirebaseBackendData] SetDocumentAsync 실패 - Collection: {collection}, DocumentId: {documentId}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string collection, string documentId)
        {
            try
            {
                await db.Collection(collection).Document(documentId).DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}