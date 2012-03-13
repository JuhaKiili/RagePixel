using UnityEngine;
using UnityEditor;
using System.IO;

public class RagePixelPaletteGUI
{
	public bool visible = false;
	public Color backgroundColor =
                (PlayerSettings.advancedLicense) ?
                new Color(0f, 0f, 0f, 0f) :
                new Color(0f, 0f, 0f, 0f);
	public bool animStripIsDirty = false;
	public bool isDirty = false;
	public int dragTargetIndex = -1;
	public int positionX;
	public int positionY;

	public Rect bounds
	{
		get
		{
			return new Rect(positionX, positionY, pixelWidth, pixelHeight);
		}
	}

	private int _maxWidth = 128;

	public int maxWidth
	{
		get
		{
			return _maxWidth;
		}
		set
		{
			if(Mathf.FloorToInt((float)value / (float)thumbnailSize) != tableWidth)
			{
				isDirty = true;
			}
			_maxWidth = value;
		}
	}

	private Texture2D _paletteTexture;

	public Texture2D paletteTexture
	{
		get
		{
			if(_paletteTexture == null || isDirty)
			{
				CreateTextureInstance();
				Refresh();
				animStripIsDirty = true;
				isDirty = false;
			}
			return _paletteTexture;
		}
	}

	private int _currentIndex = -1;

	public int currentIndex
	{
		get
		{
			return _currentIndex;
		}
		set
		{
			if(_currentIndex != value)
			{
				isDirty = true;
				_currentIndex = value;
			}
		}
	}

	private int thumbnailSize
	{
		get
		{
			return 16;
		}
	}

	public int tableWidth
	{
		get
		{
			return Mathf.FloorToInt((float)maxWidth / (float)thumbnailSize);
		}
	}

	public int tableHeight
	{
		get
		{
			return Mathf.Max(Mathf.CeilToInt((float)RagePixelUtil.Settings.palette.Length / (float)tableWidth), 1);
		}
	}

	private int _pixelWidth;

	public int pixelWidth
	{
		get
		{
			return _pixelWidth;
		}
		set
		{
			_pixelWidth = value;
		}
	}

	private int _pixelHeight;

	public int pixelHeight
	{
		get
		{
			return _pixelHeight;
		}
		set
		{
			_pixelHeight = value;
		}
	}

	public Color[] palette
	{
		get
		{
			return RagePixelUtil.Settings.palette;
		}
		set
		{
			RagePixelUtil.Settings.palette = value;
		}
	}

	public bool sizeIsDirty(int width)
	{
		return
            Mathf.FloorToInt((float)width / (float)thumbnailSize) != tableWidth ||
            Mathf.Max(Mathf.CeilToInt((float)RagePixelUtil.Settings.palette.Length / Mathf.FloorToInt((float)width / (float)thumbnailSize)), 1) != tableHeight;
	}

	public void CreateTextureInstance()
	{
		if(_paletteTexture == null)
		{
			_paletteTexture =
                    new Texture2D(
                        tableWidth * thumbnailSize + 2,
                        tableHeight * thumbnailSize + 2
                    );
			pixelWidth = tableWidth * thumbnailSize + 2;
			pixelHeight = tableHeight * thumbnailSize + 2;

			_paletteTexture.hideFlags = HideFlags.HideAndDontSave;
			_paletteTexture.filterMode = FilterMode.Point;

		}
		else
		{
			if(_paletteTexture.width != tableWidth * thumbnailSize + 2 || _paletteTexture.height != tableHeight * thumbnailSize + 2)
			{
				Object.DestroyImmediate(_paletteTexture, false);

				_paletteTexture =
                        new Texture2D(
                            tableWidth * thumbnailSize + 2,
                            tableHeight * thumbnailSize + 2
                        );

				pixelWidth = tableWidth * thumbnailSize + 2;
				pixelHeight = tableHeight * thumbnailSize + 2;

				_paletteTexture.hideFlags = HideFlags.HideAndDontSave;
				_paletteTexture.filterMode = FilterMode.Point;
			}
		}
	}

