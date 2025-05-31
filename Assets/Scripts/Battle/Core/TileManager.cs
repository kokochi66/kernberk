using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.Core
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
            foreach (var tile in tiles)
                tile.Highlight(color);
        }

        private void ClearHighlights(List<HexTile> tiles)
        {
            foreach (var tile in tiles)
                tile.ResetHighlight();
        }

        /// <summary>
        /// 이동 범위 하이라이트 (초록색)
        /// </summary>
        public void HighlightPlayerMoveRange(List<HexTile> tiles)
        {
            PushHighlightLayer(tiles, Color.green);
        }

        /// <summary>
        /// 적 공격범위 하이라이트 (빨간색)
        /// </summary>
        public void HighlightEnemyAttackPreview(List<HexTile> tiles)
        {
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
