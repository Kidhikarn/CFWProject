using Kid.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    public GameObject usrWrnTxt;
    public GameObject loginCanvas;
    public GameObject playBtn;
    public GameObject dataBank;

    public string uri;
    public string php;

    string username;

    bool nameIn;
    bool successTrigger;
    bool saved;

    DataBank daBank;

    string[] rawArray;

    // Start is called before the first frame update
    void Start()
    {
        usrWrnTxt.SetActive(false);

        successTrigger = false;
        saved = false;
        daBank = dataBank.GetComponent<DataBank>();
    }

    // Update is called once per frame
    void Update()
    {
        if (successTrigger == true)
        {
            EnablePlayBtn();
        }
    }

    // Check if username bar has something
    // Check if username exists
    // Get id from table where username matches



    public void InputUsername(string input)
    {
        if (input == null || input == "")
        {
            usrWrnTxt.SetActive(true);
            nameIn = false;
        }
        else
        {
            username = input;
            Debug.Log("Username: " + username);
            usrWrnTxt.SetActive(false);
            nameIn = true;
        }
        
    }

    public void CheckInputs(string button)
    {
        if (nameIn == true)
        {
            usrWrnTxt.SetActive(false);
            switch (button)
            {
                case "login":
                    StartCoroutine(Login());
                    break;
            }
            
        }
        else
        {
            Debug.Log("Something is missing");
        }
        
        if (nameIn == false)
        {
            usrWrnTxt.SetActive(true);
            usrWrnTxt.GetComponent<Text>().text = "Enter username please!";
        }


    }

    IEnumerator Login()
    {
        php = "php/Login.php";
        WWWForm form = new WWWForm();
        form.AddField("loginUser", username);
        //form.AddField("loginPass", password);

        using (UnityWebRequest www = UnityWebRequest.Post(uri + php, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
                string rawData = www.downloadHandler.text;
                if (rawData != "Username or Password doesn't exist.")
                {
                    rawArray = rawData.Split('`');
                }
                
                Debug.Log("Raw Data: " + rawData);
                if (rawData != "Username or password doesn't exist.")
                {
                    successTrigger = true;
                }
            }
        }
    }

    void EnablePlayBtn()
    {
        saveInDataBank();
        playBtn.SetActive(true);
        loginCanvas.SetActive(false);
    }

    void saveInDataBank()
    {
        if (saved == false)
        {
            foreach (string ele in rawArray)
            {
                Debug.Log(ele);
            }
            daBank.dataDict.Add("username", username);
            Debug.Log("username: " + daBank.dataDict["username"]);
            daBank.dataDict.Add("userID", rawArray[0]);
            Debug.Log("user ID: " + daBank.dataDict["userID"]);
            saved = true;
        }
        
    }
}
