using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class DynamicPointModel : MonoBehaviour, Model {

	public float accMax;
	public float dynFx ;
	public float dynFy ;
	public float accX;
	public float accY;
	public float dynM ;
	List<Vector3> path;
	PolyAgent agent;
	PolyCollision collision;
	int LAScale;

	public DynamicPointModel() {
		//accMax = 0.1f;
		accMax = 1f;
		LAScale = 20;
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

	Vector3 d_dir;
	public IEnumerator Move() {
		int index = 0;
		float velX = 0;
		float velY = 0;
		float dynVel = 0;
		Vector3 dynPVel = new Vector3 (0,0,0);
		
		//test
		Vector3 goalVel = new Vector3 (1, 0, 1);
		
		Vector3 current = path[index];


		while (true) {
			float distance=Vector3.Distance(agent.agent.transform.position, current);
			if(distance<=1) {
				index++;
				if(index >= path.Count) {
					agent.running = false;
					yield break;
				}
				current = path[index];
//				print (current);
				//dynVel=0;
			}

			Vector3 dir;
			float distanceToTarget=Vector3.Distance (current, agent.agent.transform.position);
			dir=Vector3.Normalize(current-agent.agent.transform.position);

			Vector3 normVel=Vector3.Normalize(dynPVel);
			
			//The change is the difference between the direction and the velocity vector
			Vector3 change=Vector3.Normalize(dir-normVel);
			
			dynPVel.x=dynPVel.x+accMax*change.x*Time.deltaTime;
			dynPVel.z=dynPVel.z+accMax*change.z*Time.deltaTime;

			// start handle collision
			d_dir = dir;
			Vector3 avoidance = Vector3.zero;
			Vector3 lookAhead = dir * LAScale;
			Line LAline = new Line(agent.agent.transform.position, 
			                       agent.agent.transform.position + lookAhead);
			if(collision.IntersectsWithObstacle (LAline)) {
				//dynPVel.x = -dynPVel.x;
				//dynPVel.z = -dynPVel.z;
			}

			agent.agent.transform.position = agent.agent.transform.position+dynPVel;
			

			yield return null;
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
	//	Gizmos.DrawLine (agent.agent.transform.position, 
	//	                 agent.agent.transform.position + d_dir * LAScale);

	}
}


