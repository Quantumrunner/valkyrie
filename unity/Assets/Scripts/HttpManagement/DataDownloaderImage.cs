using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.HttpManagement
{
    public class DataDownloaderImage : MonoBehaviour
    {
        private Uri uri = null;
        private Action<Texture2D, bool, Uri> callback_action_img = null;

        public void DownloadImageAsync(string url, Action<Texture2D, bool, Uri> action)
        {
            uri = new Uri(url);
            callback_action_img = action;

            StartCoroutine(GetData());
        }

        private IEnumerator GetData()
        {
            UnityWebRequest www_get = UnityWebRequestTexture.GetTexture(uri);
            yield return www_get.SendWebRequest();

            if (www_get.isNetworkError)
            {
                // Most probably a connection error
                Debug.Log(
                    "Error downloading data : most probably a connectivity issue (please check your internet connection)");
                callback_action_img(null, true, uri);
            }
            else if (www_get.isHttpError)
            {
                // Most probably a connection error
                Debug.Log("Error downloading data : HTTP error " + www_get.responseCode +
                          " most probably a connection error (server error)");
                callback_action_img(null, true, uri);
            }
            else
            {
                // download OK
                callback_action_img(DownloadHandlerTexture.GetContent(www_get), false, uri);
            }
        }
    }
}
