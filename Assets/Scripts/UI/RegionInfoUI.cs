﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events.World_Events;
using Inner_Maps;
using Pathfinding.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RegionInfoUI : UIMenu {

    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI regionNameLbl;
    [SerializeField] private TextMeshProUGUI regionTypeLbl;
    [SerializeField] private LocationPortrait locationPortrait;

    [Header("Main")]
    [SerializeField] private TextMeshProUGUI descriptionLbl;
    [SerializeField] private TextMeshProUGUI featuresLbl;

    [Header("Characters")]
    [SerializeField] private ScrollRect charactersScrollView;
    [SerializeField] private GameObject characterItemPrefab;

    [Header("Events")]
    [SerializeField] private GameObject worldEventNameplatePrefab;
    [SerializeField] private ScrollRect worldEventsScrollView;

    [Header("Invasion")]
    [SerializeField] private Button invadeBtn;
    [SerializeField] private Image invadeProgress;
    
    [Header("Invasion Confirmation")]
    [FormerlySerializedAs("invConfrimationGO")] [SerializeField] private GameObject invConfirmationGO;
    [FormerlySerializedAs("invConfrimationTitleLbl")] [SerializeField] private TextMeshProUGUI invConfirmationTitleLbl;

    [Header("Demolition")]
    [SerializeField] private Button demolishBtn;

    [Header("Building")]
    [SerializeField] private Button buildBtn;
    [SerializeField] private Image buildProgress;

    [Header("Demonic Landmark")]
    [SerializeField] private PlayerBuildLandmarkUI playerBuildLandmarkUI;
    [SerializeField] private PlayerResearchUI playerResearchUI;
    [SerializeField] private TheProfaneUI theProfaneUI;
    [SerializeField] private PlayerSummonMinionUI playerSummonMinionUI;
    [SerializeField] private PlayerUpgradeUI playerUpgradeUI;
    [SerializeField] private TheEyeUI theEyeUI;
    [SerializeField] private TheFingersUI fingersUI;
    [SerializeField] private TheNeedlesUI needlesUI;

    public Region activeRegion { get; private set; }
    private List<WorldEventNameplate> activeWorldEventNameplates = new List<WorldEventNameplate>();

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_ENTERED_REGION, OnCharacterEnteredRegion);
        Messenger.AddListener<Character, Region>(Signals.CHARACTER_EXITED_REGION, OnCharacterExitedRegion);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_SPAWNED, OnWorldEventSpawned);
        Messenger.AddListener<Region, WorldEvent>(Signals.WORLD_EVENT_DESPAWNED, OnWorldEventDespawned);
        Messenger.AddListener<Region>(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, ShowAppropriateContentOnSignal);
    }

    public override void OpenMenu() {
        Region previousRegion = activeRegion;
        previousRegion?.ShowTransparentBorder();
        activeRegion = _data as Region;
        base.OpenMenu();
        UpdateBasicInfo();
        UpdateRegionInfo();
        UpdateCharacters();
        UpdateEventInfo();
        ShowAppropriateContentOnOpen();
        LoadActions();
    }
    public override void CloseMenu() {
        activeRegion.ShowTransparentBorder();
        activeRegion = null;
        base.CloseMenu();
    }

    public void UpdateInfo() {
        UpdateBasicInfo();
        UpdateRegionInfo();
    }

    #region Basic Info
    private void UpdateBasicInfo() {
        locationPortrait.SetLocation(activeRegion);
        regionNameLbl.text = activeRegion.name;
        regionTypeLbl.text = activeRegion.mainLandmark.specificLandmarkType.LandmarkToString();
    }
    #endregion

    #region Main
    private void UpdateRegionInfo() {
        descriptionLbl.text = activeRegion.description;
        featuresLbl.text = string.Empty;

        if (activeRegion.features.Count == 0) {
            featuresLbl.text = $"{featuresLbl.text}None";
        } else {
            for (int i = 0; i < activeRegion.features.Count; i++) {
                RegionFeature feature = activeRegion.features[i];
                if (i != 0) {
                    featuresLbl.text = $"{featuresLbl.text}, ";
                }
                featuresLbl.text = $"{featuresLbl.text}<link=\"{i}\">{feature.name}</link>";
            }
        }
    }
    public void OnHoverFeature(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            UIManager.Instance.ShowSmallInfo(activeRegion.features[index].description);
        }
    }
    public void OnHoverExitFeature() {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

    #region Characters
    private void OnCharacterEnteredRegion(Character character, Region region) {
        if (region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void OnCharacterExitedRegion(Character character, Region region) {
        if (region == activeRegion) {
            UpdateCharacters();
        }
    }
    private void UpdateCharacters() {
        Utilities.DestroyChildren(charactersScrollView.content);
        for (int i = 0; i < activeRegion.charactersAtLocation.Count; i++) {
            Character character = activeRegion.charactersAtLocation[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, charactersScrollView.content);
            CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
            item.SetObject(character);
            item.SetAsDefaultBehaviour();
        }
        OrderCharacterItems();
    }
    public void OrderCharacterItems() {
        List<CharacterNameplateItem> visitors = new List<CharacterNameplateItem>();

        List<CharacterNameplateItem> residents = new List<CharacterNameplateItem>();
        CharacterNameplateItem[] characterItems = Utilities.GetComponentsInDirectChildren<CharacterNameplateItem>(charactersScrollView.content.gameObject);
        for (int i = 0; i < characterItems.Length; i++) {
            CharacterNameplateItem currItem = characterItems[i];
            if (currItem.character.homeRegion != null && activeRegion.id == currItem.character.homeRegion.id) {
                residents.Add(currItem);
            } else {
                visitors.Add(currItem);
            }
        }

        List<CharacterNameplateItem> orderedVisitors = new List<CharacterNameplateItem>(visitors.OrderByDescending(x => x.character.level));
        List<CharacterNameplateItem> orderedResidents = new List<CharacterNameplateItem>(residents.OrderByDescending(x => x.character.level));

        List<CharacterNameplateItem> orderedItems = new List<CharacterNameplateItem>();
        orderedItems.AddRange(orderedVisitors);
        orderedItems.AddRange(orderedResidents);
        
        for (int i = 0; i < orderedItems.Count; i++) {
            CharacterNameplateItem currItem = orderedItems[i];
            currItem.transform.SetSiblingIndex(i);
        }
    }
    #endregion

    #region Invade
    private Minion chosenMinionToInvade;
//    private void UpdateMainBtnState() {
//        if (activeRegion.coreTile.isCorrupted) {
//            invadeBtn.gameObject.SetActive(false);
//            invadeProgress.gameObject.SetActive(false);
//            if (activeRegion.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
//                //building
//                buildBtn.gameObject.SetActive(true);
//                invadeBtn.gameObject.SetActive(false);
//                buildBtn.interactable = false;
//                buildProgress.gameObject.SetActive(true);
//                buildProgress.fillAmount = ((float)activeRegion.demonicBuildingData.currentDuration / (float)activeRegion.demonicBuildingData.buildDuration);
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
//                //if active region is corrupted and landmark type is none
//                //show build button
//                buildBtn.gameObject.SetActive(true);
//                buildProgress.gameObject.SetActive(false);
//                buildBtn.interactable = true;
//                demolishBtn.gameObject.SetActive(false);
//            } else if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.THE_PORTAL) {
//                //if active region is corrupted and landmark is the portal, just show demolish button, but do not allow interaction
//                demolishBtn.gameObject.SetActive(true);
//                demolishBtn.interactable = false;
//                buildBtn.gameObject.SetActive(false);
//                buildProgress.gameObject.SetActive(false);
//            } else {
//                //if the active region is corrupted and is not the demonic portal, show the demolish button
//                demolishBtn.gameObject.SetActive(true);
//                demolishBtn.interactable = true;
//                buildBtn.gameObject.SetActive(false);
//                buildProgress.gameObject.SetActive(false);
//            }
//        } else {
//            invadeBtn.gameObject.SetActive(true);
//            demolishBtn.gameObject.SetActive(false);
//            buildBtn.gameObject.SetActive(false);
//            invadeBtn.interactable = activeRegion.CanBeInvaded();
//            if (activeRegion.demonicInvasionData.beingInvaded) {
//                //invading
//                invadeProgress.gameObject.SetActive(true);
//                invadeProgress.fillAmount = ((float)activeRegion.demonicInvasionData.currentDuration / (float)activeRegion.mainLandmark.invasionTicks);
//            } else {
//                buildProgress.gameObject.SetActive(false);
//                invadeProgress.gameObject.SetActive(false);
//            }
//        }
//    }
    public void OnClickInvade() {
        if (activeRegion.area != null) {
            //simulate as if clicking the invade button while inside the are map
            InnerMapManager.Instance.ShowAreaMap(activeRegion.area);
            StartSettlementInvasion(activeRegion.area);
        } else {
            chosenMinionToInvade = null;
            UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), onClickAction: ChooseMinionForInvasion, validityChecker: CanMinionInvade,
                title: "Invasion (" + ((int)activeRegion.mainLandmark.invasionTicks / (int)GameManager.ticksPerHour).ToString() + " hours)\nChoose a minion that will invade " + activeRegion.name + ". NOTE: That minion will be unavailable while the invasion is ongoing.",
                onHoverAction: OnHoverEnterMinionInvade, onHoverExitAction: OnHoverExitMinionInvade,
                showCover: true, layer: 25);
        }
        
    }
    private bool CanMinionInvade(Character character) {
        return !character.minion.isAssigned && character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INVADER);
    }
    private void OnHoverEnterMinionInvade(Character character) {
        if (!CanMinionInvade(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = character.name + " is already doing something else.";
            } else if (!character.minion.deadlySin.CanDoDeadlySinAction(DEADLY_SIN_ACTION.INVADER)) {
                message = character.name + " does have not the required trait: Invader";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinionInvade(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void ChooseMinionForInvasion(object c) {
        Character character = c as Character;
        chosenMinionToInvade = character.minion;
        StartInvasion();
        UIManager.Instance.HideObjectPicker();
        
    }
    public void StartInvasion() {
        activeRegion.StartInvasion(chosenMinionToInvade);
        HideStartInvasionConfirmation();
        LoadActions();
    }
    public void HideStartInvasionConfirmation() {
        chosenMinionToInvade = null;
        invConfirmationGO.SetActive(false);
    }
    private void StartSettlementInvasion(Area area) {
        PlayerManager.Instance.player.StartInvasion(area);
        //ShowCombatAbilityUI();
    }
    public void StopSettlementInvasion() {
        
    }
    #endregion

    #region Demolish
    public void OnClickDemolish() {
        Region region = activeRegion;
        LandmarkManager.Instance.CreateNewLandmarkOnTile(region.coreTile, LANDMARK_TYPE.NONE, false);
        UIManager.Instance.ShowRegionInfo(region);
    }
    #endregion

    #region Events
    private void UpdateEventInfo() {
        if(worldEventsScrollView.content.childCount > 0) {
            Utilities.DestroyChildren(worldEventsScrollView.content);
        }
        if (activeRegion.activeEvent != null) {
            GenerateWorldEventNameplate(activeRegion);
        }
    }
    private void OnWorldEventSpawned(Region region, WorldEvent we) {
        if (isShowing && activeRegion == region) {
            GenerateWorldEventNameplate(region);
        }
    }
    private void OnWorldEventDespawned(Region region, WorldEvent we) {
        if (isShowing && activeRegion == region) {
            RemoveWorldEventNameplate(we);
        }
    }
    private void GenerateWorldEventNameplate(Region region) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(worldEventNameplatePrefab.name, Vector3.zero, Quaternion.identity, worldEventsScrollView.content);
        WorldEventNameplate item = go.GetComponent<WorldEventNameplate>();
        item.SetObject(region);
        activeWorldEventNameplates.Add(item);
    }
    private void RemoveWorldEventNameplate(WorldEvent worldEvent) {
        for (int i = 0; i < activeWorldEventNameplates.Count; i++) {
            WorldEventNameplate worldEventNameplate = activeWorldEventNameplates[i];
            if (worldEventNameplate.worldEvent == worldEvent) {
                activeWorldEventNameplates.RemoveAt(i);
                ObjectPoolManager.Instance.DestroyObject(worldEventNameplate.gameObject);
                break;
            }
        }
    }
    #endregion

    #region Demonic Landmarks
    private void ShowAppropriateContentOnSignal(Region region) {
        if (region == activeRegion) {
            LoadActions();
        }
    }
    private void ShowAppropriateContentOnOpen() {
//        if (overviewTabToggle.isOn) {
//            //UpdateDemonicLandmarkToggleState();
//            OnDemonicToggleStateChanged(overviewTabToggle.isOn);
//        }
    }
    #endregion

    #region For Testing
    public void ShowLocationInfo() {
        UtilityScripts.TestingUtilities.ShowLocationInfo(activeRegion);
    }
    public void HideLocationInfo() {
        UtilityScripts.TestingUtilities.HideLocationInfo();
    }
    #endregion

    #region Actions
    [Header("Actions")] 
    [SerializeField] private RectTransform actionsTransform;
    [SerializeField] private GameObject actionItemPrefab;
    private void LoadActions() {
        Utilities.DestroyChildren(actionsTransform);
        if (activeRegion.coreTile.isCorrupted) {
            //region is corrupted
            if (activeRegion.mainLandmark.specificLandmarkType == LANDMARK_TYPE.NONE) {
                //if it doesn't have a landmark, show the build action
                ActionItem item = AddNewAction("Build", null, () => playerBuildLandmarkUI.OnClickBuild(activeRegion));
                if (activeRegion.demonicBuildingData.landmarkType != LANDMARK_TYPE.NONE) {
                    int remaining = activeRegion.demonicBuildingData.buildDuration -
                                    activeRegion.demonicBuildingData.currentDuration;
                    item.SetAsUninteractableUntil(remaining);
                }
            } else {
                //if it has a landmark then assume that the landmark is a demonic landmark and show that landmarks action.
                //if the landmark is not the portal, then show the demolish action
                if (activeRegion.mainLandmark.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL) {
                    AddNewAction("Demolish", null, OnClickDemolish);    
                }
                ActionItem item;
                int remaining = 0;
                switch (activeRegion.mainLandmark.specificLandmarkType) {
                    case LANDMARK_TYPE.THE_SPIRE:
                        TheSpire spire = activeRegion.mainLandmark as TheSpire;
                        item = AddNewAction("Extract Spell", null, () => playerResearchUI.OnClickResearch(spire));
                        if (spire.isInCooldown) {
                            remaining = spire.cooldownDuration - spire.currentCooldownTick;
                            item.SetAsUninteractableUntil(remaining);
                        }
                        break;
                    case LANDMARK_TYPE.THE_EYE:
                        TheEye eye = activeRegion.mainLandmark as TheEye;
                        item = AddNewAction("Interfere", null, () => theEyeUI.OnClickInterfere(eye));
                        if (eye.isInCooldown) {
                            remaining = eye.cooldownDuration - eye.currentCooldownTick;
                            item.SetAsUninteractableUntil(remaining);
                        }
                        break;
                    case LANDMARK_TYPE.THE_ANVIL:
                        TheAnvil anvil = activeRegion.mainLandmark as TheAnvil;
                        item = AddNewAction("Upgrade", null, () => playerUpgradeUI.OnClickUpgrade(anvil));
                        if (string.IsNullOrEmpty(anvil.upgradeIdentifier) == false) {
                            item.SetAsUninteractableUntil(anvil.dueDate);
                        }
                        break;
                    case LANDMARK_TYPE.THE_PORTAL:
                        ThePortal portal = activeRegion.mainLandmark as ThePortal;
                        item = AddNewAction("Summon", null, () => playerSummonMinionUI.OnClickSummon(portal));
                        if (portal.currentMinionToSummonIndex != -1) {
                            remaining = portal.currentSummonDuration - portal.currentSummonTick;
                            item.SetAsUninteractableUntil(remaining);
                        }
                        break;
                    case LANDMARK_TYPE.THE_FINGERS:
                        TheFingers fingers = activeRegion.mainLandmark as TheFingers;
                        item = AddNewAction("Create Faction", null, () => fingersUI.OnClickCreate(fingers));
                        if (fingers.hasBeenActivated) {
                            remaining = fingers.duration - fingers.currentTick;
                            item.SetAsUninteractableUntil(remaining);
                        }
                        break;
                    case LANDMARK_TYPE.THE_NEEDLES:
                        TheNeedles needles = activeRegion.mainLandmark as TheNeedles;
                        item = AddNewAction("Convert to Mana", null, () => needlesUI.OnClickConvert(needles));
                        if (needles.isInCooldown) {
                            remaining = needles.cooldownDuration - needles.currentCooldownTick;
                            item.SetAsUninteractableUntil(remaining);
                        }
                        break;
                    case LANDMARK_TYPE.THE_PROFANE:
                        TheProfane profane = activeRegion.mainLandmark as TheProfane;
                        item = AddNewAction("Corrupt", null, () => theProfaneUI.OnClickCorrupt(profane));
                        if (profane.isInCooldown) {
                            remaining = profane.cooldownDuration - profane.currentCooldownTick;
                            item.SetAsUninteractableUntil(remaining);
                        }
                        break;
                }
            }
        } else {
            //region is not corrupted
            //show invade action.
            if (activeRegion.CanBeInvaded()) {
                ActionItem item = AddNewAction("Invade", null, OnClickInvade);
                if (activeRegion.demonicInvasionData.beingInvaded) {
                    int remaining = activeRegion.mainLandmark.invasionTicks -
                                    activeRegion.demonicInvasionData.currentDuration;
                    item.SetAsUninteractableUntil(remaining);
                }    
            }
        }
    }
    private ActionItem AddNewAction(string actionName, Sprite actionIcon, System.Action action) {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(actionItemPrefab.name, Vector3.zero,
            Quaternion.identity, actionsTransform);
        ActionItem item = obj.GetComponent<ActionItem>();
        item.SetAction(action, actionIcon, actionName);
        return item;
    }
    #endregion
}
