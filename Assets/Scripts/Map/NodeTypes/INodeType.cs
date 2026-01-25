namespace ProjectSS.Map
{
    /// <summary>
    /// 맵 노드 타입 인터페이스
    /// Map node type interface
    /// </summary>
    public interface INodeType
    {
        Core.MapNodeType NodeType { get; }
        void OnNodeEnter();
        void OnNodeComplete();
    }
}
