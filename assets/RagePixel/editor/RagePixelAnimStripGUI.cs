using UnityEngine;
using UnityEditor;
using System.IO;

public class RagePixelAnimStripGUI
{
	public Color backgroundColor =
        (PlayerSettings.advancedLicense) ?
        new Color(0.2f, 0.2f, 0.2f, 1f) :
        new Color(0.4f, 0.4f, 0.4f, 1f);
	public RagePixelSpriteSheetGUI spriteSheetGUI;
	public bool visible = false;
	public bool isDirty = false;

	public RagePixelAnimStripGUI(RagePixelSpriteSheetGUI spriteSheetGUI)
	{
		this.spriteSheetGUI = spriteSheetGUI;
	}

	public Rect bounds
	{
		get
		{
			return new Rect(positionX, positionY, pixelWidth, pixelHeight);
		}
	}

	private Texture2D _animStripTexture;

	public Texture2D animStripTexture
	{
		get
		{
			if(_animStripTexture == null || isDirty || spriteSheetGUI.animStripIsDirty)
			{
				CreateTextureInstance();
				Refresh();
				spriteSheetGUI.animStripIsDirty = false;
				isDirty = false;
			}
			return _animStripTexture;
		}
	}

	public RagePixelSpriteSheet spriteSheet
	{
		get
		{
			return spriteSheetGUI.spriteSheet;
		}
	}

	public int currentRowKey
	{
		get
		{
			return spriteSheetGUI.currentRowKey;
		}
	}

	private int _currentCellKey;

	public int currentCellKey
	{
		get
		{
			return _currentCellKey;
		}
		set
		{
			if(_currentCellKey != value)
			{
				isDirty = true;
				_currentCellKey = value;
			}
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
			_maxWidth = value;
			if(sizeIsDirty(_maxWidth))
			{
				isDirty = true;
			}
		}
	}

