using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using System.Threading;

public class QuizManager : MonoBehaviour
{
    DataBank databa;

    Dictionary<string, string[]> qnInfo = new Dictionary<string, string[]>();

    List<string> hardQn = new List<string>();
    List<string> easyQn = new List<string>();
    List<string> qnList = new List<string>();

    int currQn;
    int attempts;
    int totalScore;

    public int attemptLimit;

    public Text qnCount, attemptCount, qnTxt, mcqA, mcqB, mcqC, mcqD, hintTxt;
    public Text scoreTxt, timeTxt;

    string currID, currDiff, currAns, currScore, currHints;
    string startTime;

    public GameObject hintBtn, hintPanel, affirmPanel, endPanel;
    public GameObject pausePanel;

    public Sprite tickImg, crossImg;
    public Sprite lightOnImg, lightOffImg;

    float currTime;
    float recordedTime;

    bool paused;
    bool hinting;

    void Start()
    {
        // can get relevant data from databa
        databa = GameObject.Find("DataBank").GetComponent<DataBank>();
        Debug.Log("Subtopic ID: " + databa.dataDict["subtopicID"]);
        Debug.Log("Easy Count: " + databa.dataDict["easyCount"]);
        Debug.Log("Difficult Count: " + databa.dataDict["diffCount"]);

        currQn = 0;
        attempts = 0;
        currTime = 0;
        recordedTime = 0;
        totalScore = 0;

        paused = false;
        hinting = false;

        RecordStart();

        // get all questions based on subtopic id and store them in a dictionary
        StartCoroutine(GetAllQnInfo());
    }

    void Update()
    {
        Timer();
        TriggerHintButton();
        DisplayCurrent();
        EndCheck();
    }
   
