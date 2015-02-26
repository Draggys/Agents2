using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Grid : MonoBehaviour {

	public Transform player;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public Node[,] grid;
	public MapLoader mapLoader=null;

	public float nodeDiameter;
	public int gridSizeX;
	public int gridSizeY;

	public MapData mapData;

	public int neighbourhood;
	public ReservationTable rTable;

	void Awake () {
		rTable = new ReservationTable ();

		MapLoader mapLoader = new MapLoader (new Vector2(20, 20), 0.5f);
		//mapData = mapLoader.LoadMap ("A", "startPos", "endPos");
		mapData = mapLoader.LoadMap ("testmap", "endPos", "startPos");

		nodeDiameter = mapData.nodeRadius * 2;
		gridWorldSize = mapData.gridWorldSize;
		// #nodes that can fit into the x-space
		gridSizeX = Mathf.RoundToInt (gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt (gridWorldSize.y / nodeDiameter);
		CreateGrid ();

		for(int i = 0; i < gridSizeX; i++) {
			for(int j = 0; j < gridSizeY; j++) {
				if(neighbourhood == 4)
					FillNeighbourhood4 (grid[i, j]);
			}
		}
		foreach (Node node in grid) {
		}
	}

	public Node getNode(int gridPosX, int gridPosY) {
		return grid [gridPosX, gridPosY];
	}

	public bool validIndex(int x, int y) {
		//TODO HÄR ÄR JAG
		if (x < 0 || x > 19)
			return false;
		if (y < 0 || y > 19)
			return false;
		return true;

		/*
		if (0 <= x && x <= gridSizeX) {
			if(0 <= y && y <= gridSizeY) {
				return true;
			}
		}
		return false;*/
	}

	void CreateGrid() {
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldTopLeft = transform.position - Vector3.right * gridWorldSize.x / 2
			+ Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++) {
			for(int y = 0; y < gridSizeY; y++) {
				Vector3 worldPoint = worldTopLeft + Vector3.right * (y * nodeDiameter + nodeRadius)
					- Vector3.forward * (x * nodeDiameter + nodeRadius);
				bool walkable = mapData.walkable[x ,y];
				grid[y, x] = new Node(walkable, worldPoint, y, x);
			}
		}
	}
	
	// Find node that player is currently standing on 
	// I.e convert a world position into a grid coordinate
	public Node NodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;

		percentX = Mathf.Clamp01 (percentX);
		percentY = Mathf.Clamp01 (percentY);

		int x = Mathf.RoundToInt ((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt ((gridSizeY - 1) * percentY);


		return grid[x, gridSizeY - 1 - y];
	}

	void OnDrawGizmos() {

		if (mapLoader == null) {

			mapLoader = new MapLoader (new Vector2(20, 20), 0.5f);
			//mapData = mapLoader.LoadMap ("A", "endPos", "startPos");
			mapData = mapLoader.LoadMap ("testmap", "endPos", "startPos");
			
			nodeDiameter = mapData.nodeRadius * 2;
			gridWorldSize = mapData.gridWorldSize;
			// #nodes that can fit into the x-space
			gridSizeX = Mathf.RoundToInt (gridWorldSize.x / nodeDiameter);
			gridSizeY = Mathf.RoundToInt (gridWorldSize.y / nodeDiameter);
			CreateGrid ();
		}

		// Draw gridWorldSize
		Gizmos.DrawWireCube (transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

		// Checks if createGrid workes


		if (grid != null) {
			Node playerNode = NodeFromWorldPoint(player.position);
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable) ? Color.white : Color.red;
				Gizmos.DrawCube (n.worldPosition, Vector3.one * (nodeDiameter - .1f));
			}
		}


	}

	public void FillNeighbourhood12(Node node) {
		FillNeighbourhood8 (node);

		int neighX = node.gridPosX - 2;
		int neighY = node.gridPosY;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                                                      new Vector3(-2, 0, 0)));
		neighX = node.gridPosX;
		neighY = node.gridPosY + 2;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                                                      new Vector3(0, 0, 2)));

		neighX = node.gridPosX + 2;
		neighY = node.gridPosY;
		if (validIndex (neighX, neighY))
			grid [node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint (node.worldPosition + 
			                                                                      new Vector3(2, 0, 0)));       

		neighX = node.gridPosX;
		neighY = node.gridPosY - 2;
		if (validIndex (neighX, neighY))
			grid [node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint (node.worldPosition + 
			                                                                      new Vector3(0, 0, -2)));   
	}

	public void FillNeighbourhood4(Node node) {
		int x = node.gridPosX;
		int y = node.gridPosY;

		if (validIndex (x + 1, y) && grid [x + 1, y].walkable) {
		//	print ("right " + (x + 1) + ", " + (y) + " | " + grid[x+1, y].gridPosX + ", " + grid[x+1, y].gridPosY);
			node.neighbours.Add (grid [x + 1, y]);
		}

		if (validIndex (x - 1, y) && grid [x - 1, y].walkable) {
		//	print ("left");
			node.neighbours.Add (grid [x - 1, y]);
		}

		if (validIndex (x, y + 1) && grid [x, y + 1].walkable) {
		//	print ("down");
			node.neighbours.Add (grid [x, y + 1]);
		}

		if (validIndex (x, y - 1) && grid [x, y - 1].walkable) {
		//	print ("up " + x + ", " + (y - 1) + " | " + grid[x, y - 1].gridPosX + ", " + grid[x, y - 1].gridPosY);
			node.neighbours.Add (grid [x, y - 1]);
		}

		/*
		int neighX = node.gridPosX;
		int neighY = node.gridPosY + 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                                                      Vector3.forward));

		neighX = node.gridPosX + 1;
		neighY = node.gridPosY;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                                                      Vector3.right));

		neighX = node.gridPosX;
		neighY = node.gridPosY - 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                                                      Vector3.back));

		neighX = node.gridPosX - 1;
		neighY = node.gridPosY;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                                                      Vector3.left));
		*/
	}

	public void FillNeighbourhood8(Node node) {
		int neighX = node.gridPosX - 1;
		int neighY = node.gridPosY - 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.left + Vector3.back));

		neighX = node.gridPosX;
		neighY = node.gridPosY + 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.forward));
		
		neighX = node.gridPosX + 1;
		neighY = node.gridPosY + 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.right + Vector3.forward));
		
		neighX = node.gridPosX - 1;
		neighY = node.gridPosY;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.left));
		
		neighX = node.gridPosX + 1;
		neighY = node.gridPosY;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.right));
		
		neighX = node.gridPosX - 1;
		neighY = node.gridPosY - 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.left + Vector3.forward));
		
		neighX = node.gridPosX;
		neighY = node.gridPosY - 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.back));
		
		neighX = node.gridPosX + 1;
		neighY = node.gridPosY - 1;
		if(validIndex (neighX, neighY))
			grid[node.gridPosX, node.gridPosY].neighbours.Add (NodeFromWorldPoint(node.worldPosition + 
			                                   Vector3.right + Vector3.back));
	}


}
