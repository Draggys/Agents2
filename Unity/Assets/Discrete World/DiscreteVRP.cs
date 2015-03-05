using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DiscreteVRP : MonoBehaviour {
	
	int ready;
	int readyS = 0;
	
	List<Agent> agents = new List<Agent> ();
	AStar astar;
	Grid grid;
	VRPD vrp;
	
	List<Node> debug = null;
	Dictionary<Agent, Node> endPos = null;
	
	void Start () {
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<Grid> ();
		
		astar = new AStar (grid.rTable);
//		Node startNode = grid.grid [Convert.ToInt32(grid.mapData.start.x), Convert.ToInt32 (grid.mapData.start.y)];
//		Node endNode = grid.grid [Convert.ToInt32 (grid.mapData.end.x), Convert.ToInt32 (grid.mapData.end.y)];

		/*
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
		*/

		endPos = new Dictionary<Agent, Node> ();
		for(int i = 0; i < grid.mapData.start.Count; i++) {
			Vector2 pos = grid.mapData.start[i];
			Node node = grid.grid[(int)(pos[0]), (int)(pos[1])];
			agents.Add (new Agent("Agent " + i, node, new List<Node> ()));
			agents [i].agent.renderer.material.color = Color.blue;

			Vector2 end = grid.mapData.end[i];
			Node endNode = grid.grid[(int)end[0], (int)end[1]];
			endPos[agents[i]] = endNode;
		}

		List<Node> customers = new List<Node> ();
		for (int i = 0; i < grid.mapData.customers.Count; i++) {
			Vector2 pos = grid.mapData.customers[i];
			Node node = grid.grid[(int)pos[0], (int)pos[1]];
			customers.Add (node);
		}
		debug = new List<Node> (customers);

		vrp = new VRPD (customers);
		ready = agents.Count;
		/*
		List<Node> customers = new List<Node> ();
		customers.Add (grid.grid[19, 19]);
		customers.Add (grid.grid[1, 19]);
		customers.Add (grid.grid[1, 1]);
		customers.Add (grid.grid[10,10]);
		customers.Add (grid.grid[19,1]);
		customers.Add (grid.grid[5, 1]);

		agents.Add (new Agent("Red", grid.grid[0, 9], new List<Node> ()));
		agents [0].agent.renderer.material.color = Color.red;
		
		agents.Add (new Agent("Blue", grid.grid[19, 19], new List<Node> ()));
		agents [1].agent.renderer.material.color = Color.blue;

		agents.Add (new Agent ("Yellow", grid.grid[1, 9], new List<Node> ()));
		agents [2].agent.renderer.material.color = Color.yellow;

		agents.Add (new Agent ("Black", grid.grid[9, 9], new List<Node> ()));
		agents [3].agent.renderer.material.color = Color.black;

		endPos = new Dictionary<Agent, Node> ();
		endPos [agents [0]] = grid.grid [15, 15];
		endPos [agents [1]] = grid.grid [1, 1];
		endPos [agents [2]] = grid.grid [1, 9];
		endPos [agents [3]] = grid.grid [9, 9];

		vrp = new VRPD (customers);
		
		ready = agents.Count;
		*/
	}
/*	
	int priority = 0;
	void Update() {
		if (ready == agents.Count && agents.Count > 0) {
			
			State state;
			
			ready = 0;
			int index = readyS;	
			for(int i = 0; i < agents.Count; i++) {
				StartCoroutine (Move (agents[index], GreedyNext (agents[index])));
				index = (index + 1) % agents.Count;
			}
			
			if(priority++ == 3){ //|| agents[index].pos == agents[index].waypoints[agents[index].wp]) {
				readyS = (readyS + 1) % agents.Count;
				priority = 0;
			}
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
*/
	void Update() {
		if (ready == agents.Count && agents.Count > 0) {
			ready = 0;
			List<Agent> reachedAgents = new List<Agent> ();
			foreach(Agent agent in agents) {
				if(agent.wpReached) {
					reachedAgents.Add (agent);
				}
				else {
					if(agent.waypoints.Count == 0) {
						agent.waypoints.Add (agent.pos);
					}
					StartCoroutine (Move (agent, agent.waypoints[0]));
				}
			}

			while(reachedAgents.Count != 0) {
				if(vrp.customers.Count == 0) {
					foreach(Agent agent in reachedAgents) {
						agent.waypoints.Add (endPos[agent]);
						StartCoroutine (Move (agent, endPos[agent]));
					}
					break;
				}
				KeyValuePair<Agent, Node> an = vrp.NextAgentAndCustomer(reachedAgents, endPos);
				an.Key.wpReached = false;
				an.Key.waypoints.Add (an.Value);
				StartCoroutine (Move (an.Key, an.Value));
				reachedAgents.Remove (an.Key);
			}
		}
	}
	
	public PathInfo RequestPath(Node startNode, Node endNode) {
		PathInfo pathInfo = astar.STAStar (startNode, endNode);
		int i = 1;
		for(int j = 0; j < pathInfo.path.Count; j++) {
			State cState = new State(pathInfo.path[j].gridPosX, pathInfo.path[j].gridPosY, i);
			State nState = new State(pathInfo.path[j].gridPosX, pathInfo.path[j].gridPosY, i+1); 
			grid.rTable.Add (cState, 1);
			grid.rTable.Add (nState, 2);
			i++;
		}
		
		return pathInfo;
	}
	
	IEnumerator Move(Agent agent, Node end) {
		State initState = new State(agent.pos.gridPosX, agent.pos.gridPosY, -1);
		grid.rTable.Add (initState, 1);
		
		Node start = agent.pos;
		//Node end = agent.waypoints[agent.wp];
		PathInfo pathInfo;
		Node pastNode = null;
		
		pathInfo = RequestPath (start, end);
		
		
		print (agent.id + " Requesting: " + "[" + start.gridPosX + ", " + start.gridPosY + "] -> [" +
		       end.gridPosX + "," + end.gridPosY + "]\n" + "With result: " + pathInfo.path.Count +
		       " and success: " + pathInfo.reachedDestination + " status: " + pathInfo.status);
		
		
		int i = 1;
		//Node pos = pathInfo.path[pathInfo.path.Count - 1];
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
		
		if (agent.pos == end) {
			debug.Remove (agent.waypoints[0]);

			agent.waypoints.RemoveAt (0);
			print ("End dest");
			agent.wpReached = true;
		}
		//agent.wpReached = false;
		
		
		ready++;
		grid.rTable.Free (initState);
	}
	
	Node GreedyNext(Agent agent) {
		if (agent.wpReached) {
			agent.wpReached = false;
			Node node = vrp.NextCustomer (agent, endPos[agent]);

			if(node == null){
				agent.waypoints.Add (endPos[agent]);
				return endPos[agent];
			}

			agent.waypoints.Add (node);
			return node;
		}
		if(agent.waypoints.Count == 0) {
			agent.waypoints.Add (agent.pos);
		}
		return agent.waypoints [0];
	}

	void OnDrawGizmos() {
		
		Gizmos.color = Color.cyan;
		/*
		foreach (KeyValuePair<State, int> p in grid.rTable.rTable) {
			if(p.Value != 0)
				Gizmos.DrawCube (grid.grid [p.Key.x, p.Key.y].worldPosition - Vector3.up, Vector3.one);
		}
		*/
		Gizmos.color = Color.yellow;
		if (debug != null) {
			foreach (Node node in debug) {
				Gizmos.DrawCube (node.worldPosition, Vector3.one);
			//	print ("drawing: " + node.gridPosX + ", " + node.gridPosY);
			}
		}
	}
}