// Combat/CombatSceneInitializer.cs
// 전투 씬 초기화 - Boot 씬 없이 직접 실행 지원

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data.Cards;
using ProjectSS.Combat.Testing;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 씬 초기화
    /// Boot 씬을 거치지 않고 직접 실행할 때 테스트 전투 설정
    /// </summary>
    public class CombatSceneInitializer : MonoBehaviour
    {
        [Header("Auto Initialize")]
        [Tooltip("씬 시작 시 자동 초기화 여부")]
        [SerializeField] private bool _autoInitialize = true;

        [Header("Test Data (Editor Only)")]
        [Tooltip("에디터에서 직접 실행 시 테스트 데이터 사용")]
        [SerializeField] private bool _useTestDataOnDirectRun = true;

        [Header("Card Pool Settings (SO 기반)")]
        [Tooltip("전사 카드풀 SO")]
        [SerializeField] private CharacterCardPoolSO _warriorCardPool;
        [Tooltip("마법사 카드풀 SO")]
        [SerializeField] private CharacterCardPoolSO _mageCardPool;
        [Tooltip("힐러 카드풀 SO")]
        [SerializeField] private CharacterCardPoolSO _healerCardPool;

        [Header("Test Card Data (SO 없을 때 폴백)")]
        [Tooltip("테스트용 기본 공격 카드")]
        [SerializeField] private CardDataSO _testStrikeCard;
        [Tooltip("테스트용 기본 방어 카드")]
        [SerializeField] private CardDataSO _testDefendCard;

        [Header("Test Party Settings")]
        [SerializeField] private int _testPartySize = 3;
        [SerializeField] private int _testPartyHP = 50;
        [SerializeField] private int _testPartyEnergy = 3;
        [SerializeField] private int _testPartySpeed = 10;

        [Header("Test Enemy Settings")]
        [SerializeField] private int _testEnemyCount = 2;
        [SerializeField] private int _testEnemyHP = 30;
        [SerializeField] private int _testEnemySpeed = 8;
        [SerializeField] private int _testEnemyDamage = 8;

        [Header("Test Encounter Settings")]
        [SerializeField] private TileType _testEncounterType = TileType.Enemy;

        private void Start()
        {
            if (_autoInitialize)
            {
                Initialize();
            }
        }

        /// <summary>
        /// 전투 씬 초기화
        /// </summary>
        public void Initialize()
        {
            // 필수 시스템 초기화 확인
            bool isDirectRun = SceneBootstrapper.IsDirectSceneRun();
            SceneBootstrapper.EnsureInitialized();

            // 씬 설정 검증 및 필요 시 자동 생성
            ValidateSceneSetup();

            // CombatManager 확인
            if (CombatManager.Instance == null)
            {
                Debug.LogError("[CombatSceneInitializer] CombatManager not found!");
                return;
            }

            // 1. GameManager에서 대기 중인 전투 데이터 확인
            if (GameManager.Instance != null && GameManager.Instance.HasPendingCombat)
            {
                Debug.Log("[CombatSceneInitializer] Starting combat from pending data...");
                StartCombatFromSetupData();
                return;
            }

            // 2. 직접 실행이고 테스트 데이터 사용 설정된 경우
            if (isDirectRun && _useTestDataOnDirectRun)
            {
                Debug.Log("[CombatSceneInitializer] Direct scene run detected. Starting test combat...");
                StartTestCombat();
                return;
            }

            // 3. 그 외의 경우 - 대기
            Debug.Log("[CombatSceneInitializer] Combat scene initialized (waiting for combat data).");
        }

        /// <summary>
        /// 설정 데이터로 전투 시작
        /// </summary>
        private void StartCombatFromSetupData()
        {
            var setupData = GameManager.Instance.ConsumePendingCombat();

            if (setupData == null || !setupData.IsValid())
            {
                Debug.LogError("[CombatSceneInitializer] Invalid combat setup data!");
                return;
            }

            var party = CreatePartyFromSetup(setupData.PartyMembers);
            var enemies = CreateEnemiesFromSetup(setupData);

            Debug.Log($"[CombatSceneInitializer] Starting combat: {setupData.EncounterType}, " +
                      $"{party.Count} party members vs {enemies.Count} enemies");

            CombatManager.Instance.StartCombat(setupData.EncounterType, party, enemies);
        }

        /// <summary>
        /// 설정 데이터로 파티 생성
        /// </summary>
        private List<PartyMemberCombat> CreatePartyFromSetup(List<PartyMemberSetup> setupList)
        {
            var party = new List<PartyMemberCombat>();

            foreach (var setup in setupList)
            {
                var go = new GameObject($"PartyMember_{setup.CharacterId}");
                var member = go.AddComponent<PartyMemberCombat>();

                member.Initialize(
                    characterId: setup.CharacterId,
                    name: setup.DisplayName,
                    charClass: setup.CharacterClass,
                    maxHP: setup.MaxHP,
                    maxEnergy: setup.MaxEnergy,
                    speed: setup.Speed
                );

                // 덱 초기화
                InitializeDeckForCharacter(member, setup.CharacterClass, setup.CharacterId);

                party.Add(member);
                Debug.Log($"[CombatSceneInitializer] Created party member from setup: {member.DisplayName}");
            }

            return party;
        }

        /// <summary>
        /// 설정 데이터로 적 생성
        /// </summary>
        private List<EnemyCombat> CreateEnemiesFromSetup(CombatSetupData setupData)
        {
            // 명시적 적 설정이 있으면 사용
            if (setupData.Enemies != null && setupData.Enemies.Count > 0)
            {
                return CreateEnemiesFromExplicitSetup(setupData.Enemies);
            }

            // 없으면 EncounterType 기반 자동 생성
            return GenerateEnemiesForEncounter(setupData.EncounterType);
        }

        /// <summary>
        /// 명시적 설정으로 적 생성
        /// </summary>
        private List<EnemyCombat> CreateEnemiesFromExplicitSetup(List<EnemySetup> setupList)
        {
            var enemies = new List<EnemyCombat>();

            foreach (var setup in setupList)
            {
                var go = new GameObject($"Enemy_{setup.EnemyId}");
                var enemy = go.AddComponent<EnemyCombat>();

                enemy.Initialize(
                    enemyId: setup.EnemyId,
                    name: setup.DisplayName,
                    enemyType: setup.EnemyType,
                    maxHP: setup.MaxHP,
                    speed: setup.Speed,
                    baseDamage: setup.BaseDamage
                );

                enemies.Add(enemy);
            }

            return enemies;
        }

        /// <summary>
        /// EncounterType 기반 적 자동 생성
        /// </summary>
        private List<EnemyCombat> GenerateEnemiesForEncounter(TileType encounterType)
        {
            var enemies = new List<EnemyCombat>();

            // EncounterType에 따른 적 구성
            int enemyCount;
            int baseHP;
            int baseDamage;
            string[] enemyTypes;

            switch (encounterType)
            {
                case TileType.Boss:
                    enemyCount = 1;
                    baseHP = 100;
                    baseDamage = 15;
                    enemyTypes = new[] { "Boss" };
                    break;

                case TileType.Elite:
                    enemyCount = 2;
                    baseHP = 50;
                    baseDamage = 12;
                    enemyTypes = new[] { "Elite_A", "Elite_B" };
                    break;

                default: // TileType.Enemy
                    enemyCount = Random.Range(2, 4);
                    baseHP = 30;
                    baseDamage = 8;
                    enemyTypes = new[] { "Slime", "Goblin", "Skeleton" };
                    break;
            }

            for (int i = 0; i < enemyCount; i++)
            {
                var go = new GameObject($"Enemy_{i}");
                var enemy = go.AddComponent<EnemyCombat>();

                var enemyType = enemyTypes[i % enemyTypes.Length];
                enemy.Initialize(
                    enemyId: $"enemy_{i}",
                    name: $"테스트 {enemyType}",
                    enemyType: enemyType,
                    maxHP: baseHP + Random.Range(-5, 10),
                    speed: 8 + i,
                    baseDamage: baseDamage + Random.Range(-2, 3)
                );

                enemies.Add(enemy);
            }

            return enemies;
        }

        /// <summary>
        /// 씬 설정 검증 및 누락된 컴포넌트 자동 생성
        /// </summary>
        private void ValidateSceneSetup()
        {
            var issues = new List<string>();

            // CombatManager 확인/생성
            if (CombatManager.Instance == null)
            {
                var existingManager = FindObjectOfType<CombatManager>();
                if (existingManager == null)
                {
                    issues.Add("CombatManager - creating");
                    var go = new GameObject("CombatManager");
                    go.AddComponent<CombatManager>();
                }
            }

            // UI 인프라 확인/생성
            SceneBootstrapper.EnsureUIInfrastructure();

            if (issues.Count > 0)
            {
                Debug.Log($"[CombatSceneInitializer] Scene validation: {string.Join(", ", issues)}");
            }
        }

        /// <summary>
        /// 테스트 전투 시작 (에디터 개발용)
        /// </summary>
        private void StartTestCombat()
        {
            var party = CreateTestParty();
            var enemies = CreateTestEnemies();

            Debug.Log($"[CombatSceneInitializer] Starting test combat: {party.Count} party members vs {enemies.Count} enemies");

            CombatManager.Instance.StartCombat(_testEncounterType, party, enemies);
        }

        /// <summary>
        /// 테스트 파티 생성
        /// </summary>
        private List<PartyMemberCombat> CreateTestParty()
        {
            var party = new List<PartyMemberCombat>();
            var classes = new[] { CharacterClass.Warrior, CharacterClass.Mage, CharacterClass.Healer };

            for (int i = 0; i < _testPartySize; i++)
            {
                var go = new GameObject($"TestPartyMember_{i}");
                var member = go.AddComponent<PartyMemberCombat>();

                var charClass = classes[i % classes.Length];
                string characterId = $"test_party_{i}";

                member.Initialize(
                    characterId: characterId,
                    name: $"테스트 {GetClassNameKorean(charClass)}",
                    charClass: charClass,
                    maxHP: _testPartyHP,
                    maxEnergy: _testPartyEnergy,
                    speed: _testPartySpeed + i // 약간의 속도 차이
                );

                // 덱 초기화 (SO 기반 우선, 없으면 테스트 데이터)
                InitializeDeckForCharacter(member, charClass, characterId);

                party.Add(member);
                Debug.Log($"[CombatSceneInitializer] Created test party member: {member.DisplayName}");
            }

            return party;
        }

        /// <summary>
        /// 캐릭터 덱 초기화 (SO 기반 또는 폴백)
        /// </summary>
        private void InitializeDeckForCharacter(PartyMemberCombat member, CharacterClass charClass, string characterId)
        {
            // 캐릭터별 카드풀 SO 확인
            CharacterCardPoolSO cardPool = GetCardPoolForClass(charClass);

            if (cardPool != null)
            {
                // SO 기반 초기화
                member.DeckManager.InitializeDeck(characterId, cardPool);
                Debug.Log($"[CombatSceneInitializer] Initialized deck from CardPool SO for {characterId}");
            }
            else if (_testStrikeCard != null && _testDefendCard != null)
            {
                // 테스트 카드 SO로 초기화
                var starterCards = CreateTestDeckFromSO(charClass);
                member.DeckManager.InitializeDeck(characterId, starterCards);
                Debug.Log($"[CombatSceneInitializer] Initialized deck from test SO cards for {characterId}");
            }
            else
            {
                // 폴백: TestCardFactory로 런타임 카드 생성
                Debug.Log($"[CombatSceneInitializer] Using TestCardFactory for {characterId}");
                var testCards = TestCardFactory.CreateStarterDeck(charClass);
                member.DeckManager.InitializeDeck(characterId, testCards);
            }
        }

        /// <summary>
        /// 클래스별 카드풀 SO 반환
        /// </summary>
        private CharacterCardPoolSO GetCardPoolForClass(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => _warriorCardPool,
                CharacterClass.Mage => _mageCardPool,
                CharacterClass.Healer => _healerCardPool,
                _ => _warriorCardPool // 기본값
            };
        }

        /// <summary>
        /// 테스트 SO 카드로 덱 생성
        /// </summary>
        private List<CardDataSO> CreateTestDeckFromSO(CharacterClass charClass)
        {
            var deck = new List<CardDataSO>();

            // 기본 카드 추가
            for (int i = 0; i < 3; i++)
            {
                if (_testStrikeCard != null) deck.Add(_testStrikeCard);
            }

            for (int i = 0; i < 2; i++)
            {
                if (_testDefendCard != null) deck.Add(_testDefendCard);
            }

            return deck;
        }

        /// <summary>
        /// 테스트 적 생성
        /// </summary>
        private List<EnemyCombat> CreateTestEnemies()
        {
            var enemies = new List<EnemyCombat>();
            var enemyTypes = new[] { "Slime", "Goblin", "Skeleton" };

            for (int i = 0; i < _testEnemyCount; i++)
            {
                var go = new GameObject($"TestEnemy_{i}");
                var enemy = go.AddComponent<EnemyCombat>();

                var enemyType = enemyTypes[i % enemyTypes.Length];
                enemy.Initialize(
                    enemyId: $"test_enemy_{i}",
                    name: $"테스트 {enemyType}",
                    enemyType: enemyType,
                    maxHP: _testEnemyHP,
                    speed: _testEnemySpeed + i,
                    baseDamage: _testEnemyDamage
                );

                enemies.Add(enemy);
                Debug.Log($"[CombatSceneInitializer] Created test enemy: {enemy.DisplayName}");
            }

            return enemies;
        }

        /// <summary>
        /// 클래스 이름 한글 변환
        /// </summary>
        private string GetClassNameKorean(CharacterClass charClass)
        {
            return charClass switch
            {
                CharacterClass.Warrior => "전사",
                CharacterClass.Mage => "마법사",
                CharacterClass.Rogue => "도적",
                CharacterClass.Healer => "힐러",
                CharacterClass.Tank => "탱커",
                _ => "캐릭터"
            };
        }

#if UNITY_EDITOR
        [ContextMenu("Start Test Combat Now")]
        private void EditorStartTestCombat()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[CombatSceneInitializer] Play 모드에서만 실행 가능합니다.");
                return;
            }

            SceneBootstrapper.EnsureInitialized();
            StartTestCombat();
        }
#endif
    }
}
