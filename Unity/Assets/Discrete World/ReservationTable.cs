using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct State {
	public int x;
	public int y;
	public int t;

	public State(int x, int y, int t) {
		this.x = x;
		this.y = y;
		this.t = t;
	}
}

public class ReservationTable {
	public Dictionary<State, int> rTable;

	public ReservationTable() {
		rTable = new Dictionary<State, int> ();
	}

	public void Add(State key, int val) {
		rTable[key] = val;
	}

	public bool Occupied(State key) {
		if (!rTable.ContainsKey (key))
			return false;
		if (rTable [key] == 0)
			return false;
		return true;
	}

	public void Free(State key) {
		rTable [key] = 0;
	}
}
