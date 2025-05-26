using UnityEngine;
using System.Collections;
using Battle.Units;
using System.Linq;
using System.Collections.Generic;
using Battle.Units;
using Battle.UIEvents;

public class EnemyUnit : MonoBehaviour
{
    public UnitStats stats;
    public GameObject uiPrefab; // UIEnemyInfo 프리팹 (Canvas 하위에 붙을 예정)
    public GameObject enemyInfoPanel;
    private UIEnemyInfo uiInstance;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        stats = new UnitStats(20, 8, 2, 25); // 임시 값
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject uiGO = Instantiate(uiPrefab, enemyInfoPanel.transform);
        uiInstance = uiGO.GetComponent<UIEnemyInfo>();
        uiInstance.Bind(stats, this.transform, uiGO); // 체력 바인딩 및 추적할 타겟 설정
    }

    public void ReceiveAttack(int attackValue)
    {
        int damage = Mathf.Max(1, attackValue - stats.Defense);
        stats.CurrentHP -= damage;
        uiInstance?.UpdateHPBar();

        Debug.Log($"[Enemy] {damage} 데미지 받음 (HP: {stats.CurrentHP}/{stats.MaxHP})");

        StartCoroutine(DamageBlink());

        if (stats.CurrentHP <= 0)
        {
            Die();
            
        }
    }

    private IEnumerator DamageBlink()
    {
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Die()
    {
        Debug.Log("[Enemy] 사망 처리");
        gameObject.SetActive(false);
        uiInstance?.gameObject.SetActive(false); // UI도 끔
    }


    public List<HexTile> GetAttackTiles(HexTile CurrentTile, HexTile[] allTiles)
    {
        List<HexTile> result = new List<HexTile>();
        HexTile center = CurrentTile;

        // 예: 전방 1칸 패턴
        foreach (HexTile tile in allTiles)
        {
            if (tile.tileX == center.tileX + 1 && tile.tileY == center.tileY)
                result.Add(tile);
        }

        return result;
    }


}
