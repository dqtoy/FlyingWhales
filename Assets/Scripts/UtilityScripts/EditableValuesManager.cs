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
	
	//getters
	//mood
	public int normalMoodMinThreshold => _normalMoodMinThreshold;
	public int normalMoodHighThreshold => _normalMoodHighThreshold;
	public int lowMoodMinThreshold => _lowMoodMinThreshold;
	public int lowMoodHighThreshold => _lowMoodHighThreshold;
	public int criticalMoodMinThreshold => _criticalMoodMinThreshold;
	public int criticalMoodHighThreshold => _criticalMoodHighThreshold;
	public int majorMentalBreakDayThreshold => _majorMentalBreakDayThreshold;

	private void Awake() {
		Instance = this;
	}
}
