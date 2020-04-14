using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.HttpManagement
{
    public class DataDownloader : MonoBehaviour
    {
        private Uri uri = null;
        private Action<string, bool, Uri> callback_action = null;

        public void DownloadAsync(string url, Action<string, bool, Uri> action)
        {
            uri = new Uri(url);
            callback_action = action;

            StartCoroutine(GetData());
        }

        private IEnumerator GetData()
        {
            UnityWebRequest www_get = UnityWebRequest.Get(uri);
            yield return www_get.SendWebRequest();

            if (www_get.isNetworkError)
            {
                // Most probably a connection error
                Debug.Log(
                    "Error downloading data : most probably a connectivity issue (please check your internet connection)");
                callback_action("ERROR NETWORK", true, uri);
            }
            else if (www_get.isHttpError)
            {
                // Most probably a connection error
                Debug.Log("Error downloading data : HTTP error " + www_get.responseCode +
                          " most probably a connection error (server error)");
                callback_action(www_get.error + " " + www_get.responseCode, true, uri);
            }
            else
            {
                // download OK
                callback_action(www_get.downloadHandler.text, false, uri);
            }
        }
    }
}
