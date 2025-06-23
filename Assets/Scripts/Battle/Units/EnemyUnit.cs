using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Battle.Units;
using Battle.UIEvents;
using Battle.Data;
using Battle.Core;
using Battle.Core.Service;
using Battle.Core.Manager;

public class EnemyUnit : BaseUnit
{
    public GameObject SelectionOutline { get; set; }
    public UIEnemyInfo EnemyInfoUI { get; set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public string AttackPattern { get; private set; }
    public bool isClicked = false;
    public List<EnemyAttackPattern> AttackPatterns { get; private set; }
    private int turnCounter = 0;

    public void Init(EnemyUnitData data)
    {
        this.unitName = data.unitName;
        this.stats = data.stats.Clone();
        this.AttackPattern = data.attackPattern;
        this.TurnInfoPrefab = data.turnIconPrefab;
        this.AttackPatterns = data.attackPatterns;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        SelectionOutline = transform.Find("EnemyOutlineObject")?.gameObject;
        if (SelectionOutline != null)
            SelectionOutline.SetActive(false);
    }

    public void ShowSelected(bool show)
    {
        if (SelectionOutline != null)
            SelectionOutline.SetActive(show);
    }

    public override void ReceiveAttack(int attackValue)
    {
        int damage = Mathf.Max(1, attackValue - stats.Defense);
        stats.CurrentHP -= damage;

        EnemyInfoUI?.UpdateHPBar();

        Debug.Log($"[Enemy] {damage} 데미지 받음 (HP: {stats.CurrentHP}/{stats.MaxHP})");

        StartCoroutine(FlashRed());

        if (stats.CurrentHP <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"[Enemy] {unitName} 사망 처리");
        gameObject.SetActive(false);
        EnemyInfoUI?.gameObject.SetActive(false);
    }

    public override IEnumerator FlashRed()
    {
        if (SpriteRenderer == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            SpriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            SpriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnMouseDown()
    {
        if (!TurnService.Instance.IsPlayerTurnActive) return;

        UnitManager.Instance.SelectEnemy(this);
    }


    public void OnTurnStart()
    {
        turnCounter++;
    }

    public EnemyAttackPattern GetCurrentPattern(EnemyPatternContext context)
    {
        List<EnemyAttackPattern> validPatterns = new();

        foreach (var pattern in AttackPatterns)
        {
            bool isActive = pattern.ShouldActivate(this, context);
            Debug.Log($"[EnemyUnit] ▶ 패턴 검사: {pattern.patternName} / 조건 결과: {isActive}");

            if (isActive)
                validPatterns.Add(pattern);
        }

        if (validPatterns.Count > 0)
        {
            var selected = validPatterns[Random.Range(0, validPatterns.Count)];
            Debug.Log($"[EnemyUnit] 🎯 랜덤 선택된 패턴: {selected.patternName}");
            return selected;
        }

        Debug.Log("[EnemyUnit] ❌ 사용 가능한 패턴 없음");
        return null;
    }

}
