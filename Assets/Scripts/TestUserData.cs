using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kid.Web
{
    public class TestUserData : WebReqs
    {
        public Text usernameTxt;
        public Text levelTxt;

        public string[] Subject { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            LoadUserData();
        }

        public void LoadUserData()
        {
            SendWebRequest();
        }

        public override void Output()
        {
            if (webRequest.downloadHandler.text == "")
            {
                Debug.Log("user not found"); //if user not found / doesnt exist
                //errorPanel.SetActive(true);
            }

            Subject = webRequest.downloadHandler.text.Split(';');
            string[] subjectData = Subject[0].Split('`');
            foreach (string element in subjectData)
            {
                Debug.Log("element: " + element);
            }
            usernameTxt.text = subjectData[0];
            levelTxt.text = subjectData[1];
        }

        /*
        public override void UpdateForm()
        {
            form = new WWWForm();
            form.AddField("uesrname", "test");
        }
        */
    }

}

