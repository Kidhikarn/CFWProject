using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebTest : MonoBehaviour
{
    private string uri;
    private string phpFile;

    void Start()
    {
        uri = "http://localhost:8080/KidsTest";
        //uri = "https://ec2-13-212-55-238.ap-southeast-1.compute.amazonaws.com/php";

        // A correct website page.
        //StartCoroutine(GetDate());
        StartCoroutine(GetUsers());
        //StartCoroutine(Login("test", "test"));
    }

    IEnumerator GetDate()
    {
        phpFile = "/EchoTest.php";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri + phpFile))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                GameObject timeTxt;
                timeTxt = GameObject.Find("timeTxt");
                Debug.Log("time game object:" + timeTxt.name);
                timeTxt.GetComponent<Text>().text = webRequest.downloadHandler.text;
            }
        }
    }

    IEnumerator GetUsers()
    {
        phpFile = "/GetUsers.php";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri + phpFile))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":Received: " + webRequest.downloadHandler.text);
            }
        }
    }

    IEnumerator Login(string username, string password)
    {
        phpFile = "/Login.php";
        WWWForm form = new WWWForm();

        form.AddField("loginUser", username);
        form.AddField("loginPass", password);

        using (UnityWebRequest www = UnityWebRequest.Post(uri + phpFile, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }
}
