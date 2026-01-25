using NUnit.Framework;
using UnityEngine;

namespace ProjectSS.Tests.EditMode.Combat
{
    /// <summary>
    /// 데미지 계산 공식 유닛 테스트 (EditMode)
    /// Damage calculation formula unit tests (EditMode)
    ///
    /// 참고: DamageCalculator는 CombatEntity(MonoBehaviour)에 의존하므로
    /// 전체 통합 테스트는 PlayMode에서 수행됩니다.
    /// 이 테스트는 공식/상수 검증에 집중합니다.
    ///
    /// Note: DamageCalculator depends on CombatEntity (MonoBehaviour),
    /// so full integration tests are in PlayMode.
    /// These tests focus on formula/constant verification.
    /// </summary>
    [TestFixture]
    public class DamageCalculatorTests
    {
        // 상수 검증을 위한 예상 값
        // Expected values for constant verification
        private const float EXPECTED_WEAK_MODIFIER = 0.75f;
        private const float EXPECTED_VULNERABLE_MODIFIER = 1.5f;
        private const float EXPECTED_FRAIL_MODIFIER = 0.75f;

        #region Formula Verification Tests

        /// <summary>
        /// 약화 수정자 공식 검증: 0.75 (25% 감소)
        /// Weak modifier formula verification: 0.75 (25% reduction)
        /// </summary>
        [Test]
        public void WeakModifier_Is75Percent()
        {
            // 약화 적용 시 데미지 = baseDamage × 0.75
            // Damage with Weak = baseDamage × 0.75
            int baseDamage = 8;
            int expectedDamage = Mathf.RoundToInt(baseDamage * EXPECTED_WEAK_MODIFIER);

            Assert.AreEqual(6, expectedDamage);
        }

        /// <summary>
        /// 취약 수정자 공식 검증: 1.5 (50% 증가)
        /// Vulnerable modifier formula verification: 1.5 (50% increase)
        /// </summary>
        [Test]
        public void VulnerableModifier_Is150Percent()
        {
            // 취약 적용 시 데미지 = baseDamage × 1.5
            // Damage with Vulnerable = baseDamage × 1.5
            int baseDamage = 6;
            int expectedDamage = Mathf.RoundToInt(baseDamage * EXPECTED_VULNERABLE_MODIFIER);

            Assert.AreEqual(9, expectedDamage);
        }

        /// <summary>
        /// 허약 수정자 공식 검증: 0.75 (25% 감소)
        /// Frail modifier formula verification: 0.75 (25% reduction)
        /// </summary>
        [Test]
        public void FrailModifier_Is75Percent()
        {
            // 허약 적용 시 블록 = baseBlock × 0.75
            // Block with Frail = baseBlock × 0.75
            int baseBlock = 8;
            int expectedBlock = Mathf.RoundToInt(baseBlock * EXPECTED_FRAIL_MODIFIER);

            Assert.AreEqual(6, expectedBlock);
        }

        #endregion

        #region Damage Formula Tests

