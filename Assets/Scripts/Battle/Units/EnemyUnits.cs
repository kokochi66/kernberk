using UnityEngine;
using System.Collections;
using Battle.Units;

public class EnemyUnit : MonoBehaviour
{
    public UnitStats stats;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        stats = new UnitStats(20, 8, 2, 3); // 임시 값
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ReceiveAttack(int attackValue)
    {
        int damage = Mathf.Max(1, attackValue - stats.Defense);
        stats.CurrentHP -= damage;

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
    }
}
