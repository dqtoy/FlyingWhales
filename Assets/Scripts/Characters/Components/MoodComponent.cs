using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoodComponent {
	
	private readonly Character _owner;
	public int moodValue { get; private set; }
	
	private bool _isInNormalMood;
	private bool _isInLowMood;
	private bool _isInCriticalMood;

	private float _currentLowMoodEffectChance;
	private float _currentCriticalMoodEffectChance;

	public MOOD_STATE moodState {
		get {
			if (_isInNormalMood) {
				return MOOD_STATE.NORMAL;
			} else if (_isInLowMood) {
				return MOOD_STATE.LOW;
			} else if (_isInCriticalMood) {
				return MOOD_STATE.CRITICAL;
			} else {
				throw new Exception($"Problem determining {_owner.name}'s mood. Because all switches are set to false.");
			}
		}
	}
	
	public MoodComponent(Character owner) {
		_owner = owner;
	}
	public void SetMoodValue(int amount) {
		moodValue = amount;
		moodValue = Mathf.Clamp(moodValue, 1, 100);
		// OnMoodChanged();
	}
	public void AdjustMoodValue(int amount, Trait fromTrait, ActualGoapNode triggerAction = null) {
		moodValue += amount;
		moodValue = Mathf.Clamp(moodValue, 1, 100);
		// OnMoodChanged();
	}
	public CHARACTER_MOOD ConvertCurrentMoodValueToType() {
		return _owner.moodComponent.ConvertMoodValueToType(moodValue);
	}
	private CHARACTER_MOOD ConvertMoodValueToType(int amount) {
		if (amount >= 1 && amount < 26) {
			return CHARACTER_MOOD.DARK;
		} else if (amount >= 26 && amount < 51) {
			return CHARACTER_MOOD.BAD;
		} else if (amount >= 51 && amount < 76) {
			return CHARACTER_MOOD.GOOD;
		} else {
			return CHARACTER_MOOD.GREAT;
		}
	}

	#region Loading
	public void Load(SaveDataCharacter saveDataCharacter) {
		SetMoodValue(saveDataCharacter.moodValue);
	}
	#endregion

	#region Events
	private void OnMoodChanged() {
		if (moodValue >= EditableValuesManager.Instance.normalMoodMinThreshold) {
			if (_isInNormalMood == false) {
				EnterNormalMood();	
			}
		} else if (moodValue >= EditableValuesManager.Instance.lowMoodMinThreshold 
		           && moodValue <= EditableValuesManager.Instance.lowMoodHighThreshold) {
			if (_isInLowMood == false) {
				EnterLowMood();	
			}
		} else if (moodValue <= EditableValuesManager.Instance.criticalMoodHighThreshold) {
			if (_isInCriticalMood == false) {
				EnterCriticalMood();	
			}
		}
	}
	#endregion

	#region Normal Mood
	private void EnterNormalMood() {
		ExitCurrentMoodState();
		_isInNormalMood = true;
		Debug.Log($"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=green>entering</color> " +
		          $"<b>normal</b> mood state");
	}
	private void ExitNormalMood() {
		_isInNormalMood = false;
		Debug.Log($"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=red>exiting</color> " +
		          $"<b>normal</b> mood state");
	}
	#endregion
	
	#region Low Mood
	private void EnterLowMood() {
		ExitCurrentMoodState();
		_isInLowMood = true;
		Debug.Log($"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=green>entering</color> " +
		          $"<b>Low</b> mood state");
	}
	private void ExitLowMood() {
		_isInLowMood = false;
		Debug.Log($"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=red>exiting</color> " +
		          $"<b>low</b> mood state");
	}
	#endregion

	#region Critical Mood
	private void EnterCriticalMood() {
		ExitCurrentMoodState();
		_isInCriticalMood = true;
		Debug.Log($"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=green>entering</color> " +
		          $"<b>critical</b> mood state");
		//start checking for major mental breaks
		StartCheckingForMajorMentalBreak();
	}
	private void ExitCriticalMood() {
		_isInCriticalMood = false;
		Debug.Log($"{GameManager.Instance.TodayLogString()} {_owner.name} is <color=red>exiting</color> " +
		          $"<b>critical</b> mood state");
		//stop checking for major mental breaks
		StopCheckingForMajorMentalBreak();
		if (_currentCriticalMoodEffectChance > 0f) {
			Messenger.AddListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
		}
	}
	private void CheckForMajorMentalBreak() {
		IncreaseMajorMentalBreakChance();
		float roll = Random.Range(0f, 100f);
		if (roll <= _currentCriticalMoodEffectChance) {
			//Trigger Major Mental Break.
			TriggerMajorMentalBreak();
		}
	}
	private void AdjustMajorMentalBreakChance(float amount) {
		_currentCriticalMoodEffectChance += amount;
		_currentCriticalMoodEffectChance = Mathf.Clamp(_currentCriticalMoodEffectChance, 0, 100f);
		if (_currentCriticalMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
		}
	}
	private void SetMajorMentalBreakChance(float amount) {
		_currentCriticalMoodEffectChance = amount;
		_currentCriticalMoodEffectChance = Mathf.Clamp(_currentCriticalMoodEffectChance, 0, 100f);
		if (_currentCriticalMoodEffectChance <= 0f) {
			Messenger.RemoveListener(Signals.HOUR_STARTED, DecreaseMajorMentalBreakChance);
		}
	}
	private void IncreaseMajorMentalBreakChance() {
		AdjustMajorMentalBreakChance(GetMajorMentalBreakChanceIncrease());
	}
	private void DecreaseMajorMentalBreakChance() {
		 AdjustMajorMentalBreakChance(GetMajorMentalBreakChanceDecrease());
	}
	private float GetMajorMentalBreakChanceIncrease() {
		return 100f / (EditableValuesManager.Instance.majorMentalBreakDayThreshold * 24f); //because there are 24 hours in a day
	}
	private float GetMajorMentalBreakChanceDecrease() {
		return (100f / (EditableValuesManager.Instance.majorMentalBreakDayThreshold * 24f)) * -1f; //because there are 24 hours in a day
	}
	private void ResetMajorMentalBreakChance() {
		SetMajorMentalBreakChance(0f);
	}
	private void StopCheckingForMajorMentalBreak() {
		Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForMajorMentalBreak);
	}
	private void StartCheckingForMajorMentalBreak() {
		Messenger.AddListener(Signals.HOUR_STARTED, CheckForMajorMentalBreak);
	}
	#endregion

	#region Major Mental Break
	private void TriggerMajorMentalBreak() {
		int roll = Random.Range(0, 4);
		if (roll == 1) {
			//Berserk
			TriggerBerserk();
		} else if (roll == 2) {
			//catatonic	
			TriggerCatatonic();
		} else if (roll == 3) {
			//suicidal
			TriggerSuicidal();
		}
		ResetMajorMentalBreakChance();
	}
	private void TriggerBerserk() {
		if (_owner.traitContainer.AddTrait(_owner, "Berserk")) {
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);	
		} else {
			Debug.LogWarning($"{_owner.name} triggered berserk mental break but could not add berserk trait to its traits!");
		}
	}
	private void CheckIfBerserkLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Berserked) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfBerserkLost);	
		}
	}
	private void TriggerCatatonic() {
		if (_owner.traitContainer.AddTrait(_owner, "Catatonic")) {
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);
		} else {
			Debug.LogWarning($"{_owner.name} triggered catatonic mental break but could not add catatonic trait to its traits!");
		}
	}
	private void CheckIfCatatonicLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Catatonic) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfCatatonicLost);	
		}
	}
	private void TriggerSuicidal() {
		if (_owner.traitContainer.AddTrait(_owner, "Suicidal")) {
			Messenger.AddListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);
		} else {
			Debug.LogWarning($"{_owner.name} triggered suicidal mental break but could not add suicidal trait to its traits!");
		}
	}
	private void CheckIfSuicidalLost(ITraitable traitable, Trait trait, Character removedBy) {
		if (traitable == _owner && trait is Catatonic) {
			//gain catharsis
			_owner.traitContainer.AddTrait(_owner, "Catharsis");
			Messenger.RemoveListener<ITraitable, Trait, Character>(Signals.TRAITABLE_LOST_TRAIT, CheckIfSuicidalLost);	
		}
	}
	#endregion
	
	#region Utilities
	private void ExitCurrentMoodState() {
		if (_isInCriticalMood) {
			ExitCriticalMood();
		}
		if (_isInNormalMood) {
			ExitNormalMood();
		}
		if (_isInLowMood) {
			ExitLowMood();
		}
	}
	#endregion
}

