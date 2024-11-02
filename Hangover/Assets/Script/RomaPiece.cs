using UnityEngine;

public class RomaPiece : Piece
{
    public override void Init(int x, int y, GridManager gridManager)
    {
        base.Init(x, y, gridManager);
        frutType = FrutType.Roma;
    }

    public override void AnimateDestruction()
    {
        base.AnimateDestruction();
        gridManager.ActivateRoma(this);
    }

    void OnMouseDown()
    {
        if (!isInvisible && gridManager != null)
        {
            gridManager.ActivateRoma(this);  // Somente ativa o PowerUp roma ao clicar
        }
    }
}