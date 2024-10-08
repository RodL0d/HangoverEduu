using UnityEngine;
using DG.Tweening;

public class Piece : MonoBehaviour
{
    public FrutType frutType; // Tipo da fruta da peça
    public int x; // Posição X da peça no tabuleiro
    public int y; // Posição Y da peça no tabuleiro
    public Board board; // Referência ao tabuleiro
    public bool isInvisible; // Determina se a peça é invisível
    public bool IsActive { get; private set; } = true;

    private Renderer pieceRenderer;

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
        if (!isInvisible && frutType != FrutType.Vazio) // Impede a seleção de peças invisíveis ou vazias
        {
            board.SelectPiece(this);
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
        // Usando uma animação com overshoot (OutBack) para dar um efeito de "crescimento elástico"
        transform.DOScale(targetScale, duration)
            .SetEase(Ease.OutBack) // Ajusta o Ease para OutBack, criando um pequeno overshoot
            .SetEase(Ease.OutElastic, 1.5f, 0.5f) // Ou Use OutElastic para efeito mais "elástico"
            .OnComplete(() =>
            {
                // Após a animação, pode adicionar mais efeitos, como uma pequena rotação para adicionar estilo
                if (targetScale == new Vector3(1.2f, 1.2f, 1.2f))
                {
                    // Adiciona uma rotação sutil para a peça ao crescer
                    transform.DORotate(new Vector3(0, 0, 10), 0.1f)
                            .SetLoops(2, LoopType.Yoyo) // Vai e volta na rotação
                            .SetEase(Ease.InOutSine);
                }
            });
    }



    public void IncreaseScale(Vector3 targetScale, float duration, bool returnToOriginal = false)
    {
        // Armazena a escala original
        Vector3 originalScale = transform.localScale;

        // Aumenta a escala com overshoot e elasticidade
        transform.DOScale(originalScale + targetScale, duration)
            .SetEase(Ease.OutBack, 1f) // OutBack cria o efeito de overshoot
            .OnComplete(() =>
            {
                if (returnToOriginal)
                {
                    // Se necessário, retorna à escala original com overshoot reverso
                    transform.DOScale(originalScale, duration)
                        .SetEase(Ease.InBack); // Animação de retorno com efeito de inback
                }
            });
    }

    void OnDestroy()
    {
        // Cancela qualquer Tween ativo associado a este transform
        transform.DOKill();
    }

    public void DestroyPiece()
    {
        IsActive = false;
        // Se necessário, adicione lógica para efeitos visuais ou animações.
        Destroy(gameObject);
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
