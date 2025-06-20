using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.Core.Manager
{
    public class TileManager : MonoBehaviour
    {
        public static TileManager Instance;

        private Stack<HighlightLayer> highlightStack = new();
        private List<HexTile> allTiles = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            allTiles = new List<HexTile>(FindObjectsOfType<HexTile>());
        }

        private class HighlightLayer
        {
            public List<HexTile> tiles;
            public Color color;

            public HighlightLayer(List<HexTile> tiles, Color color)
            {
                this.tiles = tiles;
                this.color = color;
            }
        }

        private void ApplyHighlight(List<HexTile> tiles, Color color)
        {
            HexTileState state = color == Color.green
                ? HexTileState.Movable
                : color == Color.red
                    ? HexTileState.EnemyAttackPreview
                    : HexTileState.None;

            foreach (var tile in tiles)
                tile.SetState(state);
        }

        private void ClearHighlights(List<HexTile> tiles)
        {
            Debug.Log($"🧹 [TileManager] ClearHighlights 호출됨 - 타일 개수: {tiles.Count}");

            foreach (var tile in tiles)
                tile.ResetState();
        }


        public void HighlightPlayerMoveRange(PlayerUnit unit)
        {
            if (unit == null || unit.CurrentTile == null)
                return;

            HexTile origin = unit.CurrentTile;
            int moveRange = unit.GetMoveRange();

            List<HexTile> moveableTiles = new();

            foreach (var tile in allTiles)
            {
                if (tile == origin) continue;
                if (!IsWithinRange(origin, tile, moveRange)) continue;
                if (tile.IsOccupied()) continue;

                tile.SetState(HexTileState.Movable);
                moveableTiles.Add(tile);
            }

            PushHighlightLayer(moveableTiles, Color.green);
        }

        private bool IsWithinRange(HexTile a, HexTile b, int range)
        {
            return HexDistance(a, b) <= range;
        }


        private int HexDistance(HexTile a, HexTile b)
        {
            var aCube = OffsetToCube(a.tileX, a.tileY);
            var bCube = OffsetToCube(b.tileX, b.tileY);

            return Mathf.Max(
                Mathf.Abs(aCube.x - bCube.x),
                Mathf.Abs(aCube.y - bCube.y),
                Mathf.Abs(aCube.z - bCube.z)
            );
        }

        private (int x, int y, int z) OffsetToCube(int col, int row)
        {
            int x = col;
            int z = row - (col % 2 == 0 ? col / 2 : (col + 1) / 2);
            int y = -x - z;
            return (x, y, z);
        }


        private bool IsAdjacent(HexTile from, HexTile to)
        {
            return HexDistance(from, to) == 1;
        }


        /// <summary>
        /// 적 공격범위 하이라이트 (빨간색)
        /// </summary>
        public void HighlightEnemyAttackPreview(List<HexTile> tiles)
        {
            foreach (var tile in tiles)
                tile.SetState(HexTileState.EnemyAttackPreview);

            PushHighlightLayer(tiles, Color.red);
        }


        public void PushHighlightLayer(List<HexTile> tiles, Color color)
        {
            // 기존 레이어 비활성화
            if (highlightStack.Count > 0)
                ClearHighlights(highlightStack.Peek().tiles);

            // 새 레이어 적용
            ApplyHighlight(tiles, color);
            highlightStack.Push(new HighlightLayer(tiles, color));
        }

        public void PopHighlightLayer()
        {
            if (highlightStack.Count == 0) return;

            ClearHighlights(highlightStack.Pop().tiles);

            if (highlightStack.Count > 0)
                ApplyHighlight(highlightStack.Peek().tiles, highlightStack.Peek().color);
        }

        public void ClearAllHighlights()
        {
            while (highlightStack.Count > 0)
                ClearHighlights(highlightStack.Pop().tiles);
        }

        public HexTile GetTileAt(int x, int y)
        {
            return allTiles.FirstOrDefault(t => t.tileX == x && t.tileY == y);
        }

        public List<HexTile> GetAllTiles() => allTiles;
    }
}
