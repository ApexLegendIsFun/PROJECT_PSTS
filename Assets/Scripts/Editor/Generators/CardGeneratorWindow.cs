using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 카드 생성기 에디터 윈도우
    /// Card generator editor window
    /// </summary>
    public class CardGeneratorWindow : EditorWindow
    {
        // 탭
        private int currentTab = 0;
        private readonly string[] tabNames = { "Quick Create", "Bulk Create", "Preview" };

        // Quick Create 필드
        private string koreanName = "";
        private string englishName = "";
        private CardType cardType = CardType.Attack;
        private CardRarity rarity = CardRarity.Common;
        private int energyCost = 1;
        private bool exhaustOnUse = false;
        private int damageValue = 6;
        private int blockValue = 5;
        private int drawValue = 2;
        private TargetType targetType = TargetType.SingleEnemy;

        // TRIAD 필드
        private bool showTriadOptions = false;
        private int focusCost = 0;
        private List<CharacterClass> allowedClasses = new List<CharacterClass>();
        private bool classWarrior = false;
        private bool classMage = false;
        private bool classRogue = false;

        // 업그레이드 필드
        private bool createUpgrade = false;
        private int upgradeDamageBonus = 2;
        private int upgradeBlockBonus = 2;

        // Bulk Create 필드
        private string bulkJson = "";

        // Preview
        private CardData previewCard = null;
        private Vector2 scrollPosition;

        [MenuItem("Tools/Project SS/Generators/Card Generator Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<CardGeneratorWindow>("Card Generator");
            window.minSize = new Vector2(450, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // 타이틀
            EditorGUILayout.LabelField("카드 생성기 / Card Generator", EditorStyles.boldLabel);
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
                    DrawBulkCreateTab();
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
            cardType = (CardType)EditorGUILayout.EnumPopup("카드 타입", cardType);
            rarity = (CardRarity)EditorGUILayout.EnumPopup("희귀도", rarity);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // 비용
            EditorGUILayout.LabelField("비용 (Cost)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            energyCost = EditorGUILayout.IntSlider("에너지 비용", energyCost, 0, 5);
            exhaustOnUse = EditorGUILayout.Toggle("사용 후 소멸", exhaustOnUse);

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // 효과 값
            EditorGUILayout.LabelField("효과 값 (Effect Values)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            switch (cardType)
            {
                case CardType.Attack:
                    damageValue = EditorGUILayout.IntField("데미지", damageValue);
                    targetType = (TargetType)EditorGUILayout.EnumPopup("타겟", targetType);
                    break;
                case CardType.Defense:
                    blockValue = EditorGUILayout.IntField("블록", blockValue);
                    break;
                case CardType.Skill:
                    drawValue = EditorGUILayout.IntField("드로우", drawValue);
                    break;
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);

            // TRIAD 옵션
            showTriadOptions = EditorGUILayout.Foldout(showTriadOptions, "TRIAD 옵션 (Class Restrictions)");
            if (showTriadOptions)
            {
                EditorGUI.indentLevel++;

                focusCost = EditorGUILayout.IntSlider("Focus 비용", focusCost, 0, 3);

                EditorGUILayout.LabelField("사용 가능 클래스 (비선택 = 모든 클래스):");
                EditorGUILayout.BeginHorizontal();
                classWarrior = EditorGUILayout.ToggleLeft("Warrior", classWarrior, GUILayout.Width(80));
                classMage = EditorGUILayout.ToggleLeft("Mage", classMage, GUILayout.Width(80));
                classRogue = EditorGUILayout.ToggleLeft("Rogue", classRogue, GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(10);

            // 업그레이드 옵션
            createUpgrade = EditorGUILayout.Toggle("업그레이드 버전 생성", createUpgrade);
            if (createUpgrade)
            {
                EditorGUI.indentLevel++;
                if (cardType == CardType.Attack)
                    upgradeDamageBonus = EditorGUILayout.IntField("데미지 보너스", upgradeDamageBonus);
                else if (cardType == CardType.Defense)
                    upgradeBlockBonus = EditorGUILayout.IntField("블록 보너스", upgradeBlockBonus);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(15);

            // 미리보기
            DrawCardPreview();
            EditorGUILayout.Space(15);

            // 생성 버튼
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("카드 생성 / Create Card", GUILayout.Height(40)))
            {
                CreateCard();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawCardPreview()
        {
            EditorGUILayout.LabelField("미리보기 (Preview)", EditorStyles.boldLabel);

            string fileName = GeneratorUtility.FormatCardFileName(cardType, koreanName, englishName);
            string fullName = $"{koreanName} ({englishName})";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 파일명
            EditorGUILayout.LabelField("파일명:", fileName + ".asset");
            EditorGUILayout.LabelField("이름:", fullName);
            EditorGUILayout.LabelField($"타입: {cardType} | 희귀도: {rarity} | 비용: {energyCost}");

            // 효과 미리보기
            string effectPreview = cardType switch
            {
                CardType.Attack => $"Deal {damageValue} damage",
                CardType.Defense => $"Gain {blockValue} Block",
                CardType.Skill => $"Draw {drawValue} card(s)",
                _ => ""
            };
            EditorGUILayout.LabelField("효과:", effectPreview);

            // TRIAD 정보
            if (focusCost > 0)
                EditorGUILayout.LabelField($"Focus 비용: {focusCost}");

            if (classWarrior || classMage || classRogue)
            {
                var classes = new List<string>();
                if (classWarrior) classes.Add("Warrior");
                if (classMage) classes.Add("Mage");
                if (classRogue) classes.Add("Rogue");
                EditorGUILayout.LabelField("클래스 제한:", string.Join(", ", classes));
            }

            EditorGUILayout.EndVertical();
        }

        private void CreateCard()
        {
            if (string.IsNullOrEmpty(koreanName) || string.IsNullOrEmpty(englishName))
            {
                EditorUtility.DisplayDialog("오류", "한글 이름과 영문 이름을 입력하세요.", "확인");
                return;
            }

            // 클래스 제한 설정
            allowedClasses.Clear();
            if (classWarrior) allowedClasses.Add(CharacterClass.Warrior);
            if (classMage) allowedClasses.Add(CharacterClass.Mage);
            if (classRogue) allowedClasses.Add(CharacterClass.Rogue);

            CardData card = null;

            switch (cardType)
            {
                case CardType.Attack:
                    card = CardGenerator.CreateAttackCard(
                        koreanName, englishName, damageValue, energyCost, rarity,
                        targetType, allowedClasses.Count > 0 ? allowedClasses : null, exhaustOnUse);
                    break;
                case CardType.Defense:
                    card = CardGenerator.CreateDefenseCard(
                        koreanName, englishName, blockValue, energyCost, rarity,
                        allowedClasses.Count > 0 ? allowedClasses : null, exhaustOnUse);
                    break;
                case CardType.Skill:
                    var effects = new List<CardEffect>
                    {
                        CardGenerator.CreateDrawEffect(drawValue)
                    };
                    card = CardGenerator.CreateSkillCard(
                        koreanName, englishName, effects, energyCost, rarity,
                        allowedClasses.Count > 0 ? allowedClasses : null, exhaustOnUse);
                    break;
            }

            if (card != null)
            {
                // Focus 비용 설정
                if (focusCost > 0)
                {
                    card.focusCost = focusCost;
                    EditorUtility.SetDirty(card);
                }

                // 업그레이드 생성
                if (createUpgrade)
                {
                    CardGenerator.CreateUpgradeVariant(card, "+",
                        cardType == CardType.Attack ? upgradeDamageBonus : 0,
                        cardType == CardType.Defense ? upgradeBlockBonus : 0,
                        0);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                previewCard = card;
                Selection.activeObject = card;
                EditorGUIUtility.PingObject(card);

                EditorUtility.DisplayDialog("완료",
                    $"카드가 생성되었습니다!\n{card.cardName}",
                    "확인");
            }
        }

        private void DrawBulkCreateTab()
        {
            EditorGUILayout.LabelField("JSON 대량 생성 (Bulk Create)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "JSON 형식으로 여러 카드를 한번에 생성할 수 있습니다.\n" +
                "형식: [{koreanName, englishName, cardType, rarity, energyCost, ...}]",
                MessageType.Info);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("JSON 입력:");
            bulkJson = EditorGUILayout.TextArea(bulkJson, GUILayout.Height(200));

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("템플릿 복사", GUILayout.Width(100)))
            {
                string template = @"[
  {
    ""koreanName"": ""테스트"",
    ""englishName"": ""Test"",
    ""cardType"": 0,
    ""rarity"": 1,
    ""energyCost"": 1,
    ""exhaustOnUse"": false,
    ""createUpgrade"": true
  }
]";
                EditorGUIUtility.systemCopyBuffer = template;
                Debug.Log("템플릿이 클립보드에 복사되었습니다.");
            }

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("대량 생성", GUILayout.Width(100)))
            {
                // TODO: JSON 파싱 및 대량 생성 구현
                EditorUtility.DisplayDialog("알림", "JSON 대량 생성 기능은 개발 중입니다.", "확인");
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPreviewTab()
        {
            EditorGUILayout.LabelField("카드 미리보기 (Card Preview)", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            previewCard = (CardData)EditorGUILayout.ObjectField("카드 선택", previewCard, typeof(CardData), false);

            if (previewCard != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // 카드 정보 표시
                EditorGUILayout.LabelField($"이름: {previewCard.cardName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"ID: {previewCard.cardId}");
                EditorGUILayout.LabelField($"타입: {previewCard.cardType}");
                EditorGUILayout.LabelField($"희귀도: {previewCard.rarity}");
                EditorGUILayout.LabelField($"에너지 비용: {previewCard.energyCost}");
                EditorGUILayout.LabelField($"소멸: {previewCard.exhaustOnUse}");
                EditorGUILayout.LabelField($"업그레이드됨: {previewCard.isUpgraded}");

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("효과:", EditorStyles.boldLabel);
                foreach (var effect in previewCard.effects)
                {
                    EditorGUILayout.LabelField($"  - {effect.GetDescription()}");
                }

                // TRIAD 정보
                if (previewCard.HasClassRestriction)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("클래스 제한:", EditorStyles.boldLabel);
                    foreach (var cls in previewCard.allowedClasses)
                    {
                        EditorGUILayout.LabelField($"  - {cls}");
                    }
                }

                if (previewCard.RequiresFocus)
                {
                    EditorGUILayout.LabelField($"Focus 비용: {previewCard.focusCost}");
                }

                if (previewCard.HasFocusBonus)
                {
                    EditorGUILayout.LabelField("Focus 보너스 효과:", EditorStyles.boldLabel);
                    foreach (var effect in previewCard.focusBonusEffects)
                    {
                        EditorGUILayout.LabelField($"  - {effect.GetDescription()}");
                    }
                }

                if (previewCard.upgradedVersion != null)
                {
                    EditorGUILayout.Space(5);
                    if (GUILayout.Button($"업그레이드 버전 보기: {previewCard.upgradedVersion.cardName}"))
                    {
                        previewCard = previewCard.upgradedVersion;
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);
                if (GUILayout.Button("Inspector에서 열기"))
                {
                    Selection.activeObject = previewCard;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("카드를 선택하면 상세 정보를 볼 수 있습니다.", MessageType.Info);
            }
        }
    }
}
