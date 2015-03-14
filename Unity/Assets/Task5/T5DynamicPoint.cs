using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class T5DynamicPoint : MonoBehaviour {
	
	private PolyMapLoader map = null;
	
	private List<Vector3> path = null;
	public float goalInterval;
	public float accMax;
	
	public float xLow;
	public float xHigh;
	public float zLow;
	public float zHigh;
	
	public List<T5Agent> agents;
	private List<Color> agentColors;
	
	// Use this for initialization
	void Start () {
		
		map = new PolyMapLoader ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
		                         "polygMap1/button");
		

		
		agents=new List<T5Agent>();
		agentColors=new List<Color>();
		agentColors.Add (Color.black);
		agentColors.Add (Color.blue);
		agentColors.Add (Color.yellow);
		agentColors.Add (Color.cyan);
		agentColors.Add (Color.green);

		int agentCounter = 0;

		for (int i=0; i<map.polyData.start.Count; i=i+1) {
			Vector3 startNode=map.polyData.start[i];
			Vector3 endNode=map.polyData.end[i];
			T5Agent newAgent=new T5Agent("Agent "+agentCounter, startNode, endNode);
			newAgent.agent.gameObject.renderer.material.color=agentColors[i];
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
			if(agentsAtGoal==agents.Count){
				Debug.Log("Done");
				float timeAfter=Time.time;
				Debug.Log("Time:"+(timeAfter-timeBefore));
				yield break;
			}
			//Iterate all agents
			for (int i=0; i<agents.Count; i++) {

				
				T5Agent curAgent=agents[i];
				Vector3 current=curAgent.goalPos;
				Vector3 dynPVel=curAgent.velocity;
				
				float distance = Vector3.Distance (curAgent.agent.transform.position, current);
				float curVel = Vector3.Magnitude (dynPVel);
				if (distance <= goalInterval * curVel) {
					//Reached goal
					atGoal[i]=true;	
					//continue;
				}

				
				//The code below is used for the 2nd solution of T4
				
				Vector3 newVel=curAgent.findMinimumPenaltyVel(agents,accMax,current,goalInterval);
				
				//Update the velocity vector
				curAgent.velocity=newVel;
				
				Vector3 curPos=curAgent.agent.transform.position;
				
				Vector3 moveTowards=curPos+newVel;
				float step=newVel.magnitude*Time.deltaTime;
				//Debug.Log("Step:"+step);
				curAgent.agent.transform.position = Vector3.MoveTowards(curPos,moveTowards,step);
				//curAgent.agent.transform.position = curAgent.agent.transform.position + newVel*Time.deltaTime;

				
				yield return null;
			}
			
		}
		
	}
	
	void OnDrawGizmos() {
		
		if (map == null) {
			map = new PolyMapLoader ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
			                         "polygMap1/button");
		}
		if (agentColors == null) {
			agentColors=new List<Color>();
			agentColors.Add (Color.black);
			agentColors.Add (Color.blue);
			agentColors.Add (Color.yellow);
			agentColors.Add (Color.cyan);
			agentColors.Add (Color.green);
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

		for (int i=0; i<map.polyData.start.Count; i=i+1) {
			Vector3 startNode=map.polyData.start[i];
			Vector3 endNode=map.polyData.end[i];
			Gizmos.color=Color.green;
			Gizmos.DrawCube(startNode,Vector3.one);
			Gizmos.color=Color.red;
			Gizmos.DrawCube(endNode,Vector3.one);
			Gizmos.color=agentColors[i];
			Gizmos.DrawLine(startNode,endNode);
		}


		Gizmos.color = Color.black;
		Gizmos.DrawLine (new Vector3 (xLow, 1, zLow), new Vector3 (xHigh, 1, zLow));
		Gizmos.DrawLine (new Vector3 (xHigh, 1, zLow),new Vector3(xHigh,1,zHigh));
		Gizmos.DrawLine (new Vector3(xHigh,1,zHigh), new Vector3 (xLow, 1, zHigh));
		Gizmos.DrawLine (new Vector3(xLow,1,zHigh), new Vector3 (xLow, 1, zLow));
		
		
		
		
	}
	
	
}
