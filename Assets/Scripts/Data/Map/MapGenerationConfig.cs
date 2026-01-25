using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data
{
    /// <summary>
    /// 맵 생성 설정 ScriptableObject
    /// Map generation configuration ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MapConfig", menuName = "Game/Map/Generation Config")]
    public class MapGenerationConfig : ScriptableObject
    {
        [Header("생성 모드 (Generation Mode)")]
        [Tooltip("미리 정의된 레이아웃 사용 / Use predefined layout")]
        public bool usePredefinedLayout = false;

        [Tooltip("미리 정의된 맵 레이아웃 / Predefined map layout")]
        public MapLayoutData predefinedLayout;

        [Header("맵 타입 (Map Type)")]
        [Tooltip("맵 타입 / Map type")]
        public MapType mapType = MapType.Field;

        [Header("마을 설정 (Town Settings)")]
        [Tooltip("마을 노드 수 / Town node count")]
        [Range(2, 5)]
        public int townNodeCount = 3;

        [Tooltip("상점 가중치 / Shop weight")]
        [Range(0f, 1f)]
        public float townShopWeight = 0.4f;

        [Tooltip("휴식 가중치 / Rest weight")]
        [Range(0f, 1f)]
        public float townRestWeight = 0.4f;

        [Tooltip("이벤트 가중치 / Event weight")]
        [Range(0f, 1f)]
        public float townEventWeight = 0.2f;

        [Header("필드 설정 (Field Settings)")]
        [Tooltip("필드 층 수 / Field floor count")]
        [Range(5, 15)]
        public int fieldFloors = 8;

        [Tooltip("층당 최소 노드 수 / Min nodes per floor")]
        [Range(1, 4)]
        public int fieldMinNodesPerFloor = 2;

        [Tooltip("층당 최대 노드 수 / Max nodes per floor")]
        [Range(2, 5)]
        public int fieldMaxNodesPerFloor = 3;

        [Tooltip("전투 가중치 / Combat weight")]
        [Range(0f, 1f)]
        public float fieldCombatWeight = 0.5f;

        [Tooltip("이벤트 가중치 / Event weight")]
        [Range(0f, 1f)]
        public float fieldEventWeight = 0.3f;

        [Tooltip("보물 가중치 / Treasure weight")]
        [Range(0f, 1f)]
        public float fieldTreasureWeight = 0.2f;

        [Header("던전 설정 (Dungeon Settings)")]
        [Tooltip("던전 층 수 / Dungeon floor count")]
        [Range(3, 10)]
        public int dungeonFloors = 5;

        [Tooltip("층당 최소 노드 수 / Min nodes per floor")]
        [Range(1, 3)]
        public int dungeonMinNodesPerFloor = 1;

        [Tooltip("층당 최대 노드 수 / Max nodes per floor")]
        [Range(1, 4)]
        public int dungeonMaxNodesPerFloor = 2;

        [Tooltip("전투 가중치 / Combat weight")]
        [Range(0f, 1f)]
        public float dungeonCombatWeight = 0.5f;

        [Tooltip("엘리트 가중치 / Elite weight")]
        [Range(0f, 1f)]
        public float dungeonEliteWeight = 0.5f;

        /// <summary>
        /// 현재 맵 타입에 따른 층 수 반환
        /// Get floor count based on current map type
        /// </summary>
        public int GetFloorCount()
        {
            return mapType switch
            {
                MapType.Town => 1,
                MapType.Field => fieldFloors,
                MapType.Dungeon => dungeonFloors,
                _ => fieldFloors
            };
        }

        /// <summary>
        /// 현재 맵 타입에 따른 최소 노드 수 반환
        /// Get min nodes per floor based on current map type
        /// </summary>
        public int GetMinNodesPerFloor()
        {
            return mapType switch
            {
                MapType.Town => townNodeCount,
                MapType.Field => fieldMinNodesPerFloor,
                MapType.Dungeon => dungeonMinNodesPerFloor,
                _ => 2
            };
        }

        /// <summary>
        /// 현재 맵 타입에 따른 최대 노드 수 반환
        /// Get max nodes per floor based on current map type
        /// </summary>
        public int GetMaxNodesPerFloor()
        {
            return mapType switch
            {
                MapType.Town => townNodeCount,
                MapType.Field => fieldMaxNodesPerFloor,
                MapType.Dungeon => dungeonMaxNodesPerFloor,
                _ => 4
            };
        }

        private void OnValidate()
        {
            // 최대가 최소보다 크도록 보장
            if (fieldMaxNodesPerFloor < fieldMinNodesPerFloor)
                fieldMaxNodesPerFloor = fieldMinNodesPerFloor;
            if (dungeonMaxNodesPerFloor < dungeonMinNodesPerFloor)
                dungeonMaxNodesPerFloor = dungeonMinNodesPerFloor;
        }
    }
}
