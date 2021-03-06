﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class T5DynamicCar : MonoBehaviour {
	
	private PolyMapLoader map = null;
	
	private List<Vector3> path = null;
	public float goalInterval;
	public float accMax;

	public float carLength = 2.64f;
	public float maxWheelAngle = 57.3f;
	public float dynF = 10;
	public float dynMass = 5;


	public float xLow;
	public float xHigh;
	public float zLow;
	public float zHigh;
	
	public List<T5Agent> agents;
	private List<Color> agentColors;
	
	
	List<Obstacle> obstacles = new List<Obstacle> ();
	PolyData polyData = null;
	
	List<Line> walkableLines;
	VGraph graph;
	PolygonalAStar pathFinder = new PolygonalAStar();
	
	List<Vector3>[] paths;
	int[] agentAtWaypoint;
	bool done = false;
	
	// Use this for initialization
	void Start () {
		Application.runInBackground = true;
        map = new PolyMapLoader ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
		                         "polygMap1/button");
		
		polyData = map.polyData;
		graph = new VGraph();
		walkableLines = new List<Line> ();
		
		//Create visibility graph
		CreateObstacles ();
		ConstructWalkableLines ();
		CreateInterObstacleWalk ();
		
		
		agents=new List<T5Agent>();
		agentColors=new List<Color>();
		agentColors.Add (Color.black);
		agentColors.Add (Color.blue);
		agentColors.Add (Color.yellow);
		agentColors.Add (Color.cyan);
		agentColors.Add (Color.green);
		
		
		paths = new List<Vector3>[map.polyData.start.Count];
		agentAtWaypoint=new int[map.polyData.start.Count];
		
		int agentCounter = 0;
		
		for (int i=0; i<map.polyData.start.Count; i=i+1) {
			Vector3 startNode=map.polyData.start[i];
			Vector3 endNode=map.polyData.end[i];
			T5Agent newAgent=new T5Agent("Agent "+agentCounter, startNode, endNode, agentCounter,true);
			newAgent.agent.gameObject.renderer.material.color=agentColors[i];
			List<PolyNode> ppath = pathFinder.AStarSearch (startNode, endNode, graph);
			List<Vector3> temp = new List<Vector3> ();
			foreach (PolyNode p in ppath) 
				temp.Add (p.pos);
			//temp.Add(endNode);
			paths[i]=temp;
			agentAtWaypoint[i]=0;
			agents.Add(newAgent);
			agentCounter++;
		}
		
		Debug.Log ("Agents size:" + agents.Count);
		
		
		StartCoroutine("Move");
		
	}
	
	Vector3 blackGoal = Vector3.zero;
	IEnumerator Move() {
		bool[] atGoal=new bool[agents.Count];
		for (int i=0; i<atGoal.Length; i++)
			atGoal [i] = false;
		float timeBefore = Time.time;
		
		while (true) {
			
			//Check if all agents at goal
			int agentsAtGoal=0;
			for(int i=0;i<agents.Count;i++){
				if(agents[i].isAtGoal(goalInterval)) {
					agentsAtGoal++;
				}
			}


			if(agentsAtGoal==agents.Count){
				Debug.Log("Done");
				float timeAfter=Time.time;
				Debug.Log("Time:"+(timeAfter-timeBefore));
				yield break;
			}

			//Iterate all agents
			for (int i=0; i<agents.Count; i++) {
				
				List<Vector3> curPath=paths[i];
				int curAgentAtWaypoint=agentAtWaypoint[i];
				T5Agent curAgent=agents[i];
				Vector3 current=curPath[curAgentAtWaypoint];

				//If the current agent is at it's goal it should not move anymore
				if(curAgent.isAtGoal(goalInterval)){
		//			print (agents[i].id + " done ");
					curAgent.velocity=Vector3.zero;
					continue;
				}

	
				if(Vector3.Distance(curAgent.agent.transform.position,current)<goalInterval){
					curAgentAtWaypoint++;
					agentAtWaypoint[i]=curAgentAtWaypoint;
					if(curAgentAtWaypoint>=curPath.Count){
						curAgent.velocity=Vector3.zero;
						continue;
					}
					current=curPath[curAgentAtWaypoint];
				}
				
				bool straightToGoal=curAgent.checkStraightWayToGoal(obstacles);
				if(straightToGoal){
					current=curAgent.goalPos;
					curAgentAtWaypoint=curPath.Count-1;
					agentAtWaypoint[i]=curAgentAtWaypoint;
				}


				bool stuck=curAgent.checkStuck(obstacles,current);
				//stuck = false;
			newPath:
				if(stuck){
					Vector3 pos=curAgent.agent.transform.position;
					Vector3 towardsWaypoint=Vector3.Normalize(current-pos);
					Vector3 edgeOfAgent=pos+towardsWaypoint*curAgent.agentSize;
					this.addPointToGraph(edgeOfAgent);
					List<PolyNode> ppath = pathFinder.AStarSearch (edgeOfAgent, 
					                                               curAgent.goalPos, graph);
					List<Vector3> temp = new List<Vector3> ();
					foreach (PolyNode p in ppath) 
						temp.Add (p.pos);
					
					curPath=temp;
					paths[i]=curPath;
					curAgentAtWaypoint=0;
					agentAtWaypoint[i]=0;
					current=curPath[curAgentAtWaypoint];
					print (curAgent.id + " ASTAR");
				}

				if(curAgent.id == "Agent 1")
					blackGoal = current;
			
				
				//Vector3 current=curAgent.goalPos;
				Vector3 dynPVel=curAgent.velocity;
				
				
				
				
				//The code below is used for the 2nd solution of T4
				
				curAgent.findMinimumPenaltyVelCar(ref agents,accMax,current,goalInterval,obstacles,maxWheelAngle
				                                  ,carLength);


				Vector3 newRot=curAgent.velocity;
			//	Vector3 newVel=curAgent.getCarVelocity();
				float curVel=curAgent.velSize;

				
			//	print (curAgent.id + " : " + curAgent.getCarVelocity().magnitude + "\n"
			//	       + "curVel: " + curVel + " newVel " + newVel);

				bool care = false;
				bool flag = true;
				foreach(T5Agent agent in agents) {
					if(curAgent != agent) {
						if(Vector3.Distance (curAgent.agent.transform.position, agent.agent.transform.position) < 33) {
							care = true;
							break;
						}
					}
				}
				if(!care) {
					print (curAgent.id + " I DON'T CARE !!");
					if(curVel == 0)
						curVel = 1;
				} else {
					print (curAgent.id + " I CARE !!");
				}



				Vector3 curPos=curAgent.agent.transform.position;
				
			//	Vector3 moveTowards=curPos+newVel;
			//	float step=newVel.magnitude*Time.deltaTime;
				//Debug.Log("Step:"+step);


				float wheelAngleRad = maxWheelAngle * (Mathf.PI / 180);
				float dTheta=(curAgent.velSize/carLength)*Mathf.Tan(wheelAngleRad);

				Quaternion newLookRot;
				if(care) {
					newLookRot=Quaternion.Euler(newRot);
				}
				else {
					newLookRot = Quaternion.LookRotation (current - curAgent.agent.transform.position);
				}

				if(curAgent.agent.transform.rotation!=newLookRot){
					curAgent.agent.transform.rotation = Quaternion.RotateTowards (curAgent.agent.transform.rotation, newLookRot, dTheta);
				}

				curAgent.velocity=curAgent.agent.transform.rotation.eulerAngles;

				Vector3 curDir=curAgent.agent.transform.eulerAngles;
				Vector3 newPos=curAgent.agent.transform.position;
				float angleRad=curDir.y*(Mathf.PI/180);
				newPos.x=newPos.x+(curVel*Mathf.Sin(angleRad)*Time.deltaTime);
				newPos.z=newPos.z+(curVel*Mathf.Cos(angleRad)*Time.deltaTime);

				curAgent.agent.transform.position=newPos;
				
				yield return null;
			}
			
		}
		
	}
	
	
	public void CreateInterObstacleWalk() {
		foreach (Obstacle obs in obstacles) {
			foreach(Line line in obs.edges) {
				walkableLines.Add (new Line(line.point1, line.point2)); // debug
				walkableLines.Add (new Line(line.point2, line.point1)); // debug
				
				graph.graph[line.point1].neighbours.Add (line.point2);
				graph.graph[line.point2].neighbours.Add (line.point1);
			}
		}
	}
	
	public void CreateObstacles() {
		int j = 0;
		Obstacle obs = new Obstacle ();
		foreach (int i in polyData.buttons) {
			obs.vertices.Add (polyData.nodes[j]);
			obs.id = j;
			if(i != 3) {
				//print (j);
				obs.edges.Add (polyData.lines[j++]);
			}
			else {
				obs.edges.Add (polyData.lines[j++]);
				obstacles.Add(new Obstacle(obs));
				obs.edges.Clear ();
				obs.vertices.Clear ();
			}
		}
		
		// Start
		foreach (Vector3 start in polyData.start) {
			Obstacle o = new Obstacle();
			o.id = j++;
			o.vertices.Add (start);
			obstacles.Add (o);
		}
		
		// End
		foreach (Vector3 end in polyData.end) {
			Obstacle o = new Obstacle();
			o.id = j++;
			o.vertices.Add (end);
			obstacles.Add (o);
		}
		
		// Customers
		foreach (Vector3 customer in polyData.customers) {
			Obstacle o = new Obstacle();
			o.id = j++;
			o.vertices.Add (customer);
			obstacles.Add (o);
		}
	}
	
	void ConstructWalkableLines() {
		
		// For every obstacle and it's neighbours
		// See if a line can be drawn from each vertex from the current obstacle to 
		// it's neighbours' vertices.
		// If the line does not intersect with any other line, add it to walkableLines
		int index = 0;
		foreach (Obstacle obs in obstacles) {
			foreach (Obstacle neigh in obstacles) {
				if(obs != neigh) {
					foreach(Vector3 vertex in obs.vertices) {
						PolyNode currentNode = new PolyNode();
						currentNode.pos = vertex;
						foreach(Vector3 neighVertex in neigh.vertices) {
							Line potentialLine = new Line(vertex, neighVertex);
							if(!IntersectsWithAnyLine(potentialLine)){
								walkableLines.Add (potentialLine); //debugging
								
								currentNode.neighbours.Add (neighVertex);
							}
						}
						if(graph.graph.ContainsKey (vertex)) 
							graph.graph[vertex].neighbours.AddRange (currentNode.neighbours);
						else
							graph.graph[vertex] = currentNode;
					}
				}
			}
			index++;
		}
	}
	
	public bool IntersectsWithAnyLine(Line myLine) {
		foreach (Obstacle obs in obstacles) {
			foreach (Line line in obs.edges) {
				if (myLine.point1 == line.point1 || myLine.point1 == line.point2)
					continue;
				if (myLine.point2 == line.point1 || myLine.point2 == line.point2)
					continue;
				
				if (myLine.intersect (line)) {
					//print (myLine.point1 + ", " + myLine.point2 + " vs " + line.point1 + ", " + line.point2);
					return true;
				}
			}
		}
		return false;
	}
	
	private void addPointToGraph(Vector3 newPoint){
		
		Obstacle obs = new Obstacle();
		obs.id = 1000;
		obs.vertices.Add (newPoint);
		
		foreach (Obstacle neigh in obstacles) {
			if(obs != neigh) {
				foreach(Vector3 vertex in obs.vertices) {
					PolyNode currentNode = new PolyNode();
					currentNode.pos = vertex;
					foreach(Vector3 neighVertex in neigh.vertices) {
						Line potentialLine = new Line(vertex, neighVertex);
						if(!IntersectsWithAnyLine(potentialLine)){
							walkableLines.Add (potentialLine); //debugging
							
							currentNode.neighbours.Add (neighVertex);
						}
					}
					if(graph.graph.ContainsKey (vertex)) 
						graph.graph[vertex].neighbours.AddRange (currentNode.neighbours);
					else
						graph.graph[vertex] = currentNode;
				}
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

		Gizmos.color = Color.red;
		Gizmos.DrawSphere (blackGoal, 5);
	}
	
	
}
