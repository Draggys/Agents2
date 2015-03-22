using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
public class T5DynCar : MonoBehaviour {
	
	List<Vector3> path = null;
	Line collisionLine;
	float dynVel = 0;
	T5PolyAgent agent = null;
	PolyCollision collision;
	
	public float goalInterval = 4;
	public float carLength = 2.64f;
	public float maxWheelAngle = 57.3f;
	public float dynF = 10;
	public float dynMass = 5;
	public float accMax = 1f;
	
	public int collisionSize = 10;

	public List<T5PolyAgent> AGENTS = null;

	public void SetPath(List<Vector3> path, T5PolyAgent agent, List<Line> lines) {
		this.path = path;
		this.agent = agent;
		
		collision = new PolyCollision (lines);	
	}

	public void SetAgents(ref List<T5PolyAgent> agents) {
		AGENTS = new List<T5PolyAgent> ();
		foreach (T5PolyAgent a in agents) {
			if(a != agent) {
				AGENTS.Add (a);
			}
		}
	}

	Vector3 Separation() {
		int sepDist = 10;
		Vector3 c = Vector3.zero;
		foreach (T5PolyAgent n in AGENTS) {
			if(Vector3.Distance (n.agent.transform.position, agent.agent.transform.position)
			   < sepDist) {
				c = c - (n.agent.transform.position - agent.agent.transform.position);
				n.dontMove = true;
			}
		}
		return c;
	}

	bool TooClose() {
		foreach (T5PolyAgent n in AGENTS) {
			if(Vector3.Distance(n.agent.transform.position, agent.agent.transform.position) < 20)
				return true;
		}
		return false;
	}

	public void StartCoroutineMove() {
		StartCoroutine ("Move");
		agent.running = true;
	}
	
	public void StopCoroutineMove() {
		StopCoroutine ("Move");
		agent.running = false;
	}
	
	float DegToRad(float degree){
		return (Mathf.PI * degree) / 180;
	}
	
	Vector3 current = new Vector3(0,0,0);
	Vector3 box = new Vector3(0,0,0);
	public IEnumerator Move() {
		int index = 0;
		current = path[index];

		bool carMadeIt = false;
		int limit = 100;
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
                
            float distToTarget = Vector3.Distance (current, agent.agent.transform.position);
			float neededDistToStop = (Mathf.Pow (dynVel, 2) / 2 * (dynF / dynMass));

			dynVel = dynVel + accMax*Time.deltaTime;
			
			float wheelAngleRad = maxWheelAngle * (Mathf.PI / 180);
			float dTheta=(dynVel/carLength)*Mathf.Tan(wheelAngleRad);
			Quaternion theta = Quaternion.LookRotation (current - agent.agent.transform.position);
			
			if(agent.agent.transform.rotation!=theta){
				agent.agent.transform.rotation = Quaternion.RotateTowards (agent.agent.transform.rotation, theta, dTheta);
			}
			
			Vector3 curDir=agent.agent.transform.eulerAngles;
			Vector3 newPos=agent.agent.transform.position;

			// NYTT
			Vector3 sep = Separation ();
			sep = sep.normalized;
			newPos.x = newPos.x + sep.x;
	        newPos.z = newPos.z + sep.z;


            float angleRad=curDir.y*(Mathf.PI/180);
			newPos.x=newPos.x+(dynVel*Mathf.Sin(angleRad)*Time.deltaTime);
			newPos.z=newPos.z+(dynVel*Mathf.Cos(angleRad)*Time.deltaTime);


			if(limit-- == 0) {
				limit = 200;
				agent.dontMove = false;
			}

			if(!agent.dontMove) {
				agent.agent.transform.position = newPos;
			}
			
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
