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
    public int hour;
    public int hoursPerDay;

    public PROGRESSION_SPEED currProgressionSpeed;

	public float progressionSpeed;
	public bool isPaused = true;
    //public bool hideLandmarks = true;
    public bool initiallyHideRoads = false;
    public bool allowConsole = true;
    public bool displayFPS = true;

    private const float X1_SPEED = 2f;
    private const float X2_SPEED = 1f;
    private const float X4_SPEED = 0.3f;

    private float timeElapsed;
    private bool _gameHasStarted;

    [SerializeField] private Texture2D defaultCursorTexture;
    [SerializeField] private Texture2D targetCursorTexture;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    #region getters/setters
    public bool gameHasStarted {
        get { return _gameHasStarted; }
    }
    #endregion

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        this.timeElapsed = 0f;
        _gameHasStarted = false;
        SetCursorToDefault();
    }
    private void FixedUpdate() {
        if (_gameHasStarted && !isPaused) {
            if (this.timeElapsed == 0f) {
                this.HourStarted();
            }
            this.timeElapsed += Time.deltaTime * 1f;
            if (this.timeElapsed >= this.progressionSpeed) {
                this.timeElapsed = 0f;
                this.HourEnded();
            }
        }
    }
    private void Update() {
        if (!UIManager.Instance.IsConsoleShowing() && !PlayerManager.Instance.isChoosingStartingTile) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                if (isPaused) {
                    SetProgressionSpeed(currProgressionSpeed);
                    SetPausedState(false);
                } else {
                    //pause
                    SetPausedState(true);
                }
            }
        }
    }
    #endregion


    [ContextMenu("Start Progression")]
	public void StartProgression(){
        _gameHasStarted = true;
        //SetPausedState(false);
        UIManager.Instance.SetProgressionSpeed1X();
		SchedulingManager.Instance.StartScheduleCalls ();
	}

    public GameDate Today() {
        return new GameDate(this.month, this.days, this.year, this.hour);
    }
    public GameDate EndOfTheMonth() {
        return new GameDate(this.month, daysInMonth[this.month], this.year, this.hoursPerDay);
    }
	public GameDate FirstDayOfTheMonth() {
		return new GameDate(this.month, 1, this.year, 1);
	}
	public void SetPausedState(bool isPaused){
        //Debug.Log("Set paused state to " + isPaused);
		this.isPaused = isPaused;
        Messenger.Broadcast(Signals.PAUSED, isPaused);
	}
    /*
     * Set day progression speed to 1x, 2x of 4x
     * */
	public void SetProgressionSpeed(PROGRESSION_SPEED progSpeed){
        currProgressionSpeed = progSpeed;
        //Debug.Log("Set progression speed to " + progSpeed.ToString());
        float speed = X1_SPEED;
        if (progSpeed == PROGRESSION_SPEED.X2) {
            speed = X2_SPEED;
        } else if(progSpeed == PROGRESSION_SPEED.X4){
            speed = X4_SPEED;
        }
		this.progressionSpeed = speed;
        ECS.CombatManager.Instance.updateIntervals = this.progressionSpeed / (float) ECS.CombatManager.Instance.numOfCombatActionPerDay;
        Messenger.Broadcast(Signals.PROGRESSION_SPEED_CHANGED, progSpeed);
	}
    public void HourStarted() {
        Messenger.Broadcast(Signals.HOUR_STARTED);
    }
    /*
     * Function that triggers daily actions
     * */
    public void HourEnded(){
        Messenger.Broadcast(Signals.HOUR_ENDED);
        Messenger.Broadcast(Signals.UPDATE_UI);

        this.hour += 1;
        if(this.hour > hoursPerDay) {
            this.hour = 1;
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

    #region Cursor
    public void SetCursorToDefault() {
        Cursor.SetCursor(defaultCursorTexture, hotSpot, cursorMode);
    }
    public void SetCursorToTarget() {
        Cursor.SetCursor(targetCursorTexture, hotSpot, cursorMode);
    }
    #endregion

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

    //public void StartCoroutine(Coroutine coroutine) {
    //    StartCoroutine(coroutine);
    //}
}
