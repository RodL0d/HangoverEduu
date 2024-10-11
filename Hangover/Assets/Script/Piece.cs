using UnityEngine;
using DG.Tweening;

public class Piece : MonoBehaviour
{
    public FrutType frutType; // Tipo da fruta da peça
    public int x; // Posição X da peça no tabuleiro
    public int y; // Posição Y da peça no tabuleiro
    public Board board; // Referência ao tabuleiro
    public bool isInvisible; // Determina se a peça é invisível

    private Renderer pieceRenderer;
    private bool isAnimating; // Para verificar se a animação está em andamento

    void Awake()
    {
        // Obtém o componente Renderer apenas uma vez durante a inicialização
        pieceRenderer = GetComponent<Renderer>();
    }

    public void Init(int x, int y, Board board)
    {
        this.x = x;
        this.y = y;
        this.board = board;
        SetVisibility(!isInvisible); // Define a visibilidade ao inicializar
    }

    void OnMouseDown()
    {
        if (!isInvisible && frutType != FrutType.Vazio)
        {
            if (board.selectedPiece != null && board.selectedPiece != this)
            {
                // Retorna a peça anterior à escala original
                board.selectedPiece.ReturnToOriginalScale(0.5f);
            }

            // Seleciona a nova peça
            board.SelectPiece(this);
            AnimateScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f); // Anima a nova seleção
        }
    }

    public void SetVisibility(bool isVisible)
    {
        if (pieceRenderer != null)
        {
            pieceRenderer.enabled = isVisible;
        }
    }

    public void AnimateScale(Vector3 targetScale, float duration)
    {
        // Se já estiver animando, não faça nada
        if (isAnimating) return;

        isAnimating = true; // Marca que a animação começou

        // Animação de aumento
        transform.DOScale(targetScale, duration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                isAnimating = false; // Marca que a animação terminou
            });
    }

    private void AnimateNewSelection()
    {
        AnimateScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f); // Anima a nova seleção
    }


    public void ReturnToOriginalScale(float duration)
    {
        // Se não estiver animando, volte ao tamanho original
        if (isAnimating) return;

        // Armazena a escala original
        Vector3 originalScale = Vector3.one; // Supondo que o tamanho original seja 1

        // Animação de retorno com delay
        isAnimating = true; // Marca que a animação começou
        transform.DOScale(originalScale, duration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                isAnimating = false; // Marca que a animação terminou
            });
    }

    public void IncreaseScale(Vector3 targetScale, float duration, bool returnToOriginal = false)
    {
        // Verifica se já está animando
        if (isAnimating) return;

        isAnimating = true; // Marca que a animação começou

        // Armazena a escala original
        Vector3 originalScale = transform.localScale;

        // Aumenta a escala com overshoot e elasticidade
        transform.DOScale(originalScale + targetScale, duration)
            .SetEase(Ease.OutBack, 1f)
            .OnComplete(() =>
            {
                if (returnToOriginal)
                {
                    // Se necessário, retorna à escala original com overshoot reverso
                    transform.DOScale(originalScale, duration)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => isAnimating = false); // Marca que a animação terminou
                }
                else
                {
                    isAnimating = false; // Marca que a animação terminou
                }
            });
    }

    void OnDestroy()
    {
        // Cancela qualquer Tween ativo associado a este transform
        transform.DOKill();
    }
}

// Enumeração para os tipos de frutas disponíveis
public enum FrutType
{
    Abacaxi,
    Banana,
    Manga,
    Maca,
    Melancia,
    Pinha,
    Uva,
    Poder,
    Obstacle,
    Vazio
}
