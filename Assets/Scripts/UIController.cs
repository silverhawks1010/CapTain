using System;
using System.Threading;
using UnityEngine;

public class UIController : MonoBehaviour
{

    static public UIController instance;

    [SerializeField] private AudioClip buttonClickSound = null;

    private AudioSource buttonAudioSource;    

    public GameObject startButton;
    public GameObject optionsButton;
    public GameObject exitButton;
    public GameObject easyButton;
    public GameObject mediumButton;
    public GameObject returnButton;
    public GameObject placementButton;

    public GameObject mainMenuPanel;
    public GameObject mainButtonPanel;
    public GameObject startButtonPanel;
    public GameObject GameOverPanel;
    public GameObject placementPanel;

    void Awake()
    {
        buttonAudioSource = GetComponent<AudioSource>();
        instance = this;
    }
    
    public void OnClickStartButton()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
        mainButtonPanel.SetActive(false);
        startButtonPanel.SetActive(true);
        Debug.Log("Start Button Clicked");
    }

    public void OnClickOptionsButton()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
        Debug.Log("Options Button Clicked");
    }

    public void OnClickExitButton()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
        Thread.Sleep(200);
        Application.Quit();
        Debug.Log("Exit Button Clicked");
    }

    public void OnClickEasyButton()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
        mainMenuPanel.SetActive(false);
        GameOverPanel.SetActive(true);
        Debug.Log("Easy Button Clicked");
    }

    public void OnClickMediumButton()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
        Debug.Log("Medium Button Clicked");
    }

    public void OnClickReturnButton()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
        mainButtonPanel.SetActive(true);
        startButtonPanel.SetActive(false);
        Debug.Log("Return Button Clicked");
    }

    public void OnClickPlacementButton()
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
        placementPanel.SetActive(false);
        Debug.Log("Placement Button Clicked");
    }
}
