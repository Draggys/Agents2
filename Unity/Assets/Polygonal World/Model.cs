using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface Model {
	IEnumerator Move ();
	void StartCoroutineMove();
	void StopCoroutineMove();
	void SetPath(List<Vector3> path, PolyAgent agent, List<Line> lines);
}