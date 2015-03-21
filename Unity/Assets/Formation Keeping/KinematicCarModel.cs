using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KinematicCarModel : MonoBehaviour, Model {

	List<Vector3> path = null;
	PolyAgent agent = null;
	PolyCollision collision;
	float goalInterval = 4;

	public float maxWheelAngle = 57.3f;
	public float carLength = 2.64f;
	public float vel = 10;
	public int collisionSize = 5;

	public void StartCoroutineMove() {
		StartCoroutine ("Move");
		agent.running = true;
	}

	public void StopCoroutineMove() {
		StopCoroutine ("Move");
		agent.running = false;
	}

	public void SetPath(List<Vector3> path, PolyAgent agent, List<Line> lines) {
		this.path = path;
		this.agent = agent;
		
		collision = new PolyCollision (lines);	
	}

	float DegToRad(float degree){
		return (Mathf.PI * degree) / 180;
	}

	int index = 1;
	Vector3 current = Vector3.zero;
	public IEnumerator Move (){
		int index = 0;
		current = path[index];
		
		bool carMadeIt = false;
		
		while (true) {	
			if(carMadeIt == true) {
				index++;
				if(index >= path.Count){
					index = 0;
					agent.running = false;
					yield break;
				}
				current = path[index];
				carMadeIt = false;
			}

			float wheelAngleRad=maxWheelAngle*(Mathf.PI/180);
			float dTheta=(vel/carLength)*Mathf.Tan(wheelAngleRad);
			Quaternion theta = Quaternion.LookRotation (current - agent.agent.transform.position);
			
			if(agent.agent.transform.rotation!=theta){
				agent.agent.transform.rotation = Quaternion.RotateTowards (agent.agent.transform.rotation, theta, dTheta);
			}
			
			Vector3 curDir=agent.agent.transform.eulerAngles;
			Vector3 newPos=agent.agent.transform.position;
			float angleRad=curDir.y*(Mathf.PI/180);
			newPos.x=newPos.x+(vel*Mathf.Sin(angleRad)*Time.deltaTime);
			newPos.z=newPos.z+(vel*Mathf.Cos(angleRad)*Time.deltaTime);
			agent.agent.transform.position=newPos;
			
			if(Vector3.Distance (current, agent.agent.transform.position) < goalInterval)
				carMadeIt = true;

			yield return null;
		}
	}
}
