using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data
{
    /// <summary>
    /// 미리 정의된 맵 레이아웃 데이터
    /// Predefined map layout data
    /// </summary>
    [CreateAssetMenu(fileName = "NewMapLayout", menuName = "Game/Map/Map Layout")]
    public class MapLayoutData : ScriptableObject
    {
        [Header("레이아웃 정보 (Layout Info)")]
        [Tooltip("레이아웃 ID / Layout ID")]
        public string layoutId;

        [Tooltip("레이아웃 이름 / Layout name")]
        public string layoutName;

        [Tooltip("맵 타입 / Map type")]
        public MapType mapType;

        [Header("맵 구조 (Map Structure)")]
        [Tooltip("층 레이아웃 목록 / Floor layout list")]
        public List<FloorLayoutData> floors = new List<FloorLayoutData>();

        /// <summary>
        /// 총 층 수 반환
        /// Get total floor count
        /// </summary>
        public int FloorCount => floors.Count;

        /// <summary>
        /// 특정 층의 레이아웃 반환
        /// Get floor layout at index
        /// </summary>
        public FloorLayoutData GetFloor(int index)
        {
            if (index >= 0 && index < floors.Count)
                return floors[index];
            return null;
        }

        private void OnValidate()
        {
            // 층 인덱스 자동 설정
            for (int i = 0; i < floors.Count; i++)
            {
                floors[i].floorIndex = i;
            }
        }
    }

    /// <summary>
    /// 층 레이아웃 데이터
    /// Floor layout data
    /// </summary>
    [System.Serializable]
    public class FloorLayoutData
    {
        [Tooltip("층 인덱스 (자동 설정) / Floor index (auto-set)")]
        public int floorIndex;

        [Tooltip("노드 레이아웃 목록 / Node layout list")]
        public List<NodeLayoutData> nodes = new List<NodeLayoutData>();

        /// <summary>
        /// 노드 수 반환
        /// Get node count
        /// </summary>
        public int NodeCount => nodes.Count;
    }

    /// <summary>
    /// 노드 레이아웃 데이터
    /// Node layout data
    /// </summary>
    [System.Serializable]
    public class NodeLayoutData
    {
        [Tooltip("열 인덱스 / Column index")]
        public int column;

        [Tooltip("위치 오프셋 / Position offset")]
        public Vector2 positionOffset;

        [Tooltip("노드 타입 / Node type")]
        public MapNodeType nodeType;

        [Tooltip("연결할 다음 층 노드의 열 인덱스들 / Connected next floor node columns")]
        public List<int> connectsToColumns = new List<int>();

        /// <summary>
        /// 노드 ID 생성
        /// Generate node ID
        /// </summary>
        public string GetNodeId(int floorIndex)
        {
            return $"node_{floorIndex}_{column}";
        }
    }
}
