using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    #endregion


    public int scorePlayer, jogadas,jogadasBase;

    UIManager managerUI;

    private void Start()
    {
        Initialize();
        SceneManager.sceneLoaded += Initialize;
    }
    private void Initialize()
    {
        InitializeBase();
    }

    private void Initialize(Scene scene, LoadSceneMode mode)
    {
        InitializeBase();
        scorePlayer = 0;
        if (SceneManager.GetActiveScene().name != "Menu" & SceneManager.GetActiveScene().name != "seleçãoDeFase")
        {
            UpdateJogadas(jogadasBase);
        }
    }

    private void InitializeBase()
    {
        FindButtons();
        managerUI = FindObjectOfType<UIManager>();
        Time.timeScale = 1;
    }


    private void FindButtons()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            GameObject.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadScene("seleçãoDeFase"));
            GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(ExitGame);
        }

        if (SceneManager.GetActiveScene().name == "seleçãoDeFase")
        {
            GameObject.Find("Return_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Menu"));
        }
        if (SceneManager.GetActiveScene().name == "Jogo")
        {
            GameObject.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadScene("Jogo"));
            GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(() => LoadScene("Menu"));
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void ExitGame()
    {
        Application.Quit();
    }

    public void AddScore(int scoreValue)
    {
        scorePlayer += scoreValue;
        managerUI.UpdateScore(scorePlayer);
    }

    public void UpdateGameOver(string textGameover)
    {
        managerUI.UpdateTextGameOver(textGameover); 
    }
    public void UpdateJogadas(int jogadasValue)
    {
        jogadas += jogadasValue;
        managerUI.UpdateJogadas(jogadas);
    }
}
