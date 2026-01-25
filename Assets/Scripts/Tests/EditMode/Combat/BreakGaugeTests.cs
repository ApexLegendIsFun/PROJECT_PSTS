using NUnit.Framework;
using UnityEngine;
using ProjectSS.Combat;
using ProjectSS.Data;

namespace ProjectSS.Tests.EditMode.Combat
{
    /// <summary>
    /// Break 게이지 유닛 테스트
    /// Break gauge unit tests
    /// </summary>
    [TestFixture]
    public class BreakGaugeTests
    {
        private BreakConditionData CreateDamageThresholdCondition(int threshold, int groggyTurns = 1)
        {
            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.DamageThreshold;
            condition.damageThreshold = threshold;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = true;
            return condition;
        }

        private BreakConditionData CreateHitCountCondition(int hitCount, int groggyTurns = 1)
        {
            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.HitCount;
            condition.hitCountThreshold = hitCount;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = true;
            return condition;
        }

        private BreakConditionData CreateBothCondition(int damageThreshold, int hitCount, int groggyTurns = 1)
        {
            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.Both;
            condition.damageThreshold = damageThreshold;
            condition.hitCountThreshold = hitCount;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = true;
            return condition;
        }

        #region Initialization Tests

        [Test]
        public void Constructor_WithCondition_InitializesCorrectly()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            Assert.AreEqual(0, gauge.CurrentDamage);
            Assert.AreEqual(0, gauge.CurrentHits);
            Assert.IsFalse(gauge.IsBroken);
            Assert.AreEqual(0, gauge.GroggyTurnsLeft);
            Assert.IsFalse(gauge.IsGroggy);
            Assert.IsTrue(gauge.IsBreakable);
        }

        [Test]
        public void Constructor_WithNullCondition_IsNotBreakable()
        {
            var gauge = new BreakGauge(null);

            Assert.IsFalse(gauge.IsBreakable);
        }

        #endregion

        #region Damage Threshold Tests

        [Test]
        public void CheckBreak_DamageThreshold_TriggersAtThreshold()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            bool result = gauge.CheckBreak();

