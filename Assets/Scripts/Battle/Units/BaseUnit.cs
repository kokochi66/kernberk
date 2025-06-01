// BaseUnit.cs
using UnityEngine;
using System.Collections;

namespace Battle.Units
{
    public abstract class BaseUnit : MonoBehaviour
    {
        public string unitName;
        public UnitStats stats;
        public GameObject TurnInfoPrefab { get; set; }

        public abstract void ReceiveAttack(int dmg);
        public abstract IEnumerator FlashRed();
    }
}
