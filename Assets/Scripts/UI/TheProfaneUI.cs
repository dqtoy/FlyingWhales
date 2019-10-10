using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;
using System.Linq;

public class TheProfaneUI : MonoBehaviour {

    [Header("General")]
    [SerializeField] private HorizontalScrollSnap scrollSnap;
    [SerializeField] private Button nextBtn;
    [SerializeField] private GameObject nextBtnDisabler;
    [SerializeField] private TextMeshProUGUI nextBtnLbl;

    [Header("Minion")]
    [SerializeField] private ScrollRect minionsScrollRect;
    [SerializeField] private GameObject minionItemPrefab;
    [SerializeField] private ToggleGroup minionsToggleGroup;

    [Header("Cultist")]
    [SerializeField] private ScrollRect charactersScrollRect;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ToggleGroup cultistsToggleGroup;

    [Header("Actions")]
    [SerializeField] private ScrollRect actionsScrollRect;
    [SerializeField] private GameObject stringItemPrefab;

    [Header("Cooldown")]
    [SerializeField] private GameObject cooldownGO;
    [SerializeField] private TextMeshProUGUI cooldownLbl;
    public TheProfane profane { get; private set; }
    public Minion chosenMinion { get; private set; }
    public Character chosenCharacter { get; private set; }
    public string chosenAction { get; private set; }

    private enum Profane_Step { Minion, Character, Action }
    private Profane_Step currentStep;

    #region General
    public void ShowTheProfaneUI(TheProfane profane) {
        this.profane = profane;
        if (profane.isInCooldown) {
            UpdateTheProfaneUI();
        }
        //UpdateMinionList();
        gameObject.SetActive(true);
        Reset();
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
    public void SetCurrentStep(int page) {
        currentStep = (Profane_Step)page;
        //update next and previous button;
        UpdateNextButton(true);
    }
    private void UpdateNextButton(bool updateList = false) {
        if (currentStep == Profane_Step.Minion) {
            nextBtn.interactable = chosenMinion != null && !profane.isInCooldown;
            nextBtnDisabler.SetActive(!nextBtn.interactable);
            nextBtnLbl.text = "Next";
            if (updateList) {
                UpdateMinionList();
            }
        } else if (currentStep == Profane_Step.Character) {
            nextBtn.interactable = chosenCharacter != null && !profane.isInCooldown;
            nextBtnDisabler.SetActive(!nextBtn.interactable);
            nextBtnLbl.text = "Next";
            if (updateList) {
                UpdateCharacterList();
            }
        } else if (currentStep == Profane_Step.Action) {
            if (updateList) {
                UpdatePossibleActions();
            }
        }
    }
    private void Reset() {
        chosenMinion = null;
        chosenCharacter = null;
        scrollSnap.GoToScreen(0);
        currentStep = Profane_Step.Minion;
        UpdateNextButton(true);
    }
    #endregion

    #region Minion
    private void UpdateMinionList() {
        Utilities.DestroyChildren(minionsScrollRect.content);
        List<Minion> minions = new List<Minion>(PlayerManager.Instance.player.minions);
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(minionItemPrefab.name, Vector3.zero, Quaternion.identity, minionsScrollRect.content);
            MinionCharacterItem item = go.GetComponent<MinionCharacterItem>();
            item.SetCharacter(currMinion.character);
            if (currMinion.assignedRegion == null) {
                //can be assigned
                item.SetAsToggle(minionsToggleGroup);
                item.AddOnToggleAction(() => OnClickMinion(currMinion), true);
                if (chosenMinion == currMinion || chosenMinion == null) {
                    item.SetToggleState(true);
                    SetSelectedMinion(currMinion);
                }
                item.SetCoverState(false);
            } else {
                //cannot be assigned
                item.ResetToggle();
                item.ClearClickActions();
                if (chosenMinion == currMinion) {
                    SetSelectedMinion(null);
                    item.SetToggleState(false);
                }
                go.transform.SetAsLastSibling();
                //cannot be assigned
                item.SetCoverState(true);
            }
        }
    }
    private void OnClickMinion(Minion minion) {
        if (chosenMinion == minion) {
            SetSelectedMinion(null);
        } else {
            SetSelectedMinion(minion);
        }
    }
    private void SetSelectedMinion(Minion minion) {
        chosenMinion = minion;
        //update next button
        UpdateNextButton();
    }
    #endregion

