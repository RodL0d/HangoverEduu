using UnityEngine;

public class CerejaPiece : Piece
{
    public override void Init(int x, int y, GridManager gridManager)
    {
        base.Init(x, y, gridManager);
        frutType = FrutType.Cereja;
    }

    public override void AnimateDestruction()
    {
        base.AnimateDestruction();
        gridManager.ActivateCereja(this);
    }

    void OnMouseDown()
    {
        if (!isInvisible && gridManager != null)
        {
            gridManager.ActivateCereja(this);  // Somente ativa PowerUp cereja ao clicar
        }
    }
}