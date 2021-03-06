﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class Agent {
	public Node pos;
	public List<Node> waypoints;
	public int wp;
	public bool wpReached;
	public GameObject agent;
	public string id;
	
	public Agent(string id, Node start, List<Node> waypoints) {
		this.waypoints = waypoints;
		this.id = id;
		agent = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		agent.transform.position = start.worldPosition;
		pos = start;
		wp = 0;
		wpReached = false;
	}
}

public class DiscreteMovement : MonoBehaviour {
	Stopwatch stopwatch = new Stopwatch();
	int ready;
	int readyS = 0;
	
	List<Agent> agents = new List<Agent> ();
	AStar astar;
	Grid grid;
	VRPD vrp;
	
	List<Node> debug = null;
	
	void Start () {
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<Grid> ();

		astar = new AStar (grid.rTable, grid);

		for(int i = 0; i < grid.mapData.start.Count; i++) {
			Vector2 pos = grid.mapData.start[i];
			Node node = grid.grid[(int)(pos[0]), (int)(pos[1])];
			List<Node> temp=new List<Node> ();
			Vector2 end = grid.mapData.end[i];
			Node endNode = grid.grid[(int)end[0], (int)end[1]];
			temp.Add(endNode);

			agents.Add (new Agent("Agent " + i, node, temp));
			// test
			/*
			temp.Clear ();
			temp.Add (grid.grid[1,0]);
			agents.Add (new Agent("TestAgent ", grid.grid[0,0], temp));
			State state = new State(1, 0, 2);
			grid.rTable.Add (state, 1);
			*/
			agents [i].agent.renderer.material.color = Color.blue;
		}
		stopwatch.Start();
		ready = agents.Count;

		Application.runInBackground = true;
	}

	Dictionary<Agent, int> steps = new Dictionary<Agent, int> ();

	bool STOP = false;
	int priority = 0;
	void Update() {

		if (ready == agents.Count && agents.Count > 0) {
			State state;
			
			ready = 0;
			int index = readyS;	
			for (int i = 0; i < agents.Count; i++) {
				StartCoroutine (Move (agents [index], GreedyNext (agents [index])));
				index = (index + 1) % agents.Count;
			}
			
			if (priority++ == 1) {
				readyS = (readyS + 1) % agents.Count;
				priority = 0;
			}
		}
		bool done = true;
		for (int i = 0; i < agents.Count; i++) {
			Vector2 end = grid.mapData.end[i];
			Node node = grid.grid[(int)end[0], (int)end[1]];
			if(!(agents[i].pos.gridPosX == node.gridPosX && 
			   agents[i].pos.gridPosY == node.gridPosY)) {
				done = false;
				break;
			}
		}

		if (done && agents.Count > 0 && !STOP) {
			STOP = true;
			stopwatch.Stop ();
			print ("Time elapsed: " + stopwatch.Elapsed);

			int longest = 0;
			foreach(Agent agent in agents) {
				if(steps[agent] > longest) 
					longest = steps[agent];
			}
			print ("Steps: " + longest);
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
			//yield return null;
			yield return new WaitForSeconds(0.5f);
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
		}
		//agent.wpReached = false;
		
		
		ready++;
	}
	
	Node GreedyNext(Agent agent) {
		if (agent.wpReached) {
			agent.wpReached = false;
			if (agent.waypoints.Count > 1) {
				float bestCost = -1;
				int j = -1;
				for (int i = 0; i < agent.waypoints.Count; i++) {
					float cost = astar.TrueDistance (agent.waypoints [i], agent.pos);
					if (cost == -1 || cost < bestCost) {
						bestCost = cost;
						j = i;
					}
				}
				agent.wp = j;
				return agent.waypoints [j];
			}
		}
		return agent.waypoints [agent.wp];
	}
	
	
	void OnDrawGizmos() {
		
		Gizmos.color = Color.cyan;
		
		foreach (KeyValuePair<State, int> p in grid.rTable.rTable) {
			if (p.Value != 0)
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