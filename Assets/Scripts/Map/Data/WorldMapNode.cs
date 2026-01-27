// Map/Data/WorldMapNode.cs
// 월드맵 노드 데이터

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Map.Data
{
    /// <summary>
    /// 월드맵 노드 - 단일 지역 데이터
    /// </summary>
    [System.Serializable]
    public class WorldMapNode
    {
        [Header("기본 정보")]
        [Tooltip("고유 식별자")]
        public string NodeId;

        [Tooltip("지역 타입")]
        public RegionType RegionType;

        [Tooltip("표시 이름 (예: 어둠의 숲)")]
        public string DisplayName;

        [Tooltip("맵상 위치 (UI 렌더링용)")]
        public Vector2 Position;

        [Header("연결 정보")]
        [Tooltip("연결된 노드 ID 목록")]
        public List<string> ConnectedNodeIds = new List<string>();

        [Header("던전 전용")]
        [Tooltip("전투 인카운터 ID (던전/보스 타입일 때 사용)")]
        public string EncounterId;

        [Tooltip("던전 난이도 (1-10)")]
        [Range(1, 10)]
        public int Difficulty = 1;

        // 런타임 상태 (직렬화 제외)
        [System.NonSerialized] private bool _isVisited;
        [System.NonSerialized] private bool _isCleared;
        [System.NonSerialized] private bool _isAccessible;

        #region Properties

        /// <summary>
        /// 방문 여부
        /// </summary>
        public bool IsVisited
        {
            get => _isVisited;
            set => _isVisited = value;
        }

        /// <summary>
        /// 클리어 여부
        /// </summary>
        public bool IsCleared
        {
            get => _isCleared;
            set => _isCleared = value;
        }

        /// <summary>
        /// 접근 가능 여부
        /// </summary>
        public bool IsAccessible
        {
            get => _isAccessible;
            set => _isAccessible = value;
        }

        /// <summary>
        /// 전투 노드인지 확인
        /// </summary>
        public bool IsCombatNode => RegionType == RegionType.Dungeon || RegionType == RegionType.Boss;

        /// <summary>
        /// 시작/종료 노드인지 확인
        /// </summary>
        public bool IsTerminalNode => RegionType == RegionType.Start || RegionType == RegionType.End;

        #endregion

        #region Methods

        /// <summary>
        /// 노드 방문 처리
        /// </summary>
        public void Visit()
        {
            _isVisited = true;
        }

        /// <summary>
        /// 노드 클리어 처리
        /// </summary>
        public void Clear()
        {
            _isCleared = true;
        }

        /// <summary>
        /// 런타임 상태 초기화
        /// </summary>
        public void ResetRuntimeState()
        {
            _isVisited = false;
            _isCleared = false;
            _isAccessible = RegionType == RegionType.Start;
        }

        /// <summary>
        /// 특정 노드와 연결되어 있는지 확인
        /// </summary>
        public bool IsConnectedTo(string nodeId)
        {
            return ConnectedNodeIds != null && ConnectedNodeIds.Contains(nodeId);
        }

        /// <summary>
        /// 연결 추가
        /// </summary>
        public void AddConnection(string nodeId)
        {
            if (ConnectedNodeIds == null)
                ConnectedNodeIds = new List<string>();

            if (!ConnectedNodeIds.Contains(nodeId))
                ConnectedNodeIds.Add(nodeId);
        }

        /// <summary>
        /// 연결 제거
        /// </summary>
        public void RemoveConnection(string nodeId)
        {
            ConnectedNodeIds?.Remove(nodeId);
        }

        #endregion
    }
}
