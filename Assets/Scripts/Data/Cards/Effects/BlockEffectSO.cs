// Data/Cards/Effects/BlockEffectSO.cs
// 방어막 효과 SO

using UnityEngine;
using ProjectSS.Core.Cards;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// 방어막을 부여하는 효과
    /// </summary>
    [CreateAssetMenu(fileName = "BlockEffect", menuName = "PSTS/Card Effects/Block")]
    public class BlockEffectSO : CardEffectSO
    {
        [Header("Block Settings")]
        [Tooltip("기본 방어막")]
        [SerializeField] private int _baseBlock = 5;

        [Tooltip("민첩(Dexterity) 스케일링 적용 여부")]
        [SerializeField] private bool _scalesWithDexterity = true;

        /// <summary>
        /// 기본 방어막 값
        /// </summary>
        public int BaseBlock => _baseBlock;

        /// <summary>
        /// 민첩 스케일링 여부
        /// </summary>
        public bool ScalesWithDexterity => _scalesWithDexterity;

        public override void Execute(IEffectContext context)
        {
            // TODO: Dexterity 보너스 계산 (StatusEffect 시스템 연동 후)
            int finalBlock = _baseBlock;

            context.GainBlock(finalBlock);
        }

        public override string GetValueString()
        {
            return _baseBlock.ToString();
        }
    }
}
