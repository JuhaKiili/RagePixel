using UnityEngine;
using System.Collections;
using UnityEditor;

public static class RagePixelGUIIcons
{
	private static Texture2D _penIcon;
	private static Texture2D _cursorIcon;
	private static Texture2D _fillIcon;
	private static Texture2D _animationIcon;
	private static Texture2D _replaceIcon;
	private static Texture2D _selectIcon;
	private static Texture2D _resizeIcon;

	public static Color greenButtonColor
	{
		get
		{
			if(PlayerSettings.advancedLicense)
			{
				return new Color(0.85f, 1f, 0.85f, 1f);
			}
			else
			{
				return new Color(0.85f, 1f, 0.85f, 1f);
			}
		}
	}

	public static Color redButtonColor
	{
		get
		{
			if(PlayerSettings.advancedLicense)
			{
				return new Color(1f, 0.85f, 0.85f, 1f);
			}
			else
			{
				return new Color(1f, 0.85f, 0.85f, 1f);
			}
		}
	}

	public static Color neutralButtonColor
	{
		get
		{
			if(PlayerSettings.advancedLicense)
			{
				return new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				return new Color(1f, 1f, 1f, 1f);
			}
		}
	}

	public static Texture2D Pen
	{
		get
		{ 
			if(_penIcon == null)
			{
				_penIcon = AssetDatabase.LoadAssetAtPath("Assets" + System.IO.Path.DirectorySeparatorChar + "RagePixel" + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar + "pencil.png", typeof(Texture2D)) as Texture2D;
				_penIcon.hideFlags = HideFlags.HideAndDontSave;
			}
			return _penIcon;
		}
	}

	public static Texture2D Cursor
	{
		get
		{
			if(_cursorIcon == null)
			{
				_cursorIcon = AssetDatabase.LoadAssetAtPath("Assets" + System.IO.Path.DirectorySeparatorChar + "RagePixel" + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar + "cursor.png", typeof(Texture2D)) as Texture2D;
				_cursorIcon.hideFlags = HideFlags.HideAndDontSave;
			}
			return _cursorIcon;
		}
	}

	public static Texture2D Fill
	{
		get
		{
			if(_fillIcon == null)
			{
				_fillIcon = AssetDatabase.LoadAssetAtPath("Assets" + System.IO.Path.DirectorySeparatorChar + "RagePixel" + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar + "fill.png", typeof(Texture2D)) as Texture2D;
				_fillIcon.hideFlags = HideFlags.HideAndDontSave;
			}
			return _fillIcon;
		}
	}

	public static Texture2D Animation
	{
		get
		{
			if(_animationIcon == null)
			{
				_animationIcon = AssetDatabase.LoadAssetAtPath("Assets" + System.IO.Path.DirectorySeparatorChar + "RagePixel" + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar + "animation.png", typeof(Texture2D)) as Texture2D;
				_animationIcon.hideFlags = HideFlags.HideAndDontSave;
			}
			return _animationIcon;
		}
	}

	public static Texture2D Replace
	{
		get
		{
			if(_replaceIcon == null)
			{
				_replaceIcon = AssetDatabase.LoadAssetAtPath("Assets" + System.IO.Path.DirectorySeparatorChar + "RagePixel" + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar + "replace.png", typeof(Texture2D)) as Texture2D;
				_replaceIcon.hideFlags = HideFlags.HideAndDontSave;
			}
			return _replaceIcon;
		}
	}

	public static Texture2D Select
	{
		get
		{
			if(_selectIcon == null)
			{
				_selectIcon = AssetDatabase.LoadAssetAtPath("Assets" + System.IO.Path.DirectorySeparatorChar + "RagePixel" + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar + "selection.png", typeof(Texture2D)) as Texture2D;
				_selectIcon.hideFlags = HideFlags.HideAndDontSave;
			}
			return _selectIcon;
		}
	}

	public static Texture2D Resize
	{
		get
		{
			if(_resizeIcon == null)
			{
				_resizeIcon = AssetDatabase.LoadAssetAtPath("Assets" + System.IO.Path.DirectorySeparatorChar + "RagePixel" + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar + "resize.png", typeof(Texture2D)) as Texture2D;
				_resizeIcon.hideFlags = HideFlags.HideAndDontSave;
			}
			return _resizeIcon;
		}
	}

}
