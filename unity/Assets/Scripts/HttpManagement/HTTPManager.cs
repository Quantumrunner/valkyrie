using System;
using UnityEngine;

namespace Assets.Scripts.HttpManagement
{

    /// <summary>
    /// HTTPManager is a simplified class to download/upload text files</summary>
    /// <remarks>
    /// Multiple files can be downloaded at the same time,  with one coroutine per download. Only one GameObject will be created for all connections.</remarks>
    public class HTTPManager
    {
        private static GameObject network_go = null;

        /// <summary>
        /// Download a text file, and call action() when done.</summary>
        /// <param name="url">Url of the file to download</param>
        /// <param name="action">Callback with content in string or error in case of problem, and bool containing download status (true:success).</param>
        public static void Get(string url, Action<string, bool, Uri> action)
        {
            if (network_go == null)
            {
                network_go = new GameObject("NetworkManager");
                network_go.tag = Game.BG_TASKS;
            }

            //Use WebClient Class
            DataDownloader dd = network_go.AddComponent<DataDownloader>();

            dd.DownloadAsync(url, action);
        }

        /// <summary>
        /// Download an image file, and call action() when done.</summary>
        /// <param name="url">Url of the file to download</param>
        /// <param name="action">Callback with content in string or error in case of problem, and bool containing download status (true:success).</param>
        public static void GetImage(string url, Action<Texture2D, bool, Uri> action)
        {
            if (network_go == null)
            {
                network_go = new GameObject("NetworkManager");
                network_go.tag = Game.BG_TASKS;
            }

            //Use WebClient Class
            DataDownloaderImage ddi = network_go.AddComponent<DataDownloaderImage>();

            ddi.DownloadImageAsync(url, action);
        }


        /// <summary>
        /// HTTP POST content to a URL, and call action() when done.</summary>
        /// <param name="url">Url of the form to upload to</param>
        /// <param name="content">Content to POST</param>
        /// <param name="action">Callback with error in string in case of problem, and bool containing upload status (true:success).</param>
        public static void Upload(string url, WWWForm content, Action<string, bool> action)
        {
            if (network_go == null)
            {
                network_go = new GameObject("NetworkManager");
                network_go.tag = Game.BG_TASKS;
            }

            //Use WebClient Class
            DataUploader du = network_go.AddComponent<DataUploader>();

            du.PostFormAsync(url, content, action);
        }

    }
}