using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DifferentialDriveModel : MonoBehaviour, Model {
	List<Vector3> path;
	float vel = 10f;
	float angleVel = 57f;
	PolyAgent agent = null;
	PolyCollision collision;

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
			Vector3 dir;
			Quaternion theta = Quaternion.LookRotation (current - agent.agent.transform.position);
			if(Vector3.Distance (current, agent.agent.transform.position) < vel*Time.deltaTime && theta==agent.agent.transform.rotation) {
				dir = current - agent.agent.transform.position;
				//Debug.Log("Jump");
				agent.agent.transform.position = current;
				index++;
				if(index < path.Count)
					current = path[index];
				else
					yield break;
			}
			else {
				
				if(agent.agent.transform.rotation!=theta){
					agent.agent.transform.rotation = Quaternion.RotateTowards (agent.agent.transform.rotation, theta, angleVel * Time.deltaTime);
				}
				else{
					dir = Vector3.Normalize (current - agent.agent.transform.position);					
					dir.x = dir.x * (  vel * Time.deltaTime);
					dir.z = dir.z * ( vel * Time.deltaTime);
					agent.agent.transform.position = (agent.agent.transform.position + dir);
				}
			}
			yield return null;		
		}
	}
}
