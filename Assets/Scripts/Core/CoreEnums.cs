namespace ProjectSS.Core
{
    /// <summary>
    /// 맵 타입 (허브 시스템)
    /// Map type (hub system)
    /// </summary>
    public enum MapType
    {
        Town,       // 마을 (허브) - 상점, 휴식, 이벤트
        Field,      // 필드 - 일반전투, 이벤트, 보물
        Dungeon     // 던전 - 전투, 엘리트, 보스
    }

    /// <summary>
    /// 맵 노드 타입 (개별 노드 유형)
    /// Map node type (individual node types)
    /// </summary>
    public enum MapNodeType
    {
        Combat,     // 일반 전투
        Elite,      // 엘리트 전투
        Boss,       // 보스 전투
        Rest,       // 휴식
        Event,      // 이벤트
        Shop,       // 상점
        Treasure    // 보물
    }
}
