using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;
using System.Linq;

public class TheProfaneUI : MonoBehaviour {

    [Header("General")]
    [SerializeField] private Toggle minionsToggle;
    [SerializeField] private Toggle cultistsToggle;

    [Header("Minions Tab")]
    [SerializeField] private GameObject minionsTabContentGO;
    [SerializeField] private ScrollRect minionsScrollRect;
    [SerializeField] private GameObject minionItemPrefab;
    [SerializeField] private ToggleGroup minionsToggleGroup;
    [SerializeField] private ScrollRect charactersScrollRect;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private HorizontalScrollSnap minionsTabScrollSnap;
    [SerializeField] private Button minionsTabNextBtn;
    [SerializeField] private GameObject minionsTabNextBtnDisabler;
    [SerializeField] private TextMeshProUGUI minionsTabNextBtnLbl;

    [Header("Cultist Tab")]
    [SerializeField] private GameObject cultistTabContentGO;
    [SerializeField] private ToggleGroup cultistsToggleGroup;
    [SerializeField] private ScrollRect cultistsScrollRect;
    [SerializeField] private ScrollRect cultistActionsScrollRect;
    [SerializeField] private GameObject stringItemPrefab;
    [SerializeField] private HorizontalScrollSnap cultistTabScrollSnap;
    [SerializeField] private Button cultistTabNextBtn;
    [SerializeField] private GameObject cultistTabNextBtnDisabler;
    [SerializeField] private TextMeshProUGUI cultistTabNextBtnLbl;

    [Header("Cooldown")]
    [SerializeField] private GameObject cooldownGO;
    [SerializeField] private TextMeshProUGUI cooldownLbl;
    public TheProfane profane { get; private set; }
    public Minion chosenMinion { get; private set; }
    public Character chosenCultist { get; private set; }
    public string chosenAction { get; private set; }

    private enum Minion_Tab_Step { Minion, Convert }
    private Minion_Tab_Step currentMinionTabStep;

    private enum Cultist_Tab_Step { Cultist, Action }
    private Cultist_Tab_Step currentCultistTabStep;

