using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.HttpManagement
{
    public class DataUploader : MonoBehaviour
    {
        private Uri uri = null;
        private WWWForm formFields = null;
        private Action<string, bool> callback_action = null;

        public void PostFormAsync(string url, WWWForm content, Action<string, bool> action)
        {
            uri = new Uri(url);
            formFields = content;
            callback_action = action;

            StartCoroutine(PostForm());
        }

        private IEnumerator PostForm()
        {
            UnityWebRequest www_post = UnityWebRequest.Post(uri, formFields);

            yield return www_post.SendWebRequest();

            if (www_post.isNetworkError)
            {
                // Most probably a connection error
                callback_action("ERROR NETWORK", true);
                Debug.Log(
                    "Error uploading data : most probably a connectivity issue (please check your internet connection)");
            }
            else if (www_post.isHttpError)
            {
                // Most probably a connection error
                callback_action(www_post.error + " " + www_post.responseCode, true);
                Debug.Log("Error uploading data : most probably a connection error (server error)");
            }
            else
            {
                // download OK
                callback_action(www_post.downloadHandler.text, false);
            }

        }
    }
}
