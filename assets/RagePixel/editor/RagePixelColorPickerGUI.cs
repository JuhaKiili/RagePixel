using UnityEngine;
using UnityEditor;
using System.Collections;

public class RagePixelColorPickerGUI
{
	public bool visible = false;
	public bool gizmoVisible = false;
	public bool isDirty = false;
	public int gizmoPositionX = 0;
	public int gizmoPositionY = 0;
	public int gizmoPixelWidth = 32;
	public int gizmoPixelHeight = 32;
	public int positionX = 0;
	public int positionY = 0;
	public int pixelWidth = 128;
	public int pixelHeight = 128;
	public int splitSize = 16;
	public int marginSize = 5;

	public enum GUIArea
	{
		None = 0,
		Color,
		Hue,
		Alpha
	};
	public GUIArea activeArea = GUIArea.None;
	private Color _selectedColor = new Color(0.9f, 0.5f, 0f);

	public Rect gizmoBounds
	{
		get
		{
			return new Rect(gizmoPositionX, gizmoPositionY, gizmoPixelWidth, gizmoPixelHeight);
		}
	}

	public Rect bounds
	{
		get
		{
			return new Rect(positionX, positionY, pixelWidth, pixelHeight);
		}
	}
    
	public Color selectedColor
	{
		get
		{ 
			return _selectedColor;
		}
		set
		{
			if(_selectedColor != value)
			{
				isDirty = true;
			}
			_selectedColor = value;
		}
	}

	private Texture2D _colorPickerTexture;

	public Texture2D colorPickerTexture
	{
		get
		{
			if(_colorPickerTexture == null)
			{
				CreateNewTextureInstance();
				isDirty = true;
			}
			if(isDirty)
			{
				Refresh();
				isDirty = false;
			}
			return _colorPickerTexture;
		}
	}

	private Texture2D _colorGizmoTexture;

	public Texture2D colorGizmoTexture
	{
		get
		{
			if(_colorGizmoTexture == null)
			{
				CreateNewTextureInstance();
				isDirty = true;
			}
			if(isDirty)
			{
				Refresh();
				isDirty = false;
			}
			return _colorGizmoTexture;
		}
	}

	private void CreateNewTextureInstance()
	{
		if(_colorPickerTexture != null)
		{
			Object.DestroyImmediate(_colorPickerTexture, false);
		}

		_colorPickerTexture =
            new Texture2D(
                pixelWidth,
                pixelHeight
            );
		_colorPickerTexture.hideFlags = HideFlags.HideAndDontSave;

		if(_colorGizmoTexture != null)
		{
			Object.DestroyImmediate(_colorGizmoTexture, false);
		}

		_colorGizmoTexture =
            new Texture2D(
                32,
                32
            );
		_colorGizmoTexture.hideFlags = HideFlags.HideAndDontSave;
	}

	public void Refresh()
	{
		renderColorGizmoTexture(_colorGizmoTexture);
		renderColorPickerTexture(_colorPickerTexture);
	}

