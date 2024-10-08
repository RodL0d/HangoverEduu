using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Board : MonoBehaviour
{
    [Header("Tamanho do Tabuleiro")]
    public int width;
    public int height;

    [SerializeField] private int jogadas;
    public GameObject[] piecePrefabs;
    public Piece[,] pieces;
    private Piece selectedPiece;
    private bool canSwap = true;
    public Transform cam;
    [SerializeField] private GameObject particlePopMagic;

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

    void Update()
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
                if (IsEmptyPosition(x, y, initialBools))
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

    bool IsEmptyPosition(int x, int y, bool[] initialBools)
    {
        int index = x + y * width;
        return index < initialBools.Length && initialBools[index];
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
        GameObject newPiece = Instantiate(piecePrefabs[RandomFrut()], new Vector3(x, y, 0), Quaternion.identity);
        pieces[x, y] = newPiece.GetComponent<Piece>();
        pieces[x, y]?.Init(x, y, this);
    }

    int RandomFrut()
    {
        return Random.Range(0, piecePrefabs.Length);
    }

    public void SelectPiece(Piece piece)
    {
        if (!canSwap || selectedPiece == piece) return;

        if (selectedPiece == null)
        {
            SelectFirstPiece(piece);
        }
        else if (IsAdjacent(selectedPiece, piece))
        {
            StartCoroutine(TrySwapPieces(selectedPiece, piece));
        }
        else
        {
            DeselectPreviousPiece();
            SelectNewPiece(piece);
        }
    }

    void SelectFirstPiece(Piece piece)
    {
        selectedPiece = piece;
        selectedPiece.IncreaseScale(new Vector3(0.8f, 0.8f), 0.4f);
    }

    void DeselectPreviousPiece()
    {
        selectedPiece.IncreaseScale(new Vector3(-0.8f, -0.8f), 0.3f, true);
    }

    void SelectNewPiece(Piece piece)
    {
        selectedPiece = piece;
        selectedPiece.IncreaseScale(new Vector3(0.8f, 0.8f), 0.4f);
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
            DeselectPreviousPiece();
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
        if (piece1 == null || piece2 == null) return; // Adicione esta linha

        (pieces[piece1.x, piece1.y], pieces[piece2.x, piece2.y]) = (piece2, piece1);
        piece1.Init(piece2.x, piece2.y, this);
        piece2.Init(piece1.x, piece1.y, this);

        Vector3 tempPosition = piece1.transform.position; // Verifique se piece1 e piece2 não são nulos
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

        DestroyMatchedPieces(piecesToDestroy);
        StartCoroutine(RefillBoard());
        return piecesToDestroy;
    }

    void DestroyMatchedPieces(List<Piece> piecesToDestroy)
    {
        foreach (Piece piece in piecesToDestroy)
        {
            if (piece != null && piece.gameObject != null)
            {
                pieces[piece.x, piece.y] = null;
                if (piece.frutType != FrutType.Vazio)
                {
                    Instantiate(particlePopMagic, new Vector3(piece.x, piece.y), Quaternion.identity);
                }
                piece.transform.DOKill();
                Destroy(piece.gameObject);
            }
        }
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

        MoveExistingPieces(initialBools);
        RefillNewPieces(initialBools);
        CheckForMatches(out _);
    }

    void MoveExistingPieces(bool[] initialBools)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null && !IsEmptyPosition(x, y, initialBools))
                {
                    MovePieceDownward(x, y);
                }
            }
        }
    }

    void MovePieceDownward(int x, int y)
    {
        for (int k = y + 1; k < height; k++)
        {
            if (pieces[x, k] != null)
            {
                pieces[x, k].transform.DOMoveY(y, 0.2f);
                pieces[x, k].Init(x, y, this);
                pieces[x, y] = pieces[x, k];
                pieces[x, k] = null;
                return;
            }
        }
    }

    void RefillNewPieces(bool[] initialBools)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pieces[x, y] == null && !IsEmptyPosition(x, y, initialBools))
                {
                    SpawnPiece(x, y);
                }
            }
        }
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1f);
        GameManager.instance.UpdateGameOver("Game Over");
    }
}
