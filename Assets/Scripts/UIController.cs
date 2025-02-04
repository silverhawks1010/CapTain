using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Game State UI")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;

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
        Debug.Log("Placement button clicked");
        buttonAudioSource.PlayOneShot(buttonClickSound);
        placementPanel.SetActive(false);
        
        // Démarrer le jeu via le GameManager
        if (GameManager.instance != null)
        {
            Debug.Log("Calling GameManager.StartGame()");
            GameManager.instance.StartGame();
        }
        else
        {
            Debug.LogError("GameManager instance is null! Make sure there is a GameManager in the scene.");
        }
        
        Debug.Log("Placement button processing complete");
    }

    public void UpdateTurnDisplay(bool isPlayerTurn)
    {
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "Votre tour" : "Tour de l'adversaire";
            turnText.color = isPlayerTurn ? Color.green : Color.red;
        }
    }

    public void ShowGameOver(bool playerWins)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                gameOverText.text = playerWins ? "Victoire !" : "Défaite...";
                gameOverText.color = playerWins ? Color.green : Color.red;
            }
        }
    }
}
