using UnityEngine;
using Battle.Units;
using System.Collections;

public class PlayerUnit : MonoBehaviour
{
    public string unitName = "플레이어 유닛";
    public UnitStats stats;

    private void Start()
    {
        // 예시로 초기화
        stats = new UnitStats(maxHp: 30, atk: 10, def: 3, agi: 5);
    }

    public void ReceiveAttack(int enemyDamage)
    {
        stats.TakeDamage(enemyDamage);

        if (stats.IsDead)
        {
            Debug.Log($"{unitName}이(가) 사망했습니다.");
            gameObject.SetActive(false);
        }
    }

    public HexTile CurrentTile
    {
        get;
        private set;
    }

    public void SetCurrentTile(HexTile tile)
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
}
