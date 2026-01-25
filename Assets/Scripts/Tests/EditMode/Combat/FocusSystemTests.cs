using NUnit.Framework;
using ProjectSS.Combat;

namespace ProjectSS.Tests.EditMode.Combat
{
    /// <summary>
    /// Focus 시스템 유닛 테스트
    /// Focus system unit tests
    /// </summary>
    [TestFixture]
    public class FocusSystemTests
    {
        #region Constant Tests

        [Test]
        public void MaxFocus_Is3()
        {
            Assert.AreEqual(3, FocusSystem.MAX_FOCUS);
        }

        [Test]
        public void FocusPerTurn_Is1()
        {
            Assert.AreEqual(1, FocusSystem.FOCUS_PER_TURN);
        }

        [Test]
        public void FreeTagInThreshold_Is3()
        {
            Assert.AreEqual(3, FocusSystem.FREE_TAGIN_THRESHOLD);
        }

        [Test]
        public void DamageBonusPerFocus_Is25Percent()
        {
            Assert.AreEqual(0.25f, FocusSystem.DAMAGE_BONUS_PER_FOCUS);
        }

        [Test]
        public void BlockBonusPerFocus_Is25Percent()
        {
            Assert.AreEqual(0.25f, FocusSystem.BLOCK_BONUS_PER_FOCUS);
        }

        #endregion

        #region Damage Multiplier Tests

        [Test]
        [TestCase(0, 1.0f)]
        [TestCase(1, 1.25f)]
        [TestCase(2, 1.5f)]
        [TestCase(3, 1.75f)]
        public void GetDamageMultiplier_ReturnsCorrectValue(int focusStacks, float expected)
        {
            float result = FocusSystem.GetDamageMultiplier(focusStacks);
            Assert.AreEqual(expected, result, 0.001f);
        }

        [Test]
        public void GetDamageMultiplier_ZeroFocus_ReturnsOne()
        {
            float result = FocusSystem.GetDamageMultiplier(0);
            Assert.AreEqual(1.0f, result);
        }

        [Test]
        public void GetDamageMultiplier_MaxFocus_Returns175Percent()
        {
            float result = FocusSystem.GetDamageMultiplier(FocusSystem.MAX_FOCUS);
            Assert.AreEqual(1.75f, result, 0.001f);
        }

        #endregion

        #region Block Multiplier Tests

        [Test]
        [TestCase(0, 1.0f)]
        [TestCase(1, 1.25f)]
        [TestCase(2, 1.5f)]
        [TestCase(3, 1.75f)]
        public void GetBlockMultiplier_ReturnsCorrectValue(int focusStacks, float expected)
        {
            float result = FocusSystem.GetBlockMultiplier(focusStacks);
            Assert.AreEqual(expected, result, 0.001f);
        }

        #endregion

        #region Focus Damage Bonus Tests

        [Test]
        [TestCase(10, 0, 10)]
        [TestCase(10, 1, 13)]  // 10 * 1.25 = 12.5 → rounded to 13
        [TestCase(10, 2, 15)]  // 10 * 1.5 = 15
        [TestCase(10, 3, 18)]  // 10 * 1.75 = 17.5 → rounded to 18
        public void ApplyFocusDamageBonus_ReturnsCorrectValue(int baseDamage, int focusStacks, int expected)
        {
            int result = FocusSystem.ApplyFocusDamageBonus(baseDamage, focusStacks);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ApplyFocusDamageBonus_ZeroBaseDamage_ReturnsZero()
        {
            int result = FocusSystem.ApplyFocusDamageBonus(0, 3);
            Assert.AreEqual(0, result);
        }

        #endregion

        #region Focus Block Bonus Tests

        [Test]
        [TestCase(5, 0, 5)]
        [TestCase(5, 1, 6)]   // 5 * 1.25 = 6.25 → rounded to 6
        [TestCase(5, 2, 8)]   // 5 * 1.5 = 7.5 → rounded to 8
        [TestCase(5, 3, 9)]   // 5 * 1.75 = 8.75 → rounded to 9
        public void ApplyFocusBlockBonus_ReturnsCorrectValue(int baseBlock, int focusStacks, int expected)
        {
            int result = FocusSystem.ApplyFocusBlockBonus(baseBlock, focusStacks);
            Assert.AreEqual(expected, result);
        }

        #endregion

        #region Tag-In Cost Tests

        [Test]
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, 1)]
        [TestCase(3, 0)]
        public void GetTagInCost_ReturnsCorrectValue(int focusStacks, int expected)
        {
            int result = FocusSystem.GetTagInCost(focusStacks);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetTagInCost_BelowThreshold_ReturnsOne()
        {
            int result = FocusSystem.GetTagInCost(FocusSystem.FREE_TAGIN_THRESHOLD - 1);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void GetTagInCost_AtThreshold_ReturnsZero()
        {
            int result = FocusSystem.GetTagInCost(FocusSystem.FREE_TAGIN_THRESHOLD);
            Assert.AreEqual(0, result);
        }

        #endregion

        #region Focus Gain Tests

        [Test]
        [TestCase(0, 1, 1)]
        [TestCase(1, 1, 2)]
        [TestCase(2, 1, 3)]
        [TestCase(3, 1, 3)]  // Capped at max
        public void GainFocus_ReturnsCorrectValue(int currentFocus, int amount, int expected)
        {
            int result = FocusSystem.GainFocus(currentFocus, amount);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GainFocus_ExceedsMax_CappedAtMax()
        {
            int result = FocusSystem.GainFocus(2, 5);
            Assert.AreEqual(FocusSystem.MAX_FOCUS, result);
        }

        [Test]
        public void GainFocus_AlreadyAtMax_RemainsAtMax()
        {
            int result = FocusSystem.GainFocus(FocusSystem.MAX_FOCUS, 1);
            Assert.AreEqual(FocusSystem.MAX_FOCUS, result);
        }

        [Test]
        public void GainFocus_ZeroAmount_NoChange()
        {
            int result = FocusSystem.GainFocus(2, 0);
            Assert.AreEqual(2, result);
        }

        #endregion

        #region Max Focus Check Tests

        [Test]
        public void IsMaxFocus_AtMax_ReturnsTrue()
        {
            bool result = FocusSystem.IsMaxFocus(FocusSystem.MAX_FOCUS);
            Assert.IsTrue(result);
        }

        [Test]
        public void IsMaxFocus_BelowMax_ReturnsFalse()
        {
            bool result = FocusSystem.IsMaxFocus(FocusSystem.MAX_FOCUS - 1);
            Assert.IsFalse(result);
        }

        [Test]
        public void IsMaxFocus_Zero_ReturnsFalse()
        {
            bool result = FocusSystem.IsMaxFocus(0);
            Assert.IsFalse(result);
        }

        #endregion

        #region Free Tag-In Tests

        [Test]
        public void CanFreeTagIn_AtThreshold_ReturnsTrue()
        {
            bool result = FocusSystem.CanFreeTagIn(FocusSystem.FREE_TAGIN_THRESHOLD);
            Assert.IsTrue(result);
        }

        [Test]
        public void CanFreeTagIn_BelowThreshold_ReturnsFalse()
        {
            bool result = FocusSystem.CanFreeTagIn(FocusSystem.FREE_TAGIN_THRESHOLD - 1);
            Assert.IsFalse(result);
        }

        [Test]
        public void CanFreeTagIn_Zero_ReturnsFalse()
        {
            bool result = FocusSystem.CanFreeTagIn(0);
            Assert.IsFalse(result);
        }

        #endregion
    }
}
