using BayatGames.SaveGameFree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Traits;

namespace worldcreator {
    public class CharacterInfoEditor : MonoBehaviour {

        private Character _character;

        [Header("Portrait Settings")]
        [SerializeField] private CharacterPortrait portrait;
        [SerializeField] private Dropdown templatesDropdown;

        [Header("Basic Info")]
        [SerializeField] private InputField nameField;
        [SerializeField] private Dropdown raceField;
        [SerializeField] private Dropdown genderField;
        [SerializeField] private Dropdown sexualityField;
        [SerializeField] private Dropdown roleField;
        [SerializeField] private Dropdown classField;
        [SerializeField] private Dropdown factionField;
        [SerializeField] private Dropdown moralityField;
        [SerializeField] private InputField levelField;
        [SerializeField] private Text otherInfoLbl;

        [Header("Relationship Info")]
        [SerializeField] private GameObject relationshipItemPrefab;
        [SerializeField] private ScrollRect relationshipScrollView;
        [SerializeField] private Dropdown charactersRelationshipDropdown;
        [SerializeField] private Dropdown relationshipTypesDropdown;
        [SerializeField] private Button createRelationshipBtn;

        [Header("Equipment Info")]
        [SerializeField] private GameObject itemEditorPrefab;
        [SerializeField] private ScrollRect equipmentScrollView;
        [SerializeField] private Dropdown equipmentChoicesDropdown;
        [SerializeField] private Button addEquipmentBtn;

        [Header("Inventory Info")]
        [SerializeField] private ScrollRect inventoryScrollView;
        [SerializeField] private Dropdown inventoryChoicesDropdown;
        [SerializeField] private Button addInventoryBtn;

        [Header("Attribute Info")]
        [SerializeField] private Dropdown attributeChoicesDropdown;
        [SerializeField] private Text attributeSummary;

        public Dictionary<string, PortraitSettings> portraitTemplates;

        public void Initialize() {
            Messenger.AddListener<Character, RelationshipTrait>(Signals.RELATIONSHIP_ADDED, OnRelationshipCreated);
            Messenger.AddListener<Character, RELATIONSHIP_TRAIT, AlterEgoData>(Signals.RELATIONSHIP_REMOVED, OnRelationshipRemoved);
            Messenger.AddListener<Character, Character>(Signals.ALL_RELATIONSHIP_REMOVED, OnAllRelationshipRemoved);

            //LoadEquipmentChoices();
            //LoadInventoryChoices();
        }

        public void UpdateInfo() {
            if (_character != null) {
                ShowCharacterInfo(_character);
            }
        }