	public void Refresh()
	{

		Color[] backgroundPixels = new Color[_paletteTexture.width * _paletteTexture.height];

		for(int i = 0; i < backgroundPixels.Length; i++)
			backgroundPixels[i] = backgroundColor;

		_paletteTexture.SetPixels(backgroundPixels);
        
		int y = _paletteTexture.height - thumbnailSize - 1;
		int x = 1;
                
		for(int colorIndex = 0; colorIndex < palette.Length; colorIndex++)
		{
			int showIndex = colorIndex;
			Color borderColor = Color.black;

			if(dragTargetIndex >= 0)
			{
				if(dragTargetIndex == colorIndex)
				{
					showIndex = currentIndex;
					borderColor = new Color(0.8f, 0.8f, 0f, 1f);
				}
				else if(colorIndex < currentIndex && colorIndex < dragTargetIndex || colorIndex > currentIndex && colorIndex > dragTargetIndex)
				{
					//noop
				}
				else if(colorIndex >= currentIndex && colorIndex < dragTargetIndex)
				{
					showIndex = colorIndex + 1;
				}
				else if(colorIndex <= currentIndex && colorIndex > dragTargetIndex)
				{
					showIndex = colorIndex - 1;
				}
			}
			else
			{
				if(currentIndex == showIndex)
				{
					borderColor = Color.white;
				}
			}

			showIndex = Mathf.Clamp(showIndex, 0, palette.Length - 1);

            

			Color[] thumbnail =
                getPreviewImage(
                    palette[showIndex],
                    thumbnailSize - 2,
                    thumbnailSize - 2,
                    borderColor,
                    currentIndex == showIndex
                    );

			_paletteTexture.SetPixels(x + 1, y + 1, thumbnailSize - 2, thumbnailSize - 2, thumbnail);

			x += thumbnailSize;
			if(x + thumbnailSize > _paletteTexture.width - 1)
			{
				x = 1;
				y -= thumbnailSize;
			}
		}

		Color emptySpriteBackgroundColor =
            (PlayerSettings.advancedLicense) ?
            new Color(0f, 0f, 0f, 0f) :
            new Color(0f, 0f, 0f, 0f);

		Color[] emptySprite = new Color[(thumbnailSize - 2) * (thumbnailSize - 2)];

		for(int i = 0; i < emptySprite.Length; i++)
			emptySprite[i] = emptySpriteBackgroundColor;

		if(x > 1)
		{
			while(x < pixelWidth - thumbnailSize)
			{
				_paletteTexture.SetPixels(x + 1, y + 1, thumbnailSize - 2, thumbnailSize - 2, emptySprite);
				x += thumbnailSize;
			}
		}

		//RagePixelUtil.drawPixelBorder(_paletteTexture, new Color(0f, 0f, 0f, 0.25f));
		_paletteTexture.Apply();
        
	}

	public Color[] getPreviewImage(Color color, int sizeX, int sizeY, Color borderColor, bool selected)
	{
		Color[] colorTile = new Color[sizeX * sizeY];

		for(int i = 0; i < colorTile.Length; i++)
		{
			colorTile[i] = color;
		}

		for(int pY = 0; pY < sizeY; pY++)
		{
			for(int pX = 0; pX < sizeX; pX++)
			{
				if(pX == 0 || pX == sizeX - 1 || pY == 0 || pY == sizeY - 1)
				{
					colorTile[pX + pY * sizeX] = borderColor;
				}
			}
		}

		return colorTile;
	}

	private int GetRowIndex(int localX, int localY)
	{
		int x = localX / thumbnailSize;
		int y = localY / thumbnailSize;

		int index = y * tableWidth + x;

		return index;
	}