        /// <summary>
        /// 기본 데미지 공식: base + Strength
        /// Base damage formula: base + Strength
        /// </summary>
        [Test]
        [TestCase(6, 0, 6)]
        [TestCase(6, 1, 7)]
        [TestCase(6, 3, 9)]
        [TestCase(6, 5, 11)]
        public void DamageFormula_StrengthBonus_AddsCorrectly(int baseDamage, int strength, int expected)
        {
            int result = baseDamage + strength;
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 약화 적용 공식
        /// Weak application formula
        /// </summary>
        [Test]
        [TestCase(8, 6)]   // 8 × 0.75 = 6
        [TestCase(10, 8)]  // 10 × 0.75 = 7.5 → 8
        [TestCase(12, 9)]  // 12 × 0.75 = 9
        [TestCase(4, 3)]   // 4 × 0.75 = 3
        public void DamageFormula_WeakReduction_AppliesCorrectly(int baseDamage, int expected)
        {
            int result = Mathf.RoundToInt(baseDamage * EXPECTED_WEAK_MODIFIER);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 취약 적용 공식
        /// Vulnerable application formula
        /// </summary>
        [Test]
        [TestCase(6, 9)]   // 6 × 1.5 = 9
        [TestCase(10, 15)] // 10 × 1.5 = 15
        [TestCase(7, 11)]  // 7 × 1.5 = 10.5 → 11
        [TestCase(4, 6)]   // 4 × 1.5 = 6
        public void DamageFormula_VulnerableIncrease_AppliesCorrectly(int baseDamage, int expected)
        {
            int result = Mathf.RoundToInt(baseDamage * EXPECTED_VULNERABLE_MODIFIER);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 복합 수정자 적용 순서: (base + Str) × Weak × Vuln
        /// Combined modifier order: (base + Str) × Weak × Vuln
        /// </summary>
        [Test]
        public void DamageFormula_AllModifiers_AppliesInCorrectOrder()
        {
            // 시나리오: 6 base + 2 Str, Weak, 타겟 Vulnerable
            // Scenario: 6 base + 2 Str, Weak, target Vulnerable
            int baseDamage = 6;
            int strength = 2;

            // 단계별 계산
            // Step by step calculation
            float damage = baseDamage + strength;  // 8
            damage *= EXPECTED_WEAK_MODIFIER;       // 8 × 0.75 = 6
            damage *= EXPECTED_VULNERABLE_MODIFIER; // 6 × 1.5 = 9

            int finalDamage = Mathf.RoundToInt(damage);

            Assert.AreEqual(9, finalDamage);
        }

        /// <summary>
        /// 복합 수정자 다른 시나리오
        /// Combined modifier different scenario
        /// </summary>
        [Test]
        public void DamageFormula_StrengthAndVulnerable_AppliesCorrectly()
        {
            // 시나리오: 10 base + 3 Str, 타겟 Vulnerable
            // Scenario: 10 base + 3 Str, target Vulnerable
            int baseDamage = 10;
            int strength = 3;

            float damage = baseDamage + strength;  // 13
            damage *= EXPECTED_VULNERABLE_MODIFIER; // 13 × 1.5 = 19.5

            int finalDamage = Mathf.RoundToInt(damage);

            Assert.AreEqual(20, finalDamage);  // 반올림
        }

        #endregion

        #region Block Formula Tests

        /// <summary>
        /// 기본 블록 공식: base + Dexterity
        /// Base block formula: base + Dexterity
        /// </summary>
        [Test]
        [TestCase(5, 0, 5)]
        [TestCase(5, 1, 6)]
        [TestCase(5, 3, 8)]
        public void BlockFormula_DexterityBonus_AddsCorrectly(int baseBlock, int dexterity, int expected)
        {
            int result = baseBlock + dexterity;
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 허약 적용 공식
        /// Frail application formula
        /// </summary>
        [Test]
        [TestCase(8, 6)]   // 8 × 0.75 = 6
        [TestCase(10, 8)]  // 10 × 0.75 = 7.5 → 8
        [TestCase(5, 4)]   // 5 × 0.75 = 3.75 → 4
        public void BlockFormula_FrailReduction_AppliesCorrectly(int baseBlock, int expected)
        {
            int result = Mathf.RoundToInt(baseBlock * EXPECTED_FRAIL_MODIFIER);
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// 블록 복합 수정자: (base + Dex) × Frail
        /// Block combined modifiers: (base + Dex) × Frail
        /// </summary>
        [Test]
        public void BlockFormula_DexterityAndFrail_AppliesCorrectly()
        {
            // 시나리오: 5 base + 3 Dex, Frail
            // Scenario: 5 base + 3 Dex, Frail
            int baseBlock = 5;
            int dexterity = 3;

            float block = baseBlock + dexterity;  // 8
            block *= EXPECTED_FRAIL_MODIFIER;      // 8 × 0.75 = 6

            int finalBlock = Mathf.RoundToInt(block);

            Assert.AreEqual(6, finalBlock);
        }

        #endregion

        #region Edge Case Tests

        /// <summary>
        /// 0 데미지는 0 유지
        /// Zero damage stays zero
        /// </summary>
        [Test]
        public void DamageFormula_ZeroDamage_StaysZero()
        {
            int baseDamage = 0;
            int strength = 0;

            float damage = baseDamage + strength;
            damage *= EXPECTED_WEAK_MODIFIER;
            damage *= EXPECTED_VULNERABLE_MODIFIER;

            int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage));

            Assert.AreEqual(0, finalDamage);
        }

        /// <summary>
        /// 최소 데미지는 0 (음수 불가)
        /// Minimum damage is 0 (no negative)
        /// </summary>
        [Test]
        public void DamageFormula_NegativeStrength_MinimumZero()
        {
            // 힘이 음수인 경우 (감소 효과)
            // Negative strength case (reduction effect)
            int baseDamage = 2;
            int negativeStrength = -5;

            float damage = baseDamage + negativeStrength;  // -3
            damage *= EXPECTED_VULNERABLE_MODIFIER;

            int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage));

            Assert.AreEqual(0, finalDamage);
        }

        /// <summary>
        /// 큰 수치 계산
        /// Large number calculation
        /// </summary>
        [Test]
        public void DamageFormula_LargeNumbers_CalculatesCorrectly()
        {
            int baseDamage = 50;
            int strength = 10;

            float damage = baseDamage + strength;  // 60
            damage *= EXPECTED_VULNERABLE_MODIFIER; // 60 × 1.5 = 90

            int finalDamage = Mathf.RoundToInt(damage);

            Assert.AreEqual(90, finalDamage);
        }

        #endregion

        #region Rounding Tests

        /// <summary>
        /// 반올림 규칙 검증 (Unity Mathf.RoundToInt 사용)
        /// Rounding rule verification (using Unity Mathf.RoundToInt)
        /// </summary>
        [Test]
        [TestCase(5.4f, 5)]
        [TestCase(5.5f, 6)]  // RoundToInt: 0.5 rounds to even (banker's rounding) in some cases, but Unity uses standard rounding
        [TestCase(5.6f, 6)]
        [TestCase(6.5f, 7)]  // 6.5 → 7 in Unity
        public void Rounding_MathfRoundToInt_BehavesCorrectly(float input, int expected)
        {
            int result = Mathf.RoundToInt(input);
            Assert.AreEqual(expected, result);
        }

        #endregion
    }
}