	public bool HandleGUIEvent(Event ev)
	{
		bool used = false;
		if(visible)
		{
			Vector2 localMousePos = ev.mousePosition - new Vector2(bounds.xMin, bounds.yMin) - new Vector2(marginSize, marginSize);

			Rect sbBounds = new Rect(0, 0, pixelWidth - marginSize * 2 - splitSize, pixelHeight - marginSize * 2 - splitSize);
			Rect aBounds = new Rect(0, pixelWidth - marginSize - splitSize, pixelHeight - marginSize * 2, splitSize);
			Rect hBounds = new Rect(pixelWidth - marginSize * 2 - splitSize, 0, splitSize, pixelHeight - marginSize * 2 - splitSize);

			if(bounds.Contains(ev.mousePosition))
			{
				if(ev.type == EventType.mouseUp)
				{
					activeArea = GUIArea.None;
					used = true;
				}
				else if(ev.type == EventType.mouseDown)
				{
					if(sbBounds.Contains(localMousePos))
					{
						activeArea = GUIArea.Color;
					}
					else if(hBounds.Contains(localMousePos))
					{
						activeArea = GUIArea.Hue;
					}
					else if(aBounds.Contains(localMousePos))
					{
						activeArea = GUIArea.Alpha;
					}
					else
					{
						activeArea = GUIArea.None;
					}
					used = true;
				}
			}
			else
			{
				if(Event.current.type == EventType.mouseDown)
				{
					activeArea = GUIArea.None;
				}
			}

			if((Event.current.type == EventType.mouseDrag || Event.current.type == EventType.mouseDown) && activeArea != GUIArea.None)
			{
				RagePixelHSBColor hsbcolor = new RagePixelHSBColor(selectedColor);
				switch(activeArea)
				{
				case GUIArea.Color:
					hsbcolor.s = Mathf.Clamp(localMousePos.x, 0.001f, sbBounds.width - 0.001f) / (sbBounds.width);
					hsbcolor.b = 1f - Mathf.Clamp(localMousePos.y, 0.001f, sbBounds.height - 0.001f) / (sbBounds.height);
					break;
				case GUIArea.Hue:
					hsbcolor.h = 1f - Mathf.Clamp(localMousePos.y, 0.001f, hBounds.height - 0.001f) / (hBounds.height);
					break;
				case GUIArea.Alpha:
					hsbcolor.a = Mathf.Clamp(localMousePos.x, 0.001f, aBounds.width - 0.001f) / (aBounds.width);
					break;
				}
				selectedColor = hsbcolor.ToColor();
				used = true;
			}
		}

		return used;
	}

	private void renderColorGizmoTexture(Texture2D texture)
	{
		for(int pY = 0; pY < texture.height; pY++)
		{
			for(int pX = 0; pX < texture.width; pX++)
			{
				texture.SetPixel(pX, pY, selectedColor);
			}
		}

		texture.filterMode = FilterMode.Point;

		for(int pY = 0; pY < texture.height; pY++)
		{
			for(int pX = 0; pX < texture.width; pX++)
			{
				if(pX == 0 || pX == texture.width - 1 || pY == 0 || pY == texture.height - 1)
				{
					texture.SetPixel(pX, pY, new Color(0f, 0f, 0f, 1f));
				}
			}
		}
		texture.Apply();
	}

