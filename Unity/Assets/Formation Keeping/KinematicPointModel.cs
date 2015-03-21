using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicPointModel : MonoBehaviour, Model {
	
	List<Vector3> path = null;
	PolyAgent agent = null;

	PolyCollision collision;

	float vel = 20;
	
	public void SetPath(List<Vector3> path, PolyAgent agent, List<Line> lines) {
		this.path = path;
		this.agent = agent;
		
		collision = new PolyCollision (lines);
	}
	
	public void StartCoroutineMove() {
		StartCoroutine ("Move");
		agent.running = true;
	}
	
	public void StopCoroutineMove() {
		StopCoroutine ("Move");
		agent.running = false;
	}

	public IEnumerator Move() {
		int index = 0;
		Vector3 current = path[index];
		while (true) {
			if(agent.agent.transform.position == current) {
				index++;
				if(index >= path.Count)
					yield break;
				current = path[index];
			}
			agent.agent.transform.position = Vector3.MoveTowards (agent.agent.transform.position, current, vel * Time.deltaTime);
			yield return null;
		}
	}
}
