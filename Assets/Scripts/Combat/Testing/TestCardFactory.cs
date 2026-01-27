// Combat/Testing/TestCardFactory.cs
// 런타임 테스트 카드 생성 팩토리 - Boot 씬 없이 Combat 테스트용

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data.Cards;

namespace ProjectSS.Combat.Testing
{
    /// <summary>
    /// 런타임 테스트 카드 생성 팩토리
    /// Boot 씬 없이 Combat 씬 단독 실행 시 테스트용 카드 데이터 생성
    /// ScriptableObject.CreateInstance + Reflection으로 private 필드 설정
    /// </summary>
    public static class TestCardFactory
    {
        private static Dictionary<string, CardDataSO> _cachedCards = new Dictionary<string, CardDataSO>();
        private static Dictionary<string, CardEffectSO> _cachedEffects = new Dictionary<string, CardEffectSO>();

        #region Public Methods

        /// <summary>
        /// 클래스별 스타터 덱 생성
        /// </summary>
        public static List<CardDataSO> CreateStarterDeck(CharacterClass charClass)
        {
            var deck = new List<CardDataSO>();

            // 기본 카드: Strike x3, Defend x3
            for (int i = 0; i < 3; i++)
            {
                deck.Add(GetOrCreateStrike());
            }
            for (int i = 0; i < 3; i++)
            {
                deck.Add(GetOrCreateDefend());
            }

            // 클래스별 추가 카드
            switch (charClass)
            {
                case CharacterClass.Warrior:
                    deck.Add(GetOrCreateBash());
                    break;
                case CharacterClass.Mage:
                    deck.Add(GetOrCreateZap());
                    break;
                case CharacterClass.Healer:
                    deck.Add(GetOrCreateHeal());
                    break;
                default:
                    deck.Add(GetOrCreateStrike());
                    break;
            }

            Debug.Log($"[TestCardFactory] Created starter deck for {charClass}: {deck.Count} cards");
            return deck;
        }

