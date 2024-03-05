using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QuestionsManager : MonoBehaviour
{
    DataBank databa;
    List<string> rawQnInfoList = new List<string>();
    string qnHint;
    string qnAns;
    bool hinting;
    bool paused;
    int qnCount;
    int prevQnCount;
    int attemptCount;
    float time;
    string startTime;
    float storedTime;
    int score;
    string qnID;

    public GameObject questionTxt;
    public GameObject qnCountTxt;
    public GameObject attemptCountTxt;
    public GameObject hintBtn;
    public GameObject hintPanel;
    public GameObject pausePanel;
    public GameObject timeTxt;
    public GameObject gameOverPanel;
    public GameObject scoreTxt;
    public List<GameObject> btnList = new List<GameObject>();
    public GameObject answerStatPanel;
    public Sprite rightIcon, wrongIcon;

    // Start is called before the first frame update
    void Start()
    {
        // find databank
        databa = GameObject.Find("DataBank").GetComponent<DataBank>();

        hinting = false;
        paused = false;
        qnCount = 1;
        prevQnCount = qnCount;
        attemptCount = 1;
        startTime = System.DateTime.Now.ToString();
        Debug.Log("Subtopic start time: " + startTime);
        //hintBtn.SetActive(true);

        DatabaCheck();
        UpdateQnNAttemptCounts();
        StartCoroutine(GetQnsInfo());
    }

    // Update is called once per frame
    void Update()
    {
        Timer();
        FinishCheck();
        ShowHintBtn();

        if (qnCount > 2)
        {
            GameOver();
        }
    }

    // get databa [done in start]
    // debug all needed contents
    void DatabaCheck()
    {
        if (databa != null)
        {
            Debug.Log("userID: " + databa.dataDict["userID"]);
            Debug.Log("subtopicID: " + databa.dataDict["subtopicID"]);
            Debug.Log("subtopic: " + databa.dataDict["subtopic"]);
            Debug.Log("difficulty: " + databa.dataDict["difficulty"]);
        }
        else
        {
            Debug.Log("Databa not found!");
        }
    }

    // webreq to get subtopic id [no need, subtopic ID passed thru databa]
    // webreq to get all qns with wubtopic id and difficulty combination, store info into a list
    IEnumerator GetQnsInfo()
    {
        string[] rawArray;
        string uri = "http://localhost:8080/KidsTest/Test2/";
        string php = "GetQnsInfo.php";
        WWWForm form = new WWWForm();
        form.AddField("subtopicID", databa.dataDict["subtopicID"]);
        form.AddField("difficulty", databa.dataDict["difficulty"]);

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
                rawArray = rawData.Split('!');
                foreach (string ele in rawArray)
                {
                    Debug.Log("Raw Qn Array Element: " + ele);
                    if (ele != "")
                    {
                        rawQnInfoList.Add(ele);
                    }
                }
            }
        }
        DisplayQn();
    }

    // display qn
    void DisplayQn()
    {
        hintBtn.SetActive(false);
        // create array/list
        int ranNum = Random.Range(0, rawQnInfoList.Count);
        string currRawQn = rawQnInfoList[ranNum];
        // break the rawqnarray element into bits
        string[] qnInfo = currRawQn.Split('`');
        Debug.Log("test 1: " + currRawQn);
        for (int i = 0; i < qnInfo.Length; i++)
        {
            Debug.Log("qnInfo element " + i + ": " + qnInfo[i]);
        }

        // fit accordingly
        qnID = qnInfo[0];
        questionTxt.GetComponent<Text>().text = qnInfo[1];
        qnHint = qnInfo[2];
        // shuffle answers maybe
        List<string> optionList = new List<string>();
        for (int i = 3; i < 7; i++)
        {
            optionList.Add(qnInfo[i]);
            Debug.Log("Counter: " + i);
        }
        foreach (string ele in optionList)
        {
            Debug.Log("In options list: " + ele);
        }
        for (int i = 0; i < btnList.Count; i++)
        {
            int randNum = Random.Range(0, optionList.Count);
            Debug.Log("rnad num: " + randNum);
            btnList[i].transform.GetChild(1).GetComponent<Text>().text = optionList[randNum];
            optionList.Remove(optionList[randNum]);
        }
        qnAns = qnInfo[7];
        Debug.Log("test 2: " + currRawQn);
        Debug.Log("Removed from rawqninfolist: " + currRawQn);
        rawQnInfoList.Remove(currRawQn);
    }

    public void AnswerCheck()
    {
        GameObject currentObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        string selected = currentObj.transform.GetChild(1).GetComponent<Text>().text;
        Debug.Log("Selected: " + selected);
        answerStatPanel.SetActive(true);
        if (selected == qnAns)
        {
            Debug.Log("Correct!");
            answerStatPanel.transform.GetChild(1).GetComponent<Image>().sprite = rightIcon;
            switch (attemptCount)
            {
                case 1:
                    score += 2;
                    break;

                case 2:
                    score++;
                    break;
            }
            qnCount++;
        }
        else
        {
            Debug.Log("Wrong!");
            answerStatPanel.transform.GetChild(1).GetComponent<Image>().sprite = wrongIcon;
            attemptCount++;
        }
    }

    public void CloseStat()
    {
        answerStatPanel.SetActive(false);

        if (attemptCount > 2)
        {
            qnCount++;
        }
        if (prevQnCount != qnCount)
        {
            prevQnCount = qnCount;
            GoNext();
        }
            
        UpdateQnNAttemptCounts();
    }

    void UpdateQnNAttemptCounts()
    {
        qnCountTxt.GetComponent<Text>().text = "Question: " + qnCount.ToString();
        attemptCountTxt.GetComponent<Text>().text = "Attempt: " + attemptCount.ToString();
    }

    void GoNext()
    {
        attemptCount = 1;
        storedTime += time;
        time = 0;
        scoreTxt.GetComponent<Text>().text = "Score: " + score;
        DisplayQn();
    }

    public void ToggleHint()
    {
        if (hinting == false)
        {
            hintPanel.SetActive(true);
            hintPanel.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = qnHint;
        }
        else
        {
            hintPanel.SetActive(false);
        }
        hinting = !hinting;
    }

    void Timer()
    {
        if (paused == false)
        {
            time += Time.deltaTime;

        }
        Debug.Log("Time elapsed: " + Mathf.RoundToInt(time));
        timeTxt.GetComponent<Text>().text ="Elapsed: " + Mathf.RoundToInt(time).ToString() + "s";
    }

    public void TogglePause()
    {
        if (paused == false)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
        paused = !paused;
        Debug.Log("Paused: " + paused);
    }

    void FinishCheck()
    {
        if (rawQnInfoList.Count < 1)
        {
            Debug.Log("DONE ALR LAH");
        }
    }

    void ShowHintBtn()
    {
        if (time >= 10 || attemptCount > 1)
        {
            hintBtn.SetActive(true);
        }
    }

    void GameOver()
    {
        paused = true;
        gameOverPanel.SetActive(true);
        gameOverPanel.transform.GetChild(5).GetComponent<Text>().text = score.ToString();

        Debug.Log("Start Time: " + startTime);
        //Debug.Log("Duration: " + )
    }

    // send qn attempt (on GameOver)
    IEnumerator SendQnAttempt(string startTime, string duration, string qnID)
    {
        string uri = "http://localhost:8080/KidsTest/Test2/";
        string php = "SendQnAttempt.php";
        WWWForm form = new WWWForm();
        form.AddField("startTime", startTime);
        form.AddField("duration", duration);
        form.AddField("userID", databa.dataDict["userID"]);
        form.AddField("qnID", qnID);
        form.AddField("sbtpID", databa.dataDict["sbtpID"]);

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
            }
        }
    }

    // send subtopic attempt (on GameOver)
    IEnumerator SendSubtopicAttempt(string startTime, string duration)
    {
        string uri = "http://localhost:8080/KidsTest/Test2/";
        string php = "SendSubtopicAttempt.php";
        WWWForm form = new WWWForm();
        form.AddField("startTime", startTime);
        form.AddField("duration", storedTime.ToString());
        form.AddField("score", score);
        form.AddField("userID", databa.dataDict["userID"]);
        form.AddField("sbtpID", databa.dataDict["sbtpID"]);

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
            }
        }
    }
}
