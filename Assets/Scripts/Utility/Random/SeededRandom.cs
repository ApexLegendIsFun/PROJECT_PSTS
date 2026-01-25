namespace ProjectSS.Utility
{
    /// <summary>
    /// 시드 기반 난수 생성기 (로그라이크 재현성용)
    /// Seeded random number generator for roguelike reproducibility
    /// </summary>
    public class SeededRandom
    {
        private System.Random _random;
        public int Seed { get; private set; }

        public SeededRandom(int seed)
        {
            Seed = seed;
            _random = new System.Random(seed);
        }

        public SeededRandom() : this(System.Environment.TickCount)
        {
        }

        /// <summary>
        /// 시드 재설정
        /// Reset with new seed
        /// </summary>
        public void Reset(int seed)
        {
            Seed = seed;
            _random = new System.Random(seed);
        }

        /// <summary>
        /// 0 이상 maxExclusive 미만 정수 반환
        /// Returns int from 0 to maxExclusive-1
        /// </summary>
        public int Next(int maxExclusive)
        {
            return _random.Next(maxExclusive);
        }

        /// <summary>
        /// minInclusive 이상 maxExclusive 미만 정수 반환
        /// Returns int from minInclusive to maxExclusive-1
        /// </summary>
        public int Next(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }

        /// <summary>
        /// 0.0 이상 1.0 미만 실수 반환
        /// Returns float from 0.0 to 1.0
        /// </summary>
        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }

        /// <summary>
        /// 확률 체크 (0.0 ~ 1.0)
        /// Check probability (0.0 ~ 1.0)
        /// </summary>
        public bool Chance(float probability)
        {
            return NextFloat() < probability;
        }

        /// <summary>
        /// 내부 System.Random 반환 (확장 메서드용)
        /// Get internal System.Random for extension methods
        /// </summary>
        public System.Random GetSystemRandom() => _random;
    }
}