        /// <summary>
        /// 타격 카드 생성 (6 데미지)
        /// </summary>
        public static CardDataSO GetOrCreateStrike()
        {
            const string key = "test_strike";
            if (_cachedCards.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var card = CreateCardData(
                cardId: key,
                cardName: "타격",
                cardType: CardType.Attack,
                energyCost: 1,
                description: "적에게 6의 피해를 줍니다.",
                targetType: TargetType.SingleEnemy,
                rarity: CardRarity.Starter
            );

            AddDamageEffect(card, 6, TargetType.SingleEnemy);
            _cachedCards[key] = card;
            return card;
        }

        /// <summary>
        /// 수비 카드 생성 (5 방어)
        /// </summary>
        public static CardDataSO GetOrCreateDefend()
        {
            const string key = "test_defend";
            if (_cachedCards.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var card = CreateCardData(
                cardId: key,
                cardName: "수비",
                cardType: CardType.Skill,
                energyCost: 1,
                description: "5의 방어막을 얻습니다.",
                targetType: TargetType.Self,
                rarity: CardRarity.Starter
            );

            AddBlockEffect(card, 5);
            _cachedCards[key] = card;
            return card;
        }

        /// <summary>
        /// 강타 카드 생성 (전사용 - 8 데미지 + 취약)
        /// </summary>
        public static CardDataSO GetOrCreateBash()
        {
            const string key = "test_bash";
            if (_cachedCards.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var card = CreateCardData(
                cardId: key,
                cardName: "강타",
                cardType: CardType.Attack,
                energyCost: 2,
                description: "적에게 8의 피해를 줍니다.",
                targetType: TargetType.SingleEnemy,
                rarity: CardRarity.Starter
            );

            AddDamageEffect(card, 8, TargetType.SingleEnemy);
            _cachedCards[key] = card;
            return card;
        }

        /// <summary>
        /// 전격 카드 생성 (마법사용 - 모든 적에게 4 데미지)
        /// </summary>
        public static CardDataSO GetOrCreateZap()
        {
            const string key = "test_zap";
            if (_cachedCards.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var card = CreateCardData(
                cardId: key,
                cardName: "전격",
                cardType: CardType.Attack,
                energyCost: 1,
                description: "모든 적에게 4의 피해를 줍니다.",
                targetType: TargetType.AllEnemies,
                rarity: CardRarity.Starter
            );

            AddDamageEffect(card, 4, TargetType.AllEnemies);
            _cachedCards[key] = card;
            return card;
        }

        /// <summary>
        /// 치유 카드 생성 (힐러용 - 6 회복)
        /// </summary>
        public static CardDataSO GetOrCreateHeal()
        {
            const string key = "test_heal";
            if (_cachedCards.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var card = CreateCardData(
                cardId: key,
                cardName: "치유",
                cardType: CardType.Skill,
                energyCost: 1,
                description: "6의 체력을 회복합니다.",
                targetType: TargetType.Self,
                rarity: CardRarity.Starter
            );

            AddHealEffect(card, 6);
            _cachedCards[key] = card;
            return card;
        }

        /// <summary>
        /// 캐시 클리어 (씬 전환 시 호출)
        /// </summary>
        public static void ClearCache()
        {
            foreach (var card in _cachedCards.Values)
            {
                if (card != null)
                {
                    Object.DestroyImmediate(card);
                }
            }
            _cachedCards.Clear();

            foreach (var effect in _cachedEffects.Values)
            {
                if (effect != null)
                {
                    Object.DestroyImmediate(effect);
                }
            }
            _cachedEffects.Clear();

            Debug.Log("[TestCardFactory] Cache cleared");
        }

        #endregion

        #region Card Creation

        private static CardDataSO CreateCardData(
            string cardId,
            string cardName,
            CardType cardType,
            int energyCost,
            string description,
            TargetType targetType,
            CardRarity rarity)
        {
            var card = ScriptableObject.CreateInstance<CardDataSO>();

            SetPrivateField(card, "_cardId", cardId);
            SetPrivateField(card, "_cardName", cardName);
            SetPrivateField(card, "_cardType", cardType);
            SetPrivateField(card, "_energyCost", energyCost);
            SetPrivateField(card, "_upgradedEnergyCost", energyCost);
            SetPrivateField(card, "_description", description);
            SetPrivateField(card, "_targetType", targetType);
            SetPrivateField(card, "_rarity", rarity);

            // 빈 효과 리스트 초기화
            SetPrivateField(card, "_effects", new List<CardEffectSO>());
            SetPrivateField(card, "_upgradedEffects", new List<CardEffectSO>());

            return card;
        }

        #endregion

        #region Effect Creation

        private static void AddDamageEffect(CardDataSO card, int damage, TargetType targetType)
        {
            string effectKey = $"damage_{damage}_{targetType}";

            DamageEffectSO effect;
            if (_cachedEffects.TryGetValue(effectKey, out var cached) && cached is DamageEffectSO damageEffect)
            {
                effect = damageEffect;
            }
            else
            {
                effect = ScriptableObject.CreateInstance<DamageEffectSO>();
                SetPrivateField(effect, "_baseDamage", damage);
                SetPrivateField(effect, "_targetType", targetType);
                SetPrivateField(effect, "_scalesWithStrength", true);
                _cachedEffects[effectKey] = effect;
            }

            var effects = GetPrivateField<List<CardEffectSO>>(card, "_effects") ?? new List<CardEffectSO>();
            effects.Add(effect);
            SetPrivateField(card, "_effects", effects);
        }

        private static void AddBlockEffect(CardDataSO card, int block)
        {
            string effectKey = $"block_{block}";

            BlockEffectSO effect;
            if (_cachedEffects.TryGetValue(effectKey, out var cached) && cached is BlockEffectSO blockEffect)
            {
                effect = blockEffect;
            }
            else
            {
                effect = ScriptableObject.CreateInstance<BlockEffectSO>();
                SetPrivateField(effect, "_baseBlock", block);
                SetPrivateField(effect, "_scalesWithDexterity", true);
                _cachedEffects[effectKey] = effect;
            }

            var effects = GetPrivateField<List<CardEffectSO>>(card, "_effects") ?? new List<CardEffectSO>();
            effects.Add(effect);
            SetPrivateField(card, "_effects", effects);
        }

        private static void AddHealEffect(CardDataSO card, int heal)
        {
            string effectKey = $"heal_{heal}";

            // HealEffectSO가 있으면 사용, 없으면 BlockEffect로 대체 (테스트용)
            CardEffectSO effect;
            if (_cachedEffects.TryGetValue(effectKey, out var cached) && cached != null)
            {
                effect = cached;
            }
            else
            {
                // HealEffectSO 생성 시도
                var healEffect = ScriptableObject.CreateInstance<HealEffectSO>();
                if (healEffect != null)
                {
                    SetPrivateField(healEffect, "_healAmount", heal);
                    SetPrivateField(healEffect, "_targetType", HealTargetType.Self);
                    effect = healEffect;
                }
                else
                {
                    // 폴백: Block 효과 사용
                    var blockEffect = ScriptableObject.CreateInstance<BlockEffectSO>();
                    SetPrivateField(blockEffect, "_baseBlock", heal);
                    effect = blockEffect;
                }
                _cachedEffects[effectKey] = effect;
            }

            var effects = GetPrivateField<List<CardEffectSO>>(card, "_effects") ?? new List<CardEffectSO>();
            effects.Add(effect);
            SetPrivateField(card, "_effects", effects);
        }

        #endregion

        #region Reflection Helpers

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            if (obj == null) return;

            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[TestCardFactory] Field '{fieldName}' not found on {type.Name}");
            }
        }

        private static T GetPrivateField<T>(object obj, string fieldName) where T : class
        {
            if (obj == null) return null;

            var type = obj.GetType();
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null)
            {
                return field.GetValue(obj) as T;
            }

            Debug.LogWarning($"[TestCardFactory] Field '{fieldName}' not found on {type.Name}");
            return null;
        }

        #endregion
    }
}
