using UnityEngine;
using Battle.Units;
using Battle.Core.Service;

public class HexTile : MonoBehaviour
{
    public int tileX;
    public int tileY;
    public bool isOccupied = false;

    public HexTileState State { get; private set; } = HexTileState.None;

    public void SetState(HexTileState state)
    {
        this.State = state;

        var renderer = GetComponent<SpriteRenderer>();
        switch (state)
        {
            case HexTileState.None:
                renderer.color = Color.white;
                break;
            case HexTileState.Movable:
                renderer.color = Color.green;
                break;
            case HexTileState.EnemyAttackPreview:
                renderer.color = Color.red;
                break;
        }

        // Debug.Log($"🔁 [HexTile] ({tileX}, {tileY}) 상태 변경 → {state}");
    }

    public void ResetState()
    {
        SetState(HexTileState.None);
    }

    public void SetOccupied(bool value)
    {
        isOccupied = value;
        // Debug.Log($"📌 [HexTile] ({tileX}, {tileY}) 점유 상태 → {(value ? "OCCUPIED" : "FREE")}");
    }

    public bool IsOccupied() => isOccupied;

    private void OnMouseDown()
    {
        Debug.Log($"🖱️ [HexTile] 클릭됨 → ({tileX}, {tileY}), 현재 상태: {State}");

        if (State == HexTileState.Movable)
        {
            // Debug.Log($"✅ [HexTile] 이동 시도됨 → ({tileX}, {tileY})");
            TurnService.Instance.MoveSelectedPlayerTo(this);
        }
        else
        {
            // Debug.Log($"⛔ [HexTile] 이동 불가 상태 ({State}) → 무시됨");
        }
    }
}