	private void renderColorPickerTexture(Texture2D texture)
	{
		for(int pY = 0; pY < texture.height; pY++)
		{
			for(int pX = 0; pX < texture.width; pX++)
			{
				texture.SetPixel(pX, pY, new Color(0f, 0f, 0f, 0f));
				if((pY < marginSize && pX > marginSize + 5) || (pX > texture.width - marginSize && pY < texture.height - marginSize - 5))
				{
					texture.SetPixel(pX, pY, new Color(0f, 0f, 0f, 0.1f));
				}
			}
		}

		for(int pY = splitSize + marginSize; pY < texture.height - marginSize; pY++)
		{
			for(int pX = marginSize; pX < texture.width - splitSize - marginSize; pX++)
			{
				RagePixelHSBColor hsb = new RagePixelHSBColor(new RagePixelHSBColor(selectedColor).h, (float)(pX - marginSize) / ((float)texture.width - splitSize - marginSize * 2f), (float)(pY - splitSize - marginSize) / (float)(texture.height - splitSize - marginSize * 2));
				texture.SetPixel(pX, pY, hsb.ToColor());
			}
		}

		for(int pY = splitSize + marginSize; pY < texture.height - marginSize; pY++)
		{
			for(int pX = texture.width - splitSize - marginSize; pX < texture.width - marginSize; pX++)
			{
				RagePixelHSBColor hsb = new RagePixelHSBColor((float)(pY - splitSize - marginSize) / (float)(texture.height - splitSize - marginSize * 2f), 1f, 1f);
				texture.SetPixel(pX, pY, hsb.ToColor());
			}
		}

		for(int pY = marginSize; pY < splitSize + marginSize; pY++)
		{
			for(int pX = marginSize; pX < texture.width - marginSize; pX++)
			{
				RagePixelHSBColor hsb = new RagePixelHSBColor(0f, 0f, (float)(pX - marginSize) / (float)(texture.width - marginSize * 2f));
				texture.SetPixel(pX, pY, hsb.ToColor());
			}
		}

		for(int pY = marginSize; pY < texture.height - marginSize; pY++)
		{
			for(int pX = marginSize; pX < texture.width - marginSize; pX++)
			{
				if(pX == marginSize || pX == texture.width - marginSize - 1 || pY == marginSize || pY == texture.height - marginSize - 1)
				{
					texture.SetPixel(pX, pY, new Color(0.75f, 0.75f, 0.75f, 1f));
				}
			}
		}

		RagePixelHSBColor pColor = new RagePixelHSBColor(selectedColor);
		int hueY = marginSize + splitSize + Mathf.RoundToInt((texture.height - marginSize * 2 - splitSize) * (pColor.h));

		for(int pX = texture.width - marginSize; pX < texture.width; pX++)
		{
			texture.SetPixel(pX, hueY, new Color(1f, 1f, 1f, 0.33f));
		}
		for(int pX = texture.width - marginSize + 1; pX < texture.width; pX++)
		{
			texture.SetPixel(pX, hueY - 1, new Color(1f, 1f, 1f, 0.33f));
			texture.SetPixel(pX, hueY + 1, new Color(1f, 1f, 1f, 0.33f));
		}
		for(int pX = texture.width - marginSize + 2; pX < texture.width; pX++)
		{
			texture.SetPixel(pX, hueY - 2, new Color(1f, 1f, 1f, 0.33f));
			texture.SetPixel(pX, hueY + 2, new Color(1f, 1f, 1f, 0.33f));
		}
		for(int pX = texture.width - marginSize + 3; pX < texture.width; pX++)
		{
			texture.SetPixel(pX, hueY - 3, new Color(1f, 1f, 1f, 0.33f));
			texture.SetPixel(pX, hueY + 3, new Color(1f, 1f, 1f, 0.33f));
		}

		int alphaX = marginSize + Mathf.RoundToInt((texture.width - marginSize * 2f) * Mathf.Clamp01(pColor.a));
		for(int pY = 0; pY < marginSize; pY++)
		{
			texture.SetPixel(alphaX, pY, new Color(1f, 1f, 1f, 0.33f));
		}
		for(int pY = 0; pY < marginSize - 1; pY++)
		{
			texture.SetPixel(alphaX - 1, pY, new Color(1f, 1f, 1f, 0.33f));
			texture.SetPixel(alphaX + 1, pY, new Color(1f, 1f, 1f, 0.33f));
		}
		for(int pY = 0; pY < marginSize - 2; pY++)
		{
			texture.SetPixel(alphaX - 2, pY, new Color(1f, 1f, 1f, 0.33f));
			texture.SetPixel(alphaX + 2, pY, new Color(1f, 1f, 1f, 0.33f));
		}
		for(int pY = 0; pY < marginSize - 3; pY++)
		{
			texture.SetPixel(alphaX - 3, pY, new Color(1f, 1f, 1f, 0.33f));
			texture.SetPixel(alphaX + 3, pY, new Color(1f, 1f, 1f, 0.33f));
		}

		int pickerX = marginSize + Mathf.RoundToInt((texture.width - marginSize * 2f - splitSize) * (pColor.s));
		int pickerY = marginSize + splitSize + Mathf.RoundToInt((texture.height - marginSize * 2f - splitSize) * (pColor.b));

		texture.SetPixel(pickerX, pickerY + 3, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX + 1, pickerY + 3, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX + 2, pickerY + 2, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX + 3, pickerY + 1, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX + 3, pickerY, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX + 3, pickerY - 1, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX + 2, pickerY - 2, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX + 1, pickerY - 3, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX, pickerY - 3, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX - 1, pickerY - 3, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX - 2, pickerY - 2, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX - 3, pickerY - 1, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX - 3, pickerY, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX - 3, pickerY + 1, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX - 2, pickerY + 2, new Color(0.5f, 0.5f, 0.5f, 1f));
		texture.SetPixel(pickerX - 1, pickerY + 3, new Color(0.5f, 0.5f, 0.5f, 1f));

		texture.filterMode = FilterMode.Point;

		texture.Apply();
	}

	public void CleanExit()
	{
		Object.DestroyImmediate(_colorPickerTexture, false);
		Object.DestroyImmediate(_colorGizmoTexture, false);
	}
}
