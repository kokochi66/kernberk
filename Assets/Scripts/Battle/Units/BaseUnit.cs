// BaseUnit.cs
using UnityEngine;
using System.Collections;

namespace Battle.Units
{
    public abstract class BaseUnit : MonoBehaviour
    {
        public string unitName;
        public UnitStats stats;
        public HexTile CurrentTile { get; protected set; }

        public virtual void SetCurrentTile(HexTile tile)
        {
            if (CurrentTile != null)
                CurrentTile.SetOccupied(false);

            CurrentTile = tile;
            CurrentTile.SetOccupied(true);
            float yOffset = 50f;
            transform.position = tile.transform.position + new Vector3(0, yOffset, 0);
        }

        public abstract void ReceiveAttack(int dmg);
        public abstract IEnumerator FlashRed();
    }
}
