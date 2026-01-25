namespace ProjectSS.Data
{
    /// <summary>
    /// 개별 캐릭터 상태 (Warrior, Mage, Rogue 각각)
    /// Individual character state for each party member
    /// </summary>
    [System.Serializable]
    public class CharacterState
    {
        /// <summary>
        /// 캐릭터 클래스
        /// Character class
        /// </summary>
        public CharacterClass characterClass;

        /// <summary>
        /// 현재 체력
        /// Current HP
        /// </summary>
        public int currentHP;

        /// <summary>
        /// 최대 체력
        /// Maximum HP
        /// </summary>
        public int maxHP;

        /// <summary>
        /// Focus 스택 (전투 중에만 사용)
        /// Focus stacks (used during combat only)
        /// </summary>
        public int focusStacks;

        /// <summary>
        /// 기절 상태 여부 (전투 중)
        /// Is incapacitated (during combat)
        /// </summary>
        public bool isIncapacitated;

        /// <summary>
        /// 현재 위치 (전열/후열)
        /// Current position (Active/Standby)
        /// </summary>
        public CharacterPosition position;

        /// <summary>
        /// 기본 생성자
        /// Default constructor
        /// </summary>
        public CharacterState()
        {
            characterClass = CharacterClass.Warrior;
            currentHP = 75;
            maxHP = 75;
            focusStacks = 0;
            isIncapacitated = false;
            position = CharacterPosition.Standby;
        }

        /// <summary>
        /// 클래스 데이터로 초기화
        /// Initialize from class data
        /// </summary>
        public CharacterState(CharacterClassData classData, CharacterPosition initialPosition = CharacterPosition.Standby)
        {
            characterClass = classData.classType;
            currentHP = classData.baseMaxHP;
            maxHP = classData.baseMaxHP;
            focusStacks = 0;
            isIncapacitated = false;
            position = initialPosition;
        }

        /// <summary>
        /// 전열 여부 확인
        /// Check if Active (front position)
        /// </summary>
        public bool IsActive => position == CharacterPosition.Active;

        /// <summary>
        /// 후열 여부 확인
        /// Check if Standby (back position)
        /// </summary>
        public bool IsStandby => position == CharacterPosition.Standby;

        /// <summary>
        /// 생존 여부 확인
        /// Check if alive
        /// </summary>
        public bool IsAlive => currentHP > 0;

        /// <summary>
        /// 행동 가능 여부 확인
        /// Check if can act (alive and not incapacitated)
        /// </summary>
        public bool CanAct => IsAlive && !isIncapacitated;

        /// <summary>
        /// 데미지 적용
        /// Apply damage
        /// </summary>
        public void TakeDamage(int amount)
        {
            currentHP = System.Math.Max(0, currentHP - amount);
            if (currentHP <= 0)
            {
                isIncapacitated = true;
            }
        }

        /// <summary>
        /// 체력 회복
        /// Heal HP
        /// </summary>
        public void Heal(int amount)
        {
            currentHP = System.Math.Min(maxHP, currentHP + amount);
        }

        /// <summary>
        /// 전투 종료 시 완전 회복
        /// Full recovery after combat
        /// </summary>
        public void FullRecovery()
        {
            currentHP = maxHP;
            isIncapacitated = false;
            focusStacks = 0;
        }

        /// <summary>
        /// 전투 상태 리셋 (Focus, 기절 초기화)
        /// Reset combat state (Focus, incapacitation)
        /// </summary>
        public void ResetCombatState()
        {
            focusStacks = 0;
            // isIncapacitated는 FullRecovery에서만 해제
        }

        /// <summary>
        /// 복제본 생성
        /// Create copy
        /// </summary>
        public CharacterState Clone()
        {
            return new CharacterState
            {
                characterClass = characterClass,
                currentHP = currentHP,
                maxHP = maxHP,
                focusStacks = focusStacks,
                isIncapacitated = isIncapacitated,
                position = position
            };
        }
    }
}
