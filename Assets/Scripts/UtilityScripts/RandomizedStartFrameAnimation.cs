using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizedStartFrameAnimation : MonoBehaviour {

    private Animator _animator;

	// Use this for initialization
	void Start () {
        float randomStartFrame = UnityEngine.Random.Range(0f, 1f);
        _animator = GetComponent<Animator>();
        _animator.Play("Wiggle", 0, randomStartFrame);
	}
}
