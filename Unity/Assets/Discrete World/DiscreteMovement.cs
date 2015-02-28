using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Agent {
	public Node pos;
	public List<Node> waypoints;
	public int wp;
	public GameObject agent;
	public string id;
	
	public Agent(string id, Node start, List<Node> waypoints) {
		this.waypoints = waypoints;
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = start.worldPosition;
		pos = start;
		wp = 0;
	}
}

public class DiscreteMovement : MonoBehaviour {

	int ready;
	int readyS = 0;

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
		//agents.Add (new Agent("Red", grid.grid[8, 9], w1));
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

		List<Node> w4 = new List<Node> ();
		w4.Add (grid.grid [9, 9]);
		agents.Add (new Agent ("Black", grid.grid[9, 9], w4));
		agents [3].agent.renderer.material.color = Color.black;


		ready = agents.Count;
	}

	void Update() {
		if (ready == agents.Count) {
			State state;

			ready = 0;
			int index = readyS;	
			for(int i = 0; i < agents.Count; i++) {
				StartCoroutine (Move (agents[index]));
				index = (index + 1) % agents.Count;
			}

			readyS = (readyS + 1) % agents.Count;
        }

		foreach (Agent agent in agents) {
			foreach(Agent agent2 in agents) {
				if(agent != agent2) {
					if(agent.pos == agent2.pos) {
						print ("!HEAD TO HEAD COLLISION! between: \n" + agent.id + " and " + agent2.id);
					}
				}
			}
		}
    }
	
	public void StartAllAgents() {
		bool moving = true;
		foreach (Agent agent in agents) {
			StartCoroutine (Move (agent));
		}
	}
	
	public PathInfo RequestPath(Node startNode, Node endNode) {
		PathInfo pathInfo = astar.STAStar (startNode, endNode);
		int i = 1;
		for(int j = 0; j < pathInfo.path.Count; j++) {
		//	if(j != pathInfo.path.Count - 1) {
				State cState = new State(pathInfo.path[j].gridPosX, pathInfo.path[j].gridPosY, i);
				State nState = new State(pathInfo.path[j].gridPosX, pathInfo.path[j].gridPosY, i+1); 
				grid.rTable.Add (cState, 1);
				grid.rTable.Add (nState, 2);
			//}
			i++;
		}
		
		return pathInfo;
	}

	IEnumerator Move(Agent agent) {
		State initState = new State(agent.pos.gridPosX, agent.pos.gridPosY, -1);
		grid.rTable.Add (initState, 1);

		Node start = agent.pos;
		Node end = agent.waypoints[agent.wp];
		PathInfo pathInfo;
		Node pastNode = null;

		pathInfo = RequestPath (start, end);

		/*
		print (agent.id + " Requesting: " + "[" + start.gridPosX + ", " + start.gridPosY + "] -> [" +
		       end.gridPosX + "," + end.gridPosY + "]\n" + "With result: " + pathInfo.path.Count +
		       " and success: " + pathInfo.reachedDestination + " status: " + pathInfo.status);
*/

		if(pathInfo.reachedDestination) {
			if(agent.wp != agent.waypoints.Count - 1)
				agent.wp = agent.wp + 1;
		}
		
		int i = 1;
		//Node pos = pathInfo.path[pathInfo.path.Count - 1];
		Node pos = null;
		bool pause = false;
		foreach (Node node in pathInfo.path) {
			if(!pause) {
				bool walkable = true;
				foreach(Agent a in agents) {
					if(a.pos == node) {
						walkable = false;
						break;
					}
				}
				if(walkable) {
					agent.agent.transform.position = node.worldPosition;
					agent.pos = node;
				}
				else {
					pause = true;
				}
			}

			if (pastNode != null) {
				State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
				grid.rTable.Free (state);
				state.t = state.t + 1;
				grid.rTable.Free (state);
				
				//print (agent.id + " Freeing: " + state.x + ", " + state.y + ", " + state.t);
            }
            pastNode = node;
            //yield return null;
            yield return new WaitForSeconds (0.2f);
        }
        
        if(pastNode != null) {
            State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
            grid.rTable.Free (state);
            state.t = state.t + 1;
            grid.rTable.Free (state);
        }
        
        ready++;
		grid.rTable.Free (initState);
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