// Core/Config/GameBalanceConfigSO.cs
// 게임 밸런스 설정 ScriptableObject
// 모든 하드코딩된 게임 밸런스 값을 중앙 집중 관리

using UnityEngine;

namespace ProjectSS.Core.Config
{
    /// <summary>
    /// 게임 밸런스 설정
    /// 인스펙터에서 수정 가능한 모든 게임 밸런스 상수 포함
    /// </summary>
    [CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "PSTS/Config/Game Balance")]
    public class GameBalanceConfigSO : ScriptableObject
    {
        #region Character Defaults

        [Header("Character Defaults (캐릭터 기본값)")]
        [Tooltip("기본 최대 체력")]
        [SerializeField] private int _defaultMaxHP = 100;

        [Tooltip("기본 에너지")]
        [SerializeField] private int _defaultEnergy = 3;

        [Tooltip("기본 속도")]
        [SerializeField] private int _defaultSpeed = 10;

        public int DefaultMaxHP => _defaultMaxHP;
        public int DefaultEnergy => _defaultEnergy;
        public int DefaultSpeed => _defaultSpeed;

        #endregion

        #region Combat Settings

        [Header("Combat Settings (전투 설정)")]
        [Tooltip("최대 패 크기")]
        [SerializeField] private int _maxHandSize = 10;

        [Tooltip("전투 시작 시 초기 드로우 수")]
        [SerializeField] private int _initialDrawCount = 5;

        [Tooltip("매 턴 드로우 수")]
        [SerializeField] private int _drawPerTurn = 1;

        [Tooltip("턴 간 딜레이 (초)")]
        [SerializeField] private float _turnDelay = 0.5f;

        [Tooltip("적 행동 딜레이 (초)")]
        [SerializeField] private float _enemyActionDelay = 1.0f;

        public int MaxHandSize => _maxHandSize;
        public int InitialDrawCount => _initialDrawCount;
        public int DrawPerTurn => _drawPerTurn;
        public float TurnDelay => _turnDelay;
        public float EnemyActionDelay => _enemyActionDelay;

        #endregion

        #region Enemy Defaults

        [Header("Enemy Defaults (적 기본값)")]
        [Tooltip("기본 공격 데미지")]
        [SerializeField] private int _defaultEnemyAttackDamage = 10;

        [Tooltip("기본 방어도 획득량")]
        [SerializeField] private int _defaultEnemyBlockAmount = 5;

        [Tooltip("기본 적 AI 공격 확률 (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _defaultEnemyAttackChance = 0.7f;

        [Tooltip("기본 적 AI 방어 확률 (0-1)")]
        [Range(0f, 1f)]
        [SerializeField] private float _defaultEnemyDefendChance = 0.3f;

        public int DefaultEnemyAttackDamage => _defaultEnemyAttackDamage;
        public int DefaultEnemyBlockAmount => _defaultEnemyBlockAmount;
        public float DefaultEnemyAttackChance => _defaultEnemyAttackChance;
        public float DefaultEnemyDefendChance => _defaultEnemyDefendChance;

        #endregion

        #region Scaling Settings

        [Header("Scaling Settings (스케일링 설정)")]
        [Tooltip("난이도당 체력 스케일 비율")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _hpScalePerDifficulty = 0.1f;

        [Tooltip("난이도당 데미지 스케일 비율")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _damageScalePerDifficulty = 0.1f;

        [Tooltip("난이도당 골드 보상 스케일 비율")]
        [Range(0f, 0.5f)]
        [SerializeField] private float _goldScalePerDifficulty = 0.1f;

        public float HPScalePerDifficulty => _hpScalePerDifficulty;
        public float DamageScalePerDifficulty => _damageScalePerDifficulty;
        public float GoldScalePerDifficulty => _goldScalePerDifficulty;

        #endregion

        #region Test/Fallback Settings

        [Header("Test/Fallback Settings (테스트/폴백 설정)")]
        [Tooltip("테스트 파티 크기")]
        [SerializeField] private int _testPartySize = 3;

        [Tooltip("테스트 파티원 체력")]
        [SerializeField] private int _testPartyHP = 50;

        [Tooltip("테스트 파티원 에너지")]
        [SerializeField] private int _testPartyEnergy = 3;

        [Tooltip("테스트 파티원 속도")]
        [SerializeField] private int _testPartySpeed = 10;

        [Tooltip("테스트 적 수")]
        [SerializeField] private int _testEnemyCount = 2;

        [Tooltip("테스트 적 체력")]
        [SerializeField] private int _testEnemyHP = 30;

        [Tooltip("테스트 적 속도")]
        [SerializeField] private int _testEnemySpeed = 8;

        [Tooltip("테스트 적 데미지")]
        [SerializeField] private int _testEnemyDamage = 8;

        public int TestPartySize => _testPartySize;
        public int TestPartyHP => _testPartyHP;
        public int TestPartyEnergy => _testPartyEnergy;
        public int TestPartySpeed => _testPartySpeed;
        public int TestEnemyCount => _testEnemyCount;
        public int TestEnemyHP => _testEnemyHP;
        public int TestEnemySpeed => _testEnemySpeed;
        public int TestEnemyDamage => _testEnemyDamage;

        #endregion

        #region Fallback Encounter Settings

        [Header("Fallback Encounter Settings (폴백 인카운터 설정)")]
        [Tooltip("보스 체력")]
        [SerializeField] private int _fallbackBossHP = 100;

        [Tooltip("보스 데미지")]
        [SerializeField] private int _fallbackBossDamage = 15;

        [Tooltip("엘리트 체력")]
        [SerializeField] private int _fallbackEliteHP = 50;

        [Tooltip("엘리트 데미지")]
        [SerializeField] private int _fallbackEliteDamage = 12;

        [Tooltip("일반 적 체력")]
        [SerializeField] private int _fallbackNormalHP = 30;

        [Tooltip("일반 적 데미지")]
        [SerializeField] private int _fallbackNormalDamage = 8;

        public int FallbackBossHP => _fallbackBossHP;
        public int FallbackBossDamage => _fallbackBossDamage;
        public int FallbackEliteHP => _fallbackEliteHP;
        public int FallbackEliteDamage => _fallbackEliteDamage;
        public int FallbackNormalHP => _fallbackNormalHP;
        public int FallbackNormalDamage => _fallbackNormalDamage;

        #endregion

        #region Validation

        private void OnValidate()
        {
            // 공격/방어 확률 합이 1이 되도록 자동 조정
            if (_defaultEnemyAttackChance + _defaultEnemyDefendChance > 1f)
            {
                _defaultEnemyDefendChance = 1f - _defaultEnemyAttackChance;
            }

            // 최소값 보장
            _defaultMaxHP = Mathf.Max(1, _defaultMaxHP);
            _defaultEnergy = Mathf.Max(1, _defaultEnergy);
            _defaultSpeed = Mathf.Max(1, _defaultSpeed);
            _maxHandSize = Mathf.Max(1, _maxHandSize);
            _initialDrawCount = Mathf.Max(1, _initialDrawCount);
            _drawPerTurn = Mathf.Max(0, _drawPerTurn);
        }

        #endregion
    }
}
