using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI scoreText, jogadasText, gameoverText;
    [SerializeField]Button  buttonclose;
    [SerializeField]GameObject menuPanel;

    public void RedtartScene(string sceneName)
    {
        menuPanel.SetActive(false);
        GameManager.instance.LoadScene(sceneName);
    }

    public void QuitGame(string sceneName)
    {
        GameManager.instance.LoadScene(sceneName);
    }

    public void ActiveMenu(bool ativo)
    {
        if (ativo)
        {
          menuPanel.SetActive(true);
          Time.timeScale = 0f;
        }
        else
        {
          menuPanel.SetActive(false);
          Time.timeScale = 1f;
        }
    }
    public void UpdateTextGameOver(string textGameover)
    {
        gameoverText.text = textGameover;
        menuPanel.SetActive(true);
        Time.timeScale = 0f;
        buttonclose.enabled = false;
    }
    public void UpdateJogadas(int jogadas)
    {
        jogadasText.text = jogadas.ToString(); ;
    }

    public void UpdateScore(int valueScoore)
    {
        scoreText.text = valueScoore.ToString();
    }
}