    // reminder: qninfo format:
    // id, difficulty, name, a, b, c, d, answer, score, hint
    IEnumerator GetAllQnInfo()
    {
        string[] rawArray;
        string uri = "http://localhost:8080/KidsTest/php/";
        string php = "GetAllQnInfo.php";
        WWWForm form = new WWWForm();
        form.AddField("subtopicID", databa.dataDict["subtopicID"]);

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

                // separate hard and ez qns
                foreach (string ele in rawArray)
                {
                    if (ele != "")
                    {
                        Debug.Log("Question Data: " + ele);
                        if (ele.Contains("easy"))
                        {
                            Debug.Log("Easy qn: " + ele);
                            easyQn.Add(ele);
                        }
                        else if (ele.Contains("hard"))
                        {
                            Debug.Log("Hard qn: " + ele);
                            hardQn.Add(ele);
                        }
                    }
                    
                }
                LineTheQns();
            }
        }
    }

    // shuffle both lists
    // pick 6 easy and 4 hard
    // shove em into a list
    // shuffle the new list
    // go down the list
    void LineTheQns()
    {
        List<string> tempList = new List<string>();
        int currNum;

        // add 6 easy qns into list
        for (int i = 0; i < int.Parse(databa.dataDict["easyCount"]); i++)
        {
            int randInt = UnityEngine.Random.Range(0, easyQn.Count);
            //Debug.Log(easyQn[randInt]);
            tempList.Add(easyQn[randInt]);
            //Debug.Log(easyQn[randInt]);
            easyQn.RemoveAt(randInt);
        }

        // add 4 hard qns into list
        for (int i = 0; i < int.Parse(databa.dataDict["diffCount"]); i++)
        {
            int randInt = UnityEngine.Random.Range(0, hardQn.Count);
            //Debug.Log(easyQn[randInt]);
            tempList.Add(hardQn[randInt]);
            //Debug.Log(easyQn[randInt]);
            hardQn.RemoveAt(randInt);
        }

        currNum = tempList.Count;

        // shuffle and add into qnList
        for (int i = 0; i < currNum; i++)
        {
            int randInt = UnityEngine.Random.Range(0, tempList.Count);
            //Debug.Log(easyQn[randInt]);
            qnList.Add(tempList[randInt]);
            //Debug.Log(easyQn[randInt]);
            tempList.RemoveAt(randInt);
        }

        Debug.Log("QnList Length: " + qnList.Count);

        QnDisplay();
    }

    // display qn
    // show qn, a, b, c, d
    // public Text qnCount, attemptCount, qnTxt, mcqA, mcqB, mcqC, mcqD, hintTxt;
    void QnDisplay()
    {
        currQn++;
        attempts = 1;
        recordedTime = currTime;

        hintBtn.SetActive(false);

        string[] qnArray = qnList[currQn - 1].Split('`');

        // qn id
        currID = qnArray[0];
        Debug.Log("Current Qn ID: " + currID);
        // difficulty
        currDiff = qnArray[1];
        Debug.Log("Current Qn Difficulty: " + currDiff);
        // qn name
        qnTxt.text = qnArray[2];
        // a
        mcqA.text = qnArray[3];
        // b
        mcqB.text = qnArray[4];
        // c
        mcqC.text = qnArray[5];
        // d
        mcqD.text = qnArray[6];
        // answer
        currAns = qnArray[7];
        Debug.Log("Current Qn Answer: " + currAns);
        // score
        currScore = qnArray[8];
        Debug.Log("Current Qn Score: " + currScore);
        // hints
        currHints = qnArray[9];
        Debug.Log("Current Qn Hints: " + currHints);

    }

    void DisplayCurrent()
    {
        qnCount.text = "Question: " + currQn.ToString();
        attemptCount.text = "Attempt: " + attempts.ToString();
        scoreTxt.text = "Score: " + totalScore;
    }

    public void Answer()
    {
        string selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
        Debug.Log("Selected: " + selected);

        AffirmAnswer(selected);
        
    }

    void AttemptCheck(int limit)
    {
        if (attempts > limit)
        {
            QnDisplay();
        }
    }

    void EndCheck()
    {
        if (currQn > qnList.Count)
        {
            Debug.Log("You did it");
            affirmPanel.SetActive(false);
            endPanel.SetActive(true);
            endPanel.transform.GetChild(5).GetComponent<Text>().text = totalScore.ToString();
        }
    }

    void AffirmAnswer(string selected)
    {
        affirmPanel.SetActive(true);
        if (selected == currAns)
        {
            affirmPanel.transform.GetChild(1).GetComponent<Image>().sprite = tickImg;
            affirmPanel.transform.GetChild(3).GetComponent<Text>().text = "Good Job!";
        }
        else
        {
            affirmPanel.transform.GetChild(1).GetComponent<Image>().sprite = crossImg;
            affirmPanel.transform.GetChild(3).GetComponent<Text>().text = "Try Again!";
        }
    }

    public void UnAffirm()
    {
        if (affirmPanel.transform.GetChild(3).GetComponent<Text>().text == "Good Job!")
        {
            if (attempts > 1)
            {
                totalScore += int.Parse(currScore);
            }
            else
            {
                totalScore += int.Parse(currScore) * 2;
            }
            QnDisplay();
        }
        else
        {
            attempts++;
        }
        AttemptCheck(attemptLimit);
        affirmPanel.SetActive(false);
    }

    void RecordStart()
    {
        DateTime dt = DateTime.Now;
        startTime = dt.ToString();
        Debug.Log("Quiz Started at: " + startTime);
    }

    void Timer()
    {
        if (paused == false && currQn <= qnList.Count)
        {
            currTime += Time.deltaTime;
            timeTxt.GetComponent<Text>().text = "Elapsed: " + Mathf.RoundToInt(currTime).ToString() + "s";
        }
    }

    public void TriggerPause()
    {
        paused = !paused;
        pausePanel.SetActive(paused);
    }

    void TriggerHintButton()
    {
        if ((currTime - recordedTime >= 10) || attempts == 2)
        {
            hintBtn.SetActive(true);
        }
    }

    public void TriggerHint()
    {
        hinting = !hinting;
        hintPanel.SetActive(hinting);
        
        if (hinting == true)
        {
            hintBtn.GetComponent<Image>().sprite = lightOnImg;
            hintPanel.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = currHints;
        }
        else
        {
            hintBtn.GetComponent<Image>().sprite = lightOffImg;
        }
    }

    // facilitate subtopic attempt log
    // it needs start time, duration, star number, score, best fleg, user id, subtopic id, creation time(in php)

    void RecordSubtopicAttempt()
    {

    }
}
