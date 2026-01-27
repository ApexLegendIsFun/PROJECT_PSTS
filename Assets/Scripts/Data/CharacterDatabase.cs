// Data/CharacterDatabase.cs
// 캐릭터 데이터베이스 ScriptableObject

using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 모든 캐릭터 데이터를 관리하는 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDatabase", menuName = "ProjectSS/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        [Header("Characters")]
        public List<CharacterData> Characters = new();

        /// <summary>
        /// ID로 캐릭터 찾기
        /// </summary>
        public CharacterData GetCharacterById(string id)
        {
            return Characters.Find(c => c.Id == id);
        }

        /// <summary>
        /// 캐릭터 수
        /// </summary>
        public int Count => Characters.Count;
    }
}
