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
        jogadas = jogadasBase;
        if (SceneManager.GetActiveScene().name == "Fase 1") 
        {
            UpdateJogadas(jogadas);
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
            GameObject.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadScene("sele��oDeFase"));
            GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(ExitGame);
            GameObject.Find("Config").GetComponent<Button>().onClick.AddListener(FindButton);
        }

        if (SceneManager.GetActiveScene().name == "sele��oDeFase")
        {
            FindButtonFase();
        }
        if (SceneManager.GetActiveScene().name == "Jogo")
        {
            GameObject.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadScene("Jogo"));
            GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(() => LoadScene("Menu"));
        }
    }

    void FindButtonFase()
    {
        GameObject.Find("Fase1_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Fase 1"));
        GameObject.Find("Fase2_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Fase 2"));
        GameObject.Find("Fase3_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Fase 3"));
        GameObject.Find("Fase4_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Fase 4"));
        GameObject.Find("Fase5_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Fase 5"));
        GameObject.Find("Return_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Menu"));
        GameObject.Find("Config").GetComponent<Button>().onClick.AddListener(FindButton);

    }

    public void FindButton()
    {
        GameObject.Find("ShareBt").GetComponent<Button>().onClick.AddListener(() => OpenWebSite("https://www.instagram.com/hangover_farmerss?igsh=MWswNWM4ejNseHExcg==\n"));
    }
    public void OpenWebSite(string url)
    { 
        Application.OpenURL(url);
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
