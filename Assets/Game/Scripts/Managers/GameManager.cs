using System.Collections;
using System.Collections.Generic;
using Game.Scripts;
using Game.Scripts.Helpers;
using Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>, IDestroyable
{
    public UnityEvent OnPauseGame = new UnityEvent();
    public UnityEvent OnUnpauseGame = new UnityEvent();

    private bool isGamePaused;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SoundManager.Instance.Play("Click");
            if (isGamePaused)
            {
                UnpauseGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public bool ShouldDestroyOnLoad() => true;

    public void OnSceneChanged(Scene previousScene, Scene nextScene)
    {
    }

    private void PauseGame()
    {
        isGamePaused = true;
        Time.timeScale = 0f;
        OnPauseGame?.Invoke();
    }

    private void UnpauseGame()
    {
        isGamePaused = false;
        Time.timeScale = 1f;
        OnUnpauseGame?.Invoke();
    }

    public void NewGame()
    {
        SceneLoader.Instance.Load(SceneEnum.Level1);
    }

    public void ResetGame()
    {
        AsyncOperation restartOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        restartOperation.completed += operation => {
            isGamePaused = false;
            Time.timeScale = 1f;
        };
    }

    public void MainMenu()
    {
        AsyncOperation restartOperation = SceneManager.LoadSceneAsync(SceneEnum.MainMenu.ToString());

        restartOperation.completed += operation => {
            isGamePaused = false;
            Time.timeScale = 1f;
        };
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
