using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI scoreText, jogadasText, gameoverText;
    [SerializeField] Button buttonClose;
    [SerializeField]GameObject menuPanel;

    // Método para reiniciar a cena atual
    public void RestartGame(string sceneName)
    {
        menuPanel.SetActive(false);
        GameManager.instance.LoadScene(sceneName);
    }

    public void QuitGame(string sceneName)
    {
        GameManager.instance.LoadScene(sceneName);
    }

    public void ToggleMenu(bool isActive)
    {
        menuPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0f : 1f;
    }

    // Atualiza o texto de game over e exibe o menu de fim de jogo
    public void ShowGameOver(string textGameOver)
    {
        gameoverText.text = textGameOver;
        menuPanel.SetActive(true);
        Time.timeScale = 0f;
        buttonClose.enabled = false;
    }

    // Atualiza o texto de jogadas restantes
    public void UpdateJogadas(int jogadas)
    {
        jogadasText.text = jogadas.ToString();
    }

    // Atualiza o texto da pontuação
    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
