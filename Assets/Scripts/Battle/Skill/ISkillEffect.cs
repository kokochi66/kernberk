using System.Collections;

namespace Battle.Skill
{
    public interface ISkillEffect
    {
        IEnumerator Execute(PlayerUnit user, EnemyUnit target, System.Action onComplete);
    }

}
