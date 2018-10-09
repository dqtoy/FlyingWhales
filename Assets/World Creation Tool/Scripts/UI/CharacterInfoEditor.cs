using BayatGames.SaveGameFree;
using ECS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

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
        [SerializeField] private Dropdown roleField;
        [SerializeField] private Dropdown classField;
        [SerializeField] private Dropdown factionField;
        [SerializeField] private Text otherInfoLbl;

        [Header("Relationship Info")]
        [SerializeField] private GameObject relationshipItemPrefab;
        [SerializeField] private ScrollRect relationshipScrollView;
        [SerializeField] private Dropdown charactersRelationshipDropdown;
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

        [Header("Hidden Desire")]
        [SerializeField] private Dropdown hiddenDesireChoicesDropdown;

        [Header("Secrets")]
        [SerializeField] private Dropdown secretChoicesDropdown;
        [SerializeField] private Text secretsSummaryLbl;

        [Header("Intel Reactions")]
        [SerializeField] private Dropdown intelChoicesDropdown;
        [SerializeField] private Dropdown intelEventsChoicesDropdown;
        [SerializeField] private Text intelReactionSummaryLbl;

        public Dictionary<string, PortraitSettings> portraitTemplates;

        public void Initialize() {
            Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_CREATED, OnRelationshipCreated);
            Messenger.AddListener<Relationship>(Signals.RELATIONSHIP_REMOVED, OnRelationshipRemoved);

            LoadEquipmentChoices();
            LoadInventoryChoices();
            LoadHiddenDesireChoices();
            LoadSecretChoices();
            LoadIntelReactionChoices();
        }

        public void UpdateInfo() {
            if (_character != null) {
                ShowCharacterInfo(_character);
            }
        }

        public void ShowCharacterInfo(Character character) {
            _character = character;
            portrait.GeneratePortrait(_character, 128);
            LoadDropdownOptions();
            //UpdatePortraitControls();
            UpdateBasicInfo();
            UpdateHiddenDesire();
            UpdateSecrets();
            UpdateIntelReactions();
            LoadRelationships();
            LoadCharacters();
            LoadEquipment();
            LoadInventory();
            LoadAttributeSummary();
            LoadTemplateChoices();
            Messenger.AddListener<Item, Character>(Signals.ITEM_EQUIPPED, OnItemEquipped);
            Messenger.AddListener<Item, Character>(Signals.ITEM_UNEQUIPPED, OnItemUnequipped);
            Messenger.AddListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
            Messenger.AddListener<Item, Character>(Signals.ITEM_THROWN, OnItemThrown);
            this.gameObject.SetActive(true);
        }
        public void Close() {
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_EQUIPPED, OnItemEquipped);
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_UNEQUIPPED, OnItemUnequipped);
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_THROWN, OnItemThrown);
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
            portrait.GeneratePortrait(_character, 128);
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
            roleField.ClearOptions();
            classField.ClearOptions();
            attributeChoicesDropdown.ClearOptions();

            raceField.AddOptions(Utilities.GetEnumChoices<RACE>());
            genderField.AddOptions(Utilities.GetEnumChoices<GENDER>());
            roleField.AddOptions(Utilities.GetEnumChoices<CHARACTER_ROLE>());
            classField.AddOptions(Utilities.GetFileChoices(Utilities.dataPath + "CharacterClasses/", "*.json"));
            attributeChoicesDropdown.AddOptions(Utilities.GetEnumChoices<ATTRIBUTE>());
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
            roleField.value = Utilities.GetOptionIndex(roleField, _character.role.roleType.ToString());

            classField.value = Utilities.GetOptionIndex(classField, _character.characterClass.className);
            string factionName = "Factionless";
            if (_character.faction != null) {
                factionName = _character.faction.name;
            }
            factionField.value = Utilities.GetOptionIndex(factionField, factionName);
            otherInfoLbl.text = string.Empty;
            if (_character.homeLandmark == null) {
                otherInfoLbl.text += "Home: NONE";
            } else {
                otherInfoLbl.text += "Home: " + _character.homeLandmark.landmarkName.ToString();
                //otherInfoLbl.text += "(" + _character.homeLandmark.landmarkName + ")";
            }
            if (_character.party.specificLocation == null) {
                otherInfoLbl.text += "\nLocation: NONE";
            } else {
                otherInfoLbl.text += "\nLocation: " + _character.party.specificLocation.ToString();
                if (_character.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                    if (_character.IsDefending(_character.specificLocation as BaseLandmark)) {
                        otherInfoLbl.text += " (Defending)";
                    }
                }
            }
        }
        public void SetName(string newName) {
            _character.SetName(newName);
        }
        public void SetRace(int choice) {
            RACE newRace = (RACE)Enum.Parse(typeof(RACE), raceField.options[choice].text);
            _character.ChangeRace(newRace);
            LoadAttributeSummary();
        }
        public void SetGender(int choice) {
            GENDER newGender = (GENDER)Enum.Parse(typeof(GENDER), genderField.options[choice].text);
            _character.ChangeGender(newGender);
        }
        public void SetRole(int choice) {
            CHARACTER_ROLE newRole = (CHARACTER_ROLE)Enum.Parse(typeof(CHARACTER_ROLE), roleField.options[choice].text);
            _character.AssignRole(newRole);
        }
        public void SetClass(int choice) {
            string newClass = classField.options[choice].text;
            _character.ChangeClass(newClass);
        }
        public void SetFaction(int choice) {
            string factionName = factionField.options[choice].text;
            Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionName);
            if (_character.faction != null) {
                _character.faction.RemoveCharacter(_character);
            }
            _character.SetFaction(faction);
            if (faction != null) {
                faction.AddNewCharacter(_character);
            }
            WorldCreatorUI.Instance.editFactionsMenu.UpdateItems();
        }
        #endregion

        #region Relationship Info
        private void LoadRelationships() {
            Transform[] children = Utilities.GetComponentsInDirectChildren<Transform>(relationshipScrollView.content.gameObject);
            for (int i = 0; i < children.Length; i++) {
                GameObject.Destroy(children[i].gameObject);
            }
            foreach (KeyValuePair<Character, Relationship> kvp in _character.relationships) {
                GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
                RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
                relItem.SetRelationship(kvp.Value);
            }
        }
        public void LoadCharacters() {
            List<string> options = new List<string>();
            charactersRelationshipDropdown.ClearOptions();
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character currCharacter = CharacterManager.Instance.allCharacters[i];
                if (currCharacter.id != _character.id && _character.GetRelationshipWith(currCharacter) == null) {
                    options.Add(currCharacter.name);
                }
            }
            charactersRelationshipDropdown.AddOptions(options);
            if (charactersRelationshipDropdown.options.Count == 0) {
                createRelationshipBtn.interactable = false;
            } else {
                createRelationshipBtn.interactable = true;
            }
        }
        public void CreateRelationship() {
            string chosenCharacterName = charactersRelationshipDropdown.options[charactersRelationshipDropdown.value].text;
            Character chosenCharacter = CharacterManager.Instance.GetCharacterByName(chosenCharacterName);
            CharacterManager.Instance.CreateNewRelationshipTowards(_character, chosenCharacter);
        }
        private void OnRelationshipCreated(Relationship newRel) {
            if (_character == null) {
                return;
            }
            GameObject relItemGO = GameObject.Instantiate(relationshipItemPrefab, relationshipScrollView.content);
            RelationshipEditorItem relItem = relItemGO.GetComponent<RelationshipEditorItem>();
            relItem.SetRelationship(newRel);
            LoadCharacters();
        }
        public void OnRelationshipRemoved(Relationship removedRel) {
            if (_character == null || !this.gameObject.activeSelf) {
                return;
            }
            RelationshipEditorItem itemToRemove = GetRelationshipItem(removedRel);
            if (itemToRemove != null) {
                GameObject.Destroy(itemToRemove.gameObject);
                LoadCharacters();
            }
        }
        private RelationshipEditorItem GetRelationshipItem(Relationship rel) {
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
        private void LoadEquipment() {
            Utilities.DestroyChildren(equipmentScrollView.content);
            for (int i = 0; i < _character.equippedItems.Count; i++) {
                Item currItem = _character.equippedItems[i];
                OnItemEquipped(currItem, _character);
            }
        }
        public void AddEquipment() {
            string chosenItem = equipmentChoicesDropdown.options[equipmentChoicesDropdown.value].text;
            Item item = ItemManager.Instance.allItems[chosenItem].CreateNewCopy();
            if (!_character.EquipItem(item)) {
                WorldCreatorUI.Instance.messageBox.ShowMessageBox(MESSAGE_BOX.OK, "Equipment error", "Cannot equip " + item.itemName);
            }
        }
        private void OnItemEquipped(Item item, Character character) {
            GameObject itemGO = GameObject.Instantiate(itemEditorPrefab, equipmentScrollView.content);
            ItemEditorItem itemComp = itemGO.GetComponent<ItemEditorItem>();
            itemComp.SetItem(item, character);
            itemComp.SetDeleteItemAction(() => character.UnequipItem(item));
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
            Utilities.DestroyChildren(inventoryScrollView.content);
            for (int i = 0; i < _character.inventory.Count; i++) {
                Item currItem = _character.inventory[i];
                OnItemObtained(currItem, _character);
            }
        }
        public void AddInventory() {
            string chosenItem = inventoryChoicesDropdown.options[inventoryChoicesDropdown.value].text;
            Item item = ItemManager.Instance.allItems[chosenItem].CreateNewCopy();
            _character.PickupItem(item);
        }
        private void OnItemObtained(Item item, Character character) {
            GameObject itemGO = GameObject.Instantiate(itemEditorPrefab, inventoryScrollView.content);
            ItemEditorItem itemComp = itemGO.GetComponent<ItemEditorItem>();
            itemComp.SetItem(item, character);
            itemComp.SetDeleteItemAction(() => _character.ThrowItem(item));
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

        #region Attribute Info
        private void LoadAttributeSummary() {
            attributeSummary.text = string.Empty;
            for (int i = 0; i < _character.attributes.Count; i++) {
                Attribute currAttribute = _character.attributes[i];
                attributeSummary.text += currAttribute.attribute.ToString() + "\n";
            }
        }
        public void OnClickAddRemoveAttribute() {
            int choice = attributeChoicesDropdown.value;
            ATTRIBUTE attribute = (ATTRIBUTE)Enum.Parse(typeof(ATTRIBUTE), attributeChoicesDropdown.options[choice].text);
            if (_character.GetAttribute(attribute) != null) {
                //remove
                _character.RemoveAttribute(attribute);
            } else {
                //add
                _character.AddAttribute(attribute);
            }
            LoadAttributeSummary();
        }
        #endregion

        #region Hidden Desire
        private void LoadHiddenDesireChoices() {
            hiddenDesireChoicesDropdown.ClearOptions();
            hiddenDesireChoicesDropdown.AddOptions(Utilities.GetEnumChoices<HIDDEN_DESIRE>(true));
        }
        public void SetHiddenDesire(int choice) {
            HIDDEN_DESIRE desire = (HIDDEN_DESIRE)Enum.Parse(typeof(HIDDEN_DESIRE), hiddenDesireChoicesDropdown.options[choice].text);
            CharacterManager.Instance.SetHiddenDesireForCharacter(desire, _character);
        }
        private void UpdateHiddenDesire() {
            if (_character.hiddenDesire == null) {
                hiddenDesireChoicesDropdown.value = Utilities.GetOptionIndex(hiddenDesireChoicesDropdown, "NONE");
            } else {
                hiddenDesireChoicesDropdown.value = Utilities.GetOptionIndex(hiddenDesireChoicesDropdown, _character.hiddenDesire.type.ToString());
            }
        }
        #endregion

        #region Secrets
        private void LoadSecretChoices() {
            secretChoicesDropdown.ClearOptions();

            List<string> secretsChoices = new List<string>();
            foreach (KeyValuePair<int, Secret> kvp in SecretManager.Instance.secretLookup) {
                secretsChoices.Add(kvp.Key + " - " + kvp.Value.name);
            }
            secretChoicesDropdown.AddOptions(secretsChoices);
        }
        public void AddRemoveSecrets() {
            int choice = secretChoicesDropdown.value;
            string chosen = secretChoicesDropdown.options[choice].text;
            int secretID = Int32.Parse(chosen[0].ToString());
            if (_character.HasSecret(secretID)) {
                _character.RemoveSecret(secretID);
            } else {
                _character.AddSecret(secretID);
            }
            UpdateSecrets();
        }
        private void UpdateSecrets() {
            secretsSummaryLbl.text = string.Empty;
            for (int i = 0; i < _character.secrets.Count; i++) {
                secretsSummaryLbl.text += _character.secrets[i].id + " - " +  _character.secrets[i].name + " - " + _character.secrets[i].description + "\n";
            }
        }
        #endregion

        #region Intel Reactions
        private void LoadIntelReactionChoices() {
            intelChoicesDropdown.ClearOptions();
            intelEventsChoicesDropdown.ClearOptions();

            List<string> intelChoices = new List<string>();
            foreach (KeyValuePair<int, Intel> kvp in IntelManager.Instance.intelLookup) {
                intelChoices.Add(kvp.Key.ToString() + " - " + kvp.Value.name);
            }

            intelChoicesDropdown.AddOptions(intelChoices);
            intelEventsChoicesDropdown.AddOptions(Utilities.GetEnumChoices<GAME_EVENT>());
        }
        public void AddEditIntelReaction() {
            string chosenIntelID = secretChoicesDropdown.options[intelChoicesDropdown.value].text;
            int intelID = Int32.Parse(chosenIntelID[0].ToString());
            GAME_EVENT chosenEvent = (GAME_EVENT)Enum.Parse(typeof(GAME_EVENT), intelEventsChoicesDropdown.options[intelEventsChoicesDropdown.value].text);
            _character.AddIntelReaction(intelID, chosenEvent);
            UpdateIntelReactions();
        }
        public void RemoveIntelReaction() {
            string chosenIntelID = secretChoicesDropdown.options[intelChoicesDropdown.value].text;
            int intelID = Int32.Parse(chosenIntelID[0].ToString());
            _character.RemoveIntelReaction(intelID);
            UpdateIntelReactions();
        }
        private void UpdateIntelReactions() {
            intelReactionSummaryLbl.text = string.Empty;
            foreach (KeyValuePair<int, GAME_EVENT> kvp in _character.intelReactions) {
                Intel intel = IntelManager.Instance.GetIntel(kvp.Key);
                intelReactionSummaryLbl.text += kvp.Key + "(" + intel.name + ") - " + kvp.Value.ToString() + "\n";
            }
        }
        #endregion
    }
}

