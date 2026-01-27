// Editor/Builders/ISceneBuilder.cs
// 씬 빌더 인터페이스 (Scene Builder Interface)

namespace ProjectSS.Editor
{
    /// <summary>
    /// 씬 생성을 위한 빌더 인터페이스
    /// 각 씬 타입별로 구현체를 만들어 사용
    /// </summary>
    public interface ISceneBuilder
    {
        /// <summary>
        /// 씬 이름 (파일명에서 .unity 제외)
        /// </summary>
        string SceneName { get; }

        /// <summary>
        /// 씬 생성 및 저장
        /// </summary>
        /// <param name="scenesFolder">씬 저장 폴더 경로 (예: "Assets/Scenes")</param>
        /// <returns>생성된 씬 파일 경로</returns>
        string Build(string scenesFolder);
    }
}
