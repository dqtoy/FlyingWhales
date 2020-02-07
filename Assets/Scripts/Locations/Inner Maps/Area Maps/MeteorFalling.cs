using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorFalling : MonoBehaviour {
	public MeteorVisual meteorVisual;
	
	public void OnParticleSystemStopped() {
		meteorVisual.OnMeteorFell();
	}
}
