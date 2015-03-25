using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class DiscreteVRP : MonoBehaviour {
	Stopwatch stopwatch = new Stopwatch();
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
		
		astar = new AStar (grid.rTable, grid);
		endPos = new Dictionary<Agent, Node> ();
		for(int i = 0; i < grid.mapData.start.Count; i++) {
			Vector2 pos = grid.mapData.start[i];
			Node node = grid.grid[(int)(pos[0]), (int)(pos[1])];
			agents.Add (new Agent("Agent " + i, node, new List<Node> ()));
			agents [i].agent.renderer.material.color = Color.blue;
			agents [i].wpReached = true;
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
		
		vrp = new VRPD (customers, agents.Count);
		ready = agents.Count;
		stopwatch.Start();
	}
	
	bool STOP = false;
	Dictionary<Agent, int> steps = new Dictionary<Agent, int> ();
	int priority = 0;
	bool moveTowardsGoal = false;
	void Update() {
		// TIME START
		bool done = true;
		foreach(Agent agent in agents) {
			if(!(agent.pos.gridPosX == endPos[agent].gridPosX &&
			   agent.pos.gridPosY == endPos[agent].gridPosY)) {
				done = false;
				break;
			}
		}

		if (done && !STOP) {
			STOP = true;
			int longest = 0;
			foreach(Agent agent in agents) {
				if(steps[agent] > longest) 
					longest = steps[agent];
			}
			print ("Steps: " + longest);
		}
		// TIME END

		List<Agent> reachedAgents = new List<Agent> ();
		foreach (Agent agent in agents) {
			if(agent.wpReached)
				reachedAgents.Add (agent);
		}

		if (reachedAgents.Count == agents.Count) {
			while(reachedAgents.Count > 0) {
				try{
					// Get customer and nearest agent
					KeyValuePair<Agent, Node> an = vrp.NextAgentAndCustomer (reachedAgents, endPos);
					an.Key.wpReached = false;
					an.Key.waypoints.Add (an.Value);
					StartCoroutine (Move (an.Key, an.Value));
					reachedAgents.Remove (an.Key);
				}
				catch{
					// All customers are accounted for
					if(reachedAgents.Count == agents.Count)
						moveTowardsGoal = true;
					foreach(Agent agent in reachedAgents) {
						// Move towards end position
						agent.wpReached = false;
						agent.waypoints.Add (endPos[agent]);
						StartCoroutine (Move (agent, endPos[agent]));
					}
					reachedAgents.Clear();
					break;
				}
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
		State initState = new State(agent.pos.gridPosX, agent.pos.gridPosY, 1);
		grid.rTable.Add (initState, 1);
		
		Node start = agent.pos;
		//Node end = agent.waypoints[agent.wp];
		PathInfo pathInfo;
		Node pastNode = null;
		
		pathInfo = RequestPath (start, end);
		
		/*
		print (agent.id + " Requesting: " + "[" + start.gridPosX + ", " + start.gridPosY + "] -> [" +
		       end.gridPosX + "," + end.gridPosY + "]\n" + "With result: " + pathInfo.path.Count +
		       " and success: " + pathInfo.reachedDestination + " status: " + pathInfo.status);
		*/
		
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
				walkable=true;//TODO is this ok?
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
			yield return null;
			//yield return new WaitForSeconds(0.5f);
			if(!steps.ContainsKey (agent))
				steps[agent] = 0;
			steps[agent] = steps[agent] + 1;
			grid.rTable.Free (initState);
			
			//yield return new WaitForSeconds (0.2f);
		}
		
		if(pastNode != null) {
			State state = new State(pastNode.gridPosX, pastNode.gridPosY, i++);
			grid.rTable.Free (state);
			state.t = state.t + 1;
			grid.rTable.Free (state);
		}
		
		if (agent.pos == end) {
			if (agent.waypoints.Count > 1)
				agent.waypoints.RemoveAt (agent.wp);
			//			print ("End dest");

			// Reached goal (perhaps early)
		}

		debug.Remove (agent.waypoints [agent.waypoints.Count - 1]);
		agent.wpReached = true;
		ready++;
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

		foreach (KeyValuePair<State, int> p in grid.rTable.rTable) {
			if(p.Value != 0)
				Gizmos.DrawCube (grid.grid [p.Key.x, p.Key.y].worldPosition - Vector3.up, Vector3.one);
		}

		Gizmos.color = Color.yellow;
		if (debug != null) {
			foreach (Node node in debug) {
				Gizmos.DrawCube (node.worldPosition, Vector3.one);
				//	print ("drawing: " + node.gridPosX + ", " + node.gridPosY);
			}
		}
	}
}