using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	public static int[] daysInMonth = {0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
	public int month;
	public int days;
	public int year;

    public PROGRESSION_SPEED currProgressionSpeed;

	public float progressionSpeed;
	public bool isPaused = true;

    private const float X1_SPEED = 2f;
    private const float X2_SPEED = 1f;
    private const float X4_SPEED = 0.3f;

    private float timeElapsed;

    #region For Testing
    [ContextMenu("Print Event Table")]
    public void PrintEventTable() {
        Messenger.PrintEventTable();
    }

    //[SerializeField] private HexTile center;
    //[SerializeField] private int range;
    //[ContextMenu("Get Tiles In Range")]
    //public void GetTilesTester() {
    //    List<HexTile> tiles = GridMap.Instance.GetTilesInRange(center, range);
    //    StartCoroutine(SelectSlowly(tiles));
    //    //UnityEditor.Selection.objects = tiles.Select(x => x.gameObject).ToArray();
    //    //for (int i = 0; i < tiles.Count; i++) {
    //    //    Debug.Log(tiles[i].name);
    //    //}
    //}
    #endregion

    private void Awake(){
		Instance = this;
		this.days = 1;
		this.month = 1;
		this.timeElapsed = 0f;
	}

	private void FixedUpdate(){
		if (!isPaused) {
			this.timeElapsed += Time.deltaTime * 1f;
			if(this.timeElapsed >= this.progressionSpeed){
				this.timeElapsed = 0f;
				this.WeekEnded ();
			}
		}
	}

	[ContextMenu("Start Progression")]
	public void StartProgression(){
		UIManager.Instance.SetProgressionSpeed1X();
		UIManager.Instance.x1Btn.SetAsClicked();
        Messenger.Broadcast("UpdateUI");
        SetPausedState(false);
	}

	public void TogglePause(){
		this.isPaused = !this.isPaused;
	}

	public void SetPausedState(bool isPaused){
		this.isPaused = isPaused;
	}

    /*
     * Set day progression speed to 1x, 2x of 4x
     * */
	public void SetProgressionSpeed(PROGRESSION_SPEED progSpeed){
        currProgressionSpeed = progSpeed;
        float speed = X1_SPEED;
        if (progSpeed == PROGRESSION_SPEED.X2) {
            speed = X2_SPEED;
        } else if(progSpeed == PROGRESSION_SPEED.X4){
            speed = X4_SPEED;
        }
		this.progressionSpeed = speed;
	}

    /*
     * Function that triggers daily actions
     * */
	public void WeekEnded(){
        ////		TriggerRequestPeace();
        ////EventManager.Instance.onCitizenTurnActions.Invoke();
        ////EventManager.Instance.onCityEverydayTurnActions.Invoke();
        ////EventManager.Instance.onWeekEnd.Invoke();
        ////Messenger.Broadcast("CitizenTurnActions");
        Messenger.Broadcast("CityEverydayActions");
        Messenger.Broadcast("OnDayEnd");
        ////BehaviourTreeManager.Instance.Tick ();
        ////EventManager.Instance.onUpdateUI.Invoke();
        Messenger.Broadcast("UpdateUI");

        this.days += 1;
		if (days > daysInMonth[this.month]) {
			this.days = 1;
			this.month += 1;
			if (this.month > 12) {
				this.month = 1;
				this.year += 1;
			}
		}
	}
}
