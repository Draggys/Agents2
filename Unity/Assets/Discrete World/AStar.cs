using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AStar : MonoBehaviour{
	ReservationTable rTable;
	int d = 8000;

	public AStar(ReservationTable rTable) {
		this.rTable = rTable;
	}

	Node GetNode(Node currentNode, int dir) {
		List<Node> directions = currentNode.neighbours;
		return directions [dir];
	}

	public List<Node> STAStar(Node startNode, Node targetNode) {
		if (startNode == targetNode)
			return new List<Node>();
		
		PriorityQueue<Node, float> frontier = new PriorityQueue<Node, float> ();
		Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node> ();
		Dictionary<Node, float> costSoFar = new Dictionary<Node, float> ();
		
		frontier.Enqueue(startNode, 0f);
		cameFrom [startNode] = null;
		costSoFar [startNode] = 0;
		
		Node currentNode;
		while (frontier.Count() != 0) {
			currentNode = frontier.Dequeue ();

			List<Node> tmpPath = ConstructPath(startNode, currentNode, cameFrom);
			if(tmpPath.Count == d){
				print ("Depth cut off");
				return tmpPath;
			}

			if(currentNode == targetNode) {
				print ("Target found");
				return ConstructPath(startNode, currentNode, cameFrom);
			}


			foreach(Node node in currentNode.neighbours){
				float newCost = costSoFar[currentNode] + GetCost (currentNode, node);

				State state = new State(node.gridPosX, node.gridPosY, tmpPath.Count + 1);

				if (!costSoFar.ContainsKey (node) || newCost < costSoFar[node]) {
					if(node.walkable) {
						if(!rTable.Occupied (state)) {
							costSoFar[node] = newCost;
							float priority = newCost + GridHeuristic (targetNode, node);
							frontier.Enqueue (node, priority);
							cameFrom[node] = currentNode;
						}
					}
				}
			}
		}
		
		return new List<Node> ();
	}

	public List<Node> AStarSearch(Node startNode, Node targetNode) {
		if (startNode == targetNode)
			return new List<Node>();
		
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

		return new List<Node> ();
	}

	float Heuristic(Vector3 A, Vector3 B) {
		return Mathf.Abs (A.x - B.x) + Mathf.Abs (A.y - B.y);
	}

	float GridHeuristic(Node A, Node B) {
		return Mathf.Abs (A.gridPosX - B.gridPosX) + Mathf.Abs (A.gridPosY - B.gridPosY);
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
		float diagonalCost = 14f;

		if (from.gridPosX == to.gridPosX && from.gridPosY + 1 == to.gridPosY) // up
			return straightCost;
		if (from.gridPosX + 1 == to.gridPosX && from.gridPosY == to.gridPosY) // right
			return straightCost;
		if (from.gridPosX == to.gridPosX && from.gridPosY - 1 == to.gridPosY) // down
			return straightCost;
		if (from.gridPosX - 1 == to.gridPosX && from.gridPosY == to.gridPosY) // left
			return straightCost;

		return diagonalCost;

	}
}
