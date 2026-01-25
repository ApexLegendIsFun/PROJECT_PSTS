using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Combat;

namespace ProjectSS.Run
{
    /// <summary>
    /// 전투 씬 초기화 도우미
    /// Combat scene initialization helper
    ///
    /// 씬 로드 시 필요한 참조를 자동으로 설정합니다.
    /// Automatically sets up required references when scene loads.
    /// </summary>
    public class CombatSceneInitializer : MonoBehaviour
    {
        [Header("Auto-Load Settings")]
        [Tooltip("Resources 폴더에서 자동으로 prefab 로드 / Auto-load prefabs from Resources")]
        [SerializeField] private bool autoLoadPrefabs = true;

        [Header("Enemy Spawn Settings")]
        [Tooltip("적 스폰 포인트 자동 생성 / Auto-create enemy spawn points")]
        [SerializeField] private bool autoCreateSpawnPoints = true;

        [Tooltip("적 스폰 포인트 개수 / Number of spawn points")]
        [SerializeField] private int spawnPointCount = 4;

        [Tooltip("스폰 시작 X 위치 / Spawn start X position")]
        [SerializeField] private float spawnStartX = 3f;

        [Tooltip("스폰 간격 / Spawn spacing")]
        [SerializeField] private float spawnSpacing = 2f;

        [Header("Test Combat Settings")]
        [Tooltip("테스트 전투 자동 시작 / Auto-start test combat")]
        [SerializeField] private bool autoStartTestCombat = false;

        [Tooltip("테스트용 적 데이터 / Test enemy data")]
        [SerializeField] private List<EnemyData> testEnemyDatas;

        private void Start()
        {
            InitializeScene();
        }

        /// <summary>
        /// 씬 초기화
        /// Initialize scene
        /// </summary>
        public void InitializeScene()
        {
            if (autoLoadPrefabs)
            {
                LoadPrefabsFromResources();
            }

            if (autoCreateSpawnPoints)
            {
                CreateSpawnPointsIfNeeded();
            }

            // 테스트 전투 시작
            if (autoStartTestCombat && Application.isPlaying)
            {
                StartTestCombat();
            }
        }

        /// <summary>
        /// Resources 폴더에서 prefab 로드
        /// Load prefabs from Resources folder
        /// </summary>
        private void LoadPrefabsFromResources()
        {
            var combatManager = CombatManager.Instance;
            if (combatManager == null)
            {
                Debug.LogWarning("[CombatSceneInitializer] CombatManager not found!");
                return;
            }

            // EnemyPrefab 로드 시도 (CombatManager가 null 체크를 하므로 필수는 아님)
            // CombatManager handles null enemyPrefab by creating a basic GameObject
            var enemyPrefab = Resources.Load<EnemyCombat>("Prefabs/EnemyPrefab");
            if (enemyPrefab != null)
            {
                Debug.Log("[CombatSceneInitializer] Loaded EnemyPrefab from Resources");
                // CombatManager의 private 필드에 직접 접근할 수 없으므로
                // 이 prefab은 필요시 별도의 public 메서드를 통해 설정해야 함
            }
        }

        /// <summary>
        /// 스폰 포인트 자동 생성
        /// Auto-create spawn points if needed
        /// </summary>
        private void CreateSpawnPointsIfNeeded()
        {
            // 기존 스폰 포인트 찾기
            var existingPoints = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
            if (existingPoints != null && existingPoints.Length >= spawnPointCount)
            {
                Debug.Log($"[CombatSceneInitializer] Using existing {existingPoints.Length} spawn points");
                return;
            }

            // 스폰 포인트 컨테이너 생성
            var container = new GameObject("EnemySpawnPoints");
            container.transform.position = Vector3.zero;

            // 스폰 포인트 생성
            for (int i = 0; i < spawnPointCount; i++)
            {
                var spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.SetParent(container.transform);
                spawnPoint.transform.position = new Vector3(spawnStartX + i * spawnSpacing, 0f, 0f);
                spawnPoint.tag = "EnemySpawnPoint";
            }

            Debug.Log($"[CombatSceneInitializer] Created {spawnPointCount} spawn points");
        }

