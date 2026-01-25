using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 맵 생성 설정 ScriptableObject
    /// Map generation configuration ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MapConfig", menuName = "Game/Map/Generation Config")]
    public class MapGenerationConfig : ScriptableObject
    {
        [Header("맵 구조 (Map Structure)")]
        [Tooltip("총 층 수 / Total number of floors")]
        [Range(10, 25)]
        public int numberOfFloors = 15;

        [Tooltip("층당 최소 노드 수 / Min nodes per floor")]
        [Range(1, 5)]
        public int minNodesPerFloor = 2;

        [Tooltip("층당 최대 노드 수 / Max nodes per floor")]
        [Range(2, 6)]
        public int maxNodesPerFloor = 4;

        [Header("노드 분포 (Node Distribution)")]
        [Tooltip("일반 전투 가중치 / Combat node weight")]
        [Range(0f, 1f)]
        public float combatNodeWeight = 0.45f;

        [Tooltip("이벤트 가중치 / Event node weight")]
        [Range(0f, 1f)]
        public float eventNodeWeight = 0.22f;

        [Tooltip("엘리트 가중치 / Elite node weight")]
        [Range(0f, 1f)]
        public float eliteNodeWeight = 0.08f;

        [Tooltip("휴식 가중치 / Rest node weight")]
        [Range(0f, 1f)]
        public float restNodeWeight = 0.12f;

        [Tooltip("상점 가중치 / Shop node weight")]
        [Range(0f, 1f)]
        public float shopNodeWeight = 0.08f;

        [Tooltip("보물 가중치 / Treasure node weight")]
        [Range(0f, 1f)]
        public float treasureNodeWeight = 0.05f;

        [Header("특수 규칙 (Special Rules)")]
        [Tooltip("보스 층 (마지막 층) / Boss floor (last floor)")]
        public int bossFloor = 15;

        [Tooltip("엘리트 등장 최소 층 / Min floor for elite")]
        [Range(1, 10)]
        public int minFloorsBeforeElite = 5;

        [Tooltip("휴식 보장 층 / Guaranteed rest floor")]
        [Range(5, 12)]
        public int restGuaranteeFloor = 8;

        private void OnValidate()
        {
            // 최대가 최소보다 크도록 보장
            if (maxNodesPerFloor < minNodesPerFloor)
                maxNodesPerFloor = minNodesPerFloor;

            // 보스 층이 총 층 수와 일치하도록
            bossFloor = numberOfFloors;
        }
    }
}
