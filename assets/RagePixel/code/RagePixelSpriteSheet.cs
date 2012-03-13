using UnityEngine;
using System;
using System.Collections;

public class RagePixelSpriteSheet : ScriptableObject
{

	public Material atlas;
	[SerializeField]
	private RagePixelRow[] _rows;

	public RagePixelRow[] rows
	{
		get
		{
			if(_rows == null)
			{
				_rows = new RagePixelRow[0];
			}
			return _rows;
		}
		set
		{
			_rows = value;
		}
	}

	public int thumbnailSize = 40;

	public RagePixelRow AddRow(int key, int pixelSizeX, int pixelSizeY)
	{
		RagePixelRow newRow = new RagePixelRow();

		newRow.key = key;
		newRow.pixelSizeX = pixelSizeX;
		newRow.pixelSizeY = pixelSizeY;
		newRow.newPixelSizeX = pixelSizeX;
		newRow.newPixelSizeY = pixelSizeY;

		Array.Resize(ref _rows, rows.Length + 1);
		rows[rows.Length - 1] = newRow;

		return newRow;
	}

	public RagePixelRow AddRow(int key, int index, int pixelSizeX, int pixelSizeY)
	{
		RagePixelRow newRow = new RagePixelRow();

		newRow.key = key;
		newRow.pixelSizeX = pixelSizeX;
		newRow.pixelSizeY = pixelSizeY;
		newRow.newPixelSizeX = pixelSizeX;
		newRow.newPixelSizeY = pixelSizeY;

		RagePixelRow[] tmpArr = new RagePixelRow[rows.Length + 1];

		for(int i = 0; i < tmpArr.Length; i++)
		{
			if(i < index)
			{
				tmpArr[i] = rows[i];
			}
			else if(i == index)
			{
				tmpArr[i] = newRow;
			}
			else if(i > 0)
			{
				tmpArr[i] = rows[i - 1];
			}
		}

		rows = tmpArr;

		return newRow;
	}

	public void MoveRow(int fromIndex, int toIndex)
	{
		if(fromIndex >= 0 && fromIndex < rows.Length && toIndex >= 0 && toIndex <= rows.Length)
		{
			RagePixelRow[] tmpArr = new RagePixelRow[rows.Length];

			for(int i = 0; i < tmpArr.Length; i++)
			{
				if(i == toIndex)
				{
					tmpArr[toIndex] = rows[fromIndex];
				}
				else if(i < fromIndex && i < toIndex || i > fromIndex && i > toIndex)
				{
					tmpArr[i] = rows[i];
				}
				else if(i >= fromIndex && i < toIndex)
				{
					tmpArr[i] = rows[i + 1];
				}
				else if(i <= fromIndex && i > toIndex)
				{
					tmpArr[i] = rows[i - 1];
				}
			}
			rows = tmpArr;
		}
	}

	public RagePixelRow GetRow(int key)
	{
		foreach(RagePixelRow row in rows)
		{
			if(row.key == key)
			{
				return row;
			}
		}
		return rows[0];
	}

	public RagePixelRow GetRowByName(string name)
	{
		if(GetIndexByName(name) == -1)
		{
			return null;
		}
		return rows[GetIndexByName(name)];
	}

	public int GetKey(int index)
	{
		if(rows.Length > index)
		{
			return rows[index].key;
		}

		Debug.Log("Error: Invalid array size");

		return rows[0].key;
	}

	public int GetIndex(int key)
	{
		if(rows.Length > 0)
		{
			for(int i = 0; i < rows.Length; i++)
			{
				if(rows[i].key == key)
				{
					return i;
				}
			}
		}

		return 0;
	}

	public int GetIndexByName(string name)
	{
		if(rows.Length > 0)
		{
			for(int i = 0; i < rows.Length; i++)
			{
				if(rows[i] != null && rows[i].name.Equals(name))
				{
					return i;
				}
			}
		}

		return -1;
	}

	public void RemoveRowByKey(int key)
	{
		int toBeRemovedIndex = -1;

		if(rows.Length > 1)
		{
			for(int i = 0; i < rows.Length; i++)
			{
				if(rows[i].key.Equals(key))
				{
					toBeRemovedIndex = i;
				}
			}

			if(toBeRemovedIndex >= 0)
			{
				RemoveRowByIndex(toBeRemovedIndex);
			}
		}
		else
		{
			Debug.Log("Error: Can't remove the only row.");
		}
	}

	public void RemoveRowByName(string name)
	{
		int idx = GetIndexByName(name);
		if(idx == -1)
		{
			return;
		}
		RemoveRowByIndex(idx);
	}

	public void RemoveRowByIndex(int index)
	{
		if(rows.Length > 0)
		{
			RagePixelRow[] tmpArr = new RagePixelRow[rows.Length - 1];

			for(int i = 0; i < rows.Length; i++)
			{
				if(i < index)
				{
					tmpArr[i] = rows[i];
				}
				else if(i > index)
				{
					tmpArr[i - 1] = rows[i];
				}
			}

			rows = tmpArr;
		}
	}