            Assert.IsTrue(result);
            Assert.IsTrue(gauge.IsBroken);
        }

        [Test]
        public void CheckBreak_DamageThreshold_TriggersAboveThreshold()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(25);
            bool result = gauge.CheckBreak();

            Assert.IsTrue(result);
            Assert.IsTrue(gauge.IsBroken);
        }

        [Test]
        public void CheckBreak_DamageThreshold_DoesNotTriggerBelowThreshold()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(19);
            bool result = gauge.CheckBreak();

            Assert.IsFalse(result);
            Assert.IsFalse(gauge.IsBroken);
        }

        [Test]
        public void RecordDamage_AccumulatesCorrectly()
        {
            var condition = CreateDamageThresholdCondition(30);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(10);
            gauge.RecordDamage(5);
            gauge.RecordDamage(8);

            Assert.AreEqual(23, gauge.CurrentDamage);
        }

        #endregion

        #region Hit Count Tests

        [Test]
        public void CheckBreak_HitCount_TriggersAtThreshold()
        {
            var condition = CreateHitCountCondition(5);
            var gauge = new BreakGauge(condition);

            for (int i = 0; i < 5; i++)
            {
                gauge.RecordHit();
            }

            bool result = gauge.CheckBreak();

            Assert.IsTrue(result);
            Assert.IsTrue(gauge.IsBroken);
        }

        [Test]
        public void CheckBreak_HitCount_DoesNotTriggerBelowThreshold()
        {
            var condition = CreateHitCountCondition(5);
            var gauge = new BreakGauge(condition);

            for (int i = 0; i < 4; i++)
            {
                gauge.RecordHit();
            }

            bool result = gauge.CheckBreak();

            Assert.IsFalse(result);
            Assert.IsFalse(gauge.IsBroken);
        }

        [Test]
        public void RecordHit_AccumulatesCorrectly()
        {
            var condition = CreateHitCountCondition(10);
            var gauge = new BreakGauge(condition);

            gauge.RecordHit();
            gauge.RecordHit();
            gauge.RecordHit();

            Assert.AreEqual(3, gauge.CurrentHits);
        }

        #endregion

        #region Both Condition Tests

        [Test]
        public void CheckBreak_Both_TriggersOnDamageThreshold()
        {
            var condition = CreateBothCondition(20, 5);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            bool result = gauge.CheckBreak();

            Assert.IsTrue(result);
        }

        [Test]
        public void CheckBreak_Both_TriggersOnHitCount()
        {
            var condition = CreateBothCondition(20, 5);
            var gauge = new BreakGauge(condition);

            for (int i = 0; i < 5; i++)
            {
                gauge.RecordHit();
            }

            bool result = gauge.CheckBreak();

            Assert.IsTrue(result);
        }

        [Test]
        public void CheckBreak_Both_DoesNotTriggerWhenNeitherMet()
        {
            var condition = CreateBothCondition(20, 5);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(15);
            gauge.RecordHit();
            gauge.RecordHit();

            bool result = gauge.CheckBreak();

            Assert.IsFalse(result);
        }

        #endregion

        #region Combined Record Tests

        [Test]
        public void RecordDamageAndHit_RecordsBoth()
        {
            var condition = CreateBothCondition(30, 5);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamageAndHit(10);
            gauge.RecordDamageAndHit(5);

            Assert.AreEqual(15, gauge.CurrentDamage);
            Assert.AreEqual(2, gauge.CurrentHits);
        }

        #endregion

        #region Groggy State Tests

        [Test]
        public void TriggerBreak_SetsGroggyTurns()
        {
            var condition = CreateDamageThresholdCondition(20, 2);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            Assert.AreEqual(2, gauge.GroggyTurnsLeft);
            Assert.IsTrue(gauge.IsGroggy);
        }

        [Test]
        public void TickGroggy_DecreasesGroggyTurns()
        {
            var condition = CreateDamageThresholdCondition(20, 3);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            gauge.TickGroggy();
            Assert.AreEqual(2, gauge.GroggyTurnsLeft);

            gauge.TickGroggy();
            Assert.AreEqual(1, gauge.GroggyTurnsLeft);

            gauge.TickGroggy();
            Assert.AreEqual(0, gauge.GroggyTurnsLeft);
            Assert.IsFalse(gauge.IsGroggy);
        }

        [Test]
        public void TickGroggy_AtZero_RemainsZero()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.TickGroggy();

            Assert.AreEqual(0, gauge.GroggyTurnsLeft);
        }

        [Test]
        public void IsGroggy_WhenGroggyTurnsGreaterThanZero_ReturnsTrue()
        {
            var condition = CreateDamageThresholdCondition(20, 2);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            Assert.IsTrue(gauge.IsGroggy);
        }

        [Test]
        public void IsGroggy_WhenGroggyTurnsZero_ReturnsFalse()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            Assert.IsFalse(gauge.IsGroggy);
        }

        #endregion

        #region Groggy Immunity Tests

        [Test]
        public void RecordDamage_WhenGroggy_IsIgnored()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            int damageBeforeGroggy = gauge.CurrentDamage;
            gauge.RecordDamage(10);

            Assert.AreEqual(damageBeforeGroggy, gauge.CurrentDamage);
        }

        [Test]
        public void RecordHit_WhenGroggy_IsIgnored()
        {
            var condition = CreateHitCountCondition(3);
            var gauge = new BreakGauge(condition);

            gauge.RecordHit();
            gauge.RecordHit();
            gauge.RecordHit();
            gauge.CheckBreak();

            int hitsBeforeGroggy = gauge.CurrentHits;
            gauge.RecordHit();

            Assert.AreEqual(hitsBeforeGroggy, gauge.CurrentHits);
        }

        [Test]
        public void CheckBreak_WhenGroggy_ReturnsFalse()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            bool result = gauge.CheckBreak();

            Assert.IsFalse(result);
        }

        [Test]
        public void CheckBreak_WhenAlreadyBroken_ReturnsFalse()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            bool result = gauge.CheckBreak();

            Assert.IsFalse(result);
        }

        #endregion

        #region Turn Processing Tests

        [Test]
        public void OnTurnStart_ResetCounters_WhenConfigured()
        {
            var condition = CreateDamageThresholdCondition(20);
            condition.resetOnTurnStart = true;
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(15);
            gauge.RecordHit();
            gauge.RecordHit();

            gauge.OnTurnStart();

            Assert.AreEqual(0, gauge.CurrentDamage);
            Assert.AreEqual(0, gauge.CurrentHits);
        }

        [Test]
        public void OnTurnStart_DoesNotResetCounters_WhenNotConfigured()
        {
            var condition = CreateDamageThresholdCondition(20);
            condition.resetOnTurnStart = false;
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(15);
            gauge.RecordHit();
            gauge.RecordHit();

            gauge.OnTurnStart();

            Assert.AreEqual(15, gauge.CurrentDamage);
            Assert.AreEqual(2, gauge.CurrentHits);
        }

        [Test]
        public void OnTurnStart_ResetsBrokenState_WhenGroggyEnds()
        {
            var condition = CreateDamageThresholdCondition(20, 1);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();
            gauge.TickGroggy();

            Assert.IsFalse(gauge.IsGroggy);
            Assert.IsTrue(gauge.IsBroken);

            gauge.OnTurnStart();

            Assert.IsFalse(gauge.IsBroken);
        }

        #endregion

        #region Reset Tests

        [Test]
        public void ResetTurnCounters_ResetsOnlyCounters()
        {
            var condition = CreateDamageThresholdCondition(20, 2);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            gauge.ResetTurnCounters();

            Assert.AreEqual(0, gauge.CurrentDamage);
            Assert.AreEqual(0, gauge.CurrentHits);
            Assert.IsTrue(gauge.IsBroken);
            Assert.AreEqual(2, gauge.GroggyTurnsLeft);
        }

        [Test]
        public void Reset_ResetsEverything()
        {
            var condition = CreateDamageThresholdCondition(20, 2);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);
            gauge.CheckBreak();

            gauge.Reset();

            Assert.AreEqual(0, gauge.CurrentDamage);
            Assert.AreEqual(0, gauge.CurrentHits);
            Assert.IsFalse(gauge.IsBroken);
            Assert.AreEqual(0, gauge.GroggyTurnsLeft);
        }

        #endregion

        #region Progress Tests

        [Test]
        public void GetDamageProgress_ReturnsCorrectProgress()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(10);

            Assert.AreEqual(0.5f, gauge.GetDamageProgress(), 0.001f);
        }

        [Test]
        public void GetDamageProgress_AtZero_ReturnsZero()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            Assert.AreEqual(0f, gauge.GetDamageProgress());
        }

        [Test]
        public void GetDamageProgress_AtMax_ReturnsOne()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(20);

            Assert.AreEqual(1f, gauge.GetDamageProgress(), 0.001f);
        }

        [Test]
        public void GetHitProgress_ReturnsCorrectProgress()
        {
            var condition = CreateHitCountCondition(5);
            var gauge = new BreakGauge(condition);

            gauge.RecordHit();
            gauge.RecordHit();

            Assert.AreEqual(0.4f, gauge.GetHitProgress(), 0.001f);
        }

        [Test]
        public void GetPrimaryProgress_DamageThreshold_ReturnsDamageProgress()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(10);

            Assert.AreEqual(0.5f, gauge.GetPrimaryProgress(), 0.001f);
        }

        [Test]
        public void GetPrimaryProgress_HitCount_ReturnsHitProgress()
        {
            var condition = CreateHitCountCondition(4);
            var gauge = new BreakGauge(condition);

            gauge.RecordHit();
            gauge.RecordHit();

            Assert.AreEqual(0.5f, gauge.GetPrimaryProgress(), 0.001f);
        }

        [Test]
        public void GetPrimaryProgress_Both_ReturnsMaxProgress()
        {
            var condition = CreateBothCondition(20, 4);
            var gauge = new BreakGauge(condition);

            gauge.RecordDamage(5);  // 25% damage
            gauge.RecordHit();
            gauge.RecordHit();
            gauge.RecordHit();      // 75% hits

            Assert.AreEqual(0.75f, gauge.GetPrimaryProgress(), 0.001f);
        }

        #endregion

        #region Null Condition Tests

        [Test]
        public void RecordDamage_NullCondition_DoesNothing()
        {
            var gauge = new BreakGauge(null);

            gauge.RecordDamage(10);

            Assert.AreEqual(0, gauge.CurrentDamage);
        }

        [Test]
        public void CheckBreak_NullCondition_ReturnsFalse()
        {
            var gauge = new BreakGauge(null);

            bool result = gauge.CheckBreak();

            Assert.IsFalse(result);
        }

        [Test]
        public void GetProgress_NullCondition_ReturnsZero()
        {
            var gauge = new BreakGauge(null);

            Assert.AreEqual(0f, gauge.GetDamageProgress());
            Assert.AreEqual(0f, gauge.GetHitProgress());
            Assert.AreEqual(0f, gauge.GetPrimaryProgress());
        }

        #endregion

        #region GetConditionData Tests

        [Test]
        public void GetConditionData_ReturnsCondition()
        {
            var condition = CreateDamageThresholdCondition(20);
            var gauge = new BreakGauge(condition);

            var result = gauge.GetConditionData();

            Assert.AreSame(condition, result);
        }

        [Test]
        public void GetConditionData_NullCondition_ReturnsNull()
        {
            var gauge = new BreakGauge(null);

            var result = gauge.GetConditionData();

            Assert.IsNull(result);
        }

        #endregion
    }
}
