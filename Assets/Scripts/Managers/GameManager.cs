using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	public int month;
	public int week;
	public int year;

	public float progressionSpeed = 1f;
	public bool isPaused = false;

	void Awake(){
		Instance = this;
	}

	[ContextMenu("Start Progression")]
	public void StartProgression(){
//		InvokeRepeating ("WeekEnded", 0f, 1f);
		UIManager.Instance.SetProgressionSpeed1X();
		UIManager.Instance.x1Btn.OnClick();
		StartCoroutine(WeekProgression());
	}

	public void TogglePause(){
		this.isPaused = !this.isPaused;
	}

	public void SetProgressionSpeed(float speed){
		this.progressionSpeed = speed;
	}

	IEnumerator WeekProgression(){
		while (true) {
			yield return new WaitForSeconds (progressionSpeed);
			if (!isPaused) {
				this.WeekEnded ();
			}
		}
	}

	public void WeekEnded(){
		this.week += 1;
		if (week > 4) {
			this.week = 1;
			this.month += 1;
			if (this.month > 12) {
				this.month = 1;
				this.year += 1;
			}
		}
		EventManager.Instance.onCitizenTurnActions.Invoke ();
		EventManager.Instance.onCityEverydayTurnActions.Invoke ();
		EventManager.Instance.onCitizenMove.Invoke ();
		EventManager.Instance.onWeekEnd.Invoke();
	}

}
