// Data/Cards/Effects/DrawEffectSO.cs
// 카드 드로우 효과 SO

using UnityEngine;
using ProjectSS.Core.Cards;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// 카드를 드로우하는 효과
    /// </summary>
    [CreateAssetMenu(fileName = "DrawEffect", menuName = "PSTS/Card Effects/Draw")]
    public class DrawEffectSO : CardEffectSO
    {
        [Header("Draw Settings")]
        [Tooltip("드로우할 카드 수")]
        [SerializeField] private int _drawCount = 1;

        /// <summary>
        /// 드로우 카드 수
        /// </summary>
        public int DrawCount => _drawCount;

        public override void Execute(IEffectContext context)
        {
            context.DrawCards(_drawCount);
        }

        public override string GetValueString()
        {
            return _drawCount.ToString();
        }
    }
}
