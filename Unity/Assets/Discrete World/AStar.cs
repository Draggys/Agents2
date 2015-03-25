using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct PathInfo {
	public List<Node> path;
	public bool reachedDestination;  // This is now unreliable due to recursive call of STA*
	public string status;
	
	public PathInfo(List<Node> path, bool reachedDestination, String status) {
		this.path = path;
		this.reachedDestination = reachedDestination;
		this.status = status;
	}
}

public class AStar : MonoBehaviour{
	ReservationTable rTable;
	Grid grid;
	public int d = 7;
	
	public AStar(ReservationTable rTable, Grid grid) {
		this.rTable = rTable;
		this.grid = grid;
	}
	
	// AStar created with this constructor can (SHOULD) only use AStarSearch
	public AStar() {
		print ("THIS ASTAR CONSTRUCTOR CAN (SHOULD) ONLY USE FUNCTIONS INDEPENDENT OF A RESERVATION TABLE");
	}
	
	Node GetNode(Node currentNode, int dir) {
		List<Node> directions = currentNode.neighbours;
		return directions [dir];
	}
	
	public PathInfo STAStar(Node startNode, Node targetNode, int run = 0) {
		PriorityQueue<Node, float> frontier = new PriorityQueue<Node, float> ();
		Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node> ();
		Dictionary<Node, float> costSoFar = new Dictionary<Node, float> ();

		Dictionary<Node, float> total = new Dictionary<Node, float> ();
		
		frontier.Enqueue(startNode, 0f);
		cameFrom [startNode] = null;
		costSoFar [startNode] = 0;
		total [startNode] = 0;
		
		Node currentNode;
		int i = 0;
		int count = 0;
		bool once = true;

		while (frontier.Count() != 0) {
			currentNode = frontier.Dequeue ();
			
			List<Node> path = ConstructPath (startNode, currentNode, cameFrom);
			count = path.Count;
			
			if(currentNode.gridPosX == targetNode.gridPosX &&
			   currentNode.gridPosY == targetNode.gridPosY) {
				// Staying in place is always a viable option if it's not occupied
				Node stayNode = new Node(currentNode.walkable, currentNode.worldPosition, currentNode.gridPosX,
				                         currentNode.gridPosY);
				stayNode.neighbours = currentNode.neighbours;
				stayNode.time = path.Count + 1;
				State state = new State(currentNode.gridPosX, currentNode.gridPosY, path.Count + 1);
				
				if(!rTable.Occupied (state)) {
					float newCost = costSoFar[currentNode];
					Node node = grid.grid[stayNode.gridPosX, stayNode.gridPosY];
					float td = TrueDistance(targetNode, node);
					frontier.Enqueue (stayNode, newCost + td);
					cameFrom[stayNode] = currentNode;
					costSoFar[stayNode] = newCost;
					total[stayNode] = newCost + td;
				}
			}
			if(path.Count == d) {
				int j = 0;
				foreach(Node node in path) {
					Node n = grid.grid[node.gridPosX, node.gridPosY];
//					print ("[" + node.gridPosX + "," + node.gridPosY + "] " + (j++) + ": ");
				}
//				print ("\n");
				
				return new PathInfo(path, true, "test");
			}
			
			// Move to best neighbour
			Node nextNode = null;
			foreach(Node n in currentNode.neighbours){
				Node node;
				if(!costSoFar.ContainsKey (n)){
					node = new Node(n.walkable, n.worldPosition, n.gridPosX, n.gridPosY);
					node.neighbours = n.neighbours;
				}
				else
					node = n;

				State state = new State(node.gridPosX, node.gridPosY, path.Count + 1);
				node.time = path.Count + 1;

				if(!rTable.Occupied (state)) {
					float newCost = costSoFar[currentNode] + GetCost(currentNode, node);
	
					// Find neighbour move
                    if (!costSoFar.ContainsKey (node) || newCost < costSoFar[node]) {
						if(node.walkable) {
							costSoFar[node] = newCost;
							float td = TrueDistance (targetNode, grid.grid[node.gridPosX, node.gridPosY]);
							float priority = newCost + td;
							frontier.Enqueue (node, priority);
							cameFrom[node] = currentNode;
							nextNode = node;
							total[node] = newCost + td;
						}
					}
				}
			}


		}
        
        print ("COUNT: " + count);
        print ("start: " + startNode.gridPosX + ", " + startNode.gridPosY);
		print ("end: " + targetNode.gridPosX + ", " + targetNode.gridPosY);
		throw new Exception ("MAIN A* exception");
	}
	
	
	/*
	public PathInfo STAStar(Node startNode, Node targetNode, int run = 0) {
		PathInfo ret;
		List<Node> path = new List<Node>();


		if (startNode == targetNode) {
			bool free = true;
			State state = new State(startNode.gridPosX, startNode.gridPosY, 1);

			for(int i = 1; i <= d; i++) {
				state.t = i;
				if(rTable.Occupied (state)) {
					free = false;
					break;
				}
			}

			if(free) {	
				path = new List<Node> ();
				while(path.Count != d)
					path.Add (startNode);
				ret = new PathInfo(path, true, "Stay on place. EndPos: " + path[path.Count-1].gridPosX + ", " +  + path[path.Count-1].gridPosY);
				return ret;
			}

		}
		
		PriorityQueue<Node, float> frontier = new PriorityQueue<Node, float> ();
		Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node> ();
		Dictionary<Node, float> costSoFar = new Dictionary<Node, float> ();
		
		frontier.Enqueue(startNode, 0f);
		cameFrom [startNode] = null;
		costSoFar [startNode] = 0;

		Dictionary<Node, float> G = new Dictionary<Node, float> ();
		
		Node currentNode;
		while (frontier.Count() != 0) {
			currentNode = frontier.Dequeue ();

			G[currentNode] = TrueDistance (targetNode, currentNode);

            path = ConstructPath(startNode, currentNode, cameFrom);
            if(path.Count == d){
				foreach(Node n in path) {
					n.time = 0;
				}
				ret = new PathInfo(path, false, "Depth reached. endPos: "  + path[path.Count-1].gridPosX + ", " +  + path[path.Count-1].gridPosY);
				return ret;
			}
            
            foreach(Node node in currentNode.neighbours){
				node.time = path.Count;
				print (node.time);

                float newCost = costSoFar[currentNode] + GetCost (currentNode, node);

				State state = new State(node.gridPosX, node.gridPosY, path.Count + 1);

/*
				if(currentNode == startNode) {
					State initState = state;
					State thisState = new State(currentNode.gridPosX, currentNode.gridPosY, 1);
					initState.t = -1;
			//		if(rTable.Occupied(initState) && rTable.Occupied (thisState))
			//			continue;
				}


				if (!costSoFar.ContainsKey (node) || newCost < costSoFar[node]) {
					if(node.walkable) {
						if(!rTable.Occupied (state)) {
							if(startNode != targetNode) {
								costSoFar[node] = newCost;
								float td = TrueDistance (targetNode, node);
                                //float td = GridHeuristic(targetNode, node);
								float priority = newCost + td;
								
								G[node] = td;
								
								if(G[currentNode] > G[node]){
									frontier.Enqueue (node, priority);
									cameFrom[node] = currentNode;
								}
							}
							else if(startNode == targetNode) {
								costSoFar[node] = newCost;
								float priority = newCost + GridHeuristic(targetNode, node);
								frontier.Enqueue (node, priority);
								cameFrom[node] = currentNode;
							}
						}
					}
				}
				
				else if(frontier.Count() == 0) {
				}
			}
		}

		throw new Exception ("no path found2");

		if (path.Count != 0) {
			Node node = path [path.Count - 1];
			State state = new State(node.gridPosX, node.gridPosY, path.Count);
			while (path.Count != d) {
				state.t = path.Count;
				if(!rTable.Occupied (state))
					path.Add (node);
				else
					break;
			}

			if(path.Count == d)
				return new PathInfo (path, false, "No path could be found, CAN move, endPos: "  + path[path.Count-1].gridPosX + ", " +  + path[path.Count-1].gridPosY);
		} 
		else {
			State state = new State(startNode.gridPosX, startNode.gridPosY, path.Count);
			while(path.Count != d) {
				state.t = path.Count;
				if(!rTable.Occupied (state))
					path.Add (startNode);
				else
					break;
			}

			if(path.Count == d)
				return new PathInfo (path, false, "No path could be found, can't move, endPos: " + path[path.Count-1].gridPosX + ", " +  + path[path.Count-1].gridPosY);
		}

		if (run == 2) {
			while(path.Count != d) {
				path.Add (startNode);
			}
			
			return new PathInfo (path, false, "No path could be found, can't move, endPos: " + path[path.Count-1].gridPosX + ", " +  + path[path.Count-1].gridPosY);
		}

		return STAStar (startNode, startNode, 2);
	}

*/
	
