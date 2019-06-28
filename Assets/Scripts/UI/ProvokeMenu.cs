using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvokeMenu : MonoBehaviour {

    [Header("Main")]
    [SerializeField] private ScrollRect dialogScrollView;
    [SerializeField] private GameObject dialogItemPrefab;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI instructionLbl;

    private Character targetCharacter;
    private Character actor;

    public void Open(Character targetCharacter, Character actor) {
        this.gameObject.SetActive(true);

        this.targetCharacter = targetCharacter;
        this.actor = actor;
        instructionLbl.text = "Provoke " + targetCharacter.name;

        Utilities.DestroyChildren(dialogScrollView.content);

        string targetDialogText = string.Empty;
        string actorDialogText = string.Empty;

        ProvokeAction(ref targetDialogText, ref actorDialogText);

        GameObject actorDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem actorItem = actorDialog.GetComponent<DialogItem>();
        actorItem.SetData(actor, actorDialogText, DialogItem.Position.Right);

        GameObject targetDialog = ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content);
        DialogItem item = targetDialog.GetComponent<DialogItem>();
        item.SetData(targetCharacter, targetDialogText);

        GameManager.Instance.SetPausedState(true);
    }

    private void ProvokeAction(ref string targetText, ref string actorText) {
        bool succeedProvoke = false;
        if (targetCharacter.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.ENEMY, false)) {
            //succeedProvoke = true;
            CHARACTER_MOOD currentMood = targetCharacter.currentMoodType;
            if (currentMood == CHARACTER_MOOD.GREAT) {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 70) {
                    actorText = "You should take revenge on your enemies.";
                    targetText = "I am too happy right now to even care about my enemies.";
                } else {
                    succeedProvoke = true;
                }
            } else if (currentMood == CHARACTER_MOOD.GOOD) {
                int chance = UnityEngine.Random.Range(0, 2);
                if (chance == 0) {
                    actorText = "You should take revenge on your enemies.";
                    targetText = "I am too happy right now to even care about my enemies.";
                } else {
                    succeedProvoke = true;
                }
            } else {
                succeedProvoke = true;
            }
        } else {
            actorText = "You should take revenge on your enemies.";
            targetText = "Sorry, I don't have any.";
        }

        if (succeedProvoke) {
            List<Character> enemyCharacters = targetCharacter.GetCharactersWithRelationship(RELATIONSHIP_TRAIT.ENEMY).Where(x => !x.isDead).ToList();
            Character chosenCharacter = null;
            while (chosenCharacter == null && enemyCharacters.Count > 0) {
                int index = UnityEngine.Random.Range(0, enemyCharacters.Count);
                Character character = enemyCharacters[index];
                if (character.HasJobTargettingThisCharacter(JOB_TYPE.UNDERMINE_ENEMY) || targetCharacter.jobQueue.HasJob(JOB_TYPE.UNDERMINE_ENEMY, character)) {
                    enemyCharacters.RemoveAt(index);
                } else {
                    chosenCharacter = character;
                }
            }
            if (chosenCharacter == null) {
                actorText = "You should take revenge on your enemies.";
                if (targetCharacter.jobQueue.HasJob(JOB_TYPE.UNDERMINE_ENEMY)) {
                    targetText = "That's exactly what I'm doing!"; //Don't tell me what to do!
                } else {
                    targetText = "I should, but I rather let them fight each other.";
                }
            } else {
                actorText = chosenCharacter.name + " is living " + Utilities.GetPronounString(chosenCharacter.gender, PRONOUN_TYPE.POSSESSIVE, false)
                    + " best life. Are you just gonna let your enemy be happy?";
                targetText = "I will not allow it! I'll take " + Utilities.GetPronounString(chosenCharacter.gender, PRONOUN_TYPE.OBJECTIVE, false) + " down with me!";

                //GoapPlanJob job = new GoapPlanJob("Undermine Enemy", new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = chosenCharacter });
                //job.SetCannotOverrideJob(true);
                //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                //targetCharacter.jobQueue.AddJobInQueue(job, false);
                //targetCharacter.jobQueue.ProcessFirstJobInQueue(targetCharacter);

                targetCharacter.CreateUndermineJobOnly(chosenCharacter, "provoke");

                //Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "provoke");
                //addLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //addLog.AddToFillers(chosenCharacter, chosenCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                //addLog.AddLogToInvolvedObjects();

                //PlayerManager.Instance.player.ShowNotification(addLog);
            }
        }
    }

    public void Close() {
        //UIManager.Instance.SetCoverState(false);
        //UIManager.Instance.SetSpeedTogglesState(true);
        this.gameObject.SetActive(false);
        GameManager.Instance.SetPausedState(false);
    }
}
