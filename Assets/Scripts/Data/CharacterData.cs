// Data/CharacterData.cs
// 캐릭터 데이터 ScriptableObject

using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data
{
    /// <summary>
    /// 개별 캐릭터 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "Character", menuName = "ProjectSS/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Basic Info")]
        public string Id;
        public string Name;
        public CharacterClass Class;

        [Header("Visuals")]
        public Sprite Portrait;
        public Color ThemeColor = Color.white;

        [Header("Description")]
        [TextArea(2, 4)]
        public string Description;

        [Header("Base Stats")]
        public int MaxHealth = 100;
        public int BaseEnergy = 3;
        public int BaseSpeed = 10;
    }
}
