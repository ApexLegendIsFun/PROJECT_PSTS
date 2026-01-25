using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 적 생성기 에디터 윈도우
    /// Enemy generator editor window
    /// </summary>
    public class EnemyGeneratorWindow : EditorWindow
    {
        // 탭
        private int currentTab = 0;
        private readonly string[] tabNames = { "Quick Create", "Actions", "Preview" };

        // Quick Create 필드
        private string koreanName = "";
        private string englishName = "";
        private EnemyType enemyType = EnemyType.Normal;
        private int minHP = 10;
        private int maxHP = 15;
        private int goldReward = 10;
        private float cardRewardChance = 0.5f;
        private EnemyTargetingStrategy targetingStrategy = EnemyTargetingStrategy.ActiveOnly;

        // TRIAD Break 시스템 필드
        private bool showBreakOptions = false;
        private bool enableBreak = false;
        private BreakConditionType breakType = BreakConditionType.DamageThreshold;
        private int breakDamageThreshold = 20;
        private int breakHitCount = 5;
        private int groggyTurns = 1;

        // 액션 필드
        private List<EnemyActionData> actions = new List<EnemyActionData>();
        private List<EnemyActionData> groggyActions = new List<EnemyActionData>();
        private Vector2 actionScrollPosition;

        // Preview
        private EnemyData previewEnemy = null;
        private Vector2 scrollPosition;

        [MenuItem("Tools/Project SS/Generators/Enemy Generator Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<EnemyGeneratorWindow>("Enemy Generator");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // 타이틀
            EditorGUILayout.LabelField("적 생성기 / Enemy Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 탭 선택
            currentTab = GUILayout.Toolbar(currentTab, tabNames);
            EditorGUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            switch (currentTab)
            {
                case 0:
                    DrawQuickCreateTab();
                    break;
                case 1:
                    DrawActionsTab();
                    break;
                case 2:
                    DrawPreviewTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawQuickCreateTab()
        {
            // 기본 정보
            EditorGUILayout.LabelField("기본 정보 (Basic Info)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            koreanName = EditorGUILayout.TextField("한글 이름", koreanName);
            englishName = EditorGUILayout.TextField("English Name", englishName);
            enemyType = (EnemyType)EditorGUILayout.EnumPopup("적 타입", enemyType);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // 스탯
            EditorGUILayout.LabelField("스탯 (Stats)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("체력 범위", GUILayout.Width(100));
            minHP = EditorGUILayout.IntField(minHP, GUILayout.Width(50));
            EditorGUILayout.LabelField("~", GUILayout.Width(20));
            maxHP = EditorGUILayout.IntField(maxHP, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            // 타입별 기본값 버튼
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Normal 기본값", GUILayout.Width(100)))
            {
                enemyType = EnemyType.Normal;
                minHP = 10; maxHP = 20; goldReward = 10; cardRewardChance = 0.5f;
            }
            if (GUILayout.Button("Elite 기본값", GUILayout.Width(100)))
            {
                enemyType = EnemyType.Elite;
                minHP = 80; maxHP = 120; goldReward = 30; cardRewardChance = 0.75f;
            }
            if (GUILayout.Button("Boss 기본값", GUILayout.Width(100)))
            {
                enemyType = EnemyType.Boss;
                minHP = 150; maxHP = 200; goldReward = 100; cardRewardChance = 1f;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // 보상
            EditorGUILayout.LabelField("보상 (Rewards)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            goldReward = EditorGUILayout.IntField("골드 보상", goldReward);
            cardRewardChance = EditorGUILayout.Slider("카드 보상 확률", cardRewardChance, 0f, 1f);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // 타겟팅 전략
            EditorGUILayout.LabelField("타겟팅 (Targeting)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            targetingStrategy = (EnemyTargetingStrategy)EditorGUILayout.EnumPopup("타겟 전략", targetingStrategy);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // TRIAD Break 시스템
            showBreakOptions = EditorGUILayout.Foldout(showBreakOptions, "TRIAD Break 시스템");
            if (showBreakOptions)
            {
                EditorGUI.indentLevel++;

                enableBreak = EditorGUILayout.Toggle("Break 활성화", enableBreak);

                if (enableBreak)
                {
                    breakType = (BreakConditionType)EditorGUILayout.EnumPopup("조건 타입", breakType);

                    if (breakType == BreakConditionType.DamageThreshold || breakType == BreakConditionType.Both)
                        breakDamageThreshold = EditorGUILayout.IntSlider("데미지 임계값", breakDamageThreshold, 10, 100);

                    if (breakType == BreakConditionType.HitCount || breakType == BreakConditionType.Both)
                        breakHitCount = EditorGUILayout.IntSlider("타격 횟수", breakHitCount, 1, 10);

                    groggyTurns = EditorGUILayout.IntSlider("기절 턴 수", groggyTurns, 1, 3);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(10);

            // 액션 요약
            EditorGUILayout.LabelField($"액션: {actions.Count}개 (Actions 탭에서 편집)");
            if (enableBreak)
                EditorGUILayout.LabelField($"기절 액션: {groggyActions.Count}개");

            EditorGUILayout.Space(15);

            // 미리보기
            DrawEnemyPreview();
            EditorGUILayout.Space(15);

            // 생성 버튼
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("적 생성 / Create Enemy", GUILayout.Height(40)))
            {
                CreateEnemy();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawActionsTab()
        {
            EditorGUILayout.LabelField("액션 패턴 편집 (Action Pattern)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "액션은 순서대로 반복 실행됩니다.\n" +
                "Actions are executed in order and loop.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // 일반 액션
            EditorGUILayout.LabelField("일반 액션 (Normal Actions)", EditorStyles.boldLabel);
            actionScrollPosition = EditorGUILayout.BeginScrollView(actionScrollPosition, GUILayout.Height(200));
            DrawActionList(actions, false);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+ 액션 추가"))
            {
                actions.Add(new EnemyActionData());
            }

            EditorGUILayout.Space(20);

            // 기절 액션 (Break 활성화 시)
            if (enableBreak)
            {
                EditorGUILayout.LabelField("기절 액션 (Groggy Actions)", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "Break 발동 시 실행되는 약화된 액션입니다.",
                    MessageType.Info);

                DrawActionList(groggyActions, true);

                if (GUILayout.Button("+ 기절 액션 추가"))
                {
                    groggyActions.Add(new EnemyActionData());
                }
            }
        }

        private void DrawActionList(List<EnemyActionData> actionList, bool isGroggy)
        {
            for (int i = 0; i < actionList.Count; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"액션 {i + 1}", EditorStyles.boldLabel, GUILayout.Width(60));

                actionList[i].intentType = (EnemyIntentType)EditorGUILayout.EnumPopup(actionList[i].intentType, GUILayout.Width(100));

                GUILayout.FlexibleSpace();

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    actionList.RemoveAt(i);
                    return;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;

                actionList[i].actionName = EditorGUILayout.TextField("이름", actionList[i].actionName);

                // 인텐트 타입에 따른 필드
                switch (actionList[i].intentType)
                {
                    case EnemyIntentType.Attack:
                    case EnemyIntentType.AttackBuff:
                    case EnemyIntentType.AttackDebuff:
                        actionList[i].damage = EditorGUILayout.IntField("데미지", actionList[i].damage);
                        actionList[i].hitCount = EditorGUILayout.IntSlider("타격 횟수", actionList[i].hitCount, 1, 5);
                        break;

                    case EnemyIntentType.Defend:
                        actionList[i].block = EditorGUILayout.IntField("블록", actionList[i].block);
                        break;
                }

                // 상태효과 (Buff/Debuff 포함 타입)
                if (actionList[i].intentType == EnemyIntentType.Buff ||
                    actionList[i].intentType == EnemyIntentType.Debuff ||
                    actionList[i].intentType == EnemyIntentType.AttackBuff ||
                    actionList[i].intentType == EnemyIntentType.AttackDebuff)
                {
                    actionList[i].statusEffectType = (StatusEffectType)EditorGUILayout.EnumPopup("상태효과", actionList[i].statusEffectType);
                    actionList[i].statusStacks = EditorGUILayout.IntField("스택 수", actionList[i].statusStacks);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawEnemyPreview()
        {
            EditorGUILayout.LabelField("미리보기 (Preview)", EditorStyles.boldLabel);

            string fileName = GeneratorUtility.FormatEnemyFileName(enemyType, koreanName, englishName);
            string fullName = $"{koreanName} ({englishName})";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("파일명:", fileName + ".asset");
            EditorGUILayout.LabelField("이름:", fullName);
            EditorGUILayout.LabelField($"타입: {enemyType} | HP: {minHP}-{maxHP}");
            EditorGUILayout.LabelField($"보상: {goldReward} Gold | 카드 확률: {cardRewardChance:P0}");
            EditorGUILayout.LabelField($"타겟팅: {targetingStrategy}");

            if (enableBreak)
            {
                EditorGUILayout.LabelField($"Break: {breakType} | 기절 {groggyTurns}턴");
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreviewTab()
        {
            EditorGUILayout.LabelField("적 미리보기 (Enemy Preview)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            previewEnemy = (EnemyData)EditorGUILayout.ObjectField("적 선택", previewEnemy, typeof(EnemyData), false);

            if (previewEnemy != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField($"이름: {previewEnemy.enemyName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"ID: {previewEnemy.enemyId}");
                EditorGUILayout.LabelField($"타입: {previewEnemy.enemyType}");
                EditorGUILayout.LabelField($"체력: {previewEnemy.minHealth} - {previewEnemy.maxHealth}");
                EditorGUILayout.LabelField($"골드: {previewEnemy.goldReward}");
                EditorGUILayout.LabelField($"카드 확률: {previewEnemy.cardRewardChance:P0}");
                EditorGUILayout.LabelField($"타겟팅: {previewEnemy.targetingStrategy}");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("액션 패턴:", EditorStyles.boldLabel);
                for (int i = 0; i < previewEnemy.actionPattern.Count; i++)
                {
                    var action = previewEnemy.actionPattern[i];
                    EditorGUILayout.LabelField($"  {i + 1}. [{action.intentType}] {action.GetIntentDescription()}");
                }

                if (previewEnemy.CanBebroken)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Break 조건:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  {previewEnemy.breakCondition.GetDescription()}");
                    EditorGUILayout.LabelField($"  기절 턴: {previewEnemy.breakCondition.groggyTurns}");

                    if (previewEnemy.HasGroggyActions)
                    {
                        EditorGUILayout.LabelField("기절 액션:", EditorStyles.boldLabel);
                        foreach (var action in previewEnemy.groggyActions)
                        {
                            EditorGUILayout.LabelField($"  [{action.intentType}] {action.GetIntentDescription()}");
                        }
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);
                if (GUILayout.Button("Inspector에서 열기"))
                {
                    Selection.activeObject = previewEnemy;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("적을 선택하면 상세 정보를 볼 수 있습니다.", MessageType.Info);
            }
        }

        private void CreateEnemy()
        {
            if (string.IsNullOrEmpty(koreanName) || string.IsNullOrEmpty(englishName))
            {
                EditorUtility.DisplayDialog("오류", "한글 이름과 영문 이름을 입력하세요.", "확인");
                return;
            }

            // 액션 변환
            var convertedActions = ConvertActions(actions);
            var convertedGroggyActions = enableBreak ? ConvertActions(groggyActions) : null;

            // Break 조건 생성
            BreakConditionData breakCondition = null;
            if (enableBreak)
            {
                breakCondition = breakType switch
                {
                    BreakConditionType.DamageThreshold => EnemyGenerator.CreateDamageThresholdBreak(breakDamageThreshold, groggyTurns),
                    BreakConditionType.HitCount => EnemyGenerator.CreateHitCountBreak(breakHitCount, groggyTurns),
                    BreakConditionType.Both => EnemyGenerator.CreateCombinedBreak(breakDamageThreshold, breakHitCount, groggyTurns),
                    _ => null
                };
            }

            // 적 생성
            EnemyData enemy = enemyType switch
            {
                EnemyType.Normal => EnemyGenerator.CreateNormalEnemy(
                    koreanName, englishName, minHP, maxHP, convertedActions, goldReward, cardRewardChance, targetingStrategy),
                EnemyType.Elite => EnemyGenerator.CreateEliteEnemy(
                    koreanName, englishName, minHP, maxHP, convertedActions, goldReward,
                    breakCondition, convertedGroggyActions, cardRewardChance, targetingStrategy),
                EnemyType.Boss => EnemyGenerator.CreateBoss(
                    koreanName, englishName, minHP, maxHP, convertedActions, goldReward,
                    breakCondition, convertedGroggyActions, cardRewardChance, targetingStrategy),
                _ => null
            };

            if (enemy != null)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                previewEnemy = enemy;
                Selection.activeObject = enemy;
                EditorGUIUtility.PingObject(enemy);

                EditorUtility.DisplayDialog("완료",
                    $"적이 생성되었습니다!\n{enemy.enemyName}",
                    "확인");
            }
        }

        private List<EnemyAction> ConvertActions(List<EnemyActionData> dataList)
        {
            var result = new List<EnemyAction>();
            foreach (var data in dataList)
            {
                var action = new EnemyAction
                {
                    intentType = data.intentType,
                    actionName = data.actionName,
                    damage = data.damage,
                    block = data.block,
                    hitCount = Mathf.Max(1, data.hitCount),
                    statusEffect = GeneratorUtility.LoadStatusEffect(data.statusEffectType),
                    statusStacks = data.statusStacks
                };
                result.Add(action);
            }
            return result;
        }
    }

    /// <summary>
    /// 액션 편집용 임시 데이터 구조체
    /// Temporary data structure for action editing
    /// </summary>
    [System.Serializable]
    public class EnemyActionData
    {
        public EnemyIntentType intentType = EnemyIntentType.Attack;
        public string actionName = "Attack";
        public int damage = 6;
        public int block = 5;
        public int hitCount = 1;
        public StatusEffectType statusEffectType = StatusEffectType.Weak;
        public int statusStacks = 1;
    }
}
