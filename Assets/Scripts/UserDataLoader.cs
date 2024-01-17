using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kid.Web
{
    public class UserDataLoader : WebReqs
    {
        public Text subjectTxt;
        public Text usernameTxt;
        public Text classTxt;
        public Text uuidTxt;

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
            subjectTxt.text = subjectData[1];
            usernameTxt.text = subjectData[2];
            classTxt.text = subjectData[3];
            uuidTxt.text = subjectData[4];
        }

        public override void UpdateForm()
        {
            form = new WWWForm();
            form.AddField("userId", "userId");              // Get Subtopic Id from load scene
            form.AddField("subjectId", "subjectId");              // Get Subtopic Id from load scene
            form.AddField("classId", "classId");
        }
    }

}
