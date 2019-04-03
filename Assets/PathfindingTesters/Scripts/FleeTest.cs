using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeTest : MonoBehaviour {

    [SerializeField] private Transform fleeFrom;
    [SerializeField] private Seeker seeker;
    [SerializeField] private FleeTestAIPath aiPath;

    private List<GameObject> objsInRange = new List<GameObject>();

    private Vector3 runningAwayFrom;

    public void Awake() {
        aiPath.targetReachedAction = () => OnTargetReached();
    }

    //public void Start() {
    //    if (!aiPath.hasPath) {
    //        FleePath currPath = FleePath.Construct(transform.position, fleeFrom.transform.position, 10000);
    //        currPath.aimStrength = 1;
    //        currPath.spread = 4000;

    //        // Start the path and return the result to MyCompleteFunction (which is a function you have to define, the name can of course be changed)
    //        seeker.StartPath(currPath, PathCallback);
    //    }
    //}

    private void PathCallback(Path path) {
        Debug.Log("Path callback");
    }
    private void OnTargetReached() {
        Debug.Log("Target Reached!");
        //if (objsInRange.Count > 0) {
        //    OnHositileEnter();
        //}
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        if (!objsInRange.Contains(collision.gameObject)) {
            objsInRange.Add(collision.gameObject);
        }
        //OnHositileEnter();

    }
    public void OnTriggerExit2D(Collider2D collision) {
        objsInRange.Remove(collision.gameObject);
    }
    public void OnTriggerStay2D(Collider2D collision) {
        //OnHositileEnter();
    }

    private float recheckRate = 1f;
    private float timeElapsed;

    public void Update() {
        if (objsInRange.Count == 0) {
            return;
        }
        this.timeElapsed += Time.deltaTime;
        if (this.timeElapsed > this.recheckRate) {
            timeElapsed = 0f;

            FleePath currPath = FleePath.Construct(transform.position, runningAwayFrom, 20000);
            currPath.aimStrength = 1f;
            currPath.spread = 5000;
            // Start the path and return the result to MyCompleteFunction (which is a function you have to define, the name can of course be changed)
            seeker.StartPath(currPath, PathCallback);
        }
        
    }

    //private void OnHositileEnter() {
    //    Debug.Log("Hostile Entered!");
    //    if (!aiPath.hasPath || aiPath.reachedEndOfPath || Vector2.Distance(this.transform.position, runningAwayFrom) > Vector2.Distance(this.transform.position, fleeFrom.transform.position)) {
    //        runningAwayFrom = fleeFrom.transform.position;
    //        FleePath currPath = FleePath.Construct(transform.position, runningAwayFrom, 10000);
    //        currPath.aimStrength = 1f;
    //        currPath.spread = 10000;
    //        // Start the path and return the result to MyCompleteFunction (which is a function you have to define, the name can of course be changed)
    //        seeker.StartPath(currPath, PathCallback);
    //    }
    //}
}
