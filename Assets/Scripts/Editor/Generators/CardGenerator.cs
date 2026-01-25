using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 카드 데이터 생성기
    /// Card data generator
    /// </summary>
    public static class CardGenerator
    {
        #region Menu Items

        [MenuItem("Tools/Project SS/Generators/Quick Create/Attack Card")]
        public static void QuickCreateAttackCard()
        {
            // 기본 공격 카드 생성
            var card = CreateAttackCard(
                koreanName: "새공격",
                englishName: "NewAttack",
                damage: 6,
                cost: 1,
                rarity: CardRarity.Common
            );

            if (card != null)
            {
                Selection.activeObject = card;
                EditorGUIUtility.PingObject(card);
            }
        }

        [MenuItem("Tools/Project SS/Generators/Quick Create/Defense Card")]
        public static void QuickCreateDefenseCard()
        {
            // 기본 방어 카드 생성
            var card = CreateDefenseCard(
                koreanName: "새방어",
                englishName: "NewDefense",
                block: 5,
                cost: 1,
                rarity: CardRarity.Common
            );

            if (card != null)
            {
                Selection.activeObject = card;
                EditorGUIUtility.PingObject(card);
            }
        }

        [MenuItem("Tools/Project SS/Generators/Quick Create/Skill Card")]
        public static void QuickCreateSkillCard()
        {
            // 기본 스킬 카드 생성
            var effects = new List<CardEffect>
            {
                new CardEffect
                {
                    effectType = CardEffectType.Draw,
                    value = 2,
                    targetType = TargetType.Self
                }
            };

            var card = CreateSkillCard(
                koreanName: "새스킬",
                englishName: "NewSkill",
                effects: effects,
                cost: 1,
                rarity: CardRarity.Common
            );

            if (card != null)
            {
                Selection.activeObject = card;
                EditorGUIUtility.PingObject(card);
            }
        }

        #endregion

        #region Core Creation Methods

        /// <summary>
        /// 공격 카드 생성
        /// Create attack card
        /// </summary>
        public static CardData CreateAttackCard(
            string koreanName,
            string englishName,
            int damage,
            int cost,
            CardRarity rarity,
            TargetType targetType = TargetType.SingleEnemy,
            List<CharacterClass> allowedClasses = null,
            bool exhaust = false)
        {
            var effects = new List<CardEffect>
            {
                new CardEffect
                {
                    effectType = CardEffectType.Damage,
                    value = damage,
                    targetType = targetType
                }
            };

            return CreateCard(
                koreanName: koreanName,
                englishName: englishName,
                cardType: CardType.Attack,
                rarity: rarity,
                cost: cost,
                effects: effects,
                allowedClasses: allowedClasses,
                exhaust: exhaust
            );
        }

        /// <summary>
        /// 방어 카드 생성
        /// Create defense card
        /// </summary>
        public static CardData CreateDefenseCard(
            string koreanName,
            string englishName,
            int block,
            int cost,
            CardRarity rarity,
            List<CharacterClass> allowedClasses = null,
            bool exhaust = false)
        {
            var effects = new List<CardEffect>
            {
                new CardEffect
                {
                    effectType = CardEffectType.Block,
                    value = block,
                    targetType = TargetType.Self
                }
            };

            return CreateCard(
                koreanName: koreanName,
                englishName: englishName,
                cardType: CardType.Defense,
                rarity: rarity,
                cost: cost,
                effects: effects,
                allowedClasses: allowedClasses,
                exhaust: exhaust
            );
        }

        /// <summary>
        /// 스킬 카드 생성
        /// Create skill card
        /// </summary>
        public static CardData CreateSkillCard(
            string koreanName,
            string englishName,
            List<CardEffect> effects,
            int cost,
            CardRarity rarity,
            List<CharacterClass> allowedClasses = null,
            bool exhaust = false)
        {
            return CreateCard(
                koreanName: koreanName,
                englishName: englishName,
                cardType: CardType.Skill,
                rarity: rarity,
                cost: cost,
                effects: effects,
                allowedClasses: allowedClasses,
                exhaust: exhaust
            );
        }

        /// <summary>
        /// Focus 카드 생성 (TRIAD 전용)
        /// Create Focus card (TRIAD feature)
        /// </summary>
        public static CardData CreateFocusCard(
            string koreanName,
            string englishName,
            CardType cardType,
            int focusCost,
            List<CardEffect> baseEffects,
            List<CardEffect> focusBonusEffects,
            int energyCost,
            CardRarity rarity,
            List<CharacterClass> allowedClasses)
        {
            string fileName = GeneratorUtility.FormatCardFileName(cardType, koreanName, englishName);
            string subfolder = GeneratorUtility.GetCardSubfolder(cardType);
            string fullPath = $"{GeneratorUtility.CARDS_PATH}/{subfolder}/{fileName}.asset";

            // 이미 존재하면 반환
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"카드 이미 존재: {fileName}");
                return AssetDatabase.LoadAssetAtPath<CardData>(fullPath);
            }

            // 폴더 확인
            GeneratorUtility.EnsureFolderExists($"{GeneratorUtility.CARDS_PATH}/{subfolder}");

            // 새 카드 생성
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = fileName.ToLower().Replace(" ", "_");
            card.cardName = $"{koreanName} ({englishName})";
            card.cardType = cardType;
            card.rarity = rarity;
            card.energyCost = energyCost;
            card.effects = baseEffects ?? new List<CardEffect>();
            card.focusCost = focusCost;
            card.focusBonusEffects = focusBonusEffects ?? new List<CardEffect>();
            card.allowedClasses = allowedClasses ?? new List<CharacterClass>();

            AssetDatabase.CreateAsset(card, fullPath);
            Debug.Log($"Focus 카드 생성: {fileName}");
            return card;
        }

        /// <summary>
        /// 기본 카드 생성
        /// Create card with full parameters
        /// </summary>
        public static CardData CreateCard(
            string koreanName,
            string englishName,
            CardType cardType,
            CardRarity rarity,
            int cost,
            List<CardEffect> effects,
            List<CharacterClass> allowedClasses = null,
            bool exhaust = false,
            string description = null,
            int focusCost = 0,
            List<CardEffect> focusBonusEffects = null)
        {
            string fileName = GeneratorUtility.FormatCardFileName(cardType, koreanName, englishName);
            string subfolder = GeneratorUtility.GetCardSubfolder(cardType);
            string fullPath = $"{GeneratorUtility.CARDS_PATH}/{subfolder}/{fileName}.asset";

            // 이미 존재하면 반환
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"카드 이미 존재: {fileName}");
                return AssetDatabase.LoadAssetAtPath<CardData>(fullPath);
            }

            // 폴더 확인
            GeneratorUtility.EnsureFolderExists($"{GeneratorUtility.CARDS_PATH}/{subfolder}");

            // 새 카드 생성
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = fileName.ToLower().Replace(" ", "_");
            card.cardName = $"{koreanName} ({englishName})";
            card.cardType = cardType;
            card.rarity = rarity;
            card.energyCost = cost;
            card.exhaustOnUse = exhaust;
            card.effects = effects ?? new List<CardEffect>();
            card.allowedClasses = allowedClasses ?? new List<CharacterClass>();
            card.focusCost = focusCost;
            card.focusBonusEffects = focusBonusEffects ?? new List<CardEffect>();

            if (!string.IsNullOrEmpty(description))
            {
                card.description = description;
            }

            AssetDatabase.CreateAsset(card, fullPath);
            Debug.Log($"카드 생성: {fileName}");
            return card;
        }

        /// <summary>
        /// 업그레이드 버전 생성
        /// Create upgrade variant
        /// </summary>
        public static CardData CreateUpgradeVariant(
            CardData baseCard,
            string upgradeSuffix = "+",
            int damageBonus = 0,
            int blockBonus = 0,
            int costReduction = 0)
        {
            if (baseCard == null)
            {
                Debug.LogError("기본 카드가 null입니다.");
                return null;
            }

            // 한글/영문 이름 추출
            string baseName = baseCard.cardName;
            string koreanName, englishName;

            // "한글명 (EnglishName)" 형식 파싱
            int parenIndex = baseName.IndexOf('(');
            if (parenIndex > 0)
            {
                koreanName = baseName.Substring(0, parenIndex).Trim() + upgradeSuffix;
                englishName = baseName.Substring(parenIndex + 1).TrimEnd(')').Trim() + upgradeSuffix;
            }
            else
            {
                koreanName = baseName + upgradeSuffix;
                englishName = baseCard.cardId + upgradeSuffix;
            }

            // 효과 복사 및 보너스 적용
            var upgradedEffects = new List<CardEffect>();
            foreach (var effect in baseCard.effects)
            {
                var newEffect = new CardEffect
                {
                    effectType = effect.effectType,
                    value = effect.value,
                    targetType = effect.targetType,
                    statusEffect = effect.statusEffect,
                    statusStacks = effect.statusStacks
                };

                // 보너스 적용
                if (effect.effectType == CardEffectType.Damage)
                    newEffect.value += damageBonus;
                else if (effect.effectType == CardEffectType.Block)
                    newEffect.value += blockBonus;

                upgradedEffects.Add(newEffect);
            }

            // 새 비용
            int newCost = Mathf.Max(0, baseCard.energyCost - costReduction);

            // 업그레이드 카드 생성
            var upgradedCard = CreateCard(
                koreanName: koreanName,
                englishName: englishName,
                cardType: baseCard.cardType,
                rarity: baseCard.rarity,
                cost: newCost,
                effects: upgradedEffects,
                allowedClasses: new List<CharacterClass>(baseCard.allowedClasses),
                exhaust: baseCard.exhaustOnUse,
                focusCost: baseCard.focusCost,
                focusBonusEffects: baseCard.focusBonusEffects != null
                    ? new List<CardEffect>(baseCard.focusBonusEffects)
                    : null
            );

            if (upgradedCard != null)
            {
                upgradedCard.isUpgraded = true;

                // 원본 카드에 업그레이드 버전 연결
                baseCard.upgradedVersion = upgradedCard;
                EditorUtility.SetDirty(baseCard);
            }

            return upgradedCard;
        }

        #endregion

        #region Bulk Creation

        /// <summary>
        /// 정의 목록에서 카드 대량 생성
        /// Create cards from definition list
        /// </summary>
        public static List<CardData> CreateCardsFromDefinitions(List<CardCreationData> definitions)
        {
            var createdCards = new List<CardData>();

            foreach (var def in definitions)
            {
                var card = CreateCard(
                    koreanName: def.koreanName,
                    englishName: def.englishName,
                    cardType: def.cardType,
                    rarity: def.rarity,
                    cost: def.energyCost,
                    effects: def.effects,
                    allowedClasses: def.allowedClasses,
                    exhaust: def.exhaustOnUse,
                    focusCost: def.focusCost,
                    focusBonusEffects: def.focusBonusEffects
                );

                if (card != null)
                {
                    createdCards.Add(card);

                    // 업그레이드 생성
                    if (def.createUpgrade)
                    {
                        CreateUpgradeVariant(card, "+", 2, 2, 0);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return createdCards;
        }

        #endregion

        #region Effect Builders

        /// <summary>
        /// 데미지 효과 생성
        /// Create damage effect
        /// </summary>
        public static CardEffect CreateDamageEffect(int damage, TargetType target = TargetType.SingleEnemy)
        {
            return new CardEffect
            {
                effectType = CardEffectType.Damage,
                value = damage,
                targetType = target
            };
        }

        /// <summary>
        /// 블록 효과 생성
        /// Create block effect
        /// </summary>
        public static CardEffect CreateBlockEffect(int block)
        {
            return new CardEffect
            {
                effectType = CardEffectType.Block,
                value = block,
                targetType = TargetType.Self
            };
        }

        /// <summary>
        /// 상태효과 부여 효과 생성
        /// Create apply status effect
        /// </summary>
        public static CardEffect CreateApplyStatusEffect(StatusEffectData status, int stacks, TargetType target = TargetType.SingleEnemy)
        {
            return new CardEffect
            {
                effectType = CardEffectType.ApplyStatus,
                statusEffect = status,
                statusStacks = stacks,
                targetType = target
            };
        }

        /// <summary>
        /// 드로우 효과 생성
        /// Create draw effect
        /// </summary>
        public static CardEffect CreateDrawEffect(int cards)
        {
            return new CardEffect
            {
                effectType = CardEffectType.Draw,
                value = cards,
                targetType = TargetType.Self
            };
        }

        /// <summary>
        /// 에너지 획득 효과 생성
        /// Create gain energy effect
        /// </summary>
        public static CardEffect CreateGainEnergyEffect(int energy)
        {
            return new CardEffect
            {
                effectType = CardEffectType.GainEnergy,
                value = energy,
                targetType = TargetType.Self
            };
        }

        /// <summary>
        /// 회복 효과 생성
        /// Create heal effect
        /// </summary>
        public static CardEffect CreateHealEffect(int hp)
        {
            return new CardEffect
            {
                effectType = CardEffectType.Heal,
                value = hp,
                targetType = TargetType.Self
            };
        }

        #endregion
    }

    /// <summary>
    /// 카드 생성 데이터 구조체
    /// Card creation data structure
    /// </summary>
    [System.Serializable]
    public class CardCreationData
    {
        public string koreanName;
        public string englishName;
        public CardType cardType;
        public CardRarity rarity;
        public int energyCost;
        public bool exhaustOnUse;
        public List<CardEffect> effects = new List<CardEffect>();
        public int focusCost;
        public List<CardEffect> focusBonusEffects = new List<CardEffect>();
        public List<CharacterClass> allowedClasses = new List<CharacterClass>();
        public bool createUpgrade;
    }
}