	public int thumbnailSize
	{
		get
		{
			if(spriteSheet != null)
			{
				return spriteSheet.thumbnailSize;
			}
			else
			{
				return 40;
			}
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
			if(spriteSheet != null)
			{
				return Mathf.Max(Mathf.CeilToInt((float)spriteSheet.GetRow(currentRowKey).cells.Length / (float)tableWidth), 1);
			}
			else
			{
				return 1;
			}
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

	public int dragTargetIndex = -1;
	public int positionX;
	public int positionY;

	public bool sizeIsDirty(int newMaxWidth)
	{
		if(spriteSheet != null)
		{
			return 
                Mathf.FloorToInt((float)newMaxWidth / (float)spriteSheet.thumbnailSize) != tableWidth || 
                Mathf.Max(Mathf.CeilToInt((float)spriteSheet.GetRow(currentRowKey).cells.Length / Mathf.FloorToInt((float)newMaxWidth / (float)spriteSheet.thumbnailSize)), 1) != tableHeight;
		}
		else
		{
			return true;
		}
	}

	public void CreateTextureInstance()
	{
		if(_animStripTexture == null)
		{
			_animStripTexture =
                    new Texture2D(
                        tableWidth * thumbnailSize + 2,
                        tableHeight * thumbnailSize + 2
                    );

			pixelWidth = tableWidth * thumbnailSize + 2;
			pixelHeight = tableHeight * thumbnailSize + 2;

			_animStripTexture.hideFlags = HideFlags.HideAndDontSave;
			_animStripTexture.filterMode = FilterMode.Point;
		}
		else
		{
			if(_animStripTexture.width != tableWidth * thumbnailSize + 2 || _animStripTexture.height != tableHeight * thumbnailSize + 2)
			{
				Object.DestroyImmediate(_animStripTexture, false);

				_animStripTexture =
                        new Texture2D(
                            tableWidth * thumbnailSize + 2,
                            tableHeight * thumbnailSize + 2
                        );

				pixelWidth = tableWidth * thumbnailSize + 2;
				pixelHeight = tableHeight * thumbnailSize + 2;

				_animStripTexture.hideFlags = HideFlags.HideAndDontSave;
				_animStripTexture.filterMode = FilterMode.Point;
			}
		}
	}

	public void Refresh()
	{
		Color[] backgroundPixels = new Color[_animStripTexture.width * _animStripTexture.height];

		for(int i = 0; i < backgroundPixels.Length; i++)
			backgroundPixels[i] = backgroundColor;

		_animStripTexture.SetPixels(backgroundPixels);

		int y = _animStripTexture.height - thumbnailSize - 1;
		int x = 1;

		int currentIndex = spriteSheet.GetRow(currentRowKey).GetIndex(currentCellKey);
		for(int cellIndex = 0; cellIndex < spriteSheet.GetRow(currentRowKey).cells.Length; cellIndex++)
		{
			int showIndex = cellIndex;
			Color borderColor = Color.white;

			if(dragTargetIndex >= 0)
			{
				if(dragTargetIndex == cellIndex && dragTargetIndex != currentIndex)
				{
					showIndex = currentIndex;
					borderColor = new Color(0.8f, 0.8f, 0f, 1f);
				}
				else if(cellIndex < currentIndex && cellIndex < dragTargetIndex || cellIndex > currentIndex && cellIndex > dragTargetIndex)
				{
					//noop
				}
				else if(cellIndex >= currentIndex && cellIndex < dragTargetIndex)
				{
					showIndex = cellIndex + 1;
				}
				else if(cellIndex <= currentIndex && cellIndex > dragTargetIndex)
				{
					showIndex = cellIndex - 1;
				}
			}

			Color[] thumbnail =
                spriteSheet.getPreviewImage(
                    spriteSheet.GetIndex(currentRowKey),
                    showIndex,
                    thumbnailSize - 2,
                    thumbnailSize - 2,
                    currentCellKey == spriteSheet.GetRow(currentRowKey).cells[showIndex].key,
                    borderColor);
            
			_animStripTexture.SetPixels(x + 1, y + 1, thumbnailSize - 2, thumbnailSize - 2, thumbnail);
            
			x += thumbnailSize;
			if(x + thumbnailSize > _animStripTexture.width - 1)
			{
				x = 1;
				y -= thumbnailSize;
			}
		}

		Color emptyFrameBackgroundColor =
            backgroundColor + new Color(0.02f, 0.02f, 0.02f, 1f);

		Color[] emptySprite = new Color[(thumbnailSize - 2) * (thumbnailSize - 2)];

		for(int i = 0; i < emptySprite.Length; i++)
		{
			emptySprite[i] = emptyFrameBackgroundColor;
		}

		if(x > 1)
		{
			while(x < pixelWidth - thumbnailSize)
			{
				_animStripTexture.SetPixels(x + 1, y + 1, thumbnailSize - 2, thumbnailSize - 2, emptySprite);
				x += thumbnailSize;
			}
		}

		//RagePixelUtil.drawPixelBorder(_animStripTexture, new Color(0.1f, 0.1f, 0.1f, 1f));
		RagePixelUtil.drawPixelBorder(_animStripTexture, new Color(0f, 0f, 0f, 1f));

		_animStripTexture.Apply();
	}

	public int GetRowIndex(int localX, int localY)
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
				if(index >= 0 && index < spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length && localY >= 0 && localX >= 0)
				{
					currentCellKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetKey(index);
					isDirty = true;
					return true;
				}
				else
				{
					return false;
				}

			case (EventType.mouseUp):
				if(dragTargetIndex >= 0 && dragTargetIndex != spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(currentCellKey))
				{
					dragTargetIndex = index;
					int fromIndex = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(currentCellKey);
					spriteSheet.GetRow(spriteSheetGUI.currentRowKey).MoveCell(fromIndex, Mathf.Clamp(dragTargetIndex, 0, spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length - 1));
					RagePixelUtil.RebuildAtlas(spriteSheet, false, "MoveCell");
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

				if(newdragTargetIndex >= 0 && newdragTargetIndex < spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length)
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

	public void CleanExit()
	{
		Object.DestroyImmediate(_animStripTexture, false);
	}

}