	public List<Node> AStarSearch(Node startNode, Node targetNode) {
		if (startNode == targetNode) {
			return new List<Node>();
		}
		
		PriorityQueue<Node, float> frontier = new PriorityQueue<Node, float> ();
		Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node> ();
		Dictionary<Node, float> costSoFar = new Dictionary<Node, float> ();
		
		frontier.Enqueue(startNode, 0f);
		cameFrom [startNode] = null;
		costSoFar [startNode] = 0;
		
		Node currentNode;
		while (frontier.Count() != 0) {
			currentNode = frontier.Dequeue ();
			
			if(currentNode == targetNode) {
				return ConstructPath (startNode, targetNode, cameFrom);
			}
			
			foreach(Node node in currentNode.neighbours){
				float newCost = costSoFar[currentNode] + GetCost (currentNode, node);
				if (!costSoFar.ContainsKey (node) || newCost < costSoFar[node]) {
					if(node.walkable) {
						costSoFar[node] = newCost;
						//float priority = newCost + Heuristic (targetNode.worldPosition, node.worldPosition);
						float priority = newCost + GridHeuristic (targetNode, node);
						frontier.Enqueue (node, priority);
						cameFrom[node] = currentNode;
					}
				}
			}
		}
		throw new Exception ("MINI A* exception");
		return new List<Node> ();
	}
	