// if(amount < 0 && _owner.currentMoodType == CHARACTER_MOOD.DARK) {
// if (_owner.doNotDisturb) {
// 	return;
// }
// if(_owner.currentActionNode != null && _owner.currentActionNode.action.goapType == INTERACTION_TYPE.TANTRUM) {
// 	return;
// }
// string tantrumReason = "Became " + fromTrait.GetNameInUI(_owner);
// 	if (triggerAction != null) {
// 	tantrumReason = Utilities.LogReplacer(triggerAction.currentState.descriptionLog);
// }
//
// //string tantrumLog = this.name + "'s mood was adjusted by " + amount.ToString() + " and current mood is " + currentMoodType.ToString() + ".";
// //tantrumLog += "Reason: " + tantrumReason;
// //tantrumLog += "\nRolling for Tantrum..."; 
//
// int chance = UnityEngine.Random.Range(0, 100);
//
// 	//tantrumLog += "\nRolled: " + chance.ToString();
//
// 	if (chance < 10) { 
// 	//Note: Do not cancel jobs and plans anymore, let the job priority decide if the character will do tantrum already
// 	//CancelAllJobsAndPlans();
// 	//Create Tantrum action
// 	GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob((JOB_TYPE) JOB_TYPE.TANTRUM, (INTERACTION_TYPE) INTERACTION_TYPE.TANTRUM, (IPointOfInterest) this, (IJobOwner) this);
// 	job.AddOtherData(INTERACTION_TYPE.TANTRUM, new object[] { tantrumReason });
//
// 	//tantrum.SetCannotOverrideJob(true);
// 	//tantrum.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
// 	_owner.jobQueue.AddJobInQueue(job);
// 	//jobQueue.ProcessFirstJobInQueue(this);
// 	//tantrumLog += "\n" + this.name + " started having a tantrum!";
// }
// //Debug.Log(tantrumLog);
// }