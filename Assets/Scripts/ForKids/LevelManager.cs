using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    DataBank databa;
    string[] subjectList;
    int subsAvail;
    List<string> subjectNames = new List<string>();
    List<GameObject> levelBtns = new List<GameObject>();
    bool helping;

    public Text nameTxt;
    public Text classTxt;
    public Text levelTxt;

    public GameObject prefab;
    public GameObject lPA;
    public GameObject helpPanel;

    // Start is called before the first frame update
    void Start()
    {
        databa = GameObject.Find("DataBank").GetComponent<DataBank>();
        subsAvail = 0;
        helping = false;
        helpPanel.SetActive(false);
        SubSep();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < levelBtns.Count; i++)
        {
            Debug.Log("Called");
            levelBtns[i].transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = subjectNames[i];
        }
    }

    //check how many subjects based on how many 1s there are in subject
    //spawn prefabs based on how many subjects
    //edit the picture and texts based on the subject
    //make prefab open up panel to show difficulties
    //

    //script to load scenes

    void SubSep()
    {
        if (databa != null)
        {
            Debug.Log("databa found");
            subjectList = databa.dataDict["subjects"].Split('*');
            for (int i = 0; i < subjectList.Length; i++)
            {
                if (subjectList[i] == 1.ToString())
                {
                    Debug.Log("subject: " + subjectList[i]);
                    subsAvail++;
                }
            }
            Debug.Log("subjects available: " + subsAvail);

            nameTxt.text = databa.dataDict["username"];
            classTxt.text = databa.dataDict["class"];
            levelTxt.text = databa.dataDict["level"];
            Populate();
        }
        else
        {
            Debug.Log("Databa not found");
        }
        
    }

    void Populate()
    {
        for (int i = 0; i < subsAvail; i++)
        {
            Debug.Log("Count: " + i);
            GameObject temp = Instantiate(prefab, lPA.transform);
            temp.GetComponent<RectTransform>().position = Vector2.zero;
            temp.name = "subject" + i;
        }
        SetLevelInfo();
    }

    void SetLevelInfo()
    {
        //store all child objects of LevelPanelArea in an list
        for (int i = 0; i < lPA.transform.childCount; i++)
        {
            levelBtns.Add(lPA.transform.GetChild(i).gameObject);
        }
        //check list length
        Debug.Log("Level Count: " + levelBtns.Count);
        //if only 1, check if it is first subj or second subj
        if (levelBtns.Count == 1)
        {
            for (int i = 0; i < subjectList.Length; i++)
            {
                if (subjectList[i] == 1.ToString())
                {
                    Debug.Log("Only subject " + (i + 1) + " exists!");
                    StartCoroutine(GetSubjectInfo(i + 1));
                }
            }
        }
        else
        {
            for (int i = 0; i < levelBtns.Count; i++)
            {
                StartCoroutine(GetSubjectInfo(i + 1));
            }
        }

    }

    //get the subject name from database
    IEnumerator GetSubjectInfo(int subjectID)
    {
        string uri = "http://localhost:8080/KidsTest/";
        string php = "GetSubjectInfo.php";
        WWWForm form = new WWWForm();
        form.AddField("subjectID", subjectID.ToString());

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
                subjectNames.Add(rawData);
                Debug.Log("Result: " + rawData);
            }
        }
        foreach (string ele in subjectNames)
        {
            Debug.Log("subjects: " + ele);
        }
    }

    
    public void ToggleHelp()
    {
        if (helping == false)
        {
            helpPanel.SetActive(true);
            helping = !helping;
        }
        else
        {
            helpPanel.SetActive(false);
            helping = !helping;
        }
    }

}
