using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RagePixelRow
{
	public int pixelSizeX;
	public int pixelSizeY;
	public int newPixelSizeX;
	public int newPixelSizeY;
	public int key;
	public string name;
	[HideInInspector]
	public string fontCharacter = "";
	[HideInInspector]
	public int fontYOffset = 0;
	[SerializeField]
	private RagePixelAnimation[] _animations;

	public RagePixelAnimation[] animations
	{
		get
		{
			if(_animations == null)
			{
				_animations = new RagePixelAnimation[0];
			}
			return _animations;
		}
		set
		{
			_animations = value;
		}
	}

	[SerializeField]
	private RagePixelCell[] _cells;

	public RagePixelCell[] cells
	{
		get
		{
			if(_cells == null)
			{
				_cells = new RagePixelCell[0];
			}
			return _cells;
		}
		set
		{
			_cells = value;
		}
	}

	public RagePixelCell InsertCell(int index, int key)
	{
		RagePixelCell newCell = new RagePixelCell();
		newCell.delay = 200;
		newCell.uv = new Rect();
		newCell.key = key;
		RagePixelCell[] tmpArr = new RagePixelCell[cells.Length + 1];

		for(int i = 0; i < tmpArr.Length; i++)
		{
			if(i < index)
			{
				tmpArr[i] = cells[i];

				if(i == index - 1)
				{
					newCell.uv = cells[i].uv;
				}

			}
			else if(i > index)
			{
				tmpArr[i] = cells[i - 1];
			}
			else
			{
				tmpArr[i] = newCell;
			}
		}

		cells = tmpArr;
		return newCell;
	}

	public void RemoveCellByKey(int key)
	{
		int toBeRemovedIndex = -1;

		if(cells.Length > 1)
		{
			for(int i = 0; i < cells.Length; i++)
			{
				if(cells[i].key.Equals(key))
				{
					toBeRemovedIndex = i;
				}
			}

			if(toBeRemovedIndex >= 0)
			{
				RemoveCellByIndex(toBeRemovedIndex);
			}
		}
		else
		{
			Debug.Log("Error: Can't remove the last cell. Remove row instead");
		}
	}

	public void MoveCell(int fromIndex, int toIndex)
	{
		if(fromIndex >= 0 && fromIndex < cells.Length && toIndex >= 0 && toIndex <= cells.Length)
		{
			RagePixelCell[] tmpArr = new RagePixelCell[cells.Length];

			for(int i = 0; i < tmpArr.Length; i++)
			{
				if(i == toIndex)
				{
					tmpArr[toIndex] = cells[fromIndex];
				}
				else if(i < fromIndex && i < toIndex || i > fromIndex && i > toIndex)
				{
					tmpArr[i] = cells[i];
				}
				else if(i >= fromIndex && i < toIndex)
				{
					tmpArr[i] = cells[i + 1];
				}
				else if(i <= fromIndex && i > toIndex)
				{
					tmpArr[i] = cells[i - 1];
				}
			}
			cells = tmpArr;
		}
	}

	public int GetKey(int index)
	{
		if(cells.Length > index)
		{
			return cells[index].key;
		}

		Debug.Log("Error: Indvalid array size");

		return cells[0].key;
	}

	public int GetIndex(int key)
	{
		if(cells.Length > 0)
		{
			for(int i = 0; i < cells.Length; i++)
			{
				if(cells[i].key.Equals(key))
				{
					return i;
				}
			}
		}

		return 0;
	}

	public RagePixelCell GetCell(int key)
	{
		if(cells.Length > 0)
		{
			for(int i = 0; i < cells.Length; i++)
			{
				if(cells[i].key.Equals(key))
				{
					return cells[i];
				}
			}
		}

		return cells[0];
	}

	public void RemoveCellByIndex(int index)
	{
		if(cells.Length > 0)
		{
			RagePixelCell[] tmpArr = new RagePixelCell[cells.Length - 1];

			for(int i = 0; i < cells.Length; i++)
			{
				if(i < index)
				{
					tmpArr[i] = cells[i];
				}
				else if(i > index)
				{
					tmpArr[i - 1] = cells[i];
				}
			}

			cells = tmpArr;
		}
	}

	public RagePixelAnimation AddAnimation()
	{
		RagePixelAnimation newAnim = new RagePixelAnimation();
		newAnim.mode = RagePixelSprite.AnimationMode.PlayOnce;
		newAnim.startIndex = 0;
		newAnim.endIndex = cells.Length - 1;
		newAnim.name = "New animation";

		Array.Resize(ref _animations, animations.Length + 1);
		animations[animations.Length - 1] = newAnim;
		return newAnim;
	}

	public void RemoveAnimation(int index)
	{
		if(animations.Length > 0)
		{
			if(index != animations.Length-1) 
			{
				RagePixelAnimation[] dest = Array.CreateInstance(animations.GetType().GetElementType(), animations.Length - 1) as RagePixelAnimation[];
				Array.Copy(animations, 0, dest, 0, index);
				Array.Copy(animations, index + 1, dest, index, animations.Length - index - 1);
				animations = dest;
			}
			else
			{
				Array.Resize(ref _animations, animations.Length - 1);
			}
		}
	}

	public RagePixelAnimation GetAnimationByName(string name)
	{
		if(animations.Length > 0)
		{
			for(int i = 0; i < animations.Length; i++)
			{
				if(animations[i].name.Equals(name))
				{
					return animations[i];
				}
			}
		}

		return null;
	}

	public void CopyAnimationsFrom(RagePixelRow other, bool append=false)
	{
		if(other == this)
		{
			Debug.LogError("Can't copy " + name + " from itself!");
		}
		if(!append)
		{
			Array.Resize(ref _animations, 0);
		}
		//replace existing names
		//add rest
		int oldCount = animations.Length;
		var newAnims = new List<RagePixelAnimation>();
		RagePixelAnimation[] otherAnims = other.animations;
		for(int i = 0; i < otherAnims.Length; i++)
		{
			string otherName = otherAnims[i].name;
			bool found = false;
			for(int j = 0; j < oldCount; j++)
			{
				if(animations[j].name.Equals(otherName))
				{
					animations[j] = otherAnims[i].Clone();
					found = true;
					break;
				}
			}
			if(!found)
			{
				newAnims.Add(otherAnims[i].Clone());
			}
		}
		Array.Resize(ref _animations, animations.Length + newAnims.Count);
		for(int i = 0; i < newAnims.Count; i++)
		{
			animations[i + oldCount] = newAnims[i];
		}
	}

	public void Clear()
	{
		Array.Resize(ref _cells, 0);
	}

	public void ClearUndoHistory()
	{
		for(int i = 0; i < cells.Length; i++)
		{
			if(cells[i].undoHistory == null)
			{
				cells[i].undoHistory = new ArrayList();
			}
			else
			{
				cells[i].undoHistory.Clear();
			}
		}
	}
}
