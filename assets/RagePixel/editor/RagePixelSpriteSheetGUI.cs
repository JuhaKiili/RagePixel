using UnityEngine;
using UnityEditor;
using System.IO;

public class RagePixelSpriteSheetGUI
{
	public Color backgroundColor =
                (PlayerSettings.advancedLicense) ?
                new Color(0.2f, 0.2f, 0.2f, 1f) :
                new Color(0.4f, 0.4f, 0.4f, 1f);
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

	private RagePixelSpriteSheet _spriteSheet;

	public RagePixelSpriteSheet spriteSheet
	{
		get
		{
			return _spriteSheet;
		}
		set
		{
			if(value != _spriteSheet)
			{
				isDirty = true;
				_spriteSheet = value;
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
			if(Mathf.FloorToInt((float)value / (float)thumbnailSize) != tableWidth)
			{
				isDirty = true;
			}
			_maxWidth = value;
		}
	}

	private Texture2D _spriteSheetTexture;

	public Texture2D spriteSheetTexture
	{
		get
		{
			if(_spriteSheetTexture == null || isDirty)
			{
				CreateTextureInstance();
				Refresh();
				animStripIsDirty = true; 
				isDirty = false;
			}
			return _spriteSheetTexture;
		}
	}

	private int _currentRowKey;

	public int currentRowKey
	{
		get
		{
			return _currentRowKey;
		}
		set
		{
			if(_currentRowKey != value)
			{
				isDirty = true;
				_currentRowKey = value;
			}
		}
	}

	private int thumbnailSize
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
				return Mathf.Max(Mathf.CeilToInt((float)spriteSheet.rows.Length / (float)tableWidth), 1);
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

	public bool sizeIsDirty(int width)
	{
		if(spriteSheet != null)
		{
			return
                Mathf.FloorToInt((float)width / (float)spriteSheet.thumbnailSize) != tableWidth ||
                Mathf.Max(Mathf.CeilToInt((float)spriteSheet.GetRow(currentRowKey).cells.Length / Mathf.FloorToInt((float)width / (float)spriteSheet.thumbnailSize)), 1) != tableHeight;
		}
		else
		{
			return true;
		}
	}

	public void CreateTextureInstance()
	{
		if(_spriteSheetTexture == null)
		{
			_spriteSheetTexture =
                    new Texture2D(
                        tableWidth * thumbnailSize + 2,
                        tableHeight * thumbnailSize + 2
                    );
			pixelWidth = tableWidth * thumbnailSize + 2;
			pixelHeight = tableHeight * thumbnailSize + 2;

			_spriteSheetTexture.hideFlags = HideFlags.HideAndDontSave;
			_spriteSheetTexture.filterMode = FilterMode.Point;

		}
		else
		{
			if(_spriteSheetTexture.width != tableWidth * thumbnailSize + 2 || _spriteSheetTexture.height != tableHeight * thumbnailSize + 2)
			{
				Object.DestroyImmediate(_spriteSheetTexture, false);

				_spriteSheetTexture =
                        new Texture2D(
                            tableWidth * thumbnailSize + 2,
                            tableHeight * thumbnailSize + 2
                        );

				pixelWidth = tableWidth * thumbnailSize + 2;
				pixelHeight = tableHeight * thumbnailSize + 2;

				_spriteSheetTexture.hideFlags = HideFlags.HideAndDontSave;
				_spriteSheetTexture.filterMode = FilterMode.Point;
			}
		}
	}

	public void Refresh()
	{
		if(spriteSheet != null)
		{
            
			Color[] backgroundPixels = new Color[_spriteSheetTexture.width * _spriteSheetTexture.height];

			for(int i = 0; i < backgroundPixels.Length; i++)
				backgroundPixels[i] = backgroundColor;

			_spriteSheetTexture.SetPixels(backgroundPixels);

			int y = _spriteSheetTexture.height - thumbnailSize - 1;
			int x = 1;
			int currentIndex = spriteSheet.GetIndex(currentRowKey);

			for(int rowIndex = 0; rowIndex < spriteSheet.rows.Length; rowIndex++)
			{
				int showIndex = rowIndex;
				Color borderColor = Color.white;

				if(dragTargetIndex >= 0)
				{
					if(dragTargetIndex == rowIndex && dragTargetIndex != currentIndex)
					{
						showIndex = currentIndex;
						borderColor = new Color(0.8f, 0.8f, 0f, 1f);
					}
					else if(rowIndex < currentIndex && rowIndex < dragTargetIndex || rowIndex > currentIndex && rowIndex > dragTargetIndex)
					{
						//noop
					}
					else if(rowIndex >= currentIndex && rowIndex < dragTargetIndex)
					{
						showIndex = rowIndex + 1;
					}
					else if(rowIndex <= currentIndex && rowIndex > dragTargetIndex)
					{
						showIndex = rowIndex - 1;
					}
				}

				showIndex = Mathf.Clamp(showIndex, 0, spriteSheet.rows.Length - 1);

				Color[] thumbnail =
                    spriteSheet.getPreviewImage(
                        showIndex,
                        0,
                        thumbnailSize - 2,
                        thumbnailSize - 2,
                        spriteSheet.GetKey(showIndex) == currentRowKey,
                        borderColor
                        );

				_spriteSheetTexture.SetPixels(x + 1, y + 1, thumbnailSize - 2, thumbnailSize - 2, thumbnail);

				x += thumbnailSize;
				if(x + thumbnailSize > _spriteSheetTexture.width - 1)
				{
					x = 1;
					y -= thumbnailSize;
				}
			}

			Color emptySpriteBackgroundColor =
                (PlayerSettings.advancedLicense) ?
                new Color(0.22f, 0.22f, 0.22f, 1f) :
                new Color(0.53f, 0.53f, 0.53f, 1f);

			Color[] emptySprite = new Color[(thumbnailSize - 2) * (thumbnailSize - 2)];

			for(int i = 0; i < emptySprite.Length; i++)
				emptySprite[i] = emptySpriteBackgroundColor;

			if(x > 1)
			{
				while(x < pixelWidth - thumbnailSize)
				{
					_spriteSheetTexture.SetPixels(x + 1, y + 1, thumbnailSize - 2, thumbnailSize - 2, emptySprite);
					x += thumbnailSize;
				}
			}

			RagePixelUtil.drawPixelBorder(_spriteSheetTexture, new Color(0.1f, 0.1f, 0.1f, 1f));
			_spriteSheetTexture.Apply();
		}
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
				if(index >= 0 && index < spriteSheet.rows.Length && localY >= 0 && localX >= 0)
				{
					currentRowKey = spriteSheet.GetKey(index);
					return true;
				}
				else
				{
					return false;
				}

			case (EventType.mouseUp):
				if(dragTargetIndex >= 0 && dragTargetIndex != spriteSheet.GetIndex(currentRowKey))
				{
					dragTargetIndex = index;
					int fromIndex = spriteSheet.GetIndex(currentRowKey);
					spriteSheet.MoveRow(fromIndex, dragTargetIndex);
					RagePixelUtil.RebuildAtlas(spriteSheet, false, "MoveRow");
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
				if(newdragTargetIndex >= 0 && newdragTargetIndex < spriteSheet.rows.Length)
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
		Object.DestroyImmediate(_spriteSheetTexture, false);
	}
}
