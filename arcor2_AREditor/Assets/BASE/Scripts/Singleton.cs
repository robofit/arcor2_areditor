using UnityEngine;

/// <summary>
/// Inherit from this base class to create a singleton.
/// e.g. public class MyClassName : Singleton<MyClassName> {}
/// </summary>

namespace Base {
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        // Check to see if we're about to be destroyed.
        private static bool m_ShuttingDown = false;
        private static object m_Lock = new object();
        private static T m_Instance;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T Instance {
            get {
                if (m_ShuttingDown) {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed. Returning null.");
                    return null;
                }

                lock (m_Lock) {
                    if (m_Instance == null) {
                        // Search for existing instance.
                        m_Instance = (T) FindObjectOfType(typeof(T));

                        //if there is no active object of given type, try to find in inactive objects
                        if (m_Instance == null) {
                            Object[] objects = Resources.FindObjectsOfTypeAll(typeof(T));
                            // if there is such object, it is bug and it shuld to be reported
                            if (objects.Length > 0) {
                                m_Instance = (T) objects[0];
                                Debug.LogError("Calling method of inactive object");
                            }                                
                        }
                        
                        // or create new instance
                        if (m_Instance == null) {
                            // Need to create a new GameObject to attach the singleton to.
                            GameObject singletonObject = new GameObject();
                            m_Instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";

                            // Make instance persistent.
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return m_Instance;
                }
            }
        }


        private void OnApplicationQuit() {
            m_ShuttingDown = true;
        }


        private void OnDestroy() {
            m_ShuttingDown = true;
        }
    }
}