    #region General
    public void ShowTheProfaneUI(TheProfane profane) {
        this.profane = profane;
        if (profane.isInCooldown) {
            UpdateTheProfaneUI();
        }
        //UpdateMinionList();
        gameObject.SetActive(true);
        //Reset();
        minionsToggle.isOn = true; //switch to minions tab;
        OnToggleMinionsTab(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
    public void UpdateTheProfaneUI() {
        if (profane.isInCooldown) {
            cooldownGO.SetActive(true);
            cooldownLbl.text = "In Cooldown:\n" + Mathf.FloorToInt(((float)profane.currentCooldownTick / (float)profane.cooldownDuration) * 100f).ToString() + "%";
        } else {
            cooldownGO.SetActive(false);
        }
    }
    #endregion

    #region Tabs
    public void OnToggleMinionsTab(bool isOn) {
        minionsTabContentGO.SetActive(isOn);
        if (minionsTabContentGO.activeSelf) {
            ResetMinionsTab();
        }
    }
    public void OnToggleCultistTab(bool isOn) {
        cultistTabContentGO.SetActive(isOn);
        if (cultistTabContentGO.activeSelf) {
            ResetCultistTab();
        }
    }
    #endregion


    #region Minions Tab
    public void SetCurrentMinionTabStep(int page) {
        currentMinionTabStep = (Minion_Tab_Step)page;
        //update next and previous button;
        UpdateMinionTabNextButton(true);
    }
    private void ResetMinionsTab() {
        chosenMinion = null;
        minionsTabScrollSnap.GoToScreen(0);
        currentMinionTabStep = Minion_Tab_Step.Minion;
        UpdateMinionTabNextButton(true);
    }
    private void UpdateMinionTabNextButton(bool updateList = false) {
        if (currentMinionTabStep == Minion_Tab_Step.Minion) {
            minionsTabNextBtn.interactable = chosenMinion != null && !profane.isInCooldown;
            minionsTabNextBtnDisabler.SetActive(!minionsTabNextBtn.interactable);
            minionsTabNextBtnLbl.text = "Next";
            if (updateList) {
                UpdateMinionList();
            }
        } else if (currentMinionTabStep == Minion_Tab_Step.Convert) {
            minionsTabNextBtn.interactable = !profane.isInCooldown;
            minionsTabNextBtnDisabler.SetActive(!minionsTabNextBtn.interactable);
            minionsTabNextBtnLbl.text = "Next";
            if (updateList) {
                UpdateConvertToCultistList();
            }
        }
    }

    #region Minion
    private void UpdateMinionList() {
        Utilities.DestroyChildren(minionsScrollRect.content);
        List<Minion> minions = new List<Minion>(PlayerManager.Instance.player.minions);
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(minionItemPrefab.name, Vector3.zero, Quaternion.identity, minionsScrollRect.content);
            MinionCharacterItem item = go.GetComponent<MinionCharacterItem>();
            item.SetObject(currMinion.character);
            if (currMinion.assignedRegion == null) {
                //can be assigned
                item.SetAsToggle();
                item.ClearAllOnToggleActions();
                item.AddOnToggleAction(OnClickMinion);
                item.SetToggleGroup(minionsToggleGroup);
                if (chosenMinion == currMinion || chosenMinion == null) {
                    item.SetToggleState(true);
                    SetSelectedMinion(currMinion);
                }
                item.SetInteractableState(true);
            } else {
                //cannot be assigned
                item.SetToggleGroup(null);
                item.ClearAllOnToggleActions();
                if (chosenMinion == currMinion) {
                    item.SetToggleState(false);
                    SetSelectedMinion(null);
                }
                go.transform.SetAsLastSibling();
                item.SetInteractableState(false);
            }
        }
    }
    private void OnClickMinion(Character character, bool isOn) {
        if (isOn) {
            SetSelectedMinion(character.minion);
        }
    }
    private void SetSelectedMinion(Minion minion) {
        chosenMinion = minion;
        //update next button
        UpdateMinionTabNextButton();
    }
    #endregion

    #region Convert to Cultist
    /// <summary>
    /// Load the list of characters that can be turned into cultists.
    /// </summary>
    private void UpdateConvertToCultistList() {
        Utilities.DestroyChildren(charactersScrollRect.content);
        List<Character> allCharacters = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => !x.returnedToLife && !x.isDead && x.GetNormalTrait("Disillusioned", "Evil", "Treacherous") != null && x.GetNormalTrait("Blessed") == null && x.GetNormalTrait("Cultist") == null));
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, charactersScrollRect.content);
            CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
            item.SetObject(currCharacter);
            item.ClearAllOnClickActions();
            item.SetAsButton();
            item.SetInteractableState(CanBeConvertedToCultist(currCharacter));
            item.AddOnClickAction(OnChooseCharacterToConvert);
            item.AddHoverEnterAction((character) => UIManager.Instance.ShowSmallInfo("Conversion Cost: " + profane.GetConvertToCultistCost(character).ToString() + " mana"));
            item.AddHoverExitAction((character) => UIManager.Instance.HideSmallInfo());
            if (item.coverState) {
                item.transform.SetAsLastSibling();
            } else {
                item.transform.SetAsFirstSibling();
            }
        }
    }
    private void OnChooseCharacterToConvert(Character character) {
        chosenAction = "Convert to cultist";
        if (profane.isInCooldown) {
            PlayerUI.Instance.ShowGeneralConfirmation("In Cooldown", "The profane is currently on cooldown. Action will not proceed.");
        } else {
            //show confirmation.
            UIManager.Instance.ShowYesNoConfirmation("Confirm Action", "Are you sure you want to convert " + character.name + " into a cultist? ", onClickYesAction: () => profane.DoAction(character, chosenAction), showCover: true, layer: 25);
        }
    }
    private bool CanBeConvertedToCultist(Character character) {
        int manaCost = profane.GetConvertToCultistCost(character);
        if (PlayerManager.Instance.player.mana < manaCost) {
            return false;
        }
        if (character.GetNormalTrait("Evil") != null) {
            return true;
        } else if (character.GetNormalTrait("Disillusioned") != null) {
            return true;
        } else if (character.GetNormalTrait("Treacherous") != null) {
            Character factionLeader = character.faction.leader as Character;
            return character.HasRelationshipOfTypeWith(factionLeader, RELATIONSHIP_TRAIT.ENEMY);
        }
        return false;
    }
    #endregion

    #endregion

    #region Cultist Tab

    #region Cultists
    public void SetCurrentCultistTabStep(int page) {
        currentCultistTabStep = (Cultist_Tab_Step)page;
        //update next and previous button;
        UpdateCultistTabNextButton(true);
    }
    private void ResetCultistTab() {
        chosenCultist = null;
        cultistTabScrollSnap.GoToScreen(0);
        currentCultistTabStep = Cultist_Tab_Step.Cultist;
        UpdateCultistTabNextButton(true);
    }
    private void UpdateCultistTabNextButton(bool updateList = false) {
        if (currentCultistTabStep == Cultist_Tab_Step.Cultist) {
            cultistTabNextBtn.interactable = chosenCultist != null && !profane.isInCooldown;
            cultistTabNextBtnDisabler.SetActive(!cultistTabNextBtn.interactable);
            if (updateList) {
                UpdateCultistList();
            }
        } else if (currentCultistTabStep == Cultist_Tab_Step.Action) {
            cultistTabNextBtn.interactable = !profane.isInCooldown;
            cultistTabNextBtnDisabler.SetActive(!cultistTabNextBtn.interactable);
            if (updateList) {
                UpdatePossibleActions();
            }
        }
    }
    private void UpdateCultistList() {
        Utilities.DestroyChildren(cultistsScrollRect.content);
        List<Character> cultists = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => !x.returnedToLife && !x.isDead && x.GetNormalTrait("Cultist") != null && x.role.roleType != CHARACTER_ROLE.MINION));
        for (int i = 0; i < cultists.Count; i++) {
            Character currCultist = cultists[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, cultistsScrollRect.content);
            CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
            item.SetObject(currCultist);
            item.SetInteractableState(CanDoActionsToCharacter(currCultist));
            if (item.coverState) {
                //cannot do actions to cultist
                if (chosenCultist == currCultist) {
                    SetSelectedCultist(null);
                }
            } else {
                item.SetAsToggle();
                item.ClearAllOnToggleActions();
                item.AddOnToggleAction(OnToggleCultist);
                item.SetToggleGroup(cultistsToggleGroup);
                if (chosenCultist == currCultist || chosenCultist == null) {
                    item.SetToggleState(true);
                    SetSelectedCultist(currCultist);
                }
            }
        }
    }
    private bool CanDoActionsToCharacter(Character character) {
        return !character.currentParty.icon.isTravellingOutside && character.isAtHomeRegion;
    }
    private void OnToggleCultist(Character cultist, bool isOn) {
        if (isOn) {
            SetSelectedCultist(cultist);
        }
    }
    private void SetSelectedCultist(Character character) {
        chosenCultist = character;
        Debug.Log("Selected cultist is " + (chosenCultist?.name ?? "Null"));
        //update next button
        UpdateCultistTabNextButton();
    }
    #endregion

    #region Actions
    private void UpdatePossibleActions() {
        Utilities.DestroyChildren(cultistActionsScrollRect.content);
        //character is a cultist
        List<string> actions = GetPossibleActionsForCharacter(chosenCultist);
        for (int i = 0; i < actions.Count; i++) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(stringItemPrefab.name, Vector3.zero, Quaternion.identity, cultistActionsScrollRect.content);
            StringNameplateItem item = go.GetComponent<StringNameplateItem>();
            item.SetObject(actions[i]);
            item.ClearAllOnClickActions();
            item.AddOnClickAction(SetChosenAction);
            item.ClearAllHoverEnterActions();
            item.AddHoverEnterAction(ShowActionTooltip);
            item.ClearAllHoverExitActions();
            item.AddHoverExitAction(HideActionTooltip);
            item.SetAsButton();
        }
    }
    private void SetChosenAction(string action) {
        chosenAction = action;
        if (profane.isInCooldown) {
            PlayerUI.Instance.ShowGeneralConfirmation("In Cooldown", "The profane is currently on cooldown. Action will not proceed.");
        } else {
            string message = "Are you sure you want to ";
            if (chosenAction == "Corrupt") {
                message += chosenAction + " " + chosenCultist.name + "?";
            } else if (chosenAction == "Sabotage Faction Quest") {
                message += "instruct " + chosenCultist.name + " to sabotage " + Utilities.GetPronounString(chosenCultist.gender, PRONOUN_TYPE.POSSESSIVE, false) + " factions quest?";
            } else if (chosenAction == "Destroy Supply" || chosenAction == "Destroy Food") {
                message += "instruct " + chosenCultist.name + " to " + chosenAction + "?";
            }
            //show confirmation.
            UIManager.Instance.ShowYesNoConfirmation("Confirm Action", message, onClickYesAction: () => profane.DoAction(chosenCultist, chosenAction), showCover: true, layer: 25);
        }
    }
    private void ShowActionTooltip(string action) {
        if (action == "Corrupt") {
            UIManager.Instance.ShowSmallInfo("Turn this character into a minion. This character will become a demon of " + (chosenCultist.GetNormalTrait("Cultist") as Cultist).minionData.className);
        }
    }
    private void HideActionTooltip(string action) {
        UIManager.Instance.HideSmallInfo();
    }
    private List<string> GetPossibleActionsForCharacter(Character character) {
        List<string> actions = new List<string>();
        if (character.role.roleType != CHARACTER_ROLE.MINION) {
            actions.Add("Corrupt");
        }
        if (character.homeRegion.area != null && character.homeRegion.IsFactionHere(character.faction) && character.faction.activeQuest is DivineInterventionQuest &&
            !(character.faction.activeQuest as DivineInterventionQuest).jobQueue.HasJob(JOB_TYPE.SABOTAGE_FACTION)) {
            //only allow creation of sabotage faction quest if there is no job of that type yet.
            actions.Add("Sabotage Faction Quest");
        }
        if (character.homeRegion.area != null) {
            actions.Add("Destroy Supply");
            actions.Add("Destroy Food");
        }
        return actions;
    }
    #endregion

    #endregion
}
