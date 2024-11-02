using UnityEngine;

public class AmoraPiece : Piece
{
    public override void Init(int x, int y, GridManager gridManager)
    {
        base.Init(x, y, gridManager);
        frutType = FrutType.Amora;
    }

    public override void AnimateDestruction()
    {
        base.AnimateDestruction();
        gridManager.ActivateAmora(this);
    }

    void OnMouseDown()
    {
        if (!isInvisible && gridManager != null)
        {
            gridManager.ActivateAmora(this);  // Somente ativa o PowerUp amora ao clicar
        }
    }
}