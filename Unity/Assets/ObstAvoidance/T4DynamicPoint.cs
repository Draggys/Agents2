using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class T4DynamicPoint : MonoBehaviour {

	private PolyMapLoader map = null;

	private List<Vector3> path = null;
	public float goalInterval;
	public float accMax;

	public float xLow;
	public float xHigh;
	public float zLow;
	public float zHigh;

	public List<T4Agent> agents;

	// Use this for initialization
	void Start () {

		map = new PolyMapLoader ("T4Map/x", "T4Map/y", "T4Map/goalPosT4", "T4Map/startPosT4", "T4Map/Button");

		int agentCounter = 0;

		agents=new List<T4Agent>();

		for (int i=0; i<map.polyData.start.Count; i=i+2) {
			Vector3 startNode=map.polyData.start[i];
			Vector3 endNode=map.polyData.end[i];
			T4Agent newAgent=new T4Agent("Agent "+agentCounter, startNode, endNode);
			agents.Add(newAgent);
			agentCounter++;
			}

		Debug.Log ("Agents size:" + agents.Count);

		StartCoroutine("Move");
	
	}
	
	
	IEnumerator Move() {
		bool[] atGoal=new bool[agents.Count];
		for (int i=0; i<atGoal.Length; i++)
						atGoal [i] = false;
		float timeBefore = Time.time;
		while (true) {

			//Check if all agents at goal
			int agentsAtGoal=0;
			for(int i=0;i<atGoal.Length;i++){
				if(atGoal[i]==true)
					agentsAtGoal++;
			}
			if(agentsAtGoal==agents.Count)
				yield break;
			//Iterate all agents
			for (int i=0; i<agents.Count; i++) {

				if(atGoal[i]==true){
					continue;
				}

				T4Agent curAgent=agents[i];
				Vector3 current=curAgent.goalPos;
				Vector3 dynPVel=curAgent.velocity;

				float distance = Vector3.Distance (curAgent.agent.transform.position, current);
				float curVel = Vector3.Magnitude (dynPVel);
				if (distance <= goalInterval * curVel) {
					//Reached goal
					atGoal[i]=true;	
					continue;
				}
			
				//Check collision
				Vector3 collisionVec=curAgent.collisionDetection(agents,accMax);
				//If the collisionVector is all zeros we just move along as before
				if(collisionVec.Equals(new Vector3(0,0,0))){
					//Debug.Log("CollVec zeros");
				Vector3 dir;
			
					dir = Vector3.Normalize (current - curAgent.agent.transform.position);
			
					//Debug.Log("dir="+dir);
					//Debug.Log("DynPVel="+dynPVel);

				Vector3 normVel = Vector3.Normalize (dynPVel);

				//The change is the difference between the direction and the velocity vector
					Vector3 change = dir;
					if(!normVel.Equals(dir)){
						 change = Vector3.Normalize (dir - normVel);
					}
					if(change.Equals(new Vector3(0,0,0)))
						change=dir;

					//Debug.Log("change"+change);


				float accInX = change.x / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
				float accInZ = change.z / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
			
				if (float.IsNaN (accInX)) {
					accInX = 0;
				}
				if (float.IsNaN (accInZ)) {
					accInZ = 0;
				}
			
				dynPVel.x = dynPVel.x + accMax * accInX * Time.deltaTime;
				dynPVel.z = dynPVel.z + accMax * accInZ * Time.deltaTime;

					//Debug.Log("DynPVel="+dynPVel);

					//Update the velocity vector
					curAgent.velocity=dynPVel;

					curAgent.agent.transform.position = curAgent.agent.transform.position + dynPVel;
			
				}
				//If the collision vector is not all zeros we should steer in that direction
				else{
					Debug.Log("Collision course");
					//Debug.Log("CollVec:"+collisionVec);
					Vector3 dir;
					
					dir = Vector3.Normalize (collisionVec);

					//Debug.Log("CollVec norm:"+dir);

					Vector3 normVel = Vector3.Normalize (dynPVel);
					
					//If we are on a collision course we just want to move the other way
					Vector3 change = dir;
					/*
					if(!normVel.Equals(dir)){
						change = Vector3.Normalize (dir - normVel);
					}*/
					
					float accInX = change.x / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
					float accInZ = change.z / (Mathf.Abs (change.x) + Mathf.Abs (change.z));
					
					if (float.IsNaN (accInX)) {
						accInX = 0;
					}
					if (float.IsNaN (accInZ)) {
						accInZ = 0;
					}
					
					dynPVel.x = dynPVel.x + accMax * accInX * Time.deltaTime;
					dynPVel.z = dynPVel.z + accMax * accInZ * Time.deltaTime;

					//Update the velocity vector
					curAgent.velocity=dynPVel;

					curAgent.agent.transform.position = curAgent.agent.transform.position + dynPVel;

				}


				yield return null;
				}

			}

		}

	void OnDrawGizmos() {

		if (map == null) {
						map = new PolyMapLoader ("T4Map/x", "T4Map/y", "T4Map/goalPosT4", "T4Map/startPosT4", "T4Map/Button");
				}
		
		if (map.polyData.nodes != null) {
			foreach(Vector3 node in map.polyData.nodes) {
				Gizmos.color = Color.blue;
				Gizmos.DrawCube (node, Vector3.one);
			}
			
			foreach(Line line in map.polyData.lines){
				Gizmos.color=Color.black;
				
				Gizmos.DrawLine(line.point1,line.point2);
			}
			
		}

		Gizmos.color = Color.black;
		Gizmos.DrawLine (new Vector3 (xLow, 1, zLow), new Vector3 (xHigh, 1, zLow));
		Gizmos.DrawLine (new Vector3 (xHigh, 1, zLow),new Vector3(xHigh,1,zHigh));
		Gizmos.DrawLine (new Vector3(xHigh,1,zHigh), new Vector3 (xLow, 1, zHigh));
		Gizmos.DrawLine (new Vector3(xLow,1,zHigh), new Vector3 (xLow, 1, zLow));
		

		
		
	}


}
