using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
// UniWebViewBridge is a bridge between Unity and JavaScript for WebGL builds. Needed for sending messages to the JavaScript side and receiving responses. It uses DllImport to call JavaScript functions defined in the WebGL template.
public class UniWebViewBridge : MonoBehaviour
{
        [DllImport("__Internal")]
        private static extern void RegisterRequestCallbacks(Action<string> onSuccess, Action<string> onError);

        [DllImport("__Internal")]
        private static extern void SendChannelMessage(string action, string payload);

        [DllImport("__Internal")]
        private static extern string CallChannelMessage(string action, string payload);

        [DllImport("__Internal")]
        private static extern void RequestChannelMessage(string action, string payload, int timeout);

        public static event Action<string> OnRequestSuccess;
        public static event Action<string> OnRequestError;

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        RegisterRequestCallbacks(HandleSuccess, HandleError);
#endif
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void HandleSuccess(string json)
        {
                OnRequestSuccess?.Invoke(json);
        }

        [MonoPInvokeCallback(typeof(Action<string>))]
        private static void HandleError(string error)
        {
                OnRequestError?.Invoke(error);
        }

        public static void Send(string action, object payload)
        {
                var json = payload == null ? "null" : JsonUtility.ToJson(payload);
#if UNITY_WEBGL && !UNITY_EDITOR
        SendChannelMessage(action, json);
#else
                Debug.Log($"[SIMULATED] send({action}, {json})");
#endif
        }

        public static string Call(string action, object payload)
        {
                var json = payload == null ? "null" : JsonUtility.ToJson(payload);
#if UNITY_WEBGL && !UNITY_EDITOR
        return CallChannelMessage(action, json);
#else
                Debug.Log($"[SIMULATED] call({action}, {json})");
                return null;
#endif
        }

        public static void Request(string action, object payload, int timeout = 5000)
        {
                var json = payload == null ? "null" : JsonUtility.ToJson(payload);
#if UNITY_WEBGL && !UNITY_EDITOR
        RequestChannelMessage(action, json, timeout);
#else
                Debug.Log($"[SIMULATED] request({action}, {json}, {timeout}ms)");
#endif
        }
}