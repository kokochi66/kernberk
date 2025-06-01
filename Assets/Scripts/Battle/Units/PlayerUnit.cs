using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Battle.UIEvents;
using Battle.Units;
using Battle.Data;

public class PlayerUnit : BaseUnit
{
    // 유닛 고유 스킬 데이터
    public List<UnitSkillData> skills { get; private set; }

    // 외부에서 연결되는 인스턴스 오브젝트들
    public UIPlayerInfo PlayerInfoUI { get; set; }
    public GameObject FieldSpriteInstance { get; set; }
    public HexTile CurrentTile { get; protected set; }
    public int SkillPoint { get; private set; } = 0;
    public const int MaxSkillPoint = 100;


    /// <summary>
    /// 외부에서 제공받은 데이터로 유닛 초기화
    /// </summary>
    public void Init(PlayerUnitData data)
    {
        this.unitName = data.unitName;
        this.stats = data.stats.Clone(); // ✅ 복사본 사용!
        this.TurnInfoPrefab = data.turnIconPrefab;
        this.skills = data.skillDataList;
    }

    /// <summary>
    /// 피해 처리
    /// </summary>
    public override void ReceiveAttack(int enemyDamage)
    {
        stats.TakeDamage(enemyDamage);

        PlayerInfoUI?.UpdateHPBar(); // 체력 UI 갱신

        StartCoroutine(FlashRed()); // ✅ 피해 시 깜빡임 연출

        if (stats.IsDead)
        {
            Debug.Log($"{unitName}이(가) 사망했습니다.");
            gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 유닛을 타일에 배치하고 위치 이동
    /// </summary>
    public void SetCurrentTile(HexTile tile)
    {
        if (CurrentTile != null)
            CurrentTile.SetOccupied(false);

        CurrentTile = tile;
        CurrentTile.SetOccupied(true);

        float yOffset = 50f; // 발판 위 위치
        transform.position = CurrentTile.transform.position + new Vector3(0, yOffset, 0);
    }

    /// <summary>
    /// 지정된 타일로 이동 애니메이션
    /// </summary>
    public void MoveTo(HexTile targetTile, System.Action onComplete)
    {
        if (CurrentTile != null)
            CurrentTile.SetOccupied(false);

        StartCoroutine(MoveRoutine(targetTile, onComplete));
    }

    private IEnumerator MoveRoutine(HexTile targetTile, System.Action onComplete)
    {
        Vector3 start = transform.position;
        float yOffset = 50f;
        Vector3 end = targetTile.transform.position + new Vector3(0, yOffset, 0);

        float t = 0;
        float duration = 0.3f;

        while (t < 1)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        SetCurrentTile(targetTile);
        onComplete?.Invoke();
    }

    /// <summary>
    /// 데미지를 받았을 때 깜빡이는 연출
    /// </summary>
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

    /// <summary>
    /// 스킬 포인트 증가 (기본공격 등으로 얻음)
    /// </summary>
    public void GainSkillPoint(int amount)
    {
        SkillPoint = Mathf.Min(SkillPoint + amount, MaxSkillPoint);
        Debug.Log($"{unitName} ▶️ SP 획득: {amount} → 현재 SP: {SkillPoint}");

        // 필요하다면 UI 갱신도 여기서 처리
        PlayerInfoUI?.UpdateSkillPoint(SkillPoint);
    }

    /// <summary>
    /// 스킬 사용 시 SP 소모
    /// </summary>
    public bool UseSkillPoint(int cost)
    {
        if (SkillPoint < cost)
        {
            Debug.LogWarning($"{unitName} ❌ SP 부족 (필요: {cost}, 현재: {SkillPoint})");
            return false;
        }

        SkillPoint -= cost;
        Debug.Log($"{unitName} 🌀 SP 소모: {cost} → 남은 SP: {SkillPoint}");

        PlayerInfoUI?.UpdateSkillPoint(SkillPoint);
        return true;
    }


}
