using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Board : MonoBehaviour
{
    [Header("Tamanho do tabuleiro")]
    public int width;
    public int height;

    [SerializeField] int jogadas;
    public GameObject[] piecePrefab;
    public Piece[,] pieces;
    private Piece selectedPiece;
    private bool canSwap = true;
    public Transform cam;
    [SerializeField] GameObject particle_popMagic;

    private binaryArray binaryArray;

    void Start()
    {
        pieces = new Piece[width, height];
        binaryArray = GetComponent<binaryArray>();
        InitializeBoard();
        CenterCamera();
    }

    void CenterCamera()
    {
        cam.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
    }

    private void Update()
    {
        if (GameManager.instance.jogadas == 0)
        {
            StartCoroutine(GameOver());
        }
    }

    void InitializeBoard()
    {
        bool[] initialBools = binaryArray.GetInitialBools();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;
                if (index < initialBools.Length && initialBools[index])
                {
                    pieces[x, y] = CreateEmptyPiece(x, y);
                }
                else
                {
                    SpawnPiece(x, y);
                }
            }
        }

        CheckForMatches(out _);
    }

    Piece CreateEmptyPiece(int x, int y)
    {
        GameObject emptyObject = new GameObject("EmptyPiece");
        Piece emptyPiece = emptyObject.AddComponent<Piece>();
        emptyPiece.frutType = FrutType.Vazio;
        emptyPiece.Init(x, y, this);
        emptyPiece.SetVisibility(false);
        return emptyPiece;
    }

    void SpawnPiece(int x, int y)
    {
        GameObject newPiece = Instantiate(piecePrefab[RandomFrut()], new Vector3(x, y, 0), Quaternion.identity);
        pieces[x, y] = newPiece.GetComponent<Piece>();
        pieces[x, y]?.Init(x, y, this);
    }

    int RandomFrut()
    {
        return Random.Range(0, piecePrefab.Length);
    }

    public void SelectPiece(Piece piece)
    {
        if (!canSwap || selectedPiece == piece) return;

        if (selectedPiece == null)
        {
            // Seleciona a peça pela primeira vez
            selectedPiece = piece;
            selectedPiece.IncreaseScale(new Vector3(0.8f, 0.8f), 0.4f); // Cresce a peça com overshoot
        }
        else
        {
            // Verifica se a peça selecionada é adjacente à peça atual
            if (IsAdjacent(selectedPiece, piece))
            {
                selectedPiece.IncreaseScale(new Vector3(-0.8f, -0.8f), 0.3f, true); // Volta ao tamanho normal com overshoot reverso
                piece.IncreaseScale(new Vector3(0.8f, 0.8f), 0.4f); // Cresce a nova peça selecionada
                StartCoroutine(TrySwapPieces(selectedPiece, piece));
            }
            else
            {
                // A peça não é adjacente, então apenas atualiza a seleção
                selectedPiece.IncreaseScale(new Vector3(-0.8f, -0.8f), 0.3f, true); // Volta ao tamanho normal
                selectedPiece = piece;
                selectedPiece.IncreaseScale(new Vector3(0.8f, 0.8f), 0.4f); // Cresce a nova peça
            }
        }

    }


    bool IsAdjacent(Piece piece1, Piece piece2)
    {
        return (Mathf.Abs(piece1.x - piece2.x) == 1 && piece1.y == piece2.y) ||
               (Mathf.Abs(piece1.y - piece2.y) == 1 && piece1.x == piece2.x);
    }

    IEnumerator TrySwapPieces(Piece piece1, Piece piece2)
    {
        canSwap = false;

        SwapPieces(piece1, piece2);
        yield return new WaitForSeconds(0.1f);

        if (!HasMatches())
        {
            SwapPieces(piece1, piece2);
            selectedPiece.IncreaseScale(new Vector3(-0.8f, -0.8f), 0.3f, true);
            selectedPiece.IncreaseScale(new Vector3(-0.8f, -0.8f), 0.3f, true);
        }
        else
        {
            CheckForMatches(out _);
        }

        GameManager.instance.UpdateJogadas(-1);
        selectedPiece = null;
        canSwap = true;
    }

    void SwapPieces(Piece piece1, Piece piece2)
    {
        (pieces[piece1.x, piece1.y], pieces[piece2.x, piece2.y]) = (piece2, piece1);
        piece1.Init(piece2.x, piece2.y, this);
        piece2.Init(piece1.x, piece1.y, this);

        Vector3 tempPosition = piece1.transform.position;
        piece1.transform.position = piece2.transform.position;
        piece2.transform.position = tempPosition;
    }

    bool HasMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null) continue;

                if (CheckMatchHorizontal(x, y) || CheckMatchVertical(x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool CheckMatchHorizontal(int x, int y)
    {
        FrutType currentType = pieces[x, y].frutType;
        int count = 1;

        for (int i = x + 1; i < width && pieces[i, y]?.frutType == currentType; i++)
        {
            count++;
        }

        return count >= 3;
    }

    bool CheckMatchVertical(int x, int y)
    {
        FrutType currentType = pieces[x, y].frutType;
        int count = 1;

        for (int i = y + 1; i < height && pieces[x, i]?.frutType == currentType; i++)
        {
            count++;
        }

        return count >= 3;
    }

    List<Piece> CheckForMatches(out int totalDestroyed)
    {
        List<Piece> piecesToDestroy = new List<Piece>();
        totalDestroyed = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null) continue;

                List<Piece> matchPieces = GetMatchPieces(x, y);
                if (matchPieces.Count >= 3)
                {
                    piecesToDestroy.AddRange(matchPieces);
                    totalDestroyed += matchPieces.Count;
                    GameManager.instance.AddScore(10);
                }
            }
        }

        foreach (Piece piece in piecesToDestroy)
        {
            if (piece != null && piece.gameObject != null)
            {
                pieces[piece.x, piece.y] = null;
                if (piece.frutType != FrutType.Vazio)
                {
                    Instantiate(particle_popMagic, new Vector3(piece.x, piece.y), Quaternion.identity);
                }
                // Cancela os Tweens associados ao objeto antes de destruí-lo
                piece.transform.DOKill();
                Destroy(piece.gameObject);
            }
        }


        StartCoroutine(RefillBoard());
        return piecesToDestroy;
    }

    List<Piece> GetMatchPieces(int x, int y)
    {
        List<Piece> matchPieces = new List<Piece>();
        matchPieces.AddRange(CheckMatchLine(x, y, 1, 0)); // Verifica linha horizontal
        matchPieces.AddRange(CheckMatchLine(x, y, 0, 1)); // Verifica linha vertical
        return matchPieces;
    }

    List<Piece> CheckMatchLine(int x, int y, int dx, int dy)
    {
        List<Piece> matched = new List<Piece>();
        FrutType type = pieces[x, y].frutType;
        matched.Add(pieces[x, y]);

        for (int i = 1; i < 3; i++)
        {
            int newX = x + dx * i;
            int newY = y + dy * i;

            if (newX >= width || newY >= height || pieces[newX, newY]?.frutType != type) break;
            matched.Add(pieces[newX, newY]);
        }

        return matched;
    }

    IEnumerator RefillBoard()
    {
        yield return new WaitForSeconds(0.5f);

        bool[] initialBools = binaryArray.GetInitialBools();

        // Mover peças existentes para espaços vazios
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;
                if (pieces[x, y] == null && (index >= initialBools.Length || !initialBools[index]))
                {
                    for (int k = y + 1; k < height; k++)
                    {
                        if (pieces[x, k] != null)
                        {
                            // A posição da peça anterior deve ser atualizada no array
                            MovePiece(pieces[x, k], new Vector3(x, y, 0), 0.3f);
                            pieces[x, k].Init(x, y, this);
                            pieces[x, y] = pieces[x, k];
                            pieces[x, k] = null;
                            break;
                        }
                    }
                }
            }
        }

        // Repor peças novas
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;

                if (pieces[x, y] == null && (index >= initialBools.Length || !initialBools[index]))
                {
                    GameObject newPiece = Instantiate(piecePrefab[RandomFrut()], new Vector3(x, height, 0), Quaternion.identity);
                    pieces[x, y] = newPiece.GetComponent<Piece>();
                    pieces[x, y]?.Init(x, y, this);
                    MovePiece(pieces[x, y], new Vector3(x, y, 0), 0.3f);
                }
            }
        }
        CheckForMatches(out _);
    }

    void MovePiece(Piece piece, Vector3 newPosition, float duration)
    {
        if (piece == null || piece.gameObject == null) return;  // Verifica se a peça não foi destruída
        piece.transform.DOMove(newPosition, duration).SetEase(Ease.InOutQuad);
    }
    

    bool CanMatchBeMade(int x1, int y1, int x2, int y2)
    {
        if (x1 < 0 || x1 >= width || y1 < 0 || y1 >= height ||
            x2 < 0 || x2 >= width || y2 < 0 || y2 >= height) return false;

        if (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) != 1) return false;

        SwapPieces(pieces[x1, y1], pieces[x2, y2]);
        bool matchFound = HasMatches();
        SwapPieces(pieces[x1, y1], pieces[x2, y2]);

        return matchFound;
    }

    public bool HasPossibleMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null) continue;

                if (CanMatchBeMade(x, y, x + 1, y) || CanMatchBeMade(x, y, x, y + 1))
                {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1f);
        GameManager.instance.UpdateGameOver("Game Over");
    }
}
