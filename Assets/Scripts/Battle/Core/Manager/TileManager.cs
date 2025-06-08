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



        /// <summary>
        /// 이동 범위 하이라이트 (초록색)
        /// </summary>
        public void HighlightPlayerMoveRange(PlayerUnit unit)
        {
            if (unit == null || unit.CurrentTile == null)
            {
                Debug.LogWarning("[TileManager] 유닛 또는 현재 타일 정보가 없습니다.");
                return;
            }

            HexTile origin = unit.CurrentTile;
            List<HexTile> allTiles = GetAllTiles();
            List<HexTile> moveableTiles = new();

            foreach (var tile in allTiles)
            {
                if (tile == origin) continue;
                if (!IsAdjacent(origin, tile)) continue;
                if (tile.IsOccupied()) continue;

                tile.SetState(HexTileState.Movable);
                moveableTiles.Add(tile);
            }

            PushHighlightLayer(moveableTiles, Color.green);
        }



        private bool IsAdjacent(HexTile from, HexTile to)
        {
            int dx = to.tileX - from.tileX;
            int dy = to.tileY - from.tileY;

            // 짝수열과 홀수열에 따라 인접한 타일의 상대 좌표가 다름
            (int dx, int dy)[] offsets = (from.tileX % 2 == 0)
                ? new (int, int)[] { (-1, 0), (-1, 1), (0, -1), (0, 1), (1, 0), (1, 1) }  // 짝수 열
                : new (int, int)[] { (-1, -1), (-1, 0), (0, -1), (0, 1), (1, -1), (1, 0) }; // 홀수 열

            foreach (var offset in offsets)
            {
                if (dx == offset.dx && dy == offset.dy)
                    return true;
            }

            return false;
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
