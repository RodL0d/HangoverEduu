using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Importante para usar Distinct()
using DG.Tweening;

public class GridManager : MonoBehaviour
{
    public int width, height;
    public float moveDuration = 0.5f; // Duração da animação de movimento
    public GameObject[] fruitPrefabs; // Array de prefabs de frutas
    public GameObject cerejaPrefab;   // Prefab da cereja
    public GameObject romaPrefab;     // Prefab do roma
    public GameObject amoraPrefab;    // Prefab da amora
    public Piece[,] grid; // Matriz 2D para armazenar as peças
    private Piece selectedPiece;
    private bool isMatching = false;

    // Variáveis de controle de jogadas e pontuação
    public int jogadas = 20; // Número inicial de jogadas
    private int score = 0;
    
    void Start()
    {
        grid = new Piece[width, height];
        InitializeGrid();
        StartCoroutine(VerifyAllMatchesCoroutine());
    }
    
    
    
    void InitializeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Piece piece = CreateNewPiece(x, y);

                while (CheckInitialMatches(piece))
                {
                    Destroy(piece.gameObject);
                    piece = CreateNewPiece(x, y);
                }

                grid[x, y] = piece;
            }
        }
    }

    Piece CreateNewPiece(int x, int y)
    {
        GameObject piecePrefab = GetRandomPiecePrefab();
        GameObject pieceObject = Instantiate(piecePrefab, new Vector3(x, y, 0), Quaternion.identity);
        Piece piece = pieceObject.GetComponent<Piece>();

        int powerUpChance = Random.Range(0, 100);
        if (powerUpChance < 5)
        {
            Destroy(pieceObject);
            pieceObject = Instantiate(cerejaPrefab, new Vector3(x, y, 0), Quaternion.identity);
            piece = pieceObject.AddComponent<CerejaPiece>();
        }
        else if (powerUpChance < 10)
        {
            Destroy(pieceObject);
            pieceObject = Instantiate(romaPrefab, new Vector3(x, y, 0), Quaternion.identity);
            piece = pieceObject.AddComponent<RomaPiece>();
        }
        else if (powerUpChance < 15)
        {
            Destroy(pieceObject);
            pieceObject = Instantiate(amoraPrefab, new Vector3(x, y, 0), Quaternion.identity);
            piece = pieceObject.AddComponent<AmoraPiece>();
        }
        else
        {
            piece.frutType = GetRandomFrutType();
        }

        piece.Init(x, y, this);
        return piece;
    }
    
    FrutType GetRandomFrutType()
    {
        FrutType[] frutTypes = { FrutType.Abacaxi, FrutType.Banana, FrutType.Manga, FrutType.Maca, FrutType.Melancia, FrutType.Pinha, FrutType.Uva };
        return frutTypes[Random.Range(0, frutTypes.Length)];
    }

    bool CheckInitialMatches(Piece piece)
    {
        List<Piece> matchedPieces = GetAllMatchesForPiece(piece, true);
        return matchedPieces.Count > 2;
    }
    
    private IEnumerator VerifyAllMatchesCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);  // Aguarda um segundo entre as verificações para evitar loop infinito

            List<Piece> matches = new List<Piece>();

            // Verificar todas as combinações horizontais e verticais
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Piece currentPiece = grid[x, y];
                    if (currentPiece != null)
                    {
                        List<Piece> horizontalMatch = GetMatch(currentPiece, Vector2.right);
                        List<Piece> verticalMatch = GetMatch(currentPiece, Vector2.up);

                        if (horizontalMatch.Count >= 3)
                        {
                            matches.AddRange(horizontalMatch);
                        }
                        if (verticalMatch.Count >= 3)
                        {
                            matches.AddRange(verticalMatch);
                        }
                    }
                }
            }

            // Destruir as peças que fazem parte de combinações
            foreach (Piece match in matches.Distinct())
            {
                match.MarkForDestruction();
            }

            // Espera a animação de destruição completar
            yield return new WaitForSeconds(0.5f);

            // Preencher os espaços vazios com novas peças
            FillEmptySpaces();
        }
    }
    
    private List<Piece> GetMatch(Piece startPiece, Vector2 direction)
    {
        List<Piece> match = new List<Piece> { startPiece };
        FrutType frutType = startPiece.frutType;

        int nextX = startPiece.x + (int)direction.x;
        int nextY = startPiece.y + (int)direction.y;

        while (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
        {
            Piece nextPiece = grid[nextX, nextY];
            if (nextPiece != null && nextPiece.frutType == frutType)
            {
                match.Add(nextPiece);
                nextX += (int)direction.x;
                nextY += (int)direction.y;
            }
            else
            {
                break;
            }
        }

        return match;
    }
    
    
    GameObject GetRandomPiecePrefab()
    {
        return fruitPrefabs[Random.Range(0, fruitPrefabs.Length)];
    }

    public void SelectPiece(Piece piece)
    {
        if (isMatching) return;

        if (selectedPiece == null)
        {
            selectedPiece = piece;
        }
        else if (IsAdjacent(selectedPiece, piece))
        {
            StartCoroutine(TrySwapPieces(selectedPiece, piece));
            selectedPiece = null;
            DecrementJogadas(); // Decrementa uma jogada a cada troca
        }
        else
        {
            selectedPiece = piece;
        }
    }

    bool IsAdjacent(Piece piece1, Piece piece2)
    {
        return (Mathf.Abs(piece1.x - piece2.x) == 1 && piece1.y == piece2.y) ||
               (Mathf.Abs(piece1.y - piece2.y) == 1 && piece1.x == piece2.x);
    }

    IEnumerator TrySwapPieces(Piece piece1, Piece piece2)
    {
        SwapPieces(piece1, piece2);
        yield return new WaitForSeconds(moveDuration);

        if (CheckMatches(piece1) || CheckMatches(piece2))
        {
            yield return StartCoroutine(ClearAndFillBoard());
        }
        else
        {
            SwapPieces(piece1, piece2);
            yield return new WaitForSeconds(moveDuration);
        }
    }

    void SwapPieces(Piece piece1, Piece piece2)
    {
        int tempX = piece1.x;
        int tempY = piece1.y;

        grid[piece1.x, piece1.y] = piece2;
        grid[piece2.x, piece2.y] = piece1;

        piece1.x = piece2.x;
        piece1.y = piece2.y;
        piece2.x = tempX;
        piece2.y = tempY;

        piece1.transform.DOMove(new Vector3(piece1.x, piece1.y, 0), moveDuration).SetEase(Ease.OutQuad);
        piece2.transform.DOMove(new Vector3(piece2.x, piece2.y, 0), moveDuration).SetEase(Ease.OutQuad);
    }
    
    bool CheckMatches(Piece piece)
    {
        if (piece == null) return false;

        List<Piece> matchedPieces = GetAllMatchesForPiece(piece);
        if (matchedPieces.Count > 2) // Considera match quando há 3 ou mais peças
        {
            foreach (var matchedPiece in matchedPieces)
            {
                matchedPiece.MarkForDestruction();
            }
            return true;
        }

        return false;
    }
    void DestroyMatches()
    {
        foreach (var piece in grid)
        {
            if (piece != null && piece.isMarkedForDestruction)
            {
                piece.AnimateDestruction();
            }
        }
    }

    List<Piece> GetLineMatch(int startX, int startY, int dirX, int dirY)
    {
        List<Piece> matchPieces = new List<Piece>();
        FrutType startType = grid[startX, startY].frutType;

        for (int i = 1; i < 3; i++)
        {
            int newX = startX + dirX * i;
            int newY = startY + dirY * i;

            if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                break;

            if (grid[newX, newY] != null && grid[newX, newY].frutType == startType)
            {
                matchPieces.Add(grid[newX, newY]);
            }
            else
            {
                break;
            }
        }

        return matchPieces;
    }
    
    List<Piece> GetAllMatchesForPiece(Piece piece, bool initial = false)
    {
        List<Piece> horizontalMatches = GetMatches(piece, 1, 0).ToList();
        horizontalMatches.AddRange(GetMatches(piece, -1, 0).Where(p => p != piece));

        List<Piece> verticalMatches = GetMatches(piece, 0, 1).ToList();
        verticalMatches.AddRange(GetMatches(piece, 0, -1).Where(p => p != piece));

        List<Piece> allMatches = new List<Piece>();

        if (horizontalMatches.Count >= 3)
        {
            allMatches.AddRange(horizontalMatches);
        }

        if (verticalMatches.Count >= 3)
        {
            allMatches.AddRange(verticalMatches);
        }

        return allMatches.Distinct().ToList();
    }
    
    List<Piece> GetMatches(Piece piece, int dx, int dy)
    {
        List<Piece> matchingPieces = new List<Piece> { piece };

        int newX = piece.x;
        int newY = piece.y;

        while (true)
        {
            newX += dx;
            newY += dy;

            if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                break;

            Piece nextPiece = grid[newX, newY];
            if (nextPiece != null && nextPiece.frutType == piece.frutType)
            {
                matchingPieces.Add(nextPiece);
            }
            else
            {
                break;
            }
        }

        return matchingPieces;
    }
    
    HashSet<Piece> CheckLShapeMatch(Piece piece)
    {
        HashSet<Piece> lShapeMatches = new HashSet<Piece>();

        if (piece == null) return lShapeMatches;

        if (IsLShapeMatch(piece.x, piece.y, 1, 0, 0, 1) || IsLShapeMatch(piece.x, piece.y, -1, 0, 0, 1) ||
            IsLShapeMatch(piece.x, piece.y, 1, 0, 0, -1) || IsLShapeMatch(piece.x, piece.y, -1, 0, 0, -1))
        {
            lShapeMatches.Add(piece);
        }

        return lShapeMatches;
    }

    bool IsLShapeMatch(int startX, int startY, int offsetX1, int offsetY1, int offsetX2, int offsetY2)
    {
        if (grid[startX, startY] == null)
            return false;

        FrutType startType = grid[startX, startY].frutType;

        for (int i = 1; i <= 2; i++)
        {
            int newX1 = startX + offsetX1 * i;
            int newY1 = startY + offsetY1 * i;

            int newX2 = startX + offsetX2 * i;
            int newY2 = startY + offsetY2 * i;

            if (newX1 < 0 || newY1 < 0 || newX1 >= width || newY1 >= height) return false;
            if (newX2 < 0 || newY2 < 0 || newX2 >= width || newY2 >= height) return false;

            if (grid[newX1, newY1] == null || grid[newX1, newY1].frutType != startType) return false;
            if (grid[newX2, newY2] == null || grid[newX2, newY2].frutType != startType) return false;
        }

        return true;
    }


    void HandleMatches(HashSet<Piece> matchedPieces)
    {
        foreach (var piece in matchedPieces)
        {
            if (piece != null)
            {
                piece.MarkForDestruction();
                AddScore(10);
            }
        }
    }

    void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    void DecrementJogadas()
    {
        jogadas--;
        UpdateUI();

        if (jogadas <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        FindObjectOfType<UIManager>().ShowGameOver("Game Over");
    }

    void UpdateUI()
    {
        FindObjectOfType<UIManager>().UpdateJogadas(jogadas);
        FindObjectOfType<UIManager>().UpdateScore(score);
    }

    IEnumerator ClearAndFillBoard()
    {
        isMatching = true;

        yield return StartCoroutine(ClearMatches());
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(FillEmptySpaces());
        yield return StartCoroutine(CheckAndClearMatchesAtStart());

        isMatching = false;
    }

    IEnumerator ClearMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] != null && grid[x, y].isMarkedForDestruction)
                {
                    grid[x, y].AnimateDestruction();
                    grid[x, y] = null;
                }
            }
        }
        yield return new WaitForSeconds(moveDuration);
    }

    IEnumerator FillEmptySpaces()
    {
        // Movimenta as peças existentes para baixo
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    for (int ny = y + 1; ny < height; ny++)
                    {
                        if (grid[x, ny] != null)
                        {
                            grid[x, ny].transform.DOMove(new Vector3(x, y, 0), moveDuration).SetEase(Ease.OutBounce);
                            grid[x, y] = grid[x, ny];
                            grid[x, ny] = null;
                            grid[x, y].x = x;
                            grid[x, y].y = y;
                            break;
                        }
                    }
                }
            }
        }

        // Preenche os espaços vazios com novas peças
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                {
                    Piece newPiece = CreateNewPiece(x, height + 1); // Cria a peça acima do grid
                    while (CheckInitialMatches(newPiece))
                    {
                        Destroy(newPiece.gameObject);
                        newPiece = CreateNewPiece(x, height + 1);
                    }
                    grid[x, y] = newPiece;
                    newPiece.x = x;
                    newPiece.y = y;
                    newPiece.transform.position = new Vector3(x, height + 1, 0); // Define a posição inicial acima da grade
                    newPiece.transform.DOMove(new Vector3(x, y, 0), moveDuration).SetEase(Ease.OutBounce); // Anima a descida da peça
                }
            }
        }

        yield return new WaitForSeconds(moveDuration);
    }
    
    IEnumerator CheckAndClearMatchesAtStart()
    {
        bool hasMatches;

        do
        {
            hasMatches = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (grid[x, y] != null && CheckMatches(grid[x, y]))
                    {
                        hasMatches = true;
                    }
                }
            }

            if (hasMatches)
            {
                yield return StartCoroutine(ClearAndFillBoard());
            }

        } while (hasMatches);
    }
    
    // Função para ativar o PowerUp cereja
    public void ActivateCereja(Piece cereja)
    {
        if (cereja == null || cereja.isMarkedForDestruction || isMatching) 
        {
            return;  // Evita loop e deixa que o processo atual termine
        }

        Debug.Log("Ativando PowerUp Cereja");

        isMatching = true;  // Marque como verdadeiro para evitar reaçao enquanto ativa

        List<Piece> neighborhood = GetNeighbors(cereja.x, cereja.y);
        foreach (var neighbor in neighborhood)
        {
            if (neighbor != null && !neighbor.isMarkedForDestruction)
            {
                neighbor.MarkForDestruction();
                neighbor.AnimateDestruction();
            }
        }

        // Após a conclusão das operações
        StartCoroutine(ResetMatching());
    }

    private IEnumerator ResetMatching()
    {
        yield return new WaitForSeconds(0.5f);  // Espera até todas as destruições terminarem
        isMatching = false;  // Reativa novos matches
    }
    
    // Função para obter peças vizinhas
    private List<Piece> GetNeighbors(int x, int y)
    {
        List<Piece> neighbors = new List<Piece>();

        if (x > 0) neighbors.Add(grid[x - 1, y]);
        if (x < width - 1) neighbors.Add(grid[x + 1, y]);
        if (y > 0) neighbors.Add(grid[x, y - 1]);
        if (y < height - 1) neighbors.Add(grid[x, y + 1]);

        return neighbors;
    }

