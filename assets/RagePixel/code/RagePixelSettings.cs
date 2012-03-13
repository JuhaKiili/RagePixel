using UnityEngine;
using System.Collections;

public class RagePixelSettings : ScriptableObject
{
	public bool showCameraWarnings = true;
	public bool initialSpritesheetGenerated = false;
	private RagePixelBitmap _clipboard;

	public RagePixelBitmap clipboard
	{
		get
		{
			if(_clipboard == null)
			{
				_clipboard = new RagePixelBitmap(new Color[0], 0, 0);
			}
			return _clipboard;
		}
		set
		{
			_clipboard = value;
		}
	}

	[SerializeField]
	private Color[] _palette;

	public Color[] palette
	{
		get
		{
			if(_palette == null)
			{
				_palette = new Color[2];
				_palette[0] = new Color(0f, 0f, 0f, 1f);
				_palette[1] = new Color(1f, 1f, 1f, 1f);
			}
			return _palette;
		}
		set
		{
			if(value == null)
			{
				//Debug.Log("SET AS NULL");
			}
			_palette = value;
		}
	}
        
}
