using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Agent {
	public Node start;
	//public Node end;
	public List<Node> waypoints;
	public GameObject agent;
	public string id;
	
	public Agent(string id, Node start, List<Node> waypoints) {
		this.start = start;
		this.waypoints = waypoints;
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = start.worldPosition;
	}
}

public class DiscreteMovement : MonoBehaviour {
	
	List<Agent> agents = new List<Agent> ();
	AStar astar;
	Grid grid;
	
	List<Node> debug = null;
	
	void Start () {
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<Grid> ();
		
		astar = new AStar (grid.rTable);
		Node startNode = grid.grid [Convert.ToInt32(grid.mapData.start.x), Convert.ToInt32 (grid.mapData.start.y)];
		Node endNode = grid.grid [Convert.ToInt32 (grid.mapData.end.x), Convert.ToInt32 (grid.mapData.end.y)];
		
		List<Node> w1 = new List<Node> ();
		w1.Add (grid.grid [19, 9]);
		w1.Add (grid.grid [0, 9]);
		agents.Add (new Agent("Red", grid.grid[0, 9], w1));
		agents [0].agent.renderer.material.color = Color.red;

		List<Node> w2 = new List<Node> ();
		w2.Add (grid.grid [0, 9]);
		w2.Add (grid.grid [19, 9]);
		agents.Add (new Agent ("Magenta", grid.grid[19, 9], w2));
		agents [1].agent.renderer.material.color = Color.magenta;

		List<Node> w3 = new List<Node> ();
		w3.Add (grid.grid [19, 8]);
		w3.Add (grid.grid [0, 8]);
		agents.Add (new Agent ("Yellow", grid.grid[1, 9], w3));
		agents [2].agent.renderer.material.color = Color.yellow;

		//	Node node = grid.grid [19, 0];
		//	debug = node.neighbours;
		
		StartAllAgents ();
	}
	
	public void StartAllAgents() {
		bool moving = true;
		foreach (Agent agent in agents) {
			StartCoroutine (Move (agent));
		}
	}
	
	public PathInfo RequestPath(Node startNode, Node endNode) {
		PathInfo pathInfo = astar.STAStar (startNode, endNode);
		
		/*
		int i = 1;
		foreach (Node node in path) {
			State state = new State(node.gridPosX, node.gridPosY, i++);
			grid.rTable.Add (state, 1);
		}
		*/
		
		int i = 1;
		for(int j = 0; j < pathInfo.path.Count; j++) {
			if(j != pathInfo.path.Count - 1) {
				State cState = new State(pathInfo.path[j].gridPosX, pathInfo.path[j].gridPosY, i);
				State nState = new State(pathInfo.path[j+1].gridPosX, pathInfo.path[j+1].gridPosY, i); 
				grid.rTable.Add (cState, 1);
				grid.rTable.Add (nState, 2);
			}
			i++;
		}
		
		return pathInfo;
	}
	
	IEnumerator Move(Agent agent) {
		Node start = agent.start;
		int r = 0;
		Node end = agent.waypoints[r];
		PathInfo pathInfo;
		while (true) {
			Node pastNode = null;
			pathInfo = RequestPath (start, end);
			print (agent.id + " Requesting: " + "[" + start.gridPosX + ", " + start.gridPosY + "] -> [" +
			       end.gridPosX + "," + end.gridPosY + "]\n" + "With result: " + pathInfo.path.Count +
			       " and success: " + pathInfo.reachedDestination);
	
			if(pathInfo.reachedDestination) {
				r++;
				if(r < agent.waypoints.Count) {
					end = agent.waypoints[r];
				}

				// Test remove when reached final goal
				/*
				if(r == agent.waypoints.Count + 1 && agent.id == "Red") {
					// remove when reached final destination
					print (agent.id + " yield break (" + r + "/" + agent.waypoints.Count + ")");
					int t = 1;
					foreach (Node node in pathInfo.path) {
						State state = new State(node.gridPosX, node.gridPosY, t++);
						grid.rTable.Free (state);
					}
					yield break;
				}
				*/

			}
			
			int i = 1;
			//Node pos = pathInfo.path[pathInfo.path.Count - 1];
			Node pos = null;
			foreach (Node node in pathInfo.path) {
				// Extra collision rule since request order can cause trouble
				bool walkable = true;
				foreach(Agent a in agents) {
					if(a.agent.transform.position == node.worldPosition) {
						walkable = false;
					}
				}
			//	if(walkable){
					agent.agent.transform.position = node.worldPosition;
					pos = node;
			//	}
				
				if (pastNode != null) {
					State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
					grid.rTable.Free (state);
					state.t = state.t - 1;
					grid.rTable.Free (state);
					
					//print (agent.id + " Freeing: " + state.x + ", " + state.y + ", " + state.t);
				}
				pastNode = node;
				/*
                print(agent.id + ": [" + node.gridPosX + ", " + node.gridPosY + ", " + i + "] "
                      + grid.rTable.Occupied (new State(node.gridPosX, node.gridPosY, i)));
                */

				//yield return null;
				yield return new WaitForSeconds (0.2f);
			}
			
			//start = pathInfo.path[pathInfo.path.Count - 1];
			start = pos;
			
			if(pastNode != null) {
				State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
				grid.rTable.Free (state);
				state.t = state.t - 1;
				grid.rTable.Free (state);
			}
			
		}
	}
	
	void OnDrawGizmos() {

		Gizmos.color = Color.cyan;
		foreach (KeyValuePair<State, int> p in grid.rTable.rTable) {
			if(p.Value != 0)
				Gizmos.DrawCube (grid.grid [p.Key.x, p.Key.y].worldPosition - Vector3.up, Vector3.one);
		}
		Gizmos.color = Color.yellow;
		if (debug != null) {
			foreach (Node node in debug) {
				Gizmos.DrawCube (node.worldPosition, Vector3.one);
				print ("drawing: " + node.gridPosX + ", " + node.gridPosY);
			}
		}
	}
}