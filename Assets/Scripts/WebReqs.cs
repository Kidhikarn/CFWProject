using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Kid.Web
{
    public abstract class WebReqs : MonoBehaviour
    {
        [Header("Web Request Fields")]
        public string host = "";
        public string path = "";
        public string URL { get { return host + path; } }

        public UnityWebRequest webRequest;
        public WWWForm form;

        //public GameObject errorPanel;
        //public Text errorTxt;

        void Start()
        {
            //errorPanel = GameObject.Find("ErrorInfoGrp");
            //Debug.Log(errorPanel.name);

            /*
            if (errorPanel != null)
            {
                errorTxt = GameObject.Find("errorTxt").GetComponent<Text>();
                Debug.Log(errorTxt);
            }
            */
        }

        public void SendWebRequest()
        {
            Debug.Log("Host: " + host);
            Debug.Log("Path: " + path);
            Debug.Log("URL: " + URL); //kid's
            StartCoroutine(PostWebRequest());
        }

        /// <summary>
        /// Send WebRequest
        /// </summary>
        /// <returns></returns>
        public IEnumerator PostWebRequest()
        {
            UpdateForm();
            Debug.Log("Form: " + form); //kid's
            using (webRequest = UnityWebRequest.Post(URL, form))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    //Error();
                }
                else if (webRequest.isHttpError)
                {
                    //Error();

                }
                else
                {
                    Output();
                }
            }
        }

        public virtual void UpdateForm()
        {
            form = new WWWForm();
        }

        public virtual void Output()
        {
        }

        /*
        public virtual void Error()
        {
            ErrorDisplay.OpenErrorMessage();
            ErrorDisplay.UpdateErrorMessage(webRequest.error);

            switch (webRequest.responseCode)
            {
                /// cannot reach host
                case 0:
                    ErrorDisplay.UpdateErrorFix("Oh no the database is offline or cannot be reach!!");
                    break;

                /// not found
                case 404:
                    ErrorDisplay.UpdateErrorFix("PHP request cannot be found!");
                    break;

                /// cannot access
                case 403:
                    ErrorDisplay.UpdateErrorFix(webRequest.error);
                    break;

                /// bad request
                case 400:
                    ErrorDisplay.UpdateErrorFix(webRequest.error);
                    break;

                /// internal error
                case 500:
                    ErrorDisplay.UpdateErrorFix(webRequest.error);
                    break;

                /// gateway timeout
                case 503:
                    ErrorDisplay.UpdateErrorFix(webRequest.error);
                    break;

                /// generic error
                default:
                    ErrorDisplay.UpdateErrorFix("Please Restart Your Browser Or Open A New Browser");
                    break;

            }
        }
        */
    }
}