	float Heuristic(Vector3 A, Vector3 B) {
		return Mathf.Abs (A.x - B.x) + Mathf.Abs (A.y - B.y);
	}
	
	float GridHeuristic(Node A, Node B) {
		return Mathf.Abs (A.gridPosX - B.gridPosX) + Mathf.Abs (A.gridPosY - B.gridPosY);
	}
	
	public float TrueDistance(Node A, Node B) {
		if (A.gridPosX == B.gridPosX &&
		    A.gridPosY == B.gridPosY) {
			return 0;
		}
		List<Node> path = AStarSearch (A, B);
		float cost = 0;
		Node pastNode = A;
		foreach (Node node in path) {
			cost = cost + GetCost (pastNode, node);
			pastNode = node;
		}
		
		return path.Count * 10;
		
		return cost;
	}
	
	List<Node> ConstructPath(Node start, Node target, Dictionary<Node, Node> cameFrom) {
		Node currentNode = target;
		List<Node> path = new List<Node>();
		while (currentNode != start) {
			path.Add (currentNode);
			currentNode = cameFrom[currentNode];
		}
		path.Reverse ();
		
		return path;
	}
	
	public float GetCost(Node from, Node to) {
		float straightCost = 10f;
		float noCost = 0f;
		
		if (from.gridPosX == to.gridPosX && from.gridPosY + 1 == to.gridPosY) // up
			return straightCost;
		if (from.gridPosX + 1 == to.gridPosX && from.gridPosY == to.gridPosY) // right
			return straightCost;
		if (from.gridPosX == to.gridPosX && from.gridPosY - 1 == to.gridPosY) // down
			return straightCost;
		if (from.gridPosX - 1 == to.gridPosX && from.gridPosY == to.gridPosY) // left
			return straightCost;
		
		if (from.gridPosX == to.gridPosX && 
		    from.gridPosY == to.gridPosY)
			return 0f;
		
		throw new Exception ("GetCost exception");
	}
}
