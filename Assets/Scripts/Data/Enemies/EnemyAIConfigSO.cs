// Data/Enemies/EnemyAIConfigSO.cs
// 적 AI 행동 패턴 설정 ScriptableObject
// 적별로 다양한 행동 패턴 정의 가능

using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data.Enemies
{
    /// <summary>
    /// 적 AI 행동 패턴 설정
    /// 각 적 타입별로 다른 행동 패턴 정의 가능
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyAIConfig", menuName = "PSTS/Enemy/AI Config")]
    public class EnemyAIConfigSO : ScriptableObject
    {
        #region Intent Weight

        [Serializable]
        public class IntentWeight
        {
            [Tooltip("의도 타입")]
            public EnemyIntentType Intent;

            [Tooltip("가중치 (0-1)")]
            [Range(0f, 1f)]
            public float Weight;

            public IntentWeight(EnemyIntentType intent, float weight)
            {
                Intent = intent;
                Weight = weight;
            }
        }

        #endregion

        #region Health-Based Behavior

        [Serializable]
        public class HealthBasedBehavior
        {
            [Tooltip("체력 임계값 (0-1, 이 비율 이하일 때 적용)")]
            [Range(0f, 1f)]
            public float HealthThreshold;

            [Tooltip("이 체력 상태에서의 행동 가중치")]
            public List<IntentWeight> Weights;

            public HealthBasedBehavior()
            {
                HealthThreshold = 0.3f;
                Weights = new List<IntentWeight>();
            }
        }

        #endregion

        #region Settings

        [Header("Basic Intent Weights (기본 행동 가중치)")]
        [Tooltip("기본 행동 패턴 (체력 조건 없을 때)")]
        [SerializeField] private List<IntentWeight> _defaultWeights = new List<IntentWeight>
        {
            new IntentWeight(EnemyIntentType.Attack, 0.7f),
            new IntentWeight(EnemyIntentType.Defend, 0.3f)
        };

        [Header("Health-Based Behavior (체력 기반 행동)")]
        [Tooltip("체력 기반 행동 패턴 사용 여부")]
        [SerializeField] private bool _useHealthBasedBehavior = false;

        [Tooltip("체력 기반 행동 패턴 목록 (우선순위 순)")]
        [SerializeField] private List<HealthBasedBehavior> _healthBasedBehaviors = new List<HealthBasedBehavior>();

        [Header("Special Behaviors (특수 행동)")]
        [Tooltip("첫 턴에 항상 특정 행동 수행")]
        [SerializeField] private bool _hasFirstTurnBehavior = false;

        [Tooltip("첫 턴 행동 타입")]
        [SerializeField] private EnemyIntentType _firstTurnIntent = EnemyIntentType.Attack;

        [Tooltip("연속 같은 행동 제한")]
        [SerializeField] private bool _preventConsecutiveSameAction = false;

        [Tooltip("최대 연속 같은 행동 횟수")]
        [SerializeField] private int _maxConsecutiveSameAction = 2;

        #endregion

        #region Properties

        public IReadOnlyList<IntentWeight> DefaultWeights => _defaultWeights;
        public bool UseHealthBasedBehavior => _useHealthBasedBehavior;
        public IReadOnlyList<HealthBasedBehavior> HealthBasedBehaviors => _healthBasedBehaviors;
        public bool HasFirstTurnBehavior => _hasFirstTurnBehavior;
        public EnemyIntentType FirstTurnIntent => _firstTurnIntent;
        public bool PreventConsecutiveSameAction => _preventConsecutiveSameAction;
        public int MaxConsecutiveSameAction => _maxConsecutiveSameAction;

        #endregion

        #region Intent Selection

        /// <summary>
        /// 현재 상태에 따른 의도 선택
        /// </summary>
        /// <param name="healthRatio">현재 체력 비율 (0-1)</param>
        /// <param name="isFirstTurn">첫 턴 여부</param>
        /// <param name="lastIntent">이전 의도 (연속 행동 제한용)</param>
        /// <param name="consecutiveCount">연속 같은 행동 횟수</param>
        /// <returns>선택된 의도 타입</returns>
        public EnemyIntentType SelectIntent(float healthRatio, bool isFirstTurn = false,
            EnemyIntentType? lastIntent = null, int consecutiveCount = 0)
        {
            // 첫 턴 특수 행동
            if (isFirstTurn && _hasFirstTurnBehavior)
            {
                return _firstTurnIntent;
            }

            // 사용할 가중치 결정
            List<IntentWeight> weights = GetWeightsForHealth(healthRatio);

            // 연속 행동 제한 적용
            if (_preventConsecutiveSameAction && lastIntent.HasValue &&
                consecutiveCount >= _maxConsecutiveSameAction)
            {
                weights = FilterOutIntent(weights, lastIntent.Value);
            }

            // 가중치 기반 랜덤 선택
            return SelectRandomIntent(weights);
        }

        /// <summary>
        /// 체력 비율에 따른 가중치 가져오기
        /// </summary>
        private List<IntentWeight> GetWeightsForHealth(float healthRatio)
        {
            if (!_useHealthBasedBehavior)
            {
                return new List<IntentWeight>(_defaultWeights);
            }

            // 체력 기반 행동 패턴 중 조건에 맞는 것 찾기
            foreach (var behavior in _healthBasedBehaviors)
            {
                if (healthRatio <= behavior.HealthThreshold && behavior.Weights.Count > 0)
                {
                    return new List<IntentWeight>(behavior.Weights);
                }
            }

            return new List<IntentWeight>(_defaultWeights);
        }

        /// <summary>
        /// 특정 의도 제외
        /// </summary>
        private List<IntentWeight> FilterOutIntent(List<IntentWeight> weights, EnemyIntentType intentToRemove)
        {
            var filtered = new List<IntentWeight>();
            foreach (var weight in weights)
            {
                if (weight.Intent != intentToRemove)
                {
                    filtered.Add(weight);
                }
            }

            // 모든 것이 필터링되면 원본 반환
            return filtered.Count > 0 ? filtered : weights;
        }

        /// <summary>
        /// 가중치 기반 랜덤 의도 선택
        /// </summary>
        private EnemyIntentType SelectRandomIntent(List<IntentWeight> weights)
        {
            if (weights == null || weights.Count == 0)
            {
                return EnemyIntentType.Attack;
            }

            // 총 가중치 계산
            float totalWeight = 0f;
            foreach (var w in weights)
            {
                totalWeight += w.Weight;
            }

            if (totalWeight <= 0f)
            {
                return weights[0].Intent;
            }

            // 랜덤 선택
            float random = UnityEngine.Random.value * totalWeight;
            float cumulative = 0f;

            foreach (var w in weights)
            {
                cumulative += w.Weight;
                if (random <= cumulative)
                {
                    return w.Intent;
                }
            }

            return weights[weights.Count - 1].Intent;
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // 기본 가중치가 비어있으면 기본값 추가
            if (_defaultWeights == null || _defaultWeights.Count == 0)
            {
                _defaultWeights = new List<IntentWeight>
                {
                    new IntentWeight(EnemyIntentType.Attack, 0.7f),
                    new IntentWeight(EnemyIntentType.Defend, 0.3f)
                };
            }

            // 가중치 정규화 (합이 1이 되도록)
            NormalizeWeights(_defaultWeights);

            foreach (var behavior in _healthBasedBehaviors)
            {
                if (behavior.Weights != null)
                {
                    NormalizeWeights(behavior.Weights);
                }
            }
        }

        private void NormalizeWeights(List<IntentWeight> weights)
        {
            if (weights == null || weights.Count == 0) return;

            float total = 0f;
            foreach (var w in weights)
            {
                total += w.Weight;
            }

            if (total > 0f && Mathf.Abs(total - 1f) > 0.01f)
            {
                foreach (var w in weights)
                {
                    w.Weight /= total;
                }
            }
        }

        #endregion

        #region Static Presets

        /// <summary>
        /// 기본 AI 설정 (공격 70%, 방어 30%)
        /// </summary>
        public static EnemyAIConfigSO CreateDefault()
        {
            var config = CreateInstance<EnemyAIConfigSO>();
            return config;
        }

        /// <summary>
        /// 공격적 AI 설정 (공격 90%, 방어 10%)
        /// </summary>
        public static EnemyAIConfigSO CreateAggressive()
        {
            var config = CreateInstance<EnemyAIConfigSO>();
            config._defaultWeights = new List<IntentWeight>
            {
                new IntentWeight(EnemyIntentType.Attack, 0.9f),
                new IntentWeight(EnemyIntentType.Defend, 0.1f)
            };
            return config;
        }

        /// <summary>
        /// 방어적 AI 설정 (공격 40%, 방어 60%)
        /// </summary>
        public static EnemyAIConfigSO CreateDefensive()
        {
            var config = CreateInstance<EnemyAIConfigSO>();
            config._defaultWeights = new List<IntentWeight>
            {
                new IntentWeight(EnemyIntentType.Attack, 0.4f),
                new IntentWeight(EnemyIntentType.Defend, 0.6f)
            };
            return config;
        }

        #endregion
    }
}