	public int GetTotalCellCount()
	{
		int count = 0;
		for(int i = 0; i < rows.Length; i++)
		{
			count += rows[i].cells.Length;
		}
		return count;
	}

	public void saveUndo(Color32[] buffer, RagePixelCell cell)
	{
		Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;

		int width = src.width;
		int height = src.height;
		int cellXMin = Mathf.FloorToInt(cell.uv.xMin * width);
		int cellYMin = Mathf.FloorToInt(cell.uv.yMin * height);
		int cellXMax = Mathf.FloorToInt(cell.uv.xMax * width);
		int cellYMax = Mathf.FloorToInt(cell.uv.yMax * height);

		Color32[] colors = new Color32[(cellYMax - cellYMin) * (cellXMax - cellXMin)];
		int i = 0;

		for(int y = cellYMin; y < cellYMax; y++)
		{
			for(int x = cellXMin; x < cellXMax; x++)
			{
				colors[i++] = buffer[y * width + x];
			}
		}
		cell.GetUndoHistory().Add(colors);
	}

	public void saveUndo(RagePixelCell cell)
	{
		Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;

		int width = src.width;
		int height = src.height;
		int cellXMin = Mathf.FloorToInt(cell.uv.xMin * width);
		int cellYMin = Mathf.FloorToInt(cell.uv.yMin * height);
		int cellXMax = Mathf.FloorToInt(cell.uv.xMax * width);
		int cellYMax = Mathf.FloorToInt(cell.uv.yMax * height);

		Color32[] colors = new Color32[(cellYMax - cellYMin) * (cellXMax - cellXMin)];
		int i = 0;

		for(int y = cellYMin; y < cellYMax; y++)
		{
			for(int x = cellXMin; x < cellXMax; x++)
			{
				colors[i++] = src.GetPixel(x, y);
			}
		}
		cell.GetUndoHistory().Add(colors);
	}

	public void DoUndo(RagePixelCell cell)
	{
		if(cell.GetUndoHistory().Count > 0)
		{
			Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;

			int width = src.width;
			int height = src.height;
			int cellXMin = Mathf.FloorToInt(cell.uv.xMin * width);
			int cellYMin = Mathf.FloorToInt(cell.uv.yMin * height);
			int cellXMax = Mathf.FloorToInt(cell.uv.xMax * width);
			int cellYMax = Mathf.FloorToInt(cell.uv.yMax * height);

			Color32[] colors = (Color32[])cell.GetUndoHistory()[cell.undoHistory.Count - 1];
			int i = 0;

			for(int y = cellYMin; y < cellYMax; y++)
			{
				for(int x = cellXMin; x < cellXMax; x++)
				{
					src.SetPixel(x, y, colors[i++]);
				}
			}
			src.Apply();
			cell.GetUndoHistory().RemoveAt(cell.undoHistory.Count - 1);
		}
	}

	public Color replaceColor(Color32[] before, Color oldColor, Color newColor)
	{
		Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;
		src.SetPixels32(before);
		int height = src.height;
		int width = src.width;
		Color newActualColor = newColor;
		bool actualNewColorPicked = false;

		int sampleX = -1;
		int sampleY = -1;

		for(int y = 0; y < height; y++)
		{
			for(int x = 0; x < width; x++)
			{
				if(src.GetPixel(x, y) == oldColor)
				{
					src.SetPixel(x, y, newColor);
					if(!actualNewColorPicked)
					{
						actualNewColorPicked = true;
						sampleX = x;
						sampleY = y;
						//newActualColor = src.GetPixel(x, y);
						//newActualColor = newColor;
					}
				}
			}
		}

		src.Apply();
		if(actualNewColorPicked)
		{
			newActualColor = src.GetPixel(sampleX, sampleY);
		}
		return newActualColor;
	}

	public Color replaceColor(Color32[] before, Color oldColor, Color newColor, RagePixelRow targetRow)
	{
		Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;
		src.SetPixels32(before);

		int width = src.width;
		int height = src.height;

		Color newActualColor = newColor;
		bool actualNewColorPicked = false;

		int sampleX = -1;
		int sampleY = -1;

		foreach(RagePixelCell cell in targetRow.cells)
		{

			int cellXMin = Mathf.FloorToInt(cell.uv.xMin * width);
			int cellYMin = Mathf.FloorToInt(cell.uv.yMin * height);
			int cellXMax = Mathf.FloorToInt(cell.uv.xMax * width);
			int cellYMax = Mathf.FloorToInt(cell.uv.yMax * height);

			for(int y = cellYMin; y < cellYMax; y++)
			{
				for(int x = cellXMin; x < cellXMax; x++)
				{
					if(src.GetPixel(x, y) == oldColor)
					{
						src.SetPixel(x, y, newColor);
						if(!actualNewColorPicked)
						{
							actualNewColorPicked = true;
							sampleX = x;
							sampleY = y;
							//newActualColor = src.GetPixel(x, y);
							//newActualColor = newColor;
						}
					}
				}
			}
		}

		src.Apply();
		if(actualNewColorPicked)
		{
			newActualColor = src.GetPixel(sampleX, sampleY);
		}
		return newActualColor;
	}