        /// <summary>
        /// 테스트 전투 시작
        /// Start test combat for debugging
        /// </summary>
        private void StartTestCombat()
        {
            var combatManager = CombatManager.Instance;
            if (combatManager == null)
            {
                Debug.LogError("[CombatSceneInitializer] CombatManager not found for test combat!");
                return;
            }

            // RunManager에서 파티 상태 가져오기
            var runManager = RunManager.Instance;
            PartyState partyState = null;
            List<CardData> deck = null;
            int seed = System.Environment.TickCount;

            if (runManager != null && runManager.IsRunActive)
            {
                partyState = runManager.PartyState;
                deck = runManager.Player?.GetDeckForCombat();
            }
            else
            {
                // 테스트용 기본 파티 생성
                partyState = CreateTestPartyState();
                deck = CreateTestDeck();
            }

            // 테스트 적 데이터
            var enemies = testEnemyDatas;
            if (enemies == null || enemies.Count == 0)
            {
                enemies = LoadTestEnemies();
            }

            if (enemies == null || enemies.Count == 0)
            {
                Debug.LogError("[CombatSceneInitializer] No enemies available for test combat!");
                return;
            }

            Debug.Log("[CombatSceneInitializer] Starting test combat...");
            combatManager.InitializeCombat(enemies, partyState, deck, seed);
        }

        /// <summary>
        /// 테스트용 파티 상태 생성
        /// Create test party state
        /// </summary>
        private PartyState CreateTestPartyState()
        {
            var warrior = Resources.Load<CharacterClassData>("CharacterClasses/CLASS_전사_Warrior");
            var mage = Resources.Load<CharacterClassData>("CharacterClasses/CLASS_마법사_Mage");
            var rogue = Resources.Load<CharacterClassData>("CharacterClasses/CLASS_도적_Rogue");

            if (warrior == null || mage == null || rogue == null)
            {
                Debug.LogWarning("[CombatSceneInitializer] Could not load all character classes from Resources!");

                // 폴백: 기본 파티 상태 생성
                var partyState = new PartyState();
                partyState.Initialize(null, null, null);
                return partyState;
            }

            var testPartyState = new PartyState();
            testPartyState.Initialize(warrior, mage, rogue);
            return testPartyState;
        }

        /// <summary>
        /// 테스트용 덱 생성
        /// Create test deck
        /// </summary>
        private List<CardData> CreateTestDeck()
        {
            var deck = new List<CardData>();

            // Resources에서 기본 카드 로드
            var strike = Resources.Load<CardData>("Cards/ATK_강타_Strike");
            var defend = Resources.Load<CardData>("Cards/DEF_방어_Defend");

            if (strike != null)
            {
                for (int i = 0; i < 5; i++)
                    deck.Add(strike);
            }

            if (defend != null)
            {
                for (int i = 0; i < 5; i++)
                    deck.Add(defend);
            }

            if (deck.Count == 0)
            {
                Debug.LogWarning("[CombatSceneInitializer] Could not load any cards for test deck!");
            }

            return deck;
        }

        /// <summary>
        /// 테스트용 적 로드
        /// Load test enemies
        /// </summary>
        private List<EnemyData> LoadTestEnemies()
        {
            var enemies = new List<EnemyData>();

            // Resources에서 적 로드
            var slime = Resources.Load<EnemyData>("Enemies/EN_슬라임_Slime");
            if (slime != null)
            {
                enemies.Add(slime);
            }
            else
            {
                // 모든 적 에셋 검색
                var allEnemies = Resources.LoadAll<EnemyData>("Enemies");
                if (allEnemies != null && allEnemies.Length > 0)
                {
                    enemies.Add(allEnemies[0]);
                }
            }

            return enemies;
        }
    }
}
