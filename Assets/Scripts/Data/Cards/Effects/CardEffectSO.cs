// Data/Cards/Effects/CardEffectSO.cs
// 카드 효과 ScriptableObject 추상 클래스

using UnityEngine;
using ProjectSS.Core.Cards;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// 카드 효과의 기본 클래스
    /// 모든 카드 효과는 이 클래스를 상속받아 구현
    /// </summary>
    public abstract class CardEffectSO : ScriptableObject
    {
        [Header("Effect Info")]
        [Tooltip("효과 설명 (에디터용)")]
        [TextArea(2, 4)]
        [SerializeField] protected string _description;

        /// <summary>
        /// 효과 설명
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// 효과 실행
        /// </summary>
        /// <param name="context">효과 실행 컨텍스트</param>
        public abstract void Execute(IEffectContext context);

        /// <summary>
        /// 효과 값 문자열 반환 (카드 설명 생성용)
        /// </summary>
        public abstract string GetValueString();
    }
}
