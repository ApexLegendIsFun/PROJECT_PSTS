// MainMenu/CharacterSelectPanel.cs
// TRIAD 캐릭터 선택 패널

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core;
using ProjectSS.Services;
using ProjectSS.Data;

namespace ProjectSS.MainMenu
{
    /// <summary>
    /// TRIAD (3인 파티) 캐릭터 선택 UI
    /// </summary>
    public class CharacterSelectPanel : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int _partySize = 3;

        [Header("UI References")]
        [SerializeField] private Transform _characterListContainer;
        [SerializeField] private Transform _selectedSlotsContainer;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _infoText;

        [Header("Prefabs")]
        [SerializeField] private GameObject _characterButtonPrefab;
        [SerializeField] private GameObject _selectedSlotPrefab;

        [Header("Controller Reference")]
        [SerializeField] private MainMenuController _menuController;

        // 선택된 캐릭터 ID 목록
        private List<string> _selectedCharacterIds = new();

        // 사용 가능한 캐릭터 데이터 (DataService에서 로드)
        private List<CharacterSelectData> _availableCharacters = new();

        private void Awake()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (_backButton != null)
                _backButton.onClick.RemoveListener(OnBackClicked);
        }

        /// <summary>
        /// 패널 초기화
        /// </summary>
        public void Initialize()
        {
            _selectedCharacterIds.Clear();
            LoadAvailableCharacters();
            UpdateUI();

            // TODO: 캐릭터 버튼 동적 생성
            // 현재는 수동 배치 가정
        }

        /// <summary>
        /// CharacterDatabase에서 사용 가능한 캐릭터 로드
        /// </summary>
        private void LoadAvailableCharacters()
        {
            _availableCharacters.Clear();

            var db = DataService.Instance?.Characters;
            if (db == null || db.Count == 0)
            {
                Debug.LogWarning("[CharacterSelect] CharacterDatabase not available. Using fallback characters.");
                LoadFallbackCharacters();
                return;
            }

            foreach (var charData in db.Characters)
            {
                _availableCharacters.Add(new CharacterSelectData
                {
                    Id = charData.Id,
                    Name = charData.Name,
                    Class = charData.Class,
                    Portrait = charData.Portrait
                });
            }

            Debug.Log($"[CharacterSelect] Loaded {_availableCharacters.Count} characters from CharacterDatabase");
        }

        /// <summary>
        /// 폴백: CharacterDatabase가 없을 때 기본 캐릭터 목록 사용
        /// </summary>
        private void LoadFallbackCharacters()
        {
            _availableCharacters = new List<CharacterSelectData>
            {
                new CharacterSelectData { Id = "warrior_01", Name = "검투사", Class = CharacterClass.Warrior },
                new CharacterSelectData { Id = "mage_01", Name = "마법사", Class = CharacterClass.Mage },
                new CharacterSelectData { Id = "rogue_01", Name = "암살자", Class = CharacterClass.Rogue },
                new CharacterSelectData { Id = "healer_01", Name = "사제", Class = CharacterClass.Healer },
                new CharacterSelectData { Id = "tank_01", Name = "수호자", Class = CharacterClass.Tank }
            };
        }

        #region Character Selection

        /// <summary>
        /// 캐릭터 선택/해제 토글
        /// </summary>
        public void ToggleCharacter(string characterId)
        {
            if (_selectedCharacterIds.Contains(characterId))
            {
                // 이미 선택됨 - 해제
                _selectedCharacterIds.Remove(characterId);
                Debug.Log($"[CharacterSelect] Deselected: {characterId}");
            }
            else
            {
                // 선택 가능 여부 확인
                if (_selectedCharacterIds.Count < _partySize)
                {
                    _selectedCharacterIds.Add(characterId);
                    Debug.Log($"[CharacterSelect] Selected: {characterId}");
                }
                else
                {
                    Debug.Log("[CharacterSelect] Party is full!");
                }
            }

            UpdateUI();
        }

        /// <summary>
        /// 캐릭터가 선택되었는지 확인
        /// </summary>
        public bool IsCharacterSelected(string characterId)
        {
            return _selectedCharacterIds.Contains(characterId);
        }

        /// <summary>
        /// 특정 슬롯의 캐릭터 해제
        /// </summary>
        public void DeselectSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _selectedCharacterIds.Count)
            {
                var characterId = _selectedCharacterIds[slotIndex];
                _selectedCharacterIds.RemoveAt(slotIndex);
                Debug.Log($"[CharacterSelect] Deselected slot {slotIndex}: {characterId}");
                UpdateUI();
            }
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            UpdateInfoText();
            UpdateConfirmButton();
            // TODO: 선택 슬롯 UI 업데이트
            // TODO: 캐릭터 버튼 선택 상태 업데이트
        }

        private void UpdateInfoText()
        {
            if (_infoText != null)
            {
                int remaining = _partySize - _selectedCharacterIds.Count;
                if (remaining > 0)
                {
                    _infoText.text = $"파티원을 {remaining}명 더 선택하세요";
                }
                else
                {
                    _infoText.text = "파티 구성 완료!";
                }
            }
        }

        private void UpdateConfirmButton()
        {
            if (_confirmButton != null)
            {
                _confirmButton.interactable = _selectedCharacterIds.Count == _partySize;
            }
        }

        #endregion

        #region Button Handlers

        private void OnConfirmClicked()
        {
            if (_selectedCharacterIds.Count != _partySize)
            {
                Debug.LogWarning("[CharacterSelect] Party not complete!");
                return;
            }

            Debug.Log($"[CharacterSelect] Confirmed party: {string.Join(", ", _selectedCharacterIds)}");

            if (_menuController != null)
            {
                _menuController.StartRunWithParty(_selectedCharacterIds.ToArray());
            }
        }

        private void OnBackClicked()
        {
            Debug.Log("[CharacterSelect] Back clicked");

            if (_menuController != null)
            {
                _menuController.CancelCharacterSelect();
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// 캐릭터 선택 데이터 (임시)
        /// </summary>
        [System.Serializable]
        public class CharacterSelectData
        {
            public string Id;
            public string Name;
            public CharacterClass Class;
            public Sprite Portrait;
        }

        #endregion
    }
}
