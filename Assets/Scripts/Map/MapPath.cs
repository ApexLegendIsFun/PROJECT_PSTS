using UnityEngine;

namespace ProjectSS.Map
{
    /// <summary>
    /// 맵 경로 (노드 간 연결선)
    /// Map path (connection between nodes)
    /// </summary>
    [System.Serializable]
    public class MapPath
    {
        public string FromNodeId { get; private set; }
        public string ToNodeId { get; private set; }
        public Vector2 StartPosition { get; private set; }
        public Vector2 EndPosition { get; private set; }

        public MapPath(string from, string to, Vector2 start, Vector2 end)
        {
            FromNodeId = from;
            ToNodeId = to;
            StartPosition = start;
            EndPosition = end;
        }
    }
}
