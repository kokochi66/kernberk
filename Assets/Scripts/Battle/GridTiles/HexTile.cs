using UnityEngine;
using Battle.Units;
using Battle.Core;

public class HexTile : MonoBehaviour
{
    public int tileX;
    public int tileY;
    private bool isOccupied = false;

    public void SetPosition(int x, int y)
    {
        tileX = x;
        tileY = y;
    }

    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    public bool IsOccupied() => isOccupied;

    private void OnMouseDown()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnTileClicked(this);
        }
    }

}