	public bool HandleGUIEvent(Event ev)
	{
		int localX = (int)ev.mousePosition.x - positionX;
		int localY = (int)ev.mousePosition.y - positionY;
        
		if(localX >= 0 && localX <= pixelWidth && localY >= 0 && localY <= pixelHeight)
		{
			int index = GetRowIndex(localX, localY);

			switch(ev.type)
			{
			case (EventType.mouseDown):
				if(!(ev.control || ev.command))
				{
					if(index >= 0 && index < palette.Length && localY >= 0 && localX >= 0)
					{
						currentIndex = index;
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					if(index >= 0 && index < palette.Length && localY >= 0 && localX >= 0)
					{
						RemoveColor(index);
						currentIndex = -1;
						isDirty = true;
						return true;
					}
					else
					{
						return false;
					}
				}

			case (EventType.mouseUp):
				if(dragTargetIndex >= 0 && dragTargetIndex != currentIndex)
				{
					dragTargetIndex = index;
					MoveColor(currentIndex, Mathf.Clamp(dragTargetIndex, 0, palette.Length - 1));
					currentIndex = Mathf.Clamp(dragTargetIndex, 0, palette.Length - 1);
					dragTargetIndex = -1;
					isDirty = true;
					return true;
				}
				else
				{
					dragTargetIndex = -1;
					isDirty = true;
					return true;
				}

			case (EventType.mouseDrag):
				int newdragTargetIndex = GetRowIndex(localX, localY);
				if(newdragTargetIndex >= 0 && newdragTargetIndex < palette.Length)
				{
					if(newdragTargetIndex != dragTargetIndex)
					{
						dragTargetIndex = newdragTargetIndex;
						isDirty = true;
					}
				}
				return false;
			}
		}
		return false;
	}

	public void MoveColor(int fromIndex, int toIndex)
	{
		if(fromIndex >= 0 && fromIndex < palette.Length && toIndex >= 0 && toIndex <= palette.Length)
		{
			Color[] tmpArr = new Color[palette.Length];

			for(int i = 0; i < tmpArr.Length; i++)
			{
				if(i == toIndex)
				{
					tmpArr[toIndex] = palette[fromIndex];
				}
				else if(i < fromIndex && i < toIndex || i > fromIndex && i > toIndex)
				{
					tmpArr[i] = palette[i];
				}
				else if(i >= fromIndex && i < toIndex)
				{
					tmpArr[i] = palette[i + 1];
				}
				else if(i <= fromIndex && i > toIndex)
				{
					tmpArr[i] = palette[i - 1];
				}
			}
			palette = tmpArr;
		}
	}

	public void RemoveColor(int fromIndex, int toIndex)
	{
		if(fromIndex >= 0 && fromIndex < palette.Length && toIndex >= 0 && toIndex <= palette.Length)
		{
			Color[] tmpArr = new Color[palette.Length];

			for(int i = 0; i < tmpArr.Length; i++)
			{
				if(i == toIndex)
				{
					tmpArr[toIndex] = palette[fromIndex];
				}
				else if(i < fromIndex && i < toIndex || i > fromIndex && i > toIndex)
				{
					tmpArr[i] = palette[i];
				}
				else if(i >= fromIndex && i < toIndex)
				{
					tmpArr[i] = palette[i + 1];
				}
				else if(i <= fromIndex && i > toIndex)
				{
					tmpArr[i] = palette[i - 1];
				}
			}
			palette = tmpArr;
		}
	}

	public void RemoveColor(int index)
	{
		if(palette.Length > 0)
		{
			Color[] tmpArr = new Color[palette.Length - 1];

			for(int i = 0; i < palette.Length; i++)
			{
				if(i < index)
				{
					tmpArr[i] = palette[i];
				}
				else if(i > index)
				{
					tmpArr[i - 1] = palette[i];
				}
			}

			palette = tmpArr;
		}
	}

	public void SelectColor(Color newColor)
	{
		currentIndex = -1;
		for(int colorIndex = 0; colorIndex < palette.Length; colorIndex++)
		{
			Color color = palette[colorIndex];

			if(
                Mathf.Approximately(color.r, newColor.r) &&
                Mathf.Approximately(color.g, newColor.g) &&
                Mathf.Approximately(color.b, newColor.b) &&
                Mathf.Approximately(color.a, newColor.a))
			{
				currentIndex = colorIndex;
			}
		}
	}

	public void AddColor(Color newColor)
	{
		bool alreadyInPalette = false;

		for(int colorIndex=0; colorIndex < palette.Length; colorIndex++)
		{
			Color color = palette[colorIndex];
			if(
                Mathf.Approximately(color.r, newColor.r) &&
                Mathf.Approximately(color.g, newColor.g) &&
                Mathf.Approximately(color.b, newColor.b) &&
                Mathf.Approximately(color.a, newColor.a))
			{
				currentIndex = colorIndex;
				alreadyInPalette = true;
			}
		}

		if(!alreadyInPalette)
		{
			Color[] tmpArr = new Color[palette.Length + 1];
			palette.CopyTo(tmpArr, 0);
			tmpArr[tmpArr.Length - 1] = newColor;
			isDirty = true;
			palette = tmpArr;
			currentIndex = palette.Length - 1;
		}

		EditorUtility.SetDirty(RagePixelUtil.Settings);
		AssetDatabase.SaveAssets();
	}

	public void CleanExit()
	{
		Object.DestroyImmediate(_paletteTexture, false);
	}
}
