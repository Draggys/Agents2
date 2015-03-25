using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapData {
	public bool[,] walkable;
	public List<Vector2> start;
	public List<Vector2> end;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public List<Vector2> customers;
}