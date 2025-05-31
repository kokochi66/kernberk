using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HighlightLayer : MonoBehaviour
{
    public List<HexTile> tiles;
    public Color color;

    public HighlightLayer(List<HexTile> tiles, Color color)
    {
        this.tiles = tiles;
        this.color = color;
    }

}
