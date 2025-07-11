using UnityEngine;

namespace Ncroquis.Backend
{

        [CreateAssetMenu(fileName = "PointpubIdConfig", menuName = "BackendServices/PointpubIdConfig")]
        public class PointpubIdConfig : ScriptableObject
        {

                [Header("App ID")]
                public string androidAppId;                                

                public string GetAppId()
                {
#if UNITY_ANDROID || UNITY_EDITOR
                        return androidAppId;
#elif UNITY_IPHONE
                        return "";
#endif
                }
                
        }

}