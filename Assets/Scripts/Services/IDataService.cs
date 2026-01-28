// Services/IDataService.cs
// 데이터 서비스 인터페이스
// 중앙집중 데이터 액세스 레이어 정의

using ProjectSS.Core.Config;
using ProjectSS.Data;

namespace ProjectSS.Services
{
    /// <summary>
    /// 데이터 서비스 인터페이스
    /// 모든 게임 데이터에 대한 통합 액세스 제공
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// 게임 밸런스 설정
        /// </summary>
        GameBalanceConfigSO Balance { get; }

        /// <summary>
        /// 캐릭터 데이터베이스
        /// </summary>
        CharacterDatabase Characters { get; }

        /// <summary>
        /// 데이터 서비스 초기화 여부
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 데이터 서비스 초기화
        /// </summary>
        void Initialize();

        /// <summary>
        /// 데이터 서비스 정리
        /// </summary>
        void Cleanup();
    }
}
