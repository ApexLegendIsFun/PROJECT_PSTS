// Data/Enemies/EnemyDataSO.cs
// 적 데이터 ScriptableObject

using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data.Enemies
{
    /// <summary>
    /// 적 데이터 정의
    /// 에디터에서 적의 모든 속성을 설정
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "PSTS/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        #region Identity

        [Header("Identity")]
        [Tooltip("적 고유 ID")]
        [SerializeField] private string _enemyId;

        [Tooltip("표시 이름")]
        [SerializeField] private string _displayName;

        [Tooltip("적 타입 (Slime, Goblin, Boss 등)")]
        [SerializeField] private string _enemyType;

        [Tooltip("적 설명")]
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        #endregion

        #region Base Stats

        [Header("Base Stats")]
        [Tooltip("기본 HP")]
        [SerializeField] private int _baseHP = 30;

        [Tooltip("기본 속도")]
        [SerializeField] private int _baseSpeed = 8;

        [Tooltip("기본 공격 데미지")]
        [SerializeField] private int _baseAttackDamage = 8;

        [Tooltip("기본 방어 수치")]
        [SerializeField] private int _baseBlockAmount = 5;

        #endregion

        #region Scaling

        [Header("Difficulty Scaling")]
        [Tooltip("난이도당 HP 증가율 (0.1 = 10%)")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _hpScalePerDifficulty = 0.1f;

        [Tooltip("난이도당 데미지 증가율 (0.1 = 10%)")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _damageScalePerDifficulty = 0.1f;

        #endregion

        #region Visual

        [Header("Visual")]
        [Tooltip("적 초상화")]
        [SerializeField] private Sprite _portrait;

        [Tooltip("테마 색상")]
        [SerializeField] private Color _themeColor = Color.white;

        #endregion

        #region AI Behavior

        [Header("AI Behavior")]
        [Tooltip("AI 행동 설정 (선택적, 없으면 아래 확률 사용)")]
        [SerializeField] private EnemyAIConfigSO _aiConfig;

        [Tooltip("공격 확률 (0-1) - AI Config가 없을 때 사용")]
        [Range(0f, 1f)]
        [SerializeField] private float _attackChance = 0.7f;

        [Tooltip("방어 확률 (0-1) - AI Config가 없을 때 사용")]
        [Range(0f, 1f)]
        [SerializeField] private float _defendChance = 0.3f;

        #endregion

        #region Properties

        /// <summary>
        /// 적 고유 ID
        /// </summary>
        public string EnemyId => _enemyId;

        /// <summary>
        /// 표시 이름
        /// </summary>
        public string DisplayName => _displayName;

        /// <summary>
        /// 적 타입
        /// </summary>
        public string EnemyType => _enemyType;

        /// <summary>
        /// 적 설명
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// 기본 HP
        /// </summary>
        public int BaseHP => _baseHP;

        /// <summary>
        /// 기본 속도
        /// </summary>
        public int BaseSpeed => _baseSpeed;

        /// <summary>
        /// 기본 공격 데미지
        /// </summary>
        public int BaseAttackDamage => _baseAttackDamage;

        /// <summary>
        /// 기본 방어 수치
        /// </summary>
        public int BaseBlockAmount => _baseBlockAmount;

        /// <summary>
        /// 적 초상화
        /// </summary>
        public Sprite Portrait => _portrait;

        /// <summary>
        /// 테마 색상
        /// </summary>
        public Color ThemeColor => _themeColor;

        /// <summary>
        /// 공격 확률
        /// </summary>
        public float AttackChance => _attackChance;

        /// <summary>
        /// 방어 확률
        /// </summary>
        public float DefendChance => _defendChance;

        /// <summary>
        /// AI 행동 설정 (null일 수 있음)
        /// </summary>
        public EnemyAIConfigSO AIConfig => _aiConfig;

        /// <summary>
        /// AI 설정 유무
        /// </summary>
        public bool HasAIConfig => _aiConfig != null;

        #endregion

        #region Methods

        /// <summary>
        /// 난이도에 따라 스케일된 HP 반환
        /// </summary>
        /// <param name="difficulty">난이도 레벨 (1부터 시작)</param>
        /// <returns>스케일된 HP</returns>
        public int GetScaledHP(int difficulty)
        {
            if (difficulty <= 1) return _baseHP;

            float scale = 1f + (_hpScalePerDifficulty * (difficulty - 1));
            return Mathf.RoundToInt(_baseHP * scale);
        }

        /// <summary>
        /// 난이도에 따라 스케일된 데미지 반환
        /// </summary>
        /// <param name="difficulty">난이도 레벨 (1부터 시작)</param>
        /// <returns>스케일된 데미지</returns>
        public int GetScaledDamage(int difficulty)
        {
            if (difficulty <= 1) return _baseAttackDamage;

            float scale = 1f + (_damageScalePerDifficulty * (difficulty - 1));
            return Mathf.RoundToInt(_baseAttackDamage * scale);
        }

        /// <summary>
        /// AI 행동 결정 (공격 vs 방어)
        /// </summary>
        /// <returns>true면 공격, false면 방어</returns>
        public bool ShouldAttack()
        {
            float roll = Random.value;
            return roll < _attackChance;
        }

        #endregion

        #region Editor Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ID가 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_enemyId))
            {
                _enemyId = name.ToLower().Replace(" ", "_");
            }

            // 이름이 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_displayName))
            {
                _displayName = name;
            }

            // 타입이 비어있으면 기본값 설정
            if (string.IsNullOrEmpty(_enemyType))
            {
                _enemyType = "Normal";
            }

            // 확률 합이 1을 넘지 않도록 조정
            if (_attackChance + _defendChance > 1f)
            {
                float total = _attackChance + _defendChance;
                _attackChance /= total;
                _defendChance /= total;
            }
        }
#endif

        #endregion
    }
}
