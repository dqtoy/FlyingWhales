using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class POITestingUI : MonoBehaviour {
    //This script is used to test characters and actions
    //Most of the functions here will only work if there is a currently clicked/active character
    public RectTransform rt;
    public IPointOfInterest poi { get; private set; }
    public LocationGridTile gridTile { get; private set; }

    #region Utilities
    public void ShowUI(IPointOfInterest poi) {
        if (UIManager.Instance.characterInfoUI.activeCharacter != null) {
            this.poi = poi;
            UIManager.Instance.PositionTooltip(gameObject, rt, rt);
            gameObject.SetActive(true);
        }
    }
    public void ShowUI(LocationGridTile gridTile) {
        if (UIManager.Instance.characterInfoUI.activeCharacter != null) {
            this.gridTile = gridTile;
            UIManager.Instance.PositionTooltip(gameObject, rt, rt);
            gameObject.SetActive(true);
        }
    }
    public void HideUI() {
        gameObject.SetActive(false);
        this.poi = null;
        this.gridTile = null;
    }
    #endregion

    #region Character Testing
    public void KnockoutThisCharacter() {
        if(poi is Character) {
            UIManager.Instance.characterInfoUI.activeCharacter.CreateKnockoutJob(poi as Character);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void ChatWithThisCharacter() {
        if (poi is Character) {
            UIManager.Instance.characterInfoUI.activeCharacter.nonActionEventsComponent.ForceChatCharacter(poi as Character);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void InviteToMakeLove() {
        if (poi is Character) {
            Character target = poi as Character;
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.INVITE, target, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
            //if (UIManager.Instance.characterInfoUI.activeCharacter.HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
            //    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, target);
            //    job.SetCannotOverrideJob(true);
            //    UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job, false);
            //} else {
            //    Debug.LogError("Must be paramour or lover!");
            //}
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void StealFromThisCharacter() {
        if (poi is Character) {
            //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.STEAL_FROM_CHARACTER, poi);
            //job.SetCannotOverrideJob(true);
            //UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void DrinkBlood() {
        if (poi is Character) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNGER_RECOVERY_STARVING, INTERACTION_TYPE.DRINK_BLOOD, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void Feed() {
        if (poi is Character) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNGER_RECOVERY_STARVING, INTERACTION_TYPE.FEED, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    #endregion

    #region Tile Object Testing
    public void PoisonTable() {
        if (poi is Table) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, INTERACTION_TYPE.POISON, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError(poi.name + " is not a table!");
        }
        HideUI();
    }
    public void EatAtTable() {
        if (poi is Table) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNGER_RECOVERY_STARVING, INTERACTION_TYPE.EAT, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError(poi.name + " is not a table!");
        }
        HideUI();
    }
    public void Sleep() {
        if (poi is Bed) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TIREDNESS_RECOVERY, INTERACTION_TYPE.SLEEP, poi, UIManager.Instance.characterInfoUI.activeCharacter);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job);
        } else {
            Debug.LogError(poi.name + " is not a bed!");
        }
        HideUI();
    }
    #endregion

    #region Grid Tile Testing
    public void GoHere() {
        STRUCTURE_TYPE[] _notAllowedStructures = new STRUCTURE_TYPE[] { STRUCTURE_TYPE.INN, STRUCTURE_TYPE.DWELLING, STRUCTURE_TYPE.WAREHOUSE, STRUCTURE_TYPE.PRISON };
        UIManager.Instance.characterInfoUI.activeCharacter.marker.GoTo(this.gridTile, notAllowedStructures: _notAllowedStructures);
        HideUI();
    }
    #endregion
}
