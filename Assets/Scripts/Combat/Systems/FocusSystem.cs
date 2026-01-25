namespace ProjectSS.Combat
{
    /// <summary>
    /// Focus 시스템 유틸리티
    /// Focus system utility class
    ///
    /// Focus는 후열(Standby) 캐릭터가 턴마다 축적하는 자원입니다.
    /// Focus is a resource that Standby characters accumulate each turn.
    /// - 최대 3 스택
    /// - 스킬 데미지/방어력 25% 증가 (스택당)
    /// - Tag-In 시 전부 소모
    /// </summary>
    public static class FocusSystem
    {
        /// <summary>
        /// 최대 Focus 스택
        /// Maximum Focus stacks
        /// </summary>
        public const int MAX_FOCUS = 3;

        /// <summary>
        /// 턴당 Focus 획득량
        /// Focus gained per turn
        /// </summary>
        public const int FOCUS_PER_TURN = 1;

        /// <summary>
        /// 무료 Tag-In 임계값 (Focus가 이 값 이상이면 에너지 0 소모)
        /// Free Tag-In threshold (0 energy cost if Focus >= this value)
        /// </summary>
        public const int FREE_TAGIN_THRESHOLD = 3;

        /// <summary>
        /// Focus당 데미지 보너스 배율
        /// Damage bonus multiplier per Focus stack
        /// </summary>
        public const float DAMAGE_BONUS_PER_FOCUS = 0.25f;

        /// <summary>
        /// Focus당 방어력 보너스 배율
        /// Block bonus multiplier per Focus stack
        /// </summary>
        public const float BLOCK_BONUS_PER_FOCUS = 0.25f;

        /// <summary>
        /// Focus 스택에 따른 데미지 배율 계산
        /// Calculate damage multiplier based on Focus stacks
        /// </summary>
        /// <param name="focusStacks">현재 Focus 스택 / Current Focus stacks</param>
        /// <returns>데미지 배율 (1.0 = 100%) / Damage multiplier (1.0 = 100%)</returns>
        public static float GetDamageMultiplier(int focusStacks)
        {
            return 1f + (focusStacks * DAMAGE_BONUS_PER_FOCUS);
        }

        /// <summary>
        /// Focus 스택에 따른 방어력 배율 계산
        /// Calculate block multiplier based on Focus stacks
        /// </summary>
        /// <param name="focusStacks">현재 Focus 스택 / Current Focus stacks</param>
        /// <returns>방어력 배율 (1.0 = 100%) / Block multiplier (1.0 = 100%)</returns>
        public static float GetBlockMultiplier(int focusStacks)
        {
            return 1f + (focusStacks * BLOCK_BONUS_PER_FOCUS);
        }

        /// <summary>
        /// Focus 보너스가 적용된 데미지 계산
        /// Calculate damage with Focus bonus applied
        /// </summary>
        /// <param name="baseDamage">기본 데미지 / Base damage</param>
        /// <param name="focusStacks">현재 Focus 스택 / Current Focus stacks</param>
        /// <returns>보너스 적용된 데미지 / Damage with bonus applied</returns>
        public static int ApplyFocusDamageBonus(int baseDamage, int focusStacks)
        {
            float multiplier = GetDamageMultiplier(focusStacks);
            return UnityEngine.Mathf.RoundToInt(baseDamage * multiplier);
        }

        /// <summary>
        /// Focus 보너스가 적용된 방어력 계산
        /// Calculate block with Focus bonus applied
        /// </summary>
        /// <param name="baseBlock">기본 방어력 / Base block</param>
        /// <param name="focusStacks">현재 Focus 스택 / Current Focus stacks</param>
        /// <returns>보너스 적용된 방어력 / Block with bonus applied</returns>
        public static int ApplyFocusBlockBonus(int baseBlock, int focusStacks)
        {
            float multiplier = GetBlockMultiplier(focusStacks);
            return UnityEngine.Mathf.RoundToInt(baseBlock * multiplier);
        }

        /// <summary>
        /// Tag-In 비용 계산
        /// Calculate Tag-In cost
        /// </summary>
        /// <param name="focusStacks">들어오는 캐릭터의 Focus 스택 / Incoming character's Focus stacks</param>
        /// <returns>에너지 비용 (0 또는 1) / Energy cost (0 or 1)</returns>
        public static int GetTagInCost(int focusStacks)
        {
            return focusStacks >= FREE_TAGIN_THRESHOLD ? 0 : 1;
        }

        /// <summary>
        /// Focus 획득 (최대치 제한)
        /// Gain Focus (capped at maximum)
        /// </summary>
        /// <param name="currentFocus">현재 Focus / Current Focus</param>
        /// <param name="amount">획득량 / Amount to gain</param>
        /// <returns>새로운 Focus 값 / New Focus value</returns>
        public static int GainFocus(int currentFocus, int amount)
        {
            return UnityEngine.Mathf.Min(currentFocus + amount, MAX_FOCUS);
        }

        /// <summary>
        /// Focus가 최대인지 확인
        /// Check if Focus is at maximum
        /// </summary>
        /// <param name="focusStacks">현재 Focus 스택 / Current Focus stacks</param>
        /// <returns>최대 여부 / Whether at maximum</returns>
        public static bool IsMaxFocus(int focusStacks)
        {
            return focusStacks >= MAX_FOCUS;
        }

        /// <summary>
        /// 무료 Tag-In 가능 여부 확인
        /// Check if free Tag-In is available
        /// </summary>
        /// <param name="focusStacks">Focus 스택 / Focus stacks</param>
        /// <returns>무료 Tag-In 가능 여부 / Whether free Tag-In is available</returns>
        public static bool CanFreeTagIn(int focusStacks)
        {
            return focusStacks >= FREE_TAGIN_THRESHOLD;
        }
    }
}