// Função para ativar o PowerUp roma
    public void ActivateRoma(Piece roma)
    {
        if (roma == null || roma.isMarkedForDestruction || isMatching)
        {
            return;
        }

        Debug.Log("Ativando PowerUp Roma");

        isMatching = true;

        List<Piece> neighborhood = GetNeighbors(roma.x, roma.y);
        foreach (var neighbor in neighborhood)
        {
            if (neighbor != null && !neighbor.isMarkedForDestruction)
            {
                neighbor.MarkForDestruction();
                neighbor.AnimateDestruction();
            }
        }

        StartCoroutine(ResetMatching());
    }

// Função para ativar o PowerUp amora
    public void ActivateAmora(Piece amora)
    {
        if (amora == null || amora.isMarkedForDestruction || isMatching)
        {
            return;
        }

        Debug.Log("Ativando PowerUp Amora");

        isMatching = true;

        List<Piece> neighborhood = GetNeighbors(amora.x, amora.y);
        foreach (var neighbor in neighborhood)
        {
            if (neighbor != null && !neighbor.isMarkedForDestruction)
            {
                neighbor.MarkForDestruction();
                neighbor.AnimateDestruction();
            }
        }

        StartCoroutine(ResetMatching());
    }
    
    
    List<Piece> GetAdjacentPieces(Piece piece)
    {
        List<Piece> adjacentPieces = new List<Piece>();
        int startX = Mathf.Max(piece.x - 1, 0);
        int endX = Mathf.Min(piece.x + 1, width - 1);
        int startY = Mathf.Max(piece.y - 1, 0);
        int endY = Mathf.Min(piece.y + 1, height - 1);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                if (grid[x, y] != null && grid[x, y] != piece)
                {
                    adjacentPieces.Add(grid[x, y]);
                }
            }
        }

        return adjacentPieces;
    }
    
}