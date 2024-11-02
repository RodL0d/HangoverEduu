using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #endregion

    public int scorePlayer; // Removida referência a jogadas
    public CanvasGroup faderCanvasGroup;
    public float fadeDuration = 1f;
    private UIManager managerUI;

    
    
    private void Start()
    {
        StartCoroutine(FadeIn());
        Initialize();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeBase();
        scorePlayer = 0; // Reseta o score ao carregar uma nova cena
        
        FindFaderCanvasGroup();
    }

    private void Initialize()
    {
        InitializeBase();
    }

<<<<<<< Updated upstream
    private void Initialize(Scene scene, LoadSceneMode mode)
    {
        InitializeBase();
        scorePlayer = 0;
        if (SceneManager.GetActiveScene().name != "Menu" & SceneManager.GetActiveScene().name != "seleçãoDeFase")
        {
            UpdateJogadas(jogadasBase);
        }
    }

=======
>>>>>>> Stashed changes
    private void InitializeBase()
    {
        FindButtons();
        managerUI = FindObjectOfType<UIManager>();
        Time.timeScale = 1;
    }

    private void FindButtons()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName == "Menu")
        {
            GameObject.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadScene("seleçãoDeFase"));
            GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(ExitGame);
        }
        else if (sceneName == "seleçãoDeFase")
        {
            GameObject.Find("Return_button").GetComponent<Button>().onClick.AddListener(() => LoadScene("Menu"));
        }
        else if (sceneName == "Jogo")
        {
            GameObject.Find("Play").GetComponent<Button>().onClick.AddListener(() => LoadScene("Jogo"));
            GameObject.Find("Exit").GetComponent<Button>().onClick.AddListener(() => LoadScene("Menu"));
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        faderCanvasGroup.blocksRaycasts = true;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            faderCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            faderCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        faderCanvasGroup.blocksRaycasts = false;
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }

    public void AddScore(int scoreValue)
    {
        scorePlayer += scoreValue;
        managerUI?.UpdateScore(scorePlayer);
    }
    private void FindFaderCanvasGroup()
    {
        // Encontre o CanvasGroup na nova cena
        faderCanvasGroup = GameObject.Find("FadePainel")?.GetComponent<CanvasGroup>();

        if (faderCanvasGroup == null)
        {
            Debug.LogWarning("FaderCanvasGroup not found in the new scene!");
        }
    }

    public void UpdateGameOver(string textGameover)
    {
        managerUI?.ShowGameOver(textGameover);
    }
}