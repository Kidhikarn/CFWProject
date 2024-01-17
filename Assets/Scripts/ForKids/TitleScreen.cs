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
    public GameObject pswdWrnTxt;
    public GameObject loginCanvas;
    public GameObject playBtn;
    public GameObject dataBank;

    public string uri;
    public string php;

    string username;
    string password;
    float subjects;

    bool nameIn;
    bool passIn;
    bool successTrigger;

    DataBank daBank;

    string[] rawArray;

    // Start is called before the first frame update
    void Start()
    {
        usrWrnTxt.SetActive(false);
        pswdWrnTxt.SetActive(false);

        successTrigger = false;
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
    
    public void InputPassword(string input)
    {
        if (input == null || input == "")
        {
            pswdWrnTxt.SetActive(true);
            passIn = false;
        }
        else
        {
            password = input;
            Debug.Log("Password: " + password);
            pswdWrnTxt.SetActive(false);
            passIn = true;
        }
        
    }

    public void CheckInputs(string button)
    {
        if (nameIn == true && passIn == true)
        {
            usrWrnTxt.SetActive(false);
            pswdWrnTxt.SetActive(false);
            switch (button)
            {
                case "login":
                    StartCoroutine(Login());
                    break;
                case "signup":
                    StartCoroutine(SignUp());
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

        if (passIn == false)
        {
            pswdWrnTxt.SetActive(true);
        }

    }

    IEnumerator Login()
    {
        php = "GameLogin.php";
        WWWForm form = new WWWForm();
        form.AddField("loginUser", username);
        form.AddField("loginPass", password);

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
        saveInDataBank();
    }

    IEnumerator SignUp()
    {
        php = "GameSignUp.php";
        WWWForm form = new WWWForm();
        form.AddField("loginUser", username);
        form.AddField("loginPass", password);

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
                Debug.Log("Raw Data: " + rawData);
                if (rawData == "Exists" || rawData == "Failure")
                {
                    usrWrnTxt.SetActive(true);
                    usrWrnTxt.GetComponent<Text>().text = "Try again with a different username!";
                }
                else if (rawData == "Success")
                {
                    successTrigger = true;
                }
            }
        }
        saveInDataBank();
    }

    void EnablePlayBtn()
    {
        playBtn.SetActive(true);
        loginCanvas.SetActive(false);
    }

    void saveInDataBank()
    {
        foreach(string ele in rawArray)
        {
            Debug.Log(ele);
        }
        daBank.dataDict.Add("username", username);
        Debug.Log(daBank.dataDict["username"]);
        daBank.dataDict.Add("subjects", rawArray[0]);
        Debug.Log(daBank.dataDict["subjects"]);
        daBank.dataDict.Add("level", rawArray[1]);
        Debug.Log(daBank.dataDict["level"]);
        daBank.dataDict.Add("class", rawArray[2]);
        Debug.Log(daBank.dataDict["class"]);
        daBank.dataDict.Add("experience", rawArray[3]);
        Debug.Log(daBank.dataDict["experience"]);
    }
}
