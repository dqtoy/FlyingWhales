using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditableValuesManager : MonoBehaviour {

	public static EditableValuesManager Instance;
	
	[Header("Character Values")]
	[Header("Mood")]
	[SerializeField] private int _normalMoodMinThreshold;
	[SerializeField] private int _normalMoodHighThreshold;
	[SerializeField] private int _lowMoodMinThreshold;
	[SerializeField] private int _lowMoodHighThreshold;
	[SerializeField] private int _criticalMoodMinThreshold;
	[SerializeField] private int _criticalMoodHighThreshold;
	[Tooltip("Number days a character needs to be in a critical mood to have a 100% chance to trigger a major mental break.")]
	[SerializeField] private int _majorMentalBreakDayThreshold;
	[Tooltip("Number days a character needs to be in a low mood to have a 100% chance to trigger a minor mental break.")]
	[SerializeField] private int _minorMentalBreakDayThreshold;

	[Header("Needs")] 
	[SerializeField] private float _baseFullnessDecreaseRate;
	[SerializeField] private float _baseTirednessDecreaseRate;
	[SerializeField] private float _baseHappinessDecreaseRate;
	[SerializeField] private float _baseComfortDecreaseRate;

	[Header("Mana")] 
	[SerializeField] private int _startingMana;
	[SerializeField] private int _maximumMana;
	[SerializeField] private int _summonMinionManaCost;
	[SerializeField] private int _corruptTileManaCost;
	[SerializeField] private int _triggerFlawManaCost;
	[SerializeField] private int _buildStructureManaCost;
	[SerializeField] private int _learnSpellManaCost;
	[SerializeField] private int _demonicCultRecruitmentManaCost;
	[SerializeField] private int _makeCharacterJoinCultManaCost;
	[SerializeField] private int _monsterBreedingManaCost;
	[SerializeField] private int _unlockWorldMapManaCost;
	[SerializeField] private int _unlockRegionManaCost;
	
	//getters
	//mood
	public int normalMoodMinThreshold => _normalMoodMinThreshold;
	public int lowMoodMinThreshold => _lowMoodMinThreshold;
	public int lowMoodHighThreshold => _lowMoodHighThreshold;
	public int criticalMoodHighThreshold => _criticalMoodHighThreshold;
	public int majorMentalBreakDayThreshold => _majorMentalBreakDayThreshold;
	public int minorMentalBreakDayThreshold => _minorMentalBreakDayThreshold;
	public float baseFullnessDecreaseRate => _baseFullnessDecreaseRate;
	public float baseTirednessDecreaseRate => _baseTirednessDecreaseRate;
	public float baseHappinessDecreaseRate => _baseHappinessDecreaseRate;
	public float baseComfortDecreaseRate => _baseComfortDecreaseRate;

	//mana
	public int summonMinionManaCost => _summonMinionManaCost;
	public int maximumMana => _maximumMana;
	public int startingMana => _startingMana;
	
	private void Awake() {
		Instance = this;
	}
}