    #region Cultist
    private void UpdateCharacterList() {
        Utilities.DestroyChildren(charactersScrollRect.content);
        List<Character> allCharacters = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => !x.returnedToLife && !x.isDead && x.role.roleType != CHARACTER_ROLE.MINION && x.GetNormalTrait("Disillusioned", "Evil", "Treacherous") != null && x.GetNormalTrait("Blessed") == null));
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, charactersScrollRect.content);
            CharacterItem item = go.GetComponent<CharacterItem>();
            item.SetCharacter(currCharacter);
            item.ClearClickActions();
            item.SetCoverState(!CanDoActionsToCharacter(currCharacter));
            if (item.coverState) {
                //cannot be chosen
                item.ResetToggle();
                go.transform.SetAsLastSibling();
                if (chosenCharacter == currCharacter) {
                    SetSelectedCharacter(null);
                    item.SetToggleState(false);
                }
            } else {
                //can be chosen
                go.transform.SetAsFirstSibling();
                item.SetAsToggle(minionsToggleGroup);
                item.AddOnToggleAction(() => OnClickCharacter(currCharacter), true);
                if (chosenCharacter == currCharacter || chosenCharacter == null) {
                    SetSelectedCharacter(currCharacter);
                    item.SetToggleState(true);
                }
            }
           
        }
    }
    private void OnClickCharacter(Character character) {
        if (chosenCharacter == character) {
            //SetSelectedCharacter(null);
        } else {
            SetSelectedCharacter(character);
        }
    }
    private void SetSelectedCharacter(Character character) {
        chosenCharacter = character;
        Debug.Log("Selected Character is " + (chosenCharacter?.name ?? "Null"));
        //update next button
        UpdateNextButton();
    }
    private bool CanDoActionsToCharacter(Character character) {
        if (character.GetNormalTrait("Cultist") == null) {
            //character is not yet a cultist
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
        } else {
            //character is a cultist
            return true;
        }
        return false;
    }
    #endregion

    #region Actions
    private void UpdatePossibleActions() {
        Utilities.DestroyChildren(actionsScrollRect.content);
        //character is a cultist
        List<string> actions = GetPossibleActionsForCharacter(chosenCharacter);
        for (int i = 0; i < actions.Count; i++) {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(stringItemPrefab.name, Vector3.zero, Quaternion.identity, actionsScrollRect.content);
            StringPickerItem item = go.GetComponent<StringPickerItem>();
            item.SetString(actions[i], string.Empty);
            item.onClickAction = SetChosenAction;
        }
    }
    private void SetChosenAction(string action) {
        chosenAction = action;
        if (profane.isInCooldown) {
            PlayerUI.Instance.ShowGeneralConfirmation("In Cooldown", "The profane is currently on cooldown. Action will not proceed.");
        } else {
            string message = "Are you sure you want to ";
            if (chosenAction == "Convert to cultist") {
                message += "convert " + chosenCharacter.name + " into a cultist?";
            } else if (chosenAction == "Corrupt") {
                message += chosenAction + " " + chosenCharacter.name + "?";
            } else if (chosenAction == "Sabotage Faction Quest") {
                message += " instruct " + chosenCharacter.name + " to sabotage " + Utilities.GetPronounString(chosenCharacter.gender, PRONOUN_TYPE.POSSESSIVE, false) + " factions quest?";
            } else if (chosenAction == "Destroy Supply" || chosenAction == "Destroy Food") {
                message += " instruct " + chosenCharacter.name + " to " + chosenAction + "?";
            }
            //show confirmation.
            UIManager.Instance.ShowYesNoConfirmation("Confirm Action", message, onClickYesAction: OnConfirmAction, showCover: true, layer: 25);
        }
    }
    private void OnConfirmAction() {
        profane.DoAction(chosenCharacter, chosenAction);
    }
    private List<string> GetPossibleActionsForCharacter(Character character) {
        List<string> actions = new List<string>();
        if (character.GetNormalTrait("Cultist") == null) {
            actions.Add("Convert to cultist");
        } else {
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
        }
        return actions;
    }
    #endregion
}
