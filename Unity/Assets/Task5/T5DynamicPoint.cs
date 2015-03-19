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


	List<Obstacle> obstacles = new List<Obstacle> ();
	PolyData polyData = null;
	
	List<Line> walkableLines;
	VGraph graph;
	PolygonalAStar pathFinder = new PolygonalAStar();

	List<Vector3>[] paths;
	int[] agentAtWaypoint;
	
	// Use this for initialization
	void Start () {
		
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
			T5Agent newAgent=new T5Agent("Agent "+agentCounter, startNode, endNode, agentCounter);
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
	
	
	IEnumerator Move() {
		bool[] atGoal=new bool[agents.Count];
		for (int i=0; i<atGoal.Length; i++)
			atGoal [i] = false;
		float timeBefore = Time.time;
		
		while (true) {
			
			//Check if all agents at goal
			int agentsAtGoal=0;
			for(int i=0;i<agents.Count;i++){
				if(agents[i].isAtGoal(goalInterval))
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

				List<Vector3> curPath=paths[i];
				int curAgentAtWaypoint=agentAtWaypoint[i];
				T5Agent curAgent=agents[i];
				Vector3 current=curPath[curAgentAtWaypoint];

				//If the current agent is at it's goal it should not move anymore
				if(curAgent.isAtGoal(goalInterval)){
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

				}

				//Vector3 current=curAgent.goalPos;
				Vector3 dynPVel=curAgent.velocity;



				
				//The code below is used for the 2nd solution of T4
				
				Vector3 newVel=curAgent.findMinimumPenaltyVel(agents,accMax,current,goalInterval,obstacles);
				
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

	}
	
	
}
