using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections.Generic;


public class APIController: MonoBehaviour
{
    public SpinningScript SpinningScript;
    
    public string URL_GetUser = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=AIzaSyB-VcA8mR2rOlVxlxObaZYIY27yIYFdb70";
    public string URL_StartSpin = "https://mh-dev.dreamforgecreation.com/api/v1/spinwheel/spin";
    public string URL_RecieveReward = "https://mh-dev.dreamforgecreation.com/api/v1/spinwheel/rewards?spinwheel_config_name=default_testing_spinwheel";
    public string email = "aaa@gmail.com";
    public string password = "qwerty123";
    public string returnSecureToken = "true";
    public string JWTToken;
    public TMP_Text DebugText_SpinAmount;
    public TMP_Text DebugText_ItemList;
    public TMP_Text DebugText_Status;
    public TMP_Text JSON_Body;
    public TMP_Text JSON_Raw;

    public string ObtainedReward;
    public string UnserializeditemId;
    public int spin_count = 1;
    [SerializeField] private TMP_Text JWT_Text;
    [SerializeField] private TMP_Text JWT_Translated;
    
    
    
    [System.Serializable] public class SpinRequest
    {
        public string spinwheel_config_name;
        public int spin_count = 1;
    }
    [System.Serializable] public class SpinItem
    {
        public string name;
        public string itemType;
        public int amount;
        public string itemId;
    }
    
    [System.Serializable] public class SpinDraw
    {
        public string package_id;
        public List<SpinItem> items;
        
    }
    
   [System.Serializable] public class SpinResponse
    {
        public string name;
        public int spin_count;
        public int total_cost;
        public string currency;
        public List<SpinDraw> draws;
    }

    public IEnumerator StartSpin()
    {
        var data = new SpinRequest
        {
            spinwheel_config_name = "default_testing_spinwheel",
            spin_count = spin_count
        };

        string json = JsonUtility.ToJson(data);
        if (JSON_Body != null)
            JSON_Body.text = json;

        using UnityWebRequest request = new UnityWebRequest(URL_StartSpin, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(JWTToken))
        {
            request.SetRequestHeader("Authorization", "Bearer " + JWTToken);
        }
        else
        {
            yield return null;
        }

        yield return request.SendWebRequest();

        string rawJson = request.downloadHandler?.text ?? "";

        if (JSON_Raw != null)
            JSON_Raw.text = string.IsNullOrEmpty(rawJson) ? "<empty response>" : rawJson;

        if (DebugText_Status != null)
        {
            DebugText_Status.text = "Result: " + request.result + "\n" +
                                    "Code: " + request.responseCode + "\n" +
                                    "Error: " + request.error + "\n";
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Request success");
            Debug.Log(rawJson);

            SpinResponse response = JsonUtility.FromJson<SpinResponse>(rawJson);
            
            if (response != null &&
            response.draws != null &&
            response.draws.Count > 0)
            {
                List<string> queuedItemIds = new List<string>();
                SpinItem firstItem = null;

                foreach (var draw in response.draws)
                {
                    if (draw == null || draw.items == null)
                        continue;

                    foreach (var item in draw.items)
                    {
                        if (item == null || string.IsNullOrEmpty(item.itemId))
                            continue;

                        queuedItemIds.Add(item.itemId);

                        if (firstItem == null)
                            firstItem = item;
                    }
                }

                if (queuedItemIds.Count > 0 && firstItem != null)
                {
                    if (DebugText_SpinAmount != null)
                        DebugText_SpinAmount.text = "Reward spun " + queuedItemIds.Count + " times";

                    UnserializeditemId = firstItem.itemId;
                    UnserializedItems();

                    if (SpinningScript != null)
                    {
                        SpinningScript.ReceivedBackend = true;
                        SpinningScript.QueueRewards(queuedItemIds);
                        SpinningScript.StartNextQueuedSpin();
                    }
                }
                else
                {
                    if (DebugText_SpinAmount != null)
                        DebugText_SpinAmount.text = "No rewards";
                }
            }
            else
            {
                if (DebugText_SpinAmount != null)
                    DebugText_SpinAmount.text = "No rewards";
            }
        }
        else
        {
            Debug.LogError("Request failed: " + request.responseCode);
            Debug.LogError(rawJson);
        }
    
    }
    public void UnserializedItems()
    {
        ObtainedReward = UnserializeditemId;
        // Debug.Log("ObtainedReward" + ObtainedReward);
    }


    public IEnumerator RecieveSpin()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(URL_RecieveReward))
        {
            yield return request.SendWebRequest();
        
            if (request.result == UnityWebRequest.Result.ConnectionError)
            Debug.Log(request.error);

            else
            {
                Debug.Log("ReceiveSpin: Request success");
                
            }
        }
    }

    // ----- Unused code ----- //
     // public IEnumerator SignIn()
    // {
    //     var data = new
    //     {
    //         email = email,
    //         password = password,
    //         returnSecureToken = returnSecureToken
    //     };
    //     string json = JsonUtility.ToJson(data);

    //     using UnityWebRequest request = new UnityWebRequest(URL_GetUser, "POST");
    //     byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
    //     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //     request.downloadHandler = new DownloadHandlerBuffer();
    //     request.SetRequestHeader("Content-Type", "application/json");

    //     yield return request.SendWebRequest();

    //     if (request.result == UnityWebRequest.Result.ConnectionError ||
    //         request.result == UnityWebRequest.Result.ProtocolError)
    //     {
    //         Debug.LogError(request.error);
    //         Debug.LogError(request.downloadHandler.text);
    //     }
    //     else
    //     {
    //         Debug.Log(request.downloadHandler.text);
    //     }
    // }    

}