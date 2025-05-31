using UnityEngine;
using Battle.Units;
using System.Collections;

public class PlayerUnit : BaseUnit
{

    private void Awake()
    {
        stats = new UnitStats(maxHp: 30, atk: 3, def: 3, agi: 28);
    }


    public override void ReceiveAttack(int enemyDamage)
    {
        stats.TakeDamage(enemyDamage);

        if (stats.IsDead)
        {
            Debug.Log($"{unitName}이(가) 사망했습니다.");
            gameObject.SetActive(false);
        }
    }

    public override void SetCurrentTile(HexTile tile)
    {
        if (CurrentTile != null)
            CurrentTile.SetOccupied(false);

        CurrentTile = tile;
        CurrentTile.SetOccupied(true);
        float yOffset = 50f; // 발판 위로 띄우는 높이
        transform.position = CurrentTile.transform.position + new Vector3(0, yOffset, 0);
    }

    public void MoveTo(HexTile targetTile, System.Action onComplete)
    {
        if (CurrentTile != null)
            CurrentTile.SetOccupied(false);

        StartCoroutine(MoveRoutine(targetTile, onComplete));
    }

    private IEnumerator MoveRoutine(HexTile targetTile, System.Action onComplete)
    {
        Vector3 start = transform.position;
        float yOffset = 50f; // 발판 위로 띄우는 높이
        Vector3 end = targetTile.transform.position + new Vector3(0, yOffset, 0);


        float t = 0;
        float duration = 0.3f;

        while (t < 1)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // 이동 완료 후 상태 갱신
        SetCurrentTile(targetTile);
        onComplete?.Invoke();
    }

    public override IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        for (int i = 0; i < 3; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }


}
