using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    DataBank databa;
    bool helping;
    bool subjectsAdded;
    string subjectID;
    string topicID;
    string subtopicID;
    List<string> topics = new List<string>();
    List<string> topicIDs = new List<string>();
    List<string> subtopicIDs = new List<string>();
    List<string> subtopics = new List<string>();
    List<string> subjectIDs = new List<string>();
    List<string> subjectList = new List<string>();
    List<GameObject> topicPanels = new List<GameObject>();

    Dictionary<string, string> subjectDict = new Dictionary<string, string>();

    bool subtopicking;

    public Text nameTxt;
    public Text classTxt;
    public Text subjectTxt;

    public GameObject prefab;
    public GameObject subtopPrefab;
    public GameObject lPA;
    public GameObject helpPanel;
    public GameObject subtopicPanel;
    public GameObject subjectDropDown;



    // Start is called before the first frame update
    void Start()
    {
        // find databank
        databa = GameObject.Find("DataBank").GetComponent<DataBank>();

        nameTxt.text = databa.dataDict["username"];

        // ui help
        helping = false;
        helpPanel.SetActive(false);
        subtopicPanel.SetActive(false);
        subjectDropDown.SetActive(false);
        subtopicking = false;
        subjectsAdded = true;

        StartCoroutine(GetClassEnrolment());
    }

    // Update is called once per frame
    void Update()
    {
        if (subjectIDs.Count <= subjectList.Count && subjectsAdded == false)
        {
            subjectDropDown.transform.GetChild(0).GetComponent<Dropdown>().AddOptions(subjectList);
            subjectsAdded = true;
            GetTheTopics();
        }
    }

    // level ui help
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

    // Get class enrolment id, if more than 1, immediately set text to multiple classes.
    // Step 1
    IEnumerator GetClassEnrolment()
    {
        string[] rawArray;
        // get user ID from databa
        string userID = databa.dataDict["userID"];
        Debug.Log("user ID extracted: " + userID);

        string uri = "http://localhost:8080/KidsTest/php/";
        string php = "GetClassEnrolment.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", userID);

        using (UnityWebRequest www = UnityWebRequest.Post(uri + php, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log("Form upload complete!");
                string rawData = www.downloadHandler.text;
                //Debug.Log("Result: " + rawData);
                rawArray = rawData.Split('`');
                if (rawData.Length > 2)
                {
                    classTxt.text = "Multiple Classes";
                }
                else
                {
                    StartCoroutine(GetClass(rawArray[0]));
                }
                StartCoroutine(GetSubjectEnrolment());
            }
        }
    }

    // Step 2
    IEnumerator GetClass(string classID)
    {
        string uri = "http://localhost:8080/KidsTest/php/";
        string php = "GetClass.php";
        WWWForm form = new WWWForm();
        form.AddField("classID", classID);

        using (UnityWebRequest www = UnityWebRequest.Post(uri + php, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log("Form upload complete!");
                string rawData = www.downloadHandler.text;
                //Debug.Log("Result: " + rawData);
                classTxt.text = rawData;

            }
        }
    }

    // Step 3
    IEnumerator GetSubjectEnrolment()
    {
        string[] rawArray;
        string userID = databa.dataDict["userID"];
        string uri = "http://localhost:8080/KidsTest/php/";
        string php = "GetSubjectEnrolment.php";
        WWWForm form = new WWWForm();
        form.AddField("userID", userID);

        using (UnityWebRequest www = UnityWebRequest.Post(uri + php, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log("Form upload complete!");
                string rawData = www.downloadHandler.text;
                //Debug.Log("Result: " + rawData);
                rawArray = rawData.Split('`');

                if (rawArray.Length > 2)
                {
                    //Debug.Log("Multiple subjects available!");
                    foreach (string ele in rawArray)
                    {
                        if (ele != "")
                        {
                            Debug.Log("PRINTED: " + ele);
                            subjectIDs.Add(ele);
                        }
                    }
                    DoinStuff();
                }
                else
                {
                    StartCoroutine(GetSubject(rawArray[0]));
                }
            }
        }
    }

    IEnumerator GetSubject(string subjectID)
    {
        string uri = "http://localhost:8080/KidsTest/php/";
        string php = "GetSubject.php";
        WWWForm form = new WWWForm();
        form.AddField("subjectID", subjectID);

        using (UnityWebRequest www = UnityWebRequest.Post(uri + php, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                //Debug.Log("Form upload complete!");
                string rawData = www.downloadHandler.text;
                //Debug.Log("Result: " + rawData);
                subjectList.Add(rawData);
                subjectTxt.text = rawData;
                subjectDict.Add(subjectID, rawData);
                //StartCoroutine(GetTopics(subjectID));
            }
        }
    }

    // Subject Selection Protocol
    // first get all subjects under user
    // then get all subject names
    void DoinStuff()
    {
        subjectDropDown.SetActive(true);

        foreach (string ele in subjectIDs)
        {
            StartCoroutine(GetSubject(ele));
        }

        Dropdown sbjDD = subjectDropDown.transform.GetChild(0).GetComponent<Dropdown>();
        sbjDD.ClearOptions();
        subjectsAdded = false;
    }

    // get the topics, store them in a list, for every topic, instantiate 1 panel

    public void GetTheTopics()
    {
        if (topics.Count > 0)
        {
            topics.Clear();
            topicIDs.Clear();
        }
        if (lPA.transform.childCount > 0)
        {
            foreach (Transform child in lPA.transform)
            {
                Destroy(child.gameObject);
                topicPanels.Clear();
            }
            
        }

        string subjectName = subjectDropDown.transform.GetChild(0).GetChild(0).GetComponent<Text>().text;

        for (int i = 0; i < subjectIDs.Count; i++)
        {
            if (subjectList[i] == subjectName)
            {
                subjectID = subjectIDs[i];
                StartCoroutine(GetTopics(subjectID));
            }
        }
        
    }

    // get the topics and store them in a list
    IEnumerator GetTopics(string subjectID)
    {
        string[] rawArray;

        string uri = "http://localhost:8080/KidsTest/php/";
        string php = "GetTopic.php";
        WWWForm form = new WWWForm();
        form.AddField("subjectID", subjectID);

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
                Debug.Log("Result: " + rawData);

                rawArray = rawData.Split('`');

                List<string> rawList = new List<string>();

                foreach (string ele in rawArray)
                {
                    if (ele != "")
                    {
                        rawList.Add(ele);
                        Debug.Log("Topics: " + ele);
                    }
                }

                for (int i = 0; i < rawList.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        topicIDs.Add(rawList[i]);
                    }
                    else
                    {
                        topics.Add(rawList[i]);
                    }
                }

                foreach (string ele in topicIDs)
                {
                    Debug.Log("In topicIDS: " + ele);
                }

                foreach (string ele in topics)
                {
                    Debug.Log("In topics: " + ele);
                }

                Populate();
            }
        }
    }

    // instantiate a panel for every topic
    void Populate()
    {
        foreach (string ele in topics)
        {
            GameObject temp = Instantiate(prefab, lPA.transform);
            temp.GetComponent<RectTransform>().position = Vector2.zero;
            temp.name = ele +"Topic" + "Panel";
            temp.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = ele;
        }
        ListTopics();
        AddButtonFunction(topicPanels); // after subject switch this doesn't work
    }

    // add all topic panels into a list
    void ListTopics()
    {
        foreach (Transform child in lPA.transform)
        {
            Debug.Log("Children of lpa: " + child.name);
            topicPanels.Add(child.gameObject);
        }
        
    }
    // for every panel in the list, give them button as well as function to open up panel
    void AddButtonFunction(List<GameObject> list)
    {
        Debug.Log("topic panels count: " + list.Count);
        foreach (GameObject ele in list)
        {
            Debug.Log("LPA Child name: " + ele.name);
            ele.GetComponent<Button>().onClick.AddListener(OpenSubtopicPanel); // WORKS OMG, doesn't show up on inspector tho
        }
    }

    public void OpenSubtopicPanel()
    {
        // open subtopic panel
        subtopicPanel.SetActive(true);

        // get all subtopics from this topic
        string currInd;
        for (int i = 0; i < topics.Count; i++)
        {
            if (topics[i] == UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.GetChild(1).GetChild(0).GetComponent<Text>().text)
            {

                currInd = topicIDs[i];
                StartCoroutine(GetSubtopics(currInd));
            }
        }

    }

    public void CloseSubtopicPanel()
    {
        subtopics.Clear();
        subtopicIDs.Clear();

        Debug.Log("Scroller name: " + subtopicPanel.transform.GetChild(1).GetChild(0).GetChild(0).name);
        foreach (Transform child in subtopicPanel.transform.GetChild(1).GetChild(0).GetChild(0))
        {
            Debug.Log("Child name: " + child.gameObject.name);
            Destroy(child.gameObject);
        }
        subtopicPanel.SetActive(false);
    }

    // check db for amount of unique (difficulty, subtopic) combinations
    
    // 1. Get topic number [done]
    // 2. get all subtopic numbers [done, with name]
    // 3. get all qns with the subtopic numbers
    IEnumerator GetSubtopics(string topicID)
    {
        Debug.Log("Called");
        string[] rawArray;
        string uri = "http://localhost:8080/KidsTest/php/";
        string php = "GetSubtopic.php";
        WWWForm form = new WWWForm();
        form.AddField("topicID", topicID);

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
                Debug.Log("Result: " + rawData);
                rawArray = rawData.Split('`');

                List<string> rawList = new List<string>();

                foreach (string ele in rawArray)
                {
                    if (ele != "")
                    {
                        rawList.Add(ele);
                    }
                }

                for (int i = 0; i < rawList.Count; i++)
                {
                    Debug.Log("Banana: " + rawList[i]);
                    if (i % 2 == 0)
                    {
                        subtopicIDs.Add(rawList[i]);
                    }
                    else
                    {
                        subtopics.Add(rawList[i]);
                    }
                }

                SubPopulate();
            }
        }
    }

    // populate the subtopics panel
    void SubPopulate()
    {
        for (int i = 0; i < subtopicIDs.Count; i++)
        {
            Debug.Log("Subtopic " + subtopicIDs[i] + ": " + subtopics[i]);

            GameObject temp = Instantiate(subtopPrefab, subtopicPanel.transform.GetChild(1).GetChild(0).GetChild(0));
            temp.name = subtopics[i] + "Subtopic" + "Panel";
            temp.transform.GetChild(0).GetComponent<Text>().text = subtopics[i];
            temp.transform.GetChild(1).GetComponent<Text>().text = "easy";
            temp.AddComponent<Button>();
            temp.GetComponent<Button>().onClick.AddListener(Proceed);

        }
    }

    // function for going to diff scene as well as shoving the info into databa

    public void Proceed()
    {
        GameObject currentObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        List<string> textList = new List<string>();
        for (int i = 0; i < 2; i++)
        {
            textList.Add(currentObj.transform.GetChild(i).GetComponent<Text>().text);
        }

        // get subtopicid from list based on element position of subtopic name

        for (int i = 0; i < subtopics.Count; i++)
        {
            if (subtopics[i] == textList[0])
            {
                string sbtpID = subtopicIDs[i];
                databa.dataDict.Add("subtopicID", sbtpID);
                Debug.Log(databa.dataDict["subtopicID"]);
            }
        }

        databa.dataDict.Add("subtopic", textList[0]);
        Debug.Log(databa.dataDict["subtopic"]);

        SceneManager.LoadScene("GameScene");

    }
}