	public Color[] getPreviewImage(int rowIndex, int cellIndex, int width, int height, bool selected, Color selectedBorder)
	{
		Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;
		Color[] pixels = new Color[width * height];
		RagePixelRow row = rows[Mathf.Clamp(rowIndex, 0, rows.Length - 1)];

		if(row.cells.Length > cellIndex)
		{
			if(row.cells[cellIndex] != null && src != null)
			{
				float ratioX = row.pixelSizeX >= row.pixelSizeY ? (float)row.pixelSizeX / (float)width : (float)row.pixelSizeY / (float)width;
				float ratioY = row.pixelSizeX >= row.pixelSizeY ? (float)row.pixelSizeX / (float)height : (float)row.pixelSizeY / (float)height;

				for(int y = 0; y < height; y++)
				{
					for(int x = 0; x < width; x++)
					{

						int srcX = Mathf.Clamp(Mathf.FloorToInt((float)src.width * row.cells[cellIndex].uv.x) + Mathf.FloorToInt((float)x * ratioX), 0, src.width - 1);
						int srcY = Mathf.Clamp(Mathf.FloorToInt((float)src.height * row.cells[cellIndex].uv.y) + Mathf.FloorToInt((float)y * ratioY), 0, src.height - 1);

						if(srcX <= Mathf.FloorToInt((float)src.width * (row.cells[cellIndex].uv.x + row.cells[cellIndex].uv.width)) &&
                            srcY <= Mathf.FloorToInt((float)src.height * (row.cells[cellIndex].uv.y + row.cells[cellIndex].uv.height)))
						{
							pixels[x + y * width] = selected ? src.GetPixel(srcX, srcY) : src.GetPixel(srcX, srcY) * 0.5f;
						}
						else
						{
							pixels[x + y * width] = new Color(0f, 0f, 0f, 0f);
						}
					}
				}

				if(selected)
				{
					for(int y = 0; y < height; y++)
					{
						for(int x = 0; x < width; x++)
						{
							if(x == 0 || x == width - 1 || y == 0 || y == height - 1)
							{
								pixels[x + y * width] = selectedBorder;
							}
						}
					}
				}
			}
		}
		return pixels;
	}

	public Color[] getImage(int rowIndex, int cellIndex)
	{
		Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;
		RagePixelRow row = rows[Mathf.Clamp(rowIndex, 0, rows.Length - 1)];

		if(row.cells.Length > cellIndex)
		{
			if(row.cells[cellIndex] != null && src != null)
			{
				Color[] pixels = new Color[row.pixelSizeX * row.pixelSizeY];

				for(int y = 0; y < row.pixelSizeY; y++)
				{
					for(int x = 0; x < row.pixelSizeX; x++)
					{
						int srcX = Mathf.Clamp(Mathf.FloorToInt((float)src.width * row.cells[cellIndex].uv.x) + Mathf.FloorToInt((float)x), 0, src.width - 1);
						int srcY = Mathf.Clamp(Mathf.FloorToInt((float)src.height * row.cells[cellIndex].uv.y) + Mathf.FloorToInt((float)y), 0, src.height - 1);
						pixels[x + y * row.pixelSizeX] = src.GetPixel(srcX, srcY);
					}
				}
				return pixels;
			}
		}
		return new Color[0];
	}

	public Color[] getImage(int rowIndex, int cellIndex, int left, int top, int width, int height)
	{
		Texture2D src = atlas.GetTexture("_MainTex") as Texture2D;
		RagePixelRow row = rows[Mathf.Clamp(rowIndex, 0, rows.Length - 1)];

		if(row.cells.Length > cellIndex)
		{
			if(row.cells[cellIndex] != null && src != null)
			{
				Color[] pixels = new Color[width * height];

				if(top + height <= row.pixelSizeY && left + width <= row.pixelSizeX)
				{
					for(int y = top; y < top + height; y++)
					{
						for(int x = left; x < left + width; x++)
						{
							int srcX = Mathf.Clamp(Mathf.FloorToInt((float)src.width * row.cells[cellIndex].uv.x) + Mathf.FloorToInt((float)x), 0, src.width - 1);
							int srcY = Mathf.Clamp(Mathf.FloorToInt((float)src.height * row.cells[cellIndex].uv.y) + Mathf.FloorToInt((float)y), 0, src.height - 1);
							pixels[(x - left) + (y - top) * width] = src.GetPixel(srcX, srcY);
						}
					}
					return pixels;
				}
				else
				{
					//Debug.Log("top:" + top + ", height:" + height + ", left:" + left + ", width:" + width + ", pixelsizeX:" + row.pixelSizeX + ", pixelsizeY:" + row.pixelSizeY);
				}
			}
		}
		return new Color[0];
	}

}
