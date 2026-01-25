using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ProjectSS.Data;

namespace ProjectSS.Run
{
    /// <summary>
    /// RunManager 초기화 도우미
    /// RunManager initialization helper
    ///
    /// SerializeField가 비어있는 경우 Resources 폴더에서 자동 로드합니다.
    /// Automatically loads from Resources folder if SerializeField is empty.
    /// </summary>
    public class RunManagerInitializer : MonoBehaviour
    {
        [Header("Auto-Load Settings")]
        [Tooltip("Resources에서 자동 로드 활성화 / Enable auto-load from Resources")]
        [SerializeField] private bool enableAutoLoad = true;

        [Header("Resource Paths")]
        [Tooltip("캐릭터 클래스 Resources 경로 / Character class Resources path")]
        [SerializeField] private string characterClassPath = "CharacterClasses";

        [Tooltip("카드 Resources 경로 / Card Resources path")]
        [SerializeField] private string cardPath = "Cards";

        private void Start()
        {
            if (enableAutoLoad)
            {
                InitializeRunManager();
            }
        }

        /// <summary>
        /// RunManager 초기화
        /// Initialize RunManager
        /// </summary>
        public void InitializeRunManager()
        {
            var runManager = RunManager.Instance;
            if (runManager == null)
            {
                Debug.LogWarning("[RunManagerInitializer] RunManager not found!");
                return;
            }

            // 리플렉션을 통해 private SerializeField 접근
            // Access private SerializeField via reflection
            var rmType = typeof(RunManager);

            // CharacterClassData 자동 로드
            LoadCharacterClasses(runManager, rmType);

            // StarterDeck 자동 로드 (비어있는 경우)
            LoadStarterDeck(runManager, rmType);

            Debug.Log("[RunManagerInitializer] RunManager initialization complete");
        }

        /// <summary>
        /// 캐릭터 클래스 데이터 로드
        /// Load character class data
        /// </summary>
        private void LoadCharacterClasses(RunManager runManager, System.Type rmType)
        {
            // Warrior
            var warriorField = rmType.GetField("warriorClassData", BindingFlags.NonPublic | BindingFlags.Instance);
            if (warriorField != null && warriorField.GetValue(runManager) == null)
            {
                var warrior = Resources.Load<CharacterClassData>($"{characterClassPath}/CLASS_전사_Warrior");
                if (warrior != null)
                {
                    warriorField.SetValue(runManager, warrior);
                    Debug.Log("[RunManagerInitializer] Loaded Warrior class from Resources");
                }
            }

            // Mage
            var mageField = rmType.GetField("mageClassData", BindingFlags.NonPublic | BindingFlags.Instance);
            if (mageField != null && mageField.GetValue(runManager) == null)
            {
                var mage = Resources.Load<CharacterClassData>($"{characterClassPath}/CLASS_마법사_Mage");
                if (mage != null)
                {
                    mageField.SetValue(runManager, mage);
                    Debug.Log("[RunManagerInitializer] Loaded Mage class from Resources");
                }
            }

            // Rogue
            var rogueField = rmType.GetField("rogueClassData", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rogueField != null && rogueField.GetValue(runManager) == null)
            {
                var rogue = Resources.Load<CharacterClassData>($"{characterClassPath}/CLASS_도적_Rogue");
                if (rogue != null)
                {
                    rogueField.SetValue(runManager, rogue);
                    Debug.Log("[RunManagerInitializer] Loaded Rogue class from Resources");
                }
            }
        }

        /// <summary>
        /// 스타터 덱 로드
        /// Load starter deck
        /// </summary>
        private void LoadStarterDeck(RunManager runManager, System.Type rmType)
        {
            var deckField = rmType.GetField("starterDeck", BindingFlags.NonPublic | BindingFlags.Instance);
            if (deckField == null) return;

            var currentDeck = deckField.GetValue(runManager) as List<CardData>;
            if (currentDeck != null && currentDeck.Count > 0) return;

            // 기본 스타터 덱 로드
            var deck = new List<CardData>();

            // Strike x5
            var strike = Resources.Load<CardData>($"{cardPath}/ATK_강타_Strike");
            if (strike != null)
            {
                for (int i = 0; i < 5; i++)
                    deck.Add(strike);
            }

            // Defend x4
            var defend = Resources.Load<CardData>($"{cardPath}/DEF_방어_Defend");
            if (defend != null)
            {
                for (int i = 0; i < 4; i++)
                    deck.Add(defend);
            }

            // Bash x1
            var bash = Resources.Load<CardData>($"{cardPath}/ATK_강타_Bash");
            if (bash != null)
            {
                deck.Add(bash);
            }

            if (deck.Count > 0)
            {
                deckField.SetValue(runManager, deck);
                Debug.Log($"[RunManagerInitializer] Loaded {deck.Count} cards for starter deck");
            }
        }

        /// <summary>
        /// 모든 필요한 Resources가 존재하는지 검증
        /// Validate all required Resources exist
        /// </summary>
        public bool ValidateResources()
        {
            bool allValid = true;

            // Character Classes
            string[] classNames = { "CLASS_전사_Warrior", "CLASS_마법사_Mage", "CLASS_도적_Rogue" };
            foreach (var className in classNames)
            {
                var classData = Resources.Load<CharacterClassData>($"{characterClassPath}/{className}");
                if (classData == null)
                {
                    Debug.LogWarning($"[RunManagerInitializer] Missing: {characterClassPath}/{className}");
                    allValid = false;
                }
            }

            // Cards
            string[] cardNames = { "ATK_강타_Strike", "DEF_방어_Defend" };
            foreach (var cardName in cardNames)
            {
                var cardData = Resources.Load<CardData>($"{cardPath}/{cardName}");
                if (cardData == null)
                {
                    Debug.LogWarning($"[RunManagerInitializer] Missing: {cardPath}/{cardName}");
                    allValid = false;
                }
            }

            return allValid;
        }
    }
}
