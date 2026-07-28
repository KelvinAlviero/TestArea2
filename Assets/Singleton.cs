using UnityEngine;



    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;
        private static bool applicationIsQuitting = false;

        public static bool HasInstance => instance != null;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

                    if (instance == null)
                    {
                        Debug.Log("An instance of " + typeof(T) +
                            " is needed in the scene, but there is none.");
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else if (instance != this)
            {
                try
                {
                    Destroy(gameObject);
                }
                catch
                {
                    Debug.Log("Failed to destroy duplicate singleton instance of " + typeof(T) + ".");
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
