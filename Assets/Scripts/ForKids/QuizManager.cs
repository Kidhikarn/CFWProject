using Kid.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuizManager : MonoBehaviour
{
    // game object variables
    public GameObject questionNum;
    public GameObject attemptNum;

    public GameObject questionTxt;
    public GameObject hintTxt;
    public GameObject timeTxt;
    public GameObject scoreTxt;

    public GameObject btnA, btnB, btnC, btnD;

    public Sprite loadingImg;

    public GameObject hintBtn;
    public GameObject gameOverPanel;
    public GameObject answerStatPanel;
    public GameObject hintPanel;
    public GameObject pausePanel;

    GameObject dataBankObj;

    public Sprite corrIcon, wrongIcon;


    // database data variables
    string questionText;
    string hintText;
    string questionAnswer;

    string mcqA, mcqB, mcqC, mcqD;

    List<string> qnDataList = new List<string>();
    List<string> qnList = new List<string>();
    List<string> optionList = new List<string>();

    public List<GameObject> buttonList = new List<GameObject>();
    public List<Sprite> spriteList = new List<Sprite>();

    // server images
    Texture textureA, textureB, textureC, textureD;

    // networking things
    [SerializeField]
    private string uri;
    [SerializeField]
    private string php;

    // questions things
    [SerializeField]
    int qnCount;
    int attempts;
    int score;

    float startTime;
    float currTime;
    float endTime;

    string currQnRaw;

    bool qnChange;
    bool hintOpen;
    bool paused;

    // Start is called before the first frame update
    void Start()
    {
        qnCount = 1;
        attempts = 1;
        score = 4;

        startTime = 0;
        currTime = startTime;

        uri = "http://localhost:8080/KidsTest/";

        qnChange = false;
        hintOpen = false;

        buttonList.Add(btnA);
        buttonList.Add(btnB);
        buttonList.Add(btnC);
        buttonList.Add(btnD);

        dataBankObj = GameObject.Find("DataBank");

        UpdateQNA();

        hintTxt.SetActive(false);
        hintPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        answerStatPanel.SetActive(false);

        StartCoroutine(GetQnData());
        
    }

    // Update is called once per frame
    void Update()
    {
        if (attempts > 2)
        {
            qnChange = true;
            attempts = 1;
        }
        if (qnChange == true && qnList.Count != 0)
        {
            SetDefImg();
            StartNewQuestion();
            qnChange = false;
        }
        HintCheck();

        if (qnCount > 2)
        {
            OpenGameOver();
        }

        Timer();
        scoreTxt.GetComponent<Text>().text = "Score: " + score;
        timeTxt.GetComponent<Text>().text = "Time: " + Mathf.RoundToInt(currTime) + "s";
    }

    // get qn, mcq options, correct answer & hint from db thru server

    IEnumerator GetQnData()
    {
        string[] qnArray;

        
        php = "GetQnInfo.php";
        using (UnityWebRequest webReq = UnityWebRequest.Get(uri + php))
        {
            // Request and wait for the desired page.
            yield return webReq.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webReq.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webReq.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webReq.downloadHandler.text);
                string rawData = webReq.downloadHandler.text;
                qnArray = rawData.Split(';');
                foreach (string ele in qnArray)
                {
                    qnList.Add(ele);
                }
                foreach (string question in qnArray)
                {
                    Debug.Log(question);
                }
                DisplayQn();
            }
        }
    }

    // get images
    // set them to corresponding buttons

    IEnumerator GetImages()
    {
        foreach(GameObject button in buttonList)
        {
            php = "Images/" + button.transform.GetChild(1).GetComponent<Text>().text + ".png";

            UnityWebRequest uwrt = UnityWebRequestTexture.GetTexture(uri + php);
            Debug.Log("web rq: " + uri + php);

            yield return uwrt.SendWebRequest();

            if (uwrt.isNetworkError || uwrt.isHttpError)
            {
                Debug.Log("Error: " + uwrt.error);
            }
            else
            {
                // Get downloaded asset bundle
                Texture2D tex = DownloadHandlerTexture.GetContent(uwrt);
                button.transform.GetChild(0).GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }


    // set them
    // must only be called after the coroutine "GetQnData()"
    void DisplayQn()
    {
        int randInt = UnityEngine.Random.Range(0, qnList.Count);
        Debug.Log("random int: " + randInt);

        currQnRaw = qnList[randInt];

        qnDataList.Clear();

        string[] qnData = currQnRaw.Split('`');

        foreach (string ele in qnData)
        {
            qnDataList.Add(ele);
        }

        questionTxt.GetComponent<Text>().text = qnDataList[0];
        //hintText = qnDataList[1];
        //Debug.Log("hint: " + hintText);
        optionList.Add(qnDataList[2]);
        optionList.Add(qnDataList[3]);
        optionList.Add(qnDataList[4]);
        optionList.Add(qnDataList[5]);
        
        for(int i = 0; i < buttonList.Count; i++)
        {
            int ranNum = UnityEngine.Random.Range(0, optionList.Count);
            buttonList[i].transform.GetChild(1).GetComponent<Text>().text = optionList[ranNum];
            optionList.Remove(optionList[ranNum]);
        }

        questionAnswer = qnDataList[6];
        Debug.Log("correct answer: " + questionAnswer);
        StartCoroutine(GetImages());
    }


    //check input against correct answer
    public void InputAnswer()
    {
        GameObject input = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        string inputAnswer = input.transform.GetChild(1).GetComponent<Text>().text;
        Debug.Log("Input Answer: " + inputAnswer);
        CheckAnswer(inputAnswer);
    }

    void CheckAnswer(string input)
    {
        paused = true;
        if (input == questionAnswer)
        {
            // move on to next qn
            answerStatPanel.SetActive(true);
            answerStatPanel.transform.GetChild(1).GetComponent<Image>().sprite = corrIcon;
        }
        else
        {
            Debug.Log("You are stupid");
            score--;
            answerStatPanel.SetActive(true);
            answerStatPanel.transform.GetChild(1).GetComponent<Image>().sprite = wrongIcon;
            // show hints
            attempts++;
            UpdateQNA();
        }
    }

    void UpdateQNA()
    {
        questionNum.GetComponent<Text>().text = "Question: " + qnCount;
        attemptNum.GetComponent<Text>().text = "Attempts: " + attempts;
    }

    void StartNewQuestion()
    {
        
       foreach (string ele in qnList)
       {
            if (ele == currQnRaw)
            {
                qnList.Remove(ele);
                break;
            }
       }
       

        qnCount++;
        attempts = 1;
        UpdateQNA();

        qnDataList.Clear();
        DisplayQn();
    }

    void SetDefImg()
    {
        foreach(GameObject button in buttonList)
        {
            button.transform.GetChild(0).GetComponent<Image>().sprite = loadingImg;
        }
    }

    void HintCheck()
    {
        if (attempts > 1)
        {
            hintBtn.SetActive(true);
        }
        else if (Mathf.RoundToInt(currTime) == 10 && qnCount == 1)
        {
            hintBtn.SetActive(true);
        }
        /*
        else
        {
            hintBtn.SetActive(false);
        }
        */
    }

    // we want to set the hint text upon opening the hint panel
    public void ToggleHint()
    {
        if(hintOpen == true)
        {
            hintPanel.SetActive(false);
            hintOpen = false;
        }
        else
        {
            hintPanel.SetActive(true);
            hintOpen = true;

            hintTxt.SetActive(true);
            hintText = qnDataList[1];
            hintTxt.GetComponent<Text>().text = hintText;
            Debug.Log("hint: " + hintText);
        }
    }

    public void OpenGameOver()
    {
        
        GoNext();
        paused = true;
        gameOverPanel.SetActive(true);
        gameOverPanel.transform.GetChild(5).GetComponent<Text>().text = score.ToString();
    }

    public void GoNext()
    {
        paused = false;
        answerStatPanel.SetActive(false);
        if (answerStatPanel.transform.GetChild(1).GetComponent<Image>().sprite == corrIcon)
        {
            qnChange = true;
            hintBtn.SetActive(false);
        }
    }

    void Timer()
    {
        if(paused == false)
        {
            currTime += Time.deltaTime;
            
        }
        Debug.Log("Time elapsed: " + Mathf.RoundToInt(currTime));
    }

    public void TogglePause()
    {
        paused = !paused;
        if (paused == true)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }

    void CheckDBO()
    {
        if (dataBankObj != null)
        {
            DataBank databa = dataBankObj.GetComponent<DataBank>();

            Debug.Log(databa.dataDict["username"]);
            Debug.Log(databa.dataDict["subjects"]);
            Debug.Log(databa.dataDict["level"]);
            Debug.Log(databa.dataDict["class"]);
            Debug.Log(databa.dataDict["experience"]);
        }
        else
        {
            Debug.Log("Databank not found");
        }
    }
}
