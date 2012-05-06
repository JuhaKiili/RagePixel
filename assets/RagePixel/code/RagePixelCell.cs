using UnityEngine;
using System.Collections;

[System.Serializable]
public class RagePixelCell
{
	[System.NonSerialized]
	public ArrayList undoHistory;
	public int key;
	public Rect uv;
	public int delay;
	
	public string importAssetPath = "";

	public void ClearUndoHistory()
	{
		if(undoHistory == null)
		{
			undoHistory = new ArrayList();
		}
		else
		{
			undoHistory.Clear();
		}
	}

	public ArrayList GetUndoHistory()
	{
		if(undoHistory == null)
		{
			undoHistory = new ArrayList();
		}
		return undoHistory;
	}
}
