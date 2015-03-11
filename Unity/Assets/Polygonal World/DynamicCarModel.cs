using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
public class DynamicCarModel : MonoBehaviour, Model {
	
	List<Vector3> path = null;
	Line collisionLine;
	float dynVel = 0;
	PolyAgent agent = null;
	PolyCollision collision;

	public float goalInterval = 4;
	public float carLength = 2.64f;
	public float maxWheelAngle = 57.3f;
	public float dynF = 10;
	public float dynMass = 5;
	public float accMax = 1f;
    
	public int collisionSize = 10;
	
	public DynamicCarModel() {
	}

	public void SetPath(List<Vector3> path, PolyAgent agent, List<Line> lines) {
		this.path = path;
		this.agent = agent;
		
		collision = new PolyCollision (lines);	
	}
	
	public void StartCoroutineMove() {
		StartCoroutine ("Move");
		agent.running = true;
	}
	
	float DegToRad(float degree){
		return (Mathf.PI * degree) / 180;
	}

	Vector3 current = new Vector3(0,0,0);
	Vector3 box = new Vector3(0,0,0);
	bool firstStop = true;
	public IEnumerator Move() {
		// Cheat align the vehicle in accordance to the path at spawn time
		Quaternion test = Quaternion.LookRotation (path[0] - agent.agent.transform.position);
		agent.agent.transform.rotation = Quaternion.RotateTowards (agent.agent.transform.rotation, test, 9000);

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
				
				if(firstStop) {
					// Continue a little bit
					index--;
					current = path[index] + agent.agent.transform.forward*3;
					firstStop = false;
				} else {
					firstStop = true;
				}
			}
			float distToTarget = Vector3.Distance (current, agent.agent.transform.position);
			float neededDistToStop = (Mathf.Pow (dynVel, 2) / 2 * (dynF / dynMass));
			

			//if(distToTarget > neededDistToStop) {
			dynVel = dynVel + accMax;
			//}
			//else{
			//	dynVel = dynVel - (dynF / dynMass);
			//}
			
		//	if(dynVel < 0)
		//		dynVel = 0;
			
			float wheelAngleRad = maxWheelAngle * (Mathf.PI / 180);
			float dTheta=(dynVel/carLength)*Mathf.Tan(wheelAngleRad);
			Quaternion theta = Quaternion.LookRotation (current - agent.agent.transform.position);
			
			if(agent.agent.transform.rotation!=theta){
				agent.agent.transform.rotation = Quaternion.RotateTowards (agent.agent.transform.rotation, theta, dTheta);
			}
			
			Vector3 curDir=agent.agent.transform.eulerAngles;
			Vector3 newPos=agent.agent.transform.position;
			float angleRad=curDir.y*(Mathf.PI/180);
			newPos.x=newPos.x+(dynVel*Mathf.Sin(angleRad)*Time.deltaTime);
            newPos.z=newPos.z+(dynVel*Mathf.Cos(angleRad)*Time.deltaTime);
			agent.agent.transform.position=newPos;
            
            //If the car is "almost" at the point
			if(Vector3.Distance (current, agent.agent.transform.position) < goalInterval){
                carMadeIt = true;
            }
            
			yield return null;

		}
	}


	
	public void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		if (path != null) {
			foreach(Vector3 p in path) {
		//		Gizmos.DrawSphere (p, 3);
			}
		}

	}
}
