﻿using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GoLevels()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void GoToTitle()
    {
        GameObject databa;
        databa = GameObject.Find("DataBank");
        if (databa != null)
        {
            Destroy(databa);
        }
        SceneManager.LoadScene("Title");

    }

    public void GoToGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