        public void ShowCharacterInfo(Character character) {
            _character = character;
            portrait.GeneratePortrait(_character);
            LoadDropdownOptions();
            //UpdatePortraitControls();
            UpdateBasicInfo();
            LoadRelationships();
            LoadRelationshipDropdowns();
            //LoadEquipment();
            LoadInventory();
            LoadTemplateChoices();
            Messenger.AddListener<Item, Character>(Signals.ITEM_EQUIPPED, OnItemEquipped);
            Messenger.AddListener<Item, Character>(Signals.ITEM_UNEQUIPPED, OnItemUnequipped);
            //Messenger.AddListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
            //Messenger.AddListener<Item, Character>(Signals.ITEM_THROWN, OnItemThrown);
            this.gameObject.SetActive(true);
        }
        public void Close() {
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_EQUIPPED, OnItemEquipped);
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_UNEQUIPPED, OnItemUnequipped);
            //Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
            //Messenger.RemoveListener<Item, Character>(Signals.ITEM_THROWN, OnItemThrown);
            this.gameObject.SetActive(false);
        }

        #region Portrait Editor
        public void LoadTemplateChoices() {
            if (_character == null) {
                return;
            }
            portraitTemplates = new Dictionary<string, PortraitSettings>();
            string path = Utilities.portraitsSavePath + _character.raceSetting.race + "/" + _character.gender.ToString() + "/";
            Directory.CreateDirectory(path);
            DirectoryInfo info = new DirectoryInfo(path);
            FileInfo[] files = info.GetFiles("*" + Utilities.portraitFileExt);
            for (int i = 0; i < files.Length; i++) {
                FileInfo currInfo = files[i];
                portraitTemplates.Add(currInfo.Name, SaveGame.Load<PortraitSettings>(currInfo.FullName));
            }
            templatesDropdown.ClearOptions();
            templatesDropdown.AddOptions(portraitTemplates.Keys.ToList());
        }
        public void OnValueChangedPortraitTemplate(int choice) {
            string chosenTemplateName = templatesDropdown.options[choice].text;
            PortraitSettings chosenSettings = portraitTemplates[chosenTemplateName];
            _character.SetPortraitSettings(chosenSettings);
            portrait.GeneratePortrait(_character);
        }
        //public void ApplyPortraitTemplate() {
        //    string chosenTemplateName = templatesDropdown.options[templatesDropdown.value].text;
        //    PortraitSettings chosenSettings = portraitTemplates[chosenTemplateName];
        //    _character.SetPortraitSettings(chosenSettings);
        //    portrait.GeneratePortrait(_character);
        //}
        #endregion

        #region Basic Info
        private void LoadDropdownOptions() {
            raceField.ClearOptions();
            genderField.ClearOptions();
            sexualityField.ClearOptions();
            roleField.ClearOptions();
            classField.ClearOptions();
            attributeChoicesDropdown.ClearOptions();
            moralityField.ClearOptions();

            raceField.AddOptions(Utilities.GetEnumChoices<RACE>());
            genderField.AddOptions(Utilities.GetEnumChoices<GENDER>());
            sexualityField.AddOptions(Utilities.GetEnumChoices<SEXUALITY>());
            roleField.AddOptions(Utilities.GetEnumChoices<CHARACTER_ROLE>());
            classField.AddOptions(Utilities.GetFileChoices(Utilities.dataPath + "CharacterClasses/", "*.json"));
            attributeChoicesDropdown.AddOptions(Utilities.GetEnumChoices<ATTRIBUTE>());
            moralityField.AddOptions(Utilities.GetEnumChoices<MORALITY>());
            LoadFactionDropdownOptions();
        }
        public void LoadFactionDropdownOptions() {
            factionField.ClearOptions();
            List<string> options = new List<string>();
            options.Add("Factionless");
            options.AddRange(FactionManager.Instance.allFactions.Select(x => x.name).ToList());
            factionField.AddOptions(options);
        }
        public void UpdateBasicInfo() {
            nameField.text = _character.name;
            raceField.value = Utilities.GetOptionIndex(raceField, _character.raceSetting.race.ToString());
            genderField.value = Utilities.GetOptionIndex(genderField, _character.gender.ToString());
            sexualityField.value = Utilities.GetOptionIndex(sexualityField, _character.sexuality.ToString());
            roleField.value = Utilities.GetOptionIndex(roleField, _character.role.roleType.ToString());
            moralityField.value = Utilities.GetOptionIndex(moralityField, _character.morality.ToString());

            classField.value = Utilities.GetOptionIndex(classField, _character.characterClass.className);
            string factionName = "Factionless";
            if (_character.faction != null) {
                factionName = _character.faction.name;
            }
            factionField.value = Utilities.GetOptionIndex(factionField, factionName);
            levelField.text = _character.level.ToString();
            otherInfoLbl.text = string.Empty;
            if (_character.homeArea == null) {
                otherInfoLbl.text += "Home: NONE";
            } else {
                otherInfoLbl.text += "Home: " + _character.homeArea.name;
                //otherInfoLbl.text += "(" + _character.homeLandmark.landmarkName + ")";
            }
            if (_character.party.specificLocation == null) {
                otherInfoLbl.text += "\nLocation: NONE";
            } else {
                otherInfoLbl.text += "\nLocation: " + _character.party.specificLocation.name;
            }
        }
        public void SetName(string newName) {
            _character.SetName(newName);
        }
        public void SetRace(int choice) {
            RACE newRace = (RACE)Enum.Parse(typeof(RACE), raceField.options[choice].text);
            _character.ChangeRace(newRace);
        }
        public void SetGender(int choice) {
            GENDER newGender = (GENDER)Enum.Parse(typeof(GENDER), genderField.options[choice].text);
            _character.ChangeGender(newGender);
        }
        public void SetSexuality(int choice) {
            SEXUALITY newSexuality = (SEXUALITY)Enum.Parse(typeof(SEXUALITY), sexualityField.options[choice].text);
            _character.SetSexuality(newSexuality);
        }
        public void SetRole(int choice) {
            CHARACTER_ROLE newRole = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), roleField.options[choice].text);
            _character.AssignRole(CharacterRole.GetRoleByRoleType(newRole));
        }
        public void SetClass(int choice) {
            string newClass = classField.options[choice].text;
            _character.ChangeClass(newClass);
        }
        public void SetFaction(int choice) {
            string factionName = factionField.options[choice].text;
            Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionName);
            if (_character.faction != null) {
                _character.faction.LeaveFaction(_character);
            }
            //_character.SetFaction(faction);
            if (faction != null) {
                faction.JoinFaction(_character);
            }
            WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        }
        public void SetMorality(int choice) {
            string moralityStr = moralityField.options[choice].text;
            MORALITY morality = (MORALITY)Enum.Parse(typeof(MORALITY), moralityStr);
            _character.SetMorality(morality);
        }
        public void SetLevel(string levelStr) {
            int newLevel;
            if (Int32.TryParse(levelStr, out newLevel)) {
                _character.SetLevel(newLevel);
            }
        }
        #endregion

        #region Relationship Info
        private void LoadRelationships() {
            Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(relationshipScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                GameObject.Destroy(children[i].gameObject);
            }
            foreach (KeyValuePair<AlterEgoData, CharacterRelationshipData> kvp in _character.relationships) {
                GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
                RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
                relItem.SetRelationship(kvp.Value);
            }
        }
        public void LoadRelationshipDropdowns() {
            List<string> options = new List<string>();
            charactersRelationshipDropdown.ClearOptions();
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character currCharacter = CharacterManager.Instance.allCharacters[i];
                if (currCharacter.id != _character.id && !_character.HasRelationshipWith(currCharacter)) {
                    options.Add(currCharacter.name);
                }
            }
            charactersRelationshipDropdown.AddOptions(options);
            if (charactersRelationshipDropdown.options.Count == 0) {
                createRelationshipBtn.interactable = false;
            } else {
                createRelationshipBtn.interactable = true;
            }

            relationshipTypesDropdown.ClearOptions();
            relationshipTypesDropdown.AddOptions(Utilities.GetEnumChoices<RELATIONSHIP_TRAIT>());
        }
        public void CreateRelationship() {
            string chosenCharacterName = charactersRelationshipDropdown.options[charactersRelationshipDropdown.value].text;
            string chosenRelType = relationshipTypesDropdown.options[relationshipTypesDropdown.value].text;
            Character chosenCharacter = CharacterManager.Instance.GetCharacterByName(chosenCharacterName);
            RELATIONSHIP_TRAIT rel = (RELATIONSHIP_TRAIT)Enum.Parse(typeof(RELATIONSHIP_TRAIT), chosenRelType);
            CharacterManager.Instance.CreateNewRelationshipBetween(_character, chosenCharacter, rel);
        }
        private void OnRelationshipCreated(Character character, RelationshipTrait gainedRel) {
            if (_character == null || character != _character) {
                return;
            }
            RelationshipEditorItem existingItem = GetRelationshipItem(character.GetCharacterRelationshipData(gainedRel.targetCharacter));
            if (existingItem != null) {
                existingItem.UpdateInfo();
            } else {
                GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
                RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
                relItem.SetRelationship(character.GetCharacterRelationshipData(gainedRel.targetCharacter));
            }
            LoadRelationshipDropdowns();
        }
        private void OnRelationshipRemoved(Character character, RELATIONSHIP_TRAIT rel, AlterEgoData target) {
            if (_character == null || character != _character) {
                return;
            }
            RelationshipEditorItem existingItem = GetRelationshipItem(character.GetCharacterRelationshipData(target));
            if (existingItem != null) {
                existingItem.UpdateInfo();
            }
            LoadRelationshipDropdowns();
        }
        public void OnAllRelationshipRemoved(Character character, Character target) {
            if (_character == null || !this.gameObject.activeSelf) {
                return;
            }
            RelationshipEditorItem itemToRemove = GetRelationshipItem(character.GetCharacterRelationshipData(target));
            if (itemToRemove != null) {
                GameObject.Destroy(itemToRemove.gameObject);
                LoadRelationshipDropdowns();
            }
        }
        private RelationshipEditorItem GetRelationshipItem(CharacterRelationshipData rel) {
            RelationshipEditorItem[] children = Utilities.GetComponentsInDirectChildren<RelationshipEditorItem>(relationshipScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                RelationshipEditorItem currItem = children[i];
                if (currItem.relationship == rel) {
                    return currItem;
                }
            }
            return null;
        }
        #endregion

        #region Equipment Info
        private void LoadEquipmentChoices() {
            List<string> choices = new List<string>();
            choices.AddRange(ItemManager.Instance.allWeapons.Keys);
            choices.AddRange(ItemManager.Instance.allArmors.Keys);

            equipmentChoicesDropdown.AddOptions(choices);
        }
        private void OnItemEquipped(Item item, Character character) {
            GameObject itemGO = GameObject.Instantiate(itemEditorPrefab, equipmentScrollView.content);
            ItemEditorItem itemComp = itemGO.GetComponent<ItemEditorItem>();
            itemComp.SetItem(item, character);
        }
        private void OnItemUnequipped(Item item, Character character) {
            GameObject.Destroy(GetEquipmentEditorItem(item).gameObject);
        }
        private ItemEditorItem GetEquipmentEditorItem(Item item) {
            ItemEditorItem[] children = Utilities.GetComponentsInDirectChildren<ItemEditorItem>(equipmentScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                ItemEditorItem currItem = children[i];
                if (currItem.item == item) {
                    return currItem;
                }
            }
            return null;
        }
        #endregion

        #region Inventory Info
        private void LoadInventoryChoices() {
            List<string> choices = new List<string>(ItemManager.Instance.allItems.Keys);
            inventoryChoicesDropdown.AddOptions(choices);
        }
        private void LoadInventory() {
            //Utilities.DestroyChildren(inventoryScrollView.content);
            //for (int i = 0; i < _character.inventory.Count; i++) {
            //    Item currItem = _character.inventory[i];
            //    OnItemObtained(currItem, _character);
            //}
        }
        public void AddInventory() {
            //string chosenItem = inventoryChoicesDropdown.options[inventoryChoicesDropdown.value].text;
            //Item item = ItemManager.Instance.allItems[chosenItem].CreateNewCopy();
            //_character.PickupItem(item);
        }
        private void OnItemObtained(Item item, Character character) {
            //GameObject itemGO = GameObject.Instantiate(itemEditorPrefab, inventoryScrollView.content);
            //ItemEditorItem itemComp = itemGO.GetComponent<ItemEditorItem>();
            //itemComp.SetItem(item, character);
            //itemComp.SetDeleteItemAction(() => _character.ThrowItem(item));
        }
        private void OnItemThrown(Item item, Character character) {
            GameObject.Destroy(GetInventoryEditorItem(item).gameObject);
        }
        private ItemEditorItem GetInventoryEditorItem(Item item) {
            ItemEditorItem[] children = Utilities.GetComponentsInDirectChildren<ItemEditorItem>(inventoryScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                ItemEditorItem currItem = children[i];
                if (currItem.item == item) {
                    return currItem;
                }
            }
            return null;
        }
        #endregion
    }
}

