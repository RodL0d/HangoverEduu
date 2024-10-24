using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    public GameObject[] piecePrefabs; // Array de prefabs das pe�as
    public int width; // Largura da grade
    public int height; // Altura da grade
    public Piece[,] pieces; // Array 2D para armazenar as pe�as
    public binaryArray binaryArrayInstance; // Inst�ncia do seu script binaryArray

    private Piece selectedPiece; // A pe�a atualmente selecionada
    private Vector2Int selectedPieceIndex; // Posi��o da pe�a selecionada

    void Start()
    {
        // Inicializa a grade
        pieces = new Piece[width, height];
        InitializeBoard();
    }


    void InitializeBoard()
    {
        pieces = new Piece[width, height]; // Cria a matriz para as pe�as

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Escolhe um tipo de fruta aleat�rio
                FrutType randomType = (FrutType)Random.Range(0, piecePrefabs.Length);

                // Cria uma nova pe�a a partir do prefab correspondente ao tipo de fruta
                GameObject pieceObject = Instantiate(GetPrefabByType(randomType), new Vector3(x, y, 0), Quaternion.identity);
                Piece piece = pieceObject.GetComponent<Piece>();
                piece.Init(x, y, this); // Inicializa a pe�a com sua posi��o e refer�ncia ao GridManager

                pieces[x, y] = piece; // Armazena a pe�a na matriz
            }
        }
    }

    // M�todo para obter o prefab correspondente ao tipo de fruta
    GameObject GetPrefabByType(FrutType type)
    {
        // Verifica se o tipo est� dentro dos limites do array de prefabs
        int index = (int)type; // O enum � usado como �ndice
        if (index >= 0 && index < piecePrefabs.Length)
        {
            return piecePrefabs[index]; // Retorna o prefab correspondente
        }
        return null; // Retorna nulo se o tipo n�o corresponder a nenhum prefab
    }



    public void SelectPiece(Piece piece)
    {
        if (selectedPiece == null)
        {
            selectedPiece = piece;
            selectedPieceIndex = new Vector2Int(piece.x, piece.y);
        }
        else
        {
            // Tentar trocar as pe�as
            TrySwapPieces(selectedPiece, piece);
            DeselectPiece();
        }
    }

    void DeselectPiece()
    {
        selectedPiece = null;
    }

    void TrySwapPieces(Piece a, Piece b)
    {
        if (IsAdjacent(a, b))
        {
            // Troca as posi��es
            SwapPieces(a, b);
            // Verifica se h� match ap�s a troca
            CheckMatches();
        }
        else
        {
            // Se n�o forem adjacentes, talvez queira fazer algo, como desmarc�-las
            DeselectPiece();
        }
    }

    bool IsAdjacent(Piece a, Piece b)
    {
        // Verifica se as pe�as est�o adjacentes
        return (Mathf.Abs(a.x - b.x) == 1 && a.y == b.y) || (Mathf.Abs(a.y - b.y) == 1 && a.x == b.x);
    }

    void SwapPieces(Piece a, Piece b)
    {
        // Troca as posi��es das pe�as
        int tempX = a.x;
        int tempY = a.y;

        a.x = b.x;
        a.y = b.y;

        b.x = tempX;
        b.y = tempY;

        // Atualiza o array de pe�as
        pieces[a.x, a.y] = a;
        pieces[b.x, b.y] = b;
    }

    void CheckMatches()
    {
        // Aqui voc� pode implementar a l�gica para verificar se houve matches
        // e como lidar com eles (remover, animar, etc.)
    }
}
