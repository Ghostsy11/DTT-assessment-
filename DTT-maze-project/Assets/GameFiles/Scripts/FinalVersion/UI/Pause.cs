using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    [SerializeField] bool isPaused;
    [SerializeField] GameObject pausePanel;
    void Start()
    {
        pausePanel.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (!isPaused)
        {
            PauseMenu();
        }
        else
        {
            UnPauseMenu();
        }
    }


    private void PauseMenu()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0.0f;
    }

    private void UnPauseMenu()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1.0f;
    }

}
