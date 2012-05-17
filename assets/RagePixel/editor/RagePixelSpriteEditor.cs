using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(RagePixelSprite))]
public class RagePixelSpriteEditor : Editor
{
	private float handleSize = 0.01f;
	private bool justSelected = false;
	private bool paintUndoSaved = false;
	private int defaultSceneButtonWidth = 32;
	private int defaultSceneButtonHeight = 32;
	private bool atlasTextureIsDirty = false;
	private Color32[] colorReplaceBuffer;
	private RagePixelTexelRect selection;
	private bool selectionActive = false;
	private RagePixelTexel selectionStart;
	private RagePixelTexel frontBufferPosition;
	private RagePixelTexel frontBufferDragStartPosition;
	private RagePixelTexel frontBufferDragStartMousePosition;
	private RagePixelBitmap backBuffer;
	private RagePixelBitmap frontBuffer;

	public enum Mode
	{
		Default = 0,
		Pen,
		Fill,
		Scale,
		Resize,
		Select
	};
	public Mode mode = Mode.Default;
	public Vector2 rectangleStart;
	protected bool animationStripEnabled = true;
	public enum BrushType
	{
		Brush1 = 0,
		Brush3,
		Brush5
	};
	public BrushType brushType = BrushType.Brush1;
	private RagePixelBitmap _brush11;
	private RagePixelBitmap _brush33;
	private RagePixelBitmap _brush55;

	public RagePixelBitmap brush
	{
		get
		{
			switch(brushType)
			{
			case (BrushType.Brush1):
				if(_brush11 == null)
				{
					Color[] brush;
					brush = new Color[1];
					brush[0] = new Color(1f, 1f, 1f, 1f); 
					_brush11 = new RagePixelBitmap(brush, 1, 1);
				}
				return _brush11;

			case (BrushType.Brush3):
				if(_brush33 == null)
				{
					Color[] brush;
					brush = new Color[3 * 3];
					brush[0] = new Color(1f, 1f, 1f, 0f);
					brush[1] = new Color(1f, 1f, 1f, 1f);
					brush[2] = new Color(1f, 1f, 1f, 0f);
					brush[3] = new Color(1f, 1f, 1f, 1f);
					brush[4] = new Color(1f, 1f, 1f, 1f);
					brush[5] = new Color(1f, 1f, 1f, 1f);
					brush[6] = new Color(1f, 1f, 1f, 0f);
					brush[7] = new Color(1f, 1f, 1f, 1f);
					brush[8] = new Color(1f, 1f, 1f, 0f);
					_brush33 = new RagePixelBitmap(brush, 3, 3);
				}
				return _brush33;

			case (BrushType.Brush5):
				if(_brush55 == null)
				{
					Color[] brush;
					brush = new Color[5 * 5];
					brush[0] = new Color(1f, 1f, 1f, 0f);
					brush[1] = new Color(1f, 1f, 1f, 1f);
					brush[2] = new Color(1f, 1f, 1f, 1f);
					brush[3] = new Color(1f, 1f, 1f, 1f);
					brush[4] = new Color(1f, 1f, 1f, 0f);
					brush[5] = new Color(1f, 1f, 1f, 1f);
					brush[6] = new Color(1f, 1f, 1f, 1f);
					brush[7] = new Color(1f, 1f, 1f, 1f);
					brush[8] = new Color(1f, 1f, 1f, 1f);
					brush[9] = new Color(1f, 1f, 1f, 1f);
					brush[10] = new Color(1f, 1f, 1f, 1f);
					brush[11] = new Color(1f, 1f, 1f, 1f);
					brush[12] = new Color(1f, 1f, 1f, 1f);
					brush[13] = new Color(1f, 1f, 1f, 1f);
					brush[14] = new Color(1f, 1f, 1f, 1f);
					brush[15] = new Color(1f, 1f, 1f, 1f);
					brush[16] = new Color(1f, 1f, 1f, 1f);
					brush[17] = new Color(1f, 1f, 1f, 1f);
					brush[18] = new Color(1f, 1f, 1f, 1f);
					brush[19] = new Color(1f, 1f, 1f, 1f);
					brush[20] = new Color(1f, 1f, 1f, 0f);
					brush[21] = new Color(1f, 1f, 1f, 1f);
					brush[22] = new Color(1f, 1f, 1f, 1f);
					brush[23] = new Color(1f, 1f, 1f, 1f);
					brush[24] = new Color(1f, 1f, 1f, 0f);
					_brush55 = new RagePixelBitmap(brush, 5, 5);
				}
				return _brush55;
			}
			return null;
		}       
	}

	private Camera sceneCamera
	{
		get
		{
			if(SceneView.lastActiveSceneView != null)
			{
				return SceneView.lastActiveSceneView.camera;
			}
			else
			{
				return null;
			}

		}
	}

	private int scenePixelWidth
	{
		get
		{
			if(sceneCamera != null)
			{
				return (int)sceneCamera.pixelWidth;
			}
			else
			{
				return (int)Screen.width;
			}
		}
	}

	private int scenePixelHeight
	{
		get
		{
			if(sceneCamera != null)
			{
				return (int)sceneCamera.pixelHeight;
			}
			else
			{
				return (int)Screen.height;
			}
		}
	}

	private MeshRenderer _meshRenderer;

	public MeshRenderer meshRenderer
	{
		get
		{
			if(_meshRenderer == null)
			{
				_meshRenderer = ragePixelSprite.GetComponent<MeshRenderer>();
			}
			return _meshRenderer;
		}
	}

	private MeshFilter _meshFilter;

	public MeshFilter meshFilter
	{
		get
		{
			if(_meshFilter == null)
			{
				_meshFilter = ragePixelSprite.GetComponent<MeshFilter>();
			}
			return _meshFilter;
		}
	}

	private Texture2D _spritesheetTexture;

	public Texture2D spritesheetTexture
	{
		get
		{
			if(_spritesheetTexture == null)
			{
				_spritesheetTexture = ragePixelSprite.spriteSheet.atlas.GetTexture("_MainTex") as Texture2D;
			}
			return _spritesheetTexture;
		}
	}

	private RagePixelSprite _ragePixelSprite;

	public RagePixelSprite ragePixelSprite
	{
		get
		{
			if(_ragePixelSprite == null)
			{
				_ragePixelSprite = target as RagePixelSprite;
			}
			return _ragePixelSprite;
		}
	}

	private RagePixelPaletteGUI _paletteGUI;

	RagePixelPaletteGUI paletteGUI
	{
		get
		{
			if(_paletteGUI == null)
			{
				_paletteGUI = new RagePixelPaletteGUI();
				_paletteGUI.backgroundColor = new Color(0f, 0f, 0f, 0f);
				_paletteGUI.positionX = scenePixelWidth - _paletteGUI.pixelWidth;
				_paletteGUI.positionY = 5;
				_paletteGUI.maxWidth = scenePixelWidth - 300;
			}
			return _paletteGUI;
		}
	}

	private RagePixelAnimStripGUI _animStripGUI;

	public RagePixelAnimStripGUI animStripGUI
	{
		get
		{
			if(_animStripGUI == null)
			{
				_animStripGUI = new RagePixelAnimStripGUI(spriteSheetGUI);
				_animStripGUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
				_animStripGUI.currentCellKey = ragePixelSprite.currentCellKey;
				_animStripGUI.positionX = defaultSceneButtonWidth + 10;
				_animStripGUI.positionY = scenePixelHeight - _animStripGUI.thumbnailSize - 7;
				_animStripGUI.maxWidth = scenePixelWidth - defaultSceneButtonWidth * 3 - 20;
			}
			return _animStripGUI;
		}
	}

	private RagePixelSpriteSheetGUI _spriteSheetGUI;

	public RagePixelSpriteSheetGUI spriteSheetGUI
	{
		get
		{
			if(_spriteSheetGUI == null)
			{
				_spriteSheetGUI = new RagePixelSpriteSheetGUI();
				spriteSheetGUI.currentRowKey = ragePixelSprite.currentRowKey;
				spriteSheetGUI.spriteSheet = ragePixelSprite.spriteSheet;
			}
			return _spriteSheetGUI;
		}
	}

	private RagePixelColorPickerGUI _paintColorPickerGUI;

	public RagePixelColorPickerGUI paintColorPickerGUI
	{
		get
		{
			if(_paintColorPickerGUI == null)
			{
				_paintColorPickerGUI = new RagePixelColorPickerGUI();
				_paintColorPickerGUI.gizmoVisible = false;
				_paintColorPickerGUI.visible = false;
				_paintColorPickerGUI.gizmoPositionX = 5;
				_paintColorPickerGUI.gizmoPositionY = 5;
				_paintColorPickerGUI.positionX = _paintColorPickerGUI.gizmoPositionX + _paintColorPickerGUI.gizmoPixelWidth;
				_paintColorPickerGUI.positionY = _paintColorPickerGUI.gizmoPositionY;
			}
			return _paintColorPickerGUI;
		}
	}

	private RagePixelColorPickerGUI _replaceColorPickerGUI;

	public RagePixelColorPickerGUI replaceColorPickerGUI
	{
		get
		{
			if(_replaceColorPickerGUI == null)
			{
				_replaceColorPickerGUI = new RagePixelColorPickerGUI();
				_replaceColorPickerGUI.gizmoVisible = false;
				_replaceColorPickerGUI.visible = false;
				_replaceColorPickerGUI.gizmoPositionX = (int)paintColorPickerGUI.gizmoBounds.xMax + (int)defaultSceneButtonWidth;
				_replaceColorPickerGUI.gizmoPositionY = 5;
				_replaceColorPickerGUI.gizmoPixelWidth = (int)defaultSceneButtonWidth;
				_replaceColorPickerGUI.gizmoPixelHeight = (int)defaultSceneButtonWidth;
				_replaceColorPickerGUI.positionX = _replaceColorPickerGUI.gizmoPositionX + (int)defaultSceneButtonWidth;
				_replaceColorPickerGUI.positionY = _replaceColorPickerGUI.gizmoPositionY;
			}
			return _replaceColorPickerGUI;
		}
	}

	private bool showGrid9Gizmo = false;

	public void OnDestroy()
	{
		spriteSheetGUI.CleanExit();
		animStripGUI.CleanExit();
		paintColorPickerGUI.CleanExit();
		replaceColorPickerGUI.CleanExit();
	}
        
	public override void OnInspectorGUI()
	{
		InvokeOnSelectedEvent();
		GUILayout.Space(5f);
                
		if(ragePixelSprite.spriteSheet == null)
		{
			ragePixelSprite.spriteSheet = (RagePixelSpriteSheet)EditorGUILayout.ObjectField("Sprite Sheet", ragePixelSprite.spriteSheet, typeof(RagePixelSpriteSheet), false, null);
			if(ragePixelSprite.spriteSheet != null)
			{
				if(ragePixelSprite.currentRowKey == 0 || ragePixelSprite.currentCellKey == 0)
				{
					ragePixelSprite.currentRowKey = ragePixelSprite.spriteSheet.rows[0].key;
					ragePixelSprite.currentCellKey = ragePixelSprite.spriteSheet.rows[0].cells[0].key;
				}
				_spritesheetTexture = null;
				ragePixelSprite.meshIsDirty = true;
				ragePixelSprite.refreshMesh();
			}
		}
		else if(!Application.isPlaying)
		{
			int spriteSheetGUIMargin = 7;

			RagePixelSpriteSheet oldSheet = ragePixelSprite.spriteSheet;
			ragePixelSprite.spriteSheet =
                (RagePixelSpriteSheet)EditorGUILayout.ObjectField(
                    "Sprite sheet",
                    ragePixelSprite.spriteSheet,
                    typeof(RagePixelSpriteSheet),
                    false
                    );

			if(ragePixelSprite.spriteSheet != null)
			{

				if(oldSheet != ragePixelSprite.spriteSheet)
				{
					if(ragePixelSprite.currentRowKey == 0 || ragePixelSprite.currentCellKey == 0)
					{
						ragePixelSprite.currentRowKey = ragePixelSprite.spriteSheet.rows[0].key;
						ragePixelSprite.currentCellKey = ragePixelSprite.spriteSheet.rows[0].cells[0].key;
					}
					_spritesheetTexture = null;
					ragePixelSprite.meshIsDirty = true;
					ragePixelSprite.refreshMesh();
				}

				spriteSheetGUI.maxWidth = Screen.width - spriteSheetGUIMargin;
				spriteSheetGUI.spriteSheet = ragePixelSprite.spriteSheet;
				spriteSheetGUI.currentRowKey = ragePixelSprite.currentRowKey;
				animStripGUI.currentCellKey = ragePixelSprite.currentCellKey;

				RagePixelSprite.Mode oldMode = ragePixelSprite.mode;
				ragePixelSprite.mode =
                    (RagePixelSprite.Mode)EditorGUILayout.EnumPopup(
                        "Mode",
                        ragePixelSprite.mode
                        );

				if(oldMode != ragePixelSprite.mode)
				{
					mode = Mode.Default;
					selection = null;
					selectionActive = false;
					ragePixelSprite.meshIsDirty = true;
					ragePixelSprite.refreshMesh();
					ragePixelSprite.SnapToScale();
				}

				if(ragePixelSprite.mode == RagePixelSprite.Mode.Grid9)
				{
					EditorGUI.indentLevel = 1;
					bool changed = GUI.changed;

					ragePixelSprite.grid9Left = Mathf.Max(Mathf.Min(EditorGUILayout.IntField("Left Margin", ragePixelSprite.grid9Left), ragePixelSprite.pixelSizeX - ragePixelSprite.grid9Right, ragePixelSprite.GetCurrentRow().pixelSizeX - ragePixelSprite.grid9Right - 1), 0);
					ragePixelSprite.grid9Top = Mathf.Max(Mathf.Min(EditorGUILayout.IntField("Top Margin", ragePixelSprite.grid9Top), ragePixelSprite.pixelSizeY - ragePixelSprite.grid9Bottom, ragePixelSprite.GetCurrentRow().pixelSizeY - ragePixelSprite.grid9Bottom - 1), 0);
					ragePixelSprite.grid9Right = Mathf.Max(Mathf.Min(EditorGUILayout.IntField("Right Margin", ragePixelSprite.grid9Right), ragePixelSprite.pixelSizeX - ragePixelSprite.grid9Left, ragePixelSprite.GetCurrentRow().pixelSizeX - ragePixelSprite.grid9Left - 1), 0);
					ragePixelSprite.grid9Bottom = Mathf.Max(Mathf.Min(EditorGUILayout.IntField("Bottom Margin", ragePixelSprite.grid9Bottom), ragePixelSprite.pixelSizeY - ragePixelSprite.grid9Top, ragePixelSprite.GetCurrentRow().pixelSizeY - ragePixelSprite.grid9Top - 1), 0);

					if(changed != GUI.changed)
					{
						ragePixelSprite.meshIsDirty = true;
						ragePixelSprite.refreshMesh();
					}

					showGrid9Gizmo = EditorGUILayout.Toggle("Show Gizmos", showGrid9Gizmo);
					EditorGUI.indentLevel = 0;
				}

				RagePixelSprite.PivotMode oldPivotMode = ragePixelSprite.pivotMode;
				ragePixelSprite.pivotMode = (RagePixelSprite.PivotMode)EditorGUILayout.EnumPopup("Pivot", ragePixelSprite.pivotMode);
				if(oldPivotMode != ragePixelSprite.pivotMode)
				{
					ragePixelSprite.meshIsDirty = true;
					ragePixelSprite.refreshMesh();
					ragePixelSprite.SnapToScale();
				}

				Color oldColor = ragePixelSprite.tintColor;
				ragePixelSprite.tintColor = EditorGUILayout.ColorField("Tint Color", ragePixelSprite.tintColor);
				if(ragePixelSprite.tintColor != oldColor)
				{
					ragePixelSprite.vertexColorsAreDirty = true;
					ragePixelSprite.refreshMesh();
				}

				ragePixelSprite.animationMode = (RagePixelSprite.AnimationMode)EditorGUILayout.EnumPopup("Animation Mode", ragePixelSprite.animationMode);
				ragePixelSprite.playAnimation = EditorGUILayout.Toggle("Play On Awake", ragePixelSprite.playAnimation);

				GUILayout.Space(7f);

				spriteSheetGUI.spriteSheet = ragePixelSprite.spriteSheet;
				spriteSheetGUI.currentRowKey = ragePixelSprite.currentRowKey;

				if(spriteSheetGUI.HandleGUIEvent(Event.current))
				{
					ragePixelSprite.currentRowKey = spriteSheetGUI.currentRowKey;
					ragePixelSprite.currentCellKey = ragePixelSprite.spriteSheet.GetRow(ragePixelSprite.currentRowKey).cells[0].key;
					ragePixelSprite.pixelSizeX = ragePixelSprite.GetCurrentRow().pixelSizeX;
					ragePixelSprite.pixelSizeY = ragePixelSprite.GetCurrentRow().pixelSizeY;
					ragePixelSprite.meshIsDirty = true;
					ragePixelSprite.refreshMesh();
					Repaint();
				}

				spriteSheetGUI.positionX = spriteSheetGUIMargin;

				Rect spriteSheetR = EditorGUILayout.BeginVertical();
				GUILayout.Space(spriteSheetGUI.pixelHeight);

				EditorGUI.DrawPreviewTexture(
                    new Rect(
                        spriteSheetGUIMargin,
                        spriteSheetR.yMin,
                        spriteSheetGUI.pixelWidth,
                        spriteSheetR.height
                        ), spriteSheetGUI.spriteSheetTexture);

				EditorGUILayout.EndVertical();
				if((int)spriteSheetR.yMin > 0)
				{
					spriteSheetGUI.positionY = (int)spriteSheetR.yMin;
				}

				Rect rButtons = EditorGUILayout.BeginVertical();

				GUILayout.Space(defaultSceneButtonWidth + 5f);

				GUI.color = RagePixelGUIIcons.greenButtonColor;
				if(GUI.Button(new Rect(spriteSheetGUIMargin, rButtons.yMin + 2f, defaultSceneButtonWidth + 6f, defaultSceneButtonHeight), "NEW"))
				{
					int index = ragePixelSprite.spriteSheet.GetIndex(spriteSheetGUI.currentRowKey);

					RagePixelRow row =
                        ragePixelSprite.spriteSheet.AddRow(
                            RagePixelUtil.RandomKey(),
                            index + 1,
                            ragePixelSprite.GetCurrentRow().pixelSizeX,
                            ragePixelSprite.GetCurrentRow().pixelSizeY
                            );

					RagePixelCell cell =
                        row.InsertCell(0, RagePixelUtil.RandomKey());

					ragePixelSprite.currentRowKey = row.key;
					ragePixelSprite.currentCellKey = cell.key;

					RagePixelUtil.RebuildAtlas(ragePixelSprite.spriteSheet, true, "AddRow");
					atlasTextureIsDirty = true;
				}
                
				GUI.color = RagePixelGUIIcons.redButtonColor;

				if(GUI.Button(new Rect(spriteSheetGUIMargin + defaultSceneButtonWidth + 6f, rButtons.yMin + 2f, defaultSceneButtonWidth + 6f, defaultSceneButtonHeight), "DEL"))
				{
					if(ragePixelSprite.spriteSheet.rows.Length > 1)
					{
						if(EditorUtility.DisplayDialog("Delete selected sprite (no undo)?", "Are you sure?", "Delete", "Cancel"))
						{
							int index = ragePixelSprite.spriteSheet.GetIndex(spriteSheetGUI.currentRowKey);
							ragePixelSprite.spriteSheet.RemoveRowByKey(spriteSheetGUI.currentRowKey);
							ragePixelSprite.currentRowKey = ragePixelSprite.spriteSheet.rows[Mathf.Clamp(index, 0, ragePixelSprite.spriteSheet.rows.Length - 1)].key;
							ragePixelSprite.pixelSizeX = ragePixelSprite.GetCurrentRow().pixelSizeX;
							ragePixelSprite.pixelSizeY = ragePixelSprite.GetCurrentRow().pixelSizeY;
							RagePixelUtil.RebuildAtlas(ragePixelSprite.spriteSheet, false, "DeleteRow");
							ragePixelSprite.currentCellKey = ragePixelSprite.GetCurrentRow().cells[0].key;
							ragePixelSprite.meshIsDirty = true;
							ragePixelSprite.refreshMesh();
							atlasTextureIsDirty = true;
						}
					}
					else
					{
						EditorUtility.DisplayDialog("Cannot delete", "Cannot delete the last sprite.", "OK");
					}
				}

				GUI.color = RagePixelGUIIcons.neutralButtonColor;
				if(GUI.Button(new Rect(spriteSheetGUIMargin + defaultSceneButtonWidth * 2f + 12f, rButtons.yMin + 2f, defaultSceneButtonWidth + 10f, defaultSceneButtonHeight), "EDIT"))
				{
					if(ragePixelSprite.spriteSheet != null)
					{
						RagePixelSpriteSheetEditorWindow editorWindow = ScriptableObject.CreateInstance(typeof(RagePixelSpriteSheetEditorWindow)) as RagePixelSpriteSheetEditorWindow;
						editorWindow.inspector = this;
						if(ragePixelSprite != null)
						{
							editorWindow.selectedSprite = ragePixelSprite;
						}
						editorWindow.title = "Spritesheet";
						editorWindow.spriteSheet = ragePixelSprite.spriteSheet;
						editorWindow.Show();
					}
				}
				EditorGUILayout.EndVertical();
				GUI.color = Color.white;

				GUILayout.Space(7f);
			}
		}
		else
		{
			GUILayout.Space(5f);
			EditorUtility.SetSelectedWireframeHidden(meshRenderer, true);
			GUILayout.Label("Inspector GUI disabled in play mode");
			GUILayout.Space(5f);
		}
                
	}
        
	public void OnSceneGUI()
	{
	    InvokeOnSelectedEvent();

		if(!Application.isPlaying)
		{
			if(ragePixelSprite.spriteSheet != null)
			{
				SceneGUIInit();
                
				bool colorReplaced = false;

				// GUI
				Handles.BeginGUI();
				HandleCameraWarnings();
				HandleColorPickerGUI();
				HandlePaintGUI();
				HandlePaletteGUI();
				HandleAnimationGUI();
				DrawGizmos();
				Handles.EndGUI();

				HandleKeyboard();

				switch(mode)
				{
				case (Mode.Default):
					HandleModeDefault();
					break;
				case (Mode.Pen):
					HandleModePen();
					break;
				case (Mode.Fill):
					HandleModeFill();
					break;
				case (Mode.Select):
					HandleModeSelect();
					break;
				case (Mode.Resize):
					HandleModeResize();
					break;
				}

				if(Event.current.type == EventType.mouseUp)
				{
					paintUndoSaved = false;
				}

				if(atlasTextureIsDirty && Event.current.type == EventType.mouseUp || colorReplaced)
				{
					saveTexture();
					animStripGUI.isDirty = true;
					spriteSheetGUI.isDirty = true;
					atlasTextureIsDirty = false;
				}
			}
		}
		else
		{
			DrawSpriteBoundsGizmo();
		}
	}
    
	public void HandleModeDefault()
	{
		Vector2 newScale = 
            ragePixelSprite.transform.InverseTransformPoint(
                Handles.FreeMoveHandle(
                    ragePixelSprite.transform.TransformPoint(new Vector3(ragePixelSprite.pixelSizeX, ragePixelSprite.pixelSizeY, 0f) + GetPivotOffset()), 
                    Quaternion.identity, 
                    sceneCamera.orthographicSize * handleSize, 
                    new Vector3(1f, 1f, 0f), 
                    Handles.RectangleCap
                )
            ) - GetPivotOffset();

		if(ragePixelSprite.pixelSizeX != Mathf.RoundToInt(newScale.x) || ragePixelSprite.pixelSizeY != Mathf.RoundToInt(newScale.y))
		{
			int minX = ragePixelSprite.mode != RagePixelSprite.Mode.Grid9 ? 1 : ragePixelSprite.grid9Left + ragePixelSprite.grid9Right + 1;
			int minY = ragePixelSprite.mode != RagePixelSprite.Mode.Grid9 ? 1 : ragePixelSprite.grid9Top + ragePixelSprite.grid9Bottom + 1;
            
			ragePixelSprite.pixelSizeX = Mathf.Max(Mathf.RoundToInt(newScale.x), minX);
			ragePixelSprite.pixelSizeY = Mathf.Max(Mathf.RoundToInt(newScale.y), minY);
			ragePixelSprite.meshIsDirty = true;
			ragePixelSprite.refreshMesh();
		} 
	}

	public void HandleModePen()
	{
		if(Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseDrag)
		{
			Vector3 mouseWorldPosition = sceneScreenToWorldPoint(Event.current.mousePosition);

			RagePixelTexel texel = WorldToTexelCoords(spritesheetTexture, ragePixelSprite.transform, mouseWorldPosition);

			if(texel.X >= 0 && texel.X < ragePixelSprite.GetCurrentRow().pixelSizeX &&
                texel.Y >= 0 && texel.Y < ragePixelSprite.GetCurrentRow().pixelSizeY)
			{
				Rect uv = ragePixelSprite.GetCurrentCell().uv;
				int minX = Mathf.FloorToInt(spritesheetTexture.width * uv.xMin);
				int minY = Mathf.FloorToInt(spritesheetTexture.height * uv.yMin);

				if(Event.current.button == 0)
				{
					SavePaintUndo();

					atlasTextureIsDirty = true;
                      
					/*
                    spritesheetTexture.SetPixel(
                        minX + texel.X,
                        minY + texel.Y,
                        paintColorPickerGUI.selectedColor);
                    */

					brush.PasteToTextureWithBounds(
                        minX + texel.X - Mathf.FloorToInt(brush.Width() / 2f),
                        minY + texel.Y - Mathf.FloorToInt(brush.Height() / 2f), 
                        minX, 
                        minY, 
                        minX + ragePixelSprite.GetCurrentRow().pixelSizeX,
                        minY + ragePixelSprite.GetCurrentRow().pixelSizeY, 
                        spritesheetTexture,
                        paintColorPickerGUI.selectedColor
                        );

					spritesheetTexture.Apply();
                    
					paintColorPickerGUI.selectedColor = spritesheetTexture.GetPixel(
                        minX + texel.X,
                        minY + texel.Y);
                    
				}
				else if(Event.current.button == 1)
				{
					paintColorPickerGUI.selectedColor = spritesheetTexture.GetPixel(
                        minX + texel.X,
                        minY + texel.Y);

					replaceColorPickerGUI.selectedColor = paintColorPickerGUI.selectedColor;
					paletteGUI.SelectColor(paintColorPickerGUI.selectedColor);

					if(Event.current.control || Event.current.command)
					{
						paletteGUI.AddColor(paintColorPickerGUI.selectedColor);
					}
				}
			}
			else
			{
				if(Event.current.button == 1)
				{
					paintColorPickerGUI.selectedColor = new Color(0f, 0f, 0f, 0f);
					replaceColorPickerGUI.selectedColor = paintColorPickerGUI.selectedColor;
				}
			}
			
			if(Event.current.button == 0 || Event.current.button == 1)
			{
				Event.current.Use();
			}
		}
	}

	public void HandleModeFill()
	{
		if(Event.current.type == EventType.mouseDown)
		{
			Vector3 mouseWorldPosition = sceneScreenToWorldPoint(Event.current.mousePosition);

			RagePixelTexel texel = WorldToTexelCoords(spritesheetTexture, ragePixelSprite.transform, mouseWorldPosition);

			Rect uv = ragePixelSprite.GetCurrentCell().uv;
			int minX = Mathf.FloorToInt(spritesheetTexture.width * uv.xMin);
			int minY = Mathf.FloorToInt(spritesheetTexture.height * uv.yMin);
			int maxX = Mathf.FloorToInt(spritesheetTexture.width * uv.xMax);
			int maxY = Mathf.FloorToInt(spritesheetTexture.height * uv.yMax);

			if(texel.X >= 0 && texel.X < ragePixelSprite.GetCurrentRow().pixelSizeX &&
                texel.Y >= 0 && texel.Y < ragePixelSprite.GetCurrentRow().pixelSizeY)
			{
				if(Event.current.button == 0)
				{
					Color fillTargetColor = spritesheetTexture.GetPixel(minX + texel.X, minY + texel.Y);

					if(!fillTargetColor.Equals(paintColorPickerGUI.selectedColor))
					{
						SavePaintUndo();
						atlasTextureIsDirty = true;
						FloodFill(
                            fillTargetColor,
                            paintColorPickerGUI.selectedColor,
                            spritesheetTexture,
                            minX + texel.X,
                            minY + texel.Y,
                            minX,
                            minY,
                            maxX,
                            maxY);
						spritesheetTexture.Apply();
						paintColorPickerGUI.selectedColor = spritesheetTexture.GetPixel(
                        minX + texel.X,
                        minY + texel.Y);
					}
				}
				else if(Event.current.button == 1)
				{
					paintColorPickerGUI.selectedColor = spritesheetTexture.GetPixel(
                        minX + texel.X,
                        minY + texel.Y);
					replaceColorPickerGUI.selectedColor = paintColorPickerGUI.selectedColor;
				}
			}
			else
			{
				if(Event.current.button == 1)
				{
					paintColorPickerGUI.selectedColor = new Color(0f, 0f, 0f, 0f);
					replaceColorPickerGUI.selectedColor = paintColorPickerGUI.selectedColor;
				}
			}
			
			if(Event.current.button == 0 || Event.current.button == 1)
			{
				Event.current.Use();
			}
		}
	}

	public void HandleModeSelect()
	{
		if(Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseDrag || Event.current.type == EventType.mouseUp)
		{
			Vector3 mouseWorldPosition = sceneScreenToWorldPoint(Event.current.mousePosition);

			RagePixelTexel texel = WorldToTexelCoords(spritesheetTexture, ragePixelSprite.transform, mouseWorldPosition);
			int spriteWidth = ragePixelSprite.GetCurrentRow().pixelSizeX;
			int spriteHeight = ragePixelSprite.GetCurrentRow().pixelSizeY;

			if(texel.X >= 0 && texel.X < spriteWidth &&
                texel.Y >= 0 && texel.Y < spriteHeight)
			{
				if(Event.current.type == EventType.mouseDown)
				{
					if(Event.current.button == 0)
					{
						if(selection == null)
						{
							selectionActive = false;
							selectionStart = texel;
							selection = new RagePixelTexelRect(texel.X, texel.Y, texel.X, texel.Y);
						}
						else
						{
							if(texel.X < frontBufferPosition.X || texel.Y < frontBufferPosition.Y || texel.X > frontBufferPosition.X + selection.Width() || texel.Y > frontBufferPosition.Y + selection.Height())
							{
								selectionActive = false;
								selectionStart = texel;
								selection = new RagePixelTexelRect(texel.X, texel.Y, texel.X, texel.Y);
							}
							else
							{
								frontBufferDragStartMousePosition = texel;
								frontBufferDragStartPosition = frontBufferPosition;
							}
						}
					}
				}
				if(Event.current.type == EventType.mouseDrag && Event.current.button == 0)
				{
					if(selectionActive)
					{
						RagePixelTexel movement = new RagePixelTexel(texel.X - frontBufferDragStartMousePosition.X, texel.Y - frontBufferDragStartMousePosition.Y);
						frontBufferPosition = new RagePixelTexel(frontBufferDragStartPosition.X + movement.X, frontBufferDragStartPosition.Y + movement.Y);

						Rect spriteUV = ragePixelSprite.GetCurrentCell().uv;

						PasteBitmapToSpritesheet(new RagePixelTexel(0, 0), spriteUV, backBuffer);
						PasteBitmapToSpritesheetAlpha(frontBufferPosition, spriteUV, frontBuffer);

						spritesheetTexture.Apply();
					}
					else
					{
						selection = new RagePixelTexelRect(selectionStart.X, selectionStart.Y, texel.X, texel.Y);
					}
				}
				if(Event.current.type == EventType.mouseUp && !selectionActive)
				{
					if(selection != null && selection.Width() > 1 || selection.Height() > 1)
					{
						SavePaintUndo();

						Rect spriteUV = ragePixelSprite.GetCurrentCell().uv;

						frontBuffer = GrabRectFromSpritesheet(selection);
						CutRectInSpritesheet(selection, spriteUV);
						backBuffer = GrabSprite(spriteUV);

						frontBufferPosition = new RagePixelTexel(selection.X, selection.Y);
						frontBufferDragStartPosition = new RagePixelTexel(selection.X, selection.Y);

						PasteBitmapToSpritesheetAlpha(frontBufferPosition, spriteUV, frontBuffer);

						selectionActive = true;
						spritesheetTexture.Apply();
					}
					else
					{
						selection = null;
						selectionActive = false;
					}
				}
				if(selectionActive && Event.current.type == EventType.mouseUp)
				{
					spritesheetTexture.Apply();
					atlasTextureIsDirty = true;
				}
			}
			else
			{
				if(Event.current.type != EventType.mouseDrag && Event.current.button == 0) // left click outside the sprite
				{
					mode = Mode.Default;
				}

			}
			if(Event.current.type != EventType.mouseUp && (Event.current.button == 0 || Event.current.button == 1))
			{
				Event.current.Use();
			}
		}
	}

	public void HandleModeResize()
	{
		bool isMouseUp = false;
		if(Event.current.type == EventType.MouseUp)
		{
			isMouseUp = true;
		}
                    
		Vector2 newSize = Handles.FreeMoveHandle(ragePixelSprite.transform.position + new Vector3(ragePixelSprite.GetCurrentRow().newPixelSizeX, ragePixelSprite.GetCurrentRow().newPixelSizeY, 0f) + GetPivotOffset(), Quaternion.identity, sceneCamera.orthographicSize * handleSize, new Vector3(1f, 1f, 0f), Handles.CircleCap) - ragePixelSprite.transform.position - GetPivotOffset();
		newSize.x = Mathf.Max(newSize.x, 1f);
		newSize.y = Mathf.Max(newSize.y, 1f);
                    
		ragePixelSprite.GetCurrentRow().newPixelSizeX = Mathf.Clamp(Mathf.RoundToInt(newSize.x), 1, 2048);
		ragePixelSprite.GetCurrentRow().newPixelSizeY = Mathf.Clamp(Mathf.RoundToInt(newSize.y), 1, 2048);

		if(ragePixelSprite.GetCurrentRow().pixelSizeX != Mathf.RoundToInt(newSize.x) || ragePixelSprite.GetCurrentRow().pixelSizeY != Mathf.RoundToInt(newSize.y))
		{
			if(isMouseUp)
			{
				bool userOK = false;
				if(newSize.x < ragePixelSprite.GetCurrentRow().pixelSizeX || newSize.y < ragePixelSprite.GetCurrentRow().pixelSizeY)
				{
					userOK = EditorUtility.DisplayDialog("Resize", "Sprite bitmap will be cropped to smaller size (no undo).\nAre you sure?", "OK", "Cancel");
				}
				else
				{
					userOK = true;
				}

				if(userOK)
				{
					ragePixelSprite.GetCurrentRow().ClearUndoHistory();

					ragePixelSprite.pixelSizeX = ragePixelSprite.GetCurrentRow().newPixelSizeX;
					ragePixelSprite.pixelSizeY = ragePixelSprite.GetCurrentRow().newPixelSizeY;

					bool isGrowing =
                        ragePixelSprite.GetCurrentRow().newPixelSizeX > ragePixelSprite.GetCurrentRow().pixelSizeX ||
                        ragePixelSprite.GetCurrentRow().newPixelSizeY > ragePixelSprite.GetCurrentRow().pixelSizeY;

					spriteSheetGUI.isDirty = true;
					animStripGUI.isDirty = true;
					ragePixelSprite.meshIsDirty = true;

					RagePixelUtil.RebuildAtlas(ragePixelSprite.spriteSheet, isGrowing, "resize");
				}
			}
		}
		
		if(Event.current.button == 0 || Event.current.button == 1)
		{
			Event.current.Use();
		}
	}

	public void SceneGUIInit()
	{
		GUI.backgroundColor = new Color(1f, 1f, 1f, 1f);
		GUI.color = new Color(1f, 1f, 1f, 1f);

		if(!showGrid9Gizmo)
		{
			EditorUtility.SetSelectedWireframeHidden(meshRenderer, true);
		}
		else
		{
			EditorUtility.SetSelectedWireframeHidden(meshRenderer, false);
		}

		if(mode != Mode.Default && mode != Mode.Resize || animStripGUI.visible)
		{
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		}
        
		if(Event.current.type == EventType.mouseUp)
		{
			ragePixelSprite.SnapToIntegerPosition();
		}

		Tools.pivotMode = PivotMode.Pivot;
	}

	public void HandleColorPickerGUI()
	{
		if(paintColorPickerGUI.gizmoVisible)
		{
			if(GUI.Button(paintColorPickerGUI.gizmoBounds, paintColorPickerGUI.colorGizmoTexture) && !replaceColorPickerGUI.visible)
			{
				paintColorPickerGUI.visible = !paintColorPickerGUI.visible;
			}

			float left = paintColorPickerGUI.visible ? paintColorPickerGUI.bounds.xMax : paintColorPickerGUI.gizmoBounds.xMax;

			if(GUI.Button(new Rect(left, replaceColorPickerGUI.bounds.yMin, defaultSceneButtonWidth, defaultSceneButtonWidth), RagePixelGUIIcons.Replace))
			{
				replaceColorPickerGUI.visible = true;
				paintColorPickerGUI.visible = false;
				replaceColorPickerGUI.selectedColor = paintColorPickerGUI.selectedColor;
				colorReplaceBuffer = spritesheetTexture.GetPixels32();
			}
                        
			if(paintColorPickerGUI.visible)
			{
				if(paintColorPickerGUI.HandleGUIEvent(Event.current))
				{
					if(Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseUp)
					{
						paletteGUI.currentIndex = -1;
					}
				}

				GUI.DrawTexture(paintColorPickerGUI.bounds, paintColorPickerGUI.colorPickerTexture);
			}
		}

		if(replaceColorPickerGUI.visible)
		{
			if(GUI.Button(replaceColorPickerGUI.gizmoBounds, replaceColorPickerGUI.colorGizmoTexture))
			{
				//noop
			}
			if(GUI.Button(new Rect(replaceColorPickerGUI.bounds.xMax + 5, replaceColorPickerGUI.bounds.yMin, 102, 16), "Apply to sprite"))
			{
				foreach(RagePixelCell cell in ragePixelSprite.GetCurrentRow().cells)
				{
					ragePixelSprite.spriteSheet.saveUndo(colorReplaceBuffer, cell);
				}

				paintColorPickerGUI.selectedColor = ragePixelSprite.spriteSheet.replaceColor(colorReplaceBuffer, paintColorPickerGUI.selectedColor, replaceColorPickerGUI.selectedColor, ragePixelSprite.GetCurrentRow());
				replaceColorPickerGUI.selectedColor = paintColorPickerGUI.selectedColor;
				spriteSheetGUI.isDirty = true;
				animStripGUI.isDirty = true;
				saveTexture();
				ragePixelSprite.refreshMesh();
				EditorUtility.SetDirty(ragePixelSprite);
				replaceColorPickerGUI.visible = false;
			}
			if(GUI.Button(new Rect(replaceColorPickerGUI.bounds.xMax + 5, replaceColorPickerGUI.bounds.yMin + 20, 102, 16), "Apply to atlas"))
			{
				foreach(RagePixelRow row in ragePixelSprite.spriteSheet.rows)
				{
					foreach(RagePixelCell cell in row.cells)
					{
						ragePixelSprite.spriteSheet.saveUndo(colorReplaceBuffer, cell);
					}
				}
				ragePixelSprite.spriteSheet.replaceColor(colorReplaceBuffer, paintColorPickerGUI.selectedColor, replaceColorPickerGUI.selectedColor);
				paintColorPickerGUI.selectedColor = replaceColorPickerGUI.selectedColor;
				spriteSheetGUI.isDirty = true;
				animStripGUI.isDirty = true;
				saveTexture();
				ragePixelSprite.refreshMesh();
				EditorUtility.SetDirty(ragePixelSprite);
				replaceColorPickerGUI.visible = false;
			}
			if(GUI.Button(new Rect(replaceColorPickerGUI.bounds.xMax + 5, replaceColorPickerGUI.bounds.yMin + 40, 102, 16), "Cancel"))
			{
				spriteSheetGUI.isDirty = true;
				animStripGUI.isDirty = true;
				spritesheetTexture.SetPixels32(colorReplaceBuffer);
				spritesheetTexture.Apply();
				saveTexture();
				EditorUtility.SetDirty(ragePixelSprite);
				replaceColorPickerGUI.visible = false;
			}
			if(replaceColorPickerGUI.visible)
			{
				if(replaceColorPickerGUI.HandleGUIEvent(Event.current))
				{
					ragePixelSprite.spriteSheet.replaceColor(colorReplaceBuffer, paintColorPickerGUI.selectedColor, replaceColorPickerGUI.selectedColor, ragePixelSprite.GetCurrentRow());
					if(Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseUp)
					{
						paletteGUI.currentIndex = -1;
					}
				}
				GUI.DrawTexture(replaceColorPickerGUI.bounds, replaceColorPickerGUI.colorPickerTexture);
			}
		}
	}

	public void HandlePaintGUI()
	{
		int guiPosX = 5;
		int guiPosY = (int)(scenePixelHeight / 2f - defaultSceneButtonWidth * 5f * 0.5f);

		GUI.color = GetSceneButtonColor(mode == Mode.Default);
		if(GUI.Button(new Rect(guiPosX, guiPosY, defaultSceneButtonWidth, defaultSceneButtonWidth), RagePixelGUIIcons.Cursor))
		{
			selection = null;
			selectionActive = false;
			paintColorPickerGUI.gizmoVisible = false;
			mode = Mode.Default;
			Tools.current = Tool.Move;
		}

		GUI.color = GetSceneButtonColor(mode == Mode.Pen);
		if(GUI.Button(new Rect(guiPosX, guiPosY += defaultSceneButtonHeight, defaultSceneButtonWidth, defaultSceneButtonWidth), RagePixelGUIIcons.Pen))
		{
			selection = null;
			selectionActive = false;
			paintColorPickerGUI.gizmoVisible = true;
			mode = Mode.Pen;
			Tools.current = Tool.None;
		}
		if(mode == Mode.Pen)
		{

			GUI.color = GetSceneButtonColor(brushType == BrushType.Brush1);
			if(GUI.Button(new Rect(guiPosX + defaultSceneButtonWidth + 2, guiPosY, 9, 8), ""))
			{
				brushType = BrushType.Brush1;
			}
            
			GUI.color = GetSceneButtonColor(brushType == BrushType.Brush3);
			if(GUI.Button(new Rect(guiPosX + defaultSceneButtonWidth + 2, guiPosY + 10, 12, 10), ""))
			{
				brushType = BrushType.Brush3;
			}
			GUI.color = GetSceneButtonColor(brushType == BrushType.Brush5);
			if(GUI.Button(new Rect(guiPosX + defaultSceneButtonWidth + 2, guiPosY + 20, 15, 12), ""))
			{
				brushType = BrushType.Brush5;
			}
		}

		GUI.color = GetSceneButtonColor(mode == Mode.Fill);
		if(GUI.Button(new Rect(guiPosX, guiPosY += defaultSceneButtonWidth, defaultSceneButtonWidth, defaultSceneButtonWidth), RagePixelGUIIcons.Fill))
		{
			selection = null;
			selectionActive = false;
			paintColorPickerGUI.gizmoVisible = true;
			mode = Mode.Fill;
			Tools.current = Tool.None;
		}

		GUI.color = GetSceneButtonColor(mode == Mode.Select);
		if(ragePixelSprite.mode == RagePixelSprite.Mode.Grid9)
		{
			GUI.color = new Color(1f, 1f, 1f, 0.1f);
		}
		if(GUI.Button(new Rect(guiPosX, guiPosY += defaultSceneButtonWidth, defaultSceneButtonWidth, defaultSceneButtonWidth), RagePixelGUIIcons.Select) && ragePixelSprite.mode != RagePixelSprite.Mode.Grid9)
		{
			selection = null;
			selectionActive = false;
			mode = Mode.Select;
			Tools.current = Tool.None;
		}

		GUI.color = GetSceneButtonColor(mode == Mode.Resize);
		if(ragePixelSprite.mode == RagePixelSprite.Mode.Grid9)
		{
			GUI.color = new Color(1f, 1f, 1f, 0.1f);
		}
		if(GUI.Button(new Rect(guiPosX, guiPosY += defaultSceneButtonWidth, defaultSceneButtonWidth, defaultSceneButtonWidth), RagePixelGUIIcons.Resize) && ragePixelSprite.mode != RagePixelSprite.Mode.Grid9)
		{
			ragePixelSprite.SnapToScale();
			selection = null;
			selectionActive = false;
			mode = Mode.Resize;
			Tools.current = Tool.None;
		}
	}

	public void HandlePaletteGUI()
	{
		if(mode != Mode.Default && !replaceColorPickerGUI.visible)
		{
			int guiPosX = defaultSceneButtonWidth + 5;
			if(paintColorPickerGUI.visible)
			{
				guiPosX += paintColorPickerGUI.pixelWidth;
			}
			if(replaceColorPickerGUI.visible)
			{
				guiPosX += replaceColorPickerGUI.pixelWidth + 145 + 5;
			}

			GUI.color = Color.white;
			paletteGUI.positionX = guiPosX + defaultSceneButtonWidth + 2;
			paletteGUI.maxWidth = scenePixelWidth - guiPosX - 130;
			paletteGUI.positionY = 5;

			GUI.DrawTexture(paletteGUI.bounds, paletteGUI.paletteTexture);

			if(paletteGUI.HandleGUIEvent(Event.current))
			{
				if(paletteGUI.currentIndex >= 0)
				{
					paintColorPickerGUI.selectedColor = RagePixelUtil.Settings.palette[paletteGUI.currentIndex];
					Repaint();
				}
			}
		}
	}

	public void HandleAnimationGUI()
	{
		int guiPosX = 5;
        
		if(animationStripEnabled)
		{
			GUI.color = GetSceneButtonColor(animStripGUI.visible);
			if(GUI.Button(new Rect(guiPosX, scenePixelHeight - 40, defaultSceneButtonWidth, defaultSceneButtonWidth), RagePixelGUIIcons.Animation))
			{
				animStripGUI.visible = !animStripGUI.visible;
				animStripGUI.isDirty = true;
			}

			if(animStripGUI.visible)
			{
				GUI.color = Color.white;
				animStripGUI.maxWidth = scenePixelWidth - defaultSceneButtonWidth * 3 - 20 - 20;
				animStripGUI.positionY = scenePixelHeight - 8 - animStripGUI.pixelHeight;

				GUI.DrawTexture(animStripGUI.bounds, animStripGUI.animStripTexture);

				GUI.color = RagePixelGUIIcons.greenButtonColor;
				if(GUI.Button(new Rect(scenePixelWidth - (defaultSceneButtonWidth + 6f) * 2 - 5, scenePixelHeight - 40, defaultSceneButtonWidth + 6f, defaultSceneButtonHeight), "NEW"))
				{
					int index = ragePixelSprite.GetCurrentRow().GetIndex(ragePixelSprite.currentCellKey) + 1;
					RagePixelCell cell = ragePixelSprite.GetCurrentRow().InsertCell(index, RagePixelUtil.RandomKey());
					ragePixelSprite.currentCellKey = cell.key;
					RagePixelUtil.RebuildAtlas(ragePixelSprite.spriteSheet, true, "AddCell");
					atlasTextureIsDirty = true;
					spriteSheetGUI.isDirty = true;
					animStripGUI.isDirty = true;
				}
				GUI.color = RagePixelGUIIcons.redButtonColor;
				if(GUI.Button(new Rect(scenePixelWidth - (defaultSceneButtonWidth + 6f) * 1 - 5, scenePixelHeight - 40, defaultSceneButtonWidth + 6f, defaultSceneButtonHeight), "DEL"))
				{
					if(ragePixelSprite.GetCurrentRow().cells.Length > 1)
					{
						if(EditorUtility.DisplayDialog("Delete selected frame (no undo)?", "Are you sure?", "Delete", "Cancel"))
						{
							int index = ragePixelSprite.GetCurrentRow().GetIndex(ragePixelSprite.currentCellKey);
							ragePixelSprite.GetCurrentRow().RemoveCellByKey(ragePixelSprite.currentCellKey);
							RagePixelUtil.RebuildAtlas(ragePixelSprite.spriteSheet, false, "DeleteCell");
							ragePixelSprite.currentCellKey = ragePixelSprite.GetCurrentRow().cells[Mathf.Clamp(index, 0, ragePixelSprite.GetCurrentRow().cells.Length - 1)].key;
							atlasTextureIsDirty = true;
							spriteSheetGUI.isDirty = true;
							animStripGUI.isDirty = true;
						}
					}
					else
					{
						EditorUtility.DisplayDialog("Cannot delete", "Cannot delete the last frame.", "OK");
					}
				}

				if(animStripGUI.HandleGUIEvent(Event.current))
				{
					ragePixelSprite.currentCellKey = animStripGUI.currentCellKey;
					ragePixelSprite.refreshMesh();
				}
				else
				{
					if(Event.current.type == EventType.mouseDown && mode == Mode.Default && Event.current.button == 0)
					{
						animStripGUI.visible = false;
						Event.current.Use();
					}
				}
			}
		}
	}

	public void HandleKeyboard()
	{
		if (Event.current.type == EventType.keyDown)
		{
			if(Event.current.keyCode == KeyCode.Alpha1)
			{
				brushType = BrushType.Brush1;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.Alpha2)
			{
				brushType = BrushType.Brush3;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.Alpha3)
			{
				brushType = BrushType.Brush5;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.RightBracket)
			{
				brushType = (BrushType)((int)brushType < 2 ? (int)brushType + 1 : 0);
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.LeftBracket)
			{
				brushType = (BrushType)((int)brushType > 0 ? (int)brushType - 1 : 2);
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.RightArrow && (Event.current.control || Event.current.command))
			{
				selection = null;
				selectionActive = false;
				ragePixelSprite.shiftCell(1, true);
				ragePixelSprite.refreshMesh();
				animStripGUI.isDirty = true;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.LeftArrow && (Event.current.control || Event.current.command))
			{
				selection = null;
				selectionActive = false;
				ragePixelSprite.shiftCell(-1, true);
				ragePixelSprite.refreshMesh();
				animStripGUI.isDirty = true;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.Z && (Event.current.control || Event.current.command) && Event.current.alt)
			{
				DoPaintUndo();
				animStripGUI.isDirty = true;
				spriteSheetGUI.isDirty = true;
				atlasTextureIsDirty = true;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.D && (Event.current.control || Event.current.command) && Event.current.alt && mode == Mode.Select)
			{
				if (selectionActive)
				{
					backBuffer.PasteBitmap(selection.X, selection.Y, frontBuffer);
					animStripGUI.isDirty = true;
					spriteSheetGUI.isDirty = true;
					atlasTextureIsDirty = true;
				}
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.X && (Event.current.control || Event.current.command) && Event.current.alt && mode == Mode.Select)
			{
				SavePaintUndo();
				RagePixelUtil.Settings.clipboard = new RagePixelBitmap(frontBuffer.pixels, frontBuffer.Width(), frontBuffer.Height());
				selectionActive = false;
				Rect currentUV = ragePixelSprite.GetCurrentCell().uv;
				Rect selectionUV = new Rect(
	                currentUV.xMin + (float)selection.X / (float)spritesheetTexture.width,
	                currentUV.yMin + (float)selection.Y / (float)spritesheetTexture.height,
	                (float)(selection.X2 - selection.X + 1) / (float)spritesheetTexture.width,
	                (float)(selection.Y2 - selection.Y + 1) / (float)spritesheetTexture.height
	                );
				RagePixelUtil.clearPixels(spritesheetTexture, selectionUV);
				spritesheetTexture.Apply();
				atlasTextureIsDirty = true;
	
				selection = null;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.C && (Event.current.control || Event.current.command) && Event.current.alt && mode == Mode.Select)
			{
				RagePixelUtil.Settings.clipboard = new RagePixelBitmap(frontBuffer.pixels, frontBuffer.Width(), frontBuffer.Height());
				selection = null;
				selectionActive = false;
				Event.current.Use();
			}
			if(Event.current.keyCode == KeyCode.V && (Event.current.control || Event.current.command) && Event.current.alt)
			{
				if(RagePixelUtil.Settings.clipboard != null)
				{
					mode = Mode.Select;
	
					SavePaintUndo();
	
					Rect spriteUV = ragePixelSprite.GetCurrentCell().uv;
	
					selection = new RagePixelTexelRect(
	                    0,
	                    0,
	                    Mathf.Min(RagePixelUtil.Settings.clipboard.Width(), ragePixelSprite.GetCurrentRow().pixelSizeX),
	                    Mathf.Min(RagePixelUtil.Settings.clipboard.Height(), ragePixelSprite.GetCurrentRow().pixelSizeY)
	                    );
	
					backBuffer = GrabSprite(spriteUV);
					frontBuffer = RagePixelUtil.Settings.clipboard;
	
					frontBufferPosition = new RagePixelTexel(0, 0);
					frontBufferDragStartPosition = new RagePixelTexel(0, 0);
	
					PasteBitmapToSpritesheetAlpha(frontBufferPosition, spriteUV, frontBuffer);
	
					selectionActive = true;
					spritesheetTexture.Apply();
	
					Event.current.Use();
				}
			}
		}
	}

	public void HandleCameraWarnings()
	{
		int guiPosY = 0;
		int guiPosX = 0;

		if(RagePixelUtil.Settings.showCameraWarnings)
		{
            
			if(!sceneCamera.isOrthoGraphic || !SceneCameraFacingCorrectly())
			{
                
				guiPosY = 5;
				if(!sceneCamera.isOrthoGraphic)
				{
					guiPosX = (int)sceneCamera.pixelWidth / 2 - 90;
					GUI.color = new Color(1f, 0.33f, 0.33f);
					GUI.Label(new Rect(guiPosX, guiPosY, sceneCamera.pixelWidth, 20), "WARNING: sceneview camera is perspective");
					guiPosY += 15;
				}
				if(!SceneCameraFacingCorrectly())
				{
					guiPosX = (int)sceneCamera.pixelWidth / 2 - 105;
					GUI.color = new Color(1f, 0.33f, 0.33f);
					GUI.Label(new Rect(guiPosX, guiPosY, sceneCamera.pixelWidth, 20), "WARNING: Sceneview camera orientation != BACK");
				}

				guiPosX = (int)sceneCamera.pixelWidth - 180;
				guiPosY = 42;
				GUI.Label(new Rect(guiPosX, guiPosY, sceneCamera.pixelWidth, 20), "Right-click -->");
				GUI.color = Color.white;
			}
		}
	}

	public void OnSelected()
	{
		if(ragePixelSprite.spriteSheet == null && !RagePixelUtil.Settings.initialSpritesheetGenerated)
		{
			InitializeEmptyProject();
		}

		mode = Mode.Default;
		Tools.current = Tool.Move;
	}

	public void InitializeEmptyProject()
	{
		ragePixelSprite.transform.localScale = new Vector3(1f, 1f, 1f);
		List<UnityEngine.Object> assets = RagePixelUtil.allAssets;

		int count = 0;
		foreach(UnityEngine.Object asset in assets)
		{
			if(asset is RagePixelSpriteSheet)
			{
				if(!asset.name.Equals("BasicFont"))
				{
					count++;
				}
			}
		}

		if(count == 0)
		{
			if(EditorUtility.DisplayDialog("RagePixel Spritesheet", "Create new RagePixel spritesheet automatically?", "Create", "Cancel"))
			{
				ragePixelSprite.spriteSheet = RagePixelUtil.CreateNewSpritesheet();
				ragePixelSprite.currentRowKey = ragePixelSprite.spriteSheet.rows[0].key;
				ragePixelSprite.currentCellKey = ragePixelSprite.GetCurrentRow().cells[0].key;
				ragePixelSprite.refreshMesh();
				RagePixelUtil.Settings.initialSpritesheetGenerated = true;
			}

			if(Camera.mainCamera != null)
			{
				if(Camera.mainCamera.GetComponent(typeof(RagePixelCamera)) == null)
				{
					if(EditorUtility.DisplayDialog("RagePixel Camera", "Setup RagePixel camera automatically?\nCamera resolution = 1024x768\nPixel size = 2.", "Do it", "Cancel"))
					{
						RagePixelCamera ragecam = Camera.mainCamera.gameObject.AddComponent(typeof(RagePixelCamera)) as RagePixelCamera;
						ragecam.resolutionPixelWidth = (int)1024;
						ragecam.resolutionPixelHeight = (int)768;

						ragecam.pixelSize = 2;
						RagePixelUtil.ResetCamera(ragecam);

						ragePixelSprite.transform.position = ragecam.transform.position * 0.2f;
						ragePixelSprite.transform.position = new Vector3(ragePixelSprite.transform.position.x, ragePixelSprite.transform.position.y, 0f);
					}
				}
			}

		}
		else
		{
			/*
            ragePixelText.spriteSheet = sheet;
            ragePixelText.currentRowKey = ragePixelText.spriteSheet.rows[0].key;
            ragePixelText.currentCellKey = ragePixelText.GetCurrentRow().cells[0].key;
            ragePixelText.refreshMesh();
            */
		}
	}

	public void DrawGizmos()
	{
		DrawSpriteBoundsGizmo();

		if(showGrid9Gizmo)
		{
			DrawGrid9Gizmo();
		}
	}

	public void DrawGrid9Gizmo()
	{
		Handles.color = new Color(0f, 0f, 0f, 0.75f);
		Handles.DrawLine(
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(ragePixelSprite.grid9Left, -3f, 0f))
                ),
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(ragePixelSprite.grid9Left, ragePixelSprite.pixelSizeY + 3f, 0f))
                )
            );
		Handles.DrawLine(
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(ragePixelSprite.pixelSizeX - ragePixelSprite.grid9Right, -3f, 0f))
                ),
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(ragePixelSprite.pixelSizeX - ragePixelSprite.grid9Right, ragePixelSprite.pixelSizeY + 3f, 0f))
                )
            );
		Handles.DrawLine(
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(-3f, ragePixelSprite.grid9Bottom, 0f))
                ),
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(ragePixelSprite.pixelSizeX + 3f, ragePixelSprite.grid9Bottom, 0f))
                )
            );
		Handles.DrawLine(
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(-3f, ragePixelSprite.pixelSizeY - ragePixelSprite.grid9Top, 0f))
                ),
            worldToSceneScreenPoint(
                ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(ragePixelSprite.pixelSizeX + 3f, ragePixelSprite.pixelSizeY - ragePixelSprite.grid9Top, 0f))
                )
            );
		Handles.color = new Color(1f, 1f, 1f, 1f);
	}

	public void DrawSpriteBoundsGizmo()
	{
		Vector3 offset = new Vector3();
		Color spriteGizmoCol = Color.white;

		switch(mode)
		{
		case (Mode.Resize):
			spriteGizmoCol = new Color(0.7f, 1f, 0.7f, 0.3f);
			offset.x = ragePixelSprite.GetCurrentRow().newPixelSizeX;
			offset.y = ragePixelSprite.GetCurrentRow().newPixelSizeY;

			break;
		case (Mode.Default):
		case (Mode.Fill):
		case (Mode.Pen):
		case (Mode.Scale):
		case (Mode.Select):
			DrawSelectionBox();

			spriteGizmoCol = new Color(1f, 1f, 1f, 0.2f);
			offset.x = ragePixelSprite.pixelSizeX;
			offset.y = ragePixelSprite.pixelSizeY;
			break;
		}
                
		Vector3[] spriteGizmoVerts = new Vector3[4];

		spriteGizmoVerts[0] = worldToSceneScreenPoint(ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(0f, 0f, 0f)));
		spriteGizmoVerts[1] = worldToSceneScreenPoint(ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(0f, offset.y, 0f)));
		spriteGizmoVerts[2] = worldToSceneScreenPoint(ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(offset.x, offset.y, 0f)));
		spriteGizmoVerts[3] = worldToSceneScreenPoint(ragePixelSprite.transform.TransformPoint(GetPivotOffset() + new Vector3(offset.x, 0f, 0f)));

		Handles.DrawSolidRectangleWithOutline(spriteGizmoVerts, new Color(0f, 0f, 0f, 0f), spriteGizmoCol);
	}

	public void cancelColorReplacing()
	{
		spriteSheetGUI.spriteSheetTexture.SetPixels32(colorReplaceBuffer);
		spriteSheetGUI.spriteSheetTexture.Apply();
		saveTexture();
	}

	public void SavePaintUndo()
	{
		if(!paintUndoSaved)
		{
			paintUndoSaved = true;
			ragePixelSprite.spriteSheet.saveUndo(ragePixelSprite.GetCurrentCell());
		}
	}

	public void DoPaintUndo()
	{
		paintUndoSaved = false;
		ragePixelSprite.spriteSheet.DoUndo(ragePixelSprite.GetCurrentCell());
		EditorUtility.SetDirty(ragePixelSprite);
	}

	public void DrawSelectionBox()
	{
		if(selection != null)
		{
			Vector3[] verts = new Vector3[4];
			if(!selectionActive)
			{
				verts[0] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(selection.X, selection.Y)));
				verts[1] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(selection.X + selection.Width(), selection.Y)));
				verts[2] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(selection.X + selection.Width(), selection.Y + selection.Height())));
				verts[3] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(selection.X, selection.Y + selection.Height())));
				Handles.DrawSolidRectangleWithOutline(verts, new Color(0f, 0f, 0f, 0.04f), new Color(0f, 0f, 0f, 0.5f));
			}
			else
			{
				verts[0] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(frontBufferPosition.X, frontBufferPosition.Y)));
				verts[1] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(frontBufferPosition.X + selection.Width(), frontBufferPosition.Y)));
				verts[2] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(frontBufferPosition.X + selection.Width(), frontBufferPosition.Y + selection.Height())));
				verts[3] = worldToSceneScreenPoint(TexelCoordsToWorld(spritesheetTexture, ragePixelSprite.transform, new RagePixelTexel(frontBufferPosition.X, frontBufferPosition.Y + selection.Height())));
				Handles.DrawSolidRectangleWithOutline(verts, new Color(1f, 1f, 1f, 0.04f), new Color(1f, 1f, 1f, 0.5f));
			}
		}
	}

	public void HandleInspectorSpritesheetRemove()
	{
		if(ragePixelSprite.spriteSheet.rows.Length > 1)
		{
			if(EditorUtility.DisplayDialog("Delete selected sprite?", "Are you sure?", "Delete", "Cancel"))
			{
				int index = ragePixelSprite.spriteSheet.GetIndex(ragePixelSprite.GetCurrentRow().key);
				ragePixelSprite.spriteSheet.RemoveRowByKey(ragePixelSprite.GetCurrentRow().key);
				if(index > 0)
				{
					ragePixelSprite.currentRowKey = ragePixelSprite.spriteSheet.rows[index - 1].key;
				}
				else
				{
					ragePixelSprite.currentRowKey = ragePixelSprite.spriteSheet.rows[0].key;
				}
				RagePixelUtil.RebuildAtlas(ragePixelSprite.spriteSheet, false, "RemoveRow");
				spriteSheetGUI.isDirty = true;
				animStripGUI.isDirty = true;
			}
		}
	}

	public void RefreshPreviewLayer()
	{
		if(meshRenderer != null)
		{
			if(meshRenderer.sharedMaterials.Length == 1)
			{
				Material[] materials = new Material[2];
				Material layerMaterial = new Material(Shader.Find("RagePixel/Basic"));

				Texture2D texture = new Texture2D(ragePixelSprite.GetCurrentRow().pixelSizeX, ragePixelSprite.GetCurrentRow().pixelSizeY);
				texture.SetPixel(0, 0, new Color(0f, 1f, 0f, 1f));
				layerMaterial.SetTexture("_MainTex", texture);
				materials[0] = meshRenderer.sharedMaterials[0];
				materials[1] = layerMaterial;

				meshRenderer.sharedMaterials = materials;
			}
		}
	}

	public void FloodFill(Color oldColor, Color color, Texture2D tex, int fX, int fY, int minX, int minY, int maxX, int maxY)
	{
		tex.SetPixel(fX, fY, color);
		for(int y = Mathf.Max(fY - 1, minY); y <= Mathf.Min(fY + 1, maxY); y++)
		{
			for(int x = Mathf.Max(fX - 1, minX); x <= Mathf.Min(fX + 1, maxX); x++)
			{
				if(x == fX || y == fY)
				{
					if(tex.GetPixel(x, y).Equals(oldColor))
					{
						FloodFill(oldColor, color, tex, x, y, minX, minY, maxX, maxY);
					}
				}
			}
		}
	}
        
	public Color[] getScaledImage(Texture2D src, int width, int height, Color bgColor)
	{
		Color[] pixels = new Color[width * height];

		float ratioX = (float)src.width / (float)width;
		float ratioY = (float)src.height / (float)height;

		for(int y = 0; y < height; y++)
		{
			for(int x = 0; x < width; x++)
			{
				int srcX = Mathf.Clamp(Mathf.FloorToInt((float)x * ratioX), 0, src.width - 1);
				int srcY = Mathf.Clamp(Mathf.FloorToInt((float)y * ratioY), 0, src.height - 1);
				Color pixel = src.GetPixel(srcX, srcY);
				if(pixel.a >= 0.99f)
				{
					pixels[x + y * width] = pixel;
				}
				else
				{
					pixels[x + y * width] = pixel * pixel.a + bgColor * (1f - pixel.a);
				}
			}
		}
		return pixels;
	}

	public void ShowDebugInfo()
	{       

		if(meshFilter != null)
		{
			Mesh m = meshFilter.sharedMesh;

			if(m != null)
			{
				Handles.BeginGUI();
				for(int i = 0; i < m.vertexCount; i++)
				{
					//float screenHeight = sceneCamera.orthographicSize * 2f; 
                    
					Vector3 screenPos = worldToSceneScreenPoint(ragePixelSprite.transform.TransformPoint(m.vertices[i]));

					Rect r = new Rect(screenPos.x + 5, screenPos.y, 200, 20);
					GUI.Label(r, "POS:" + m.vertices[i].ToString());
					Rect r2 = new Rect(screenPos.x + 5, screenPos.y + 12, 200, 20);
					GUI.Label(r2, "UV: " + m.uv[i].x.ToString() + "," + m.uv[i].y.ToString());
				}
				Handles.EndGUI();
			}
			else
			{

			}
		}
	}

	public RagePixelTexel WorldToTexelCoords(Texture2D tex, Transform t, Vector3 worldPos)
	{
		RagePixelTexel coords = new RagePixelTexel();
		Vector3 localPos = t.InverseTransformPoint(worldPos) - GetPivotOffset();

		switch(ragePixelSprite.mode)
		{
		case (RagePixelSprite.Mode.Default):
			coords.X = Mathf.FloorToInt(localPos.x);
			coords.Y = Mathf.FloorToInt(localPos.y);

			if(coords.X >= 0 && coords.Y >= 0 && coords.X < ragePixelSprite.pixelSizeX && coords.Y < ragePixelSprite.pixelSizeY)
			{
				coords.X = coords.X % ragePixelSprite.GetCurrentRow().pixelSizeX;
				coords.Y = coords.Y % ragePixelSprite.GetCurrentRow().pixelSizeY;
			}
			break;
		case (RagePixelSprite.Mode.Grid9):
                
			coords.X = Mathf.FloorToInt(localPos.x);
			coords.Y = Mathf.FloorToInt(localPos.y);

			if(coords.X >= 0 && coords.Y >= 0 && coords.X < ragePixelSprite.pixelSizeX && coords.Y < ragePixelSprite.pixelSizeY)
			{
				if(coords.X < ragePixelSprite.grid9Left)
				{
					//noop
				}
				else if(coords.X >= ragePixelSprite.pixelSizeX - ragePixelSprite.grid9Right)
				{
					coords.X = ragePixelSprite.GetCurrentRow().pixelSizeX - (ragePixelSprite.pixelSizeX - coords.X);
				}
				else
				{
					coords.X = ragePixelSprite.grid9Left + (coords.X - ragePixelSprite.grid9Left) % (ragePixelSprite.GetCurrentRow().pixelSizeX - ragePixelSprite.grid9Left - ragePixelSprite.grid9Right);
				}

				if(coords.Y < ragePixelSprite.grid9Bottom)
				{
					//noop
				}
				else if(coords.Y >= ragePixelSprite.pixelSizeY - ragePixelSprite.grid9Top)
				{
					coords.Y = ragePixelSprite.GetCurrentRow().pixelSizeY - (ragePixelSprite.pixelSizeY - coords.Y);
				}
				else
				{
					coords.Y = ragePixelSprite.grid9Bottom + (coords.Y - ragePixelSprite.grid9Bottom) % (ragePixelSprite.GetCurrentRow().pixelSizeY - ragePixelSprite.grid9Top - ragePixelSprite.grid9Bottom);
				}
			}
			break;
		}
        
        
		return coords;
	}

	public Vector3 GetPivotOffset()
	{
		Vector3 pivotOffset = new Vector3();
		switch(mode)
		{
		case (Mode.Resize):
			switch(ragePixelSprite.pivotMode)
			{
			case (RagePixelSprite.PivotMode.BottomLeft):
				pivotOffset.x = 0f;
				pivotOffset.y = 0f;
				break;
			case (RagePixelSprite.PivotMode.Bottom):
				pivotOffset.x = -ragePixelSprite.GetCurrentRow().newPixelSizeX / 2f;
				pivotOffset.y = 0f;
				break;
			case (RagePixelSprite.PivotMode.Middle):
				pivotOffset.x = -ragePixelSprite.GetCurrentRow().newPixelSizeX / 2f;
				pivotOffset.y = -ragePixelSprite.GetCurrentRow().newPixelSizeY / 2f;
				break;
			}

			break;
		case (Mode.Default):
		case (Mode.Fill):
		case (Mode.Pen):
		case (Mode.Scale):
		case (Mode.Select):
			switch(ragePixelSprite.pivotMode)
			{
			case (RagePixelSprite.PivotMode.BottomLeft):
				pivotOffset.x = 0f;
				pivotOffset.y = 0f;
				break;
			case (RagePixelSprite.PivotMode.Bottom):
				pivotOffset.x = -ragePixelSprite.pixelSizeX / 2f;
				pivotOffset.y = 0f;
				break;
			case (RagePixelSprite.PivotMode.Middle):
				pivotOffset.x = -ragePixelSprite.pixelSizeX / 2f;
				pivotOffset.y = -ragePixelSprite.pixelSizeY / 2f;
				break;
			}
			break;
		}
		return pivotOffset;
	}

	public Vector3 TexelCoordsToWorld(Texture2D tex, Transform t, RagePixelTexel texel)
	{
		Vector3 v = new Vector3(texel.X, texel.Y, 0f);
		//v.Scale(new Vector3(1f/t.localScale.x,1f/t.localScale.y,1f));
		return t.TransformPoint(v + GetPivotOffset());
	}

	public Vector3 worldToSceneScreenPoint(Vector3 worldPos)
	{
		Camera sceneCamera = SceneView.lastActiveSceneView.camera;
		Vector3 screenPos = sceneCamera.WorldToScreenPoint(worldPos);
		return new Vector3(screenPos.x + 1f, -screenPos.y + sceneCamera.pixelHeight + 3f, 0f);
	}

	public Vector3 sceneScreenToWorldPoint(Vector3 sceneScreenPoint)
	{
		Camera sceneCamera = SceneView.lastActiveSceneView.camera;
		float screenHeight = sceneCamera.orthographicSize * 2f;
		float screenWidth = screenHeight * sceneCamera.aspect;

		Vector3 worldPos = new Vector3(
            (sceneScreenPoint.x / sceneCamera.pixelWidth) * screenWidth - screenWidth * 0.5f,
            ((-(sceneScreenPoint.y) / sceneCamera.pixelHeight) * screenHeight + screenHeight * 0.5f),
            0f);

		worldPos += sceneCamera.transform.position;
		worldPos.z = 0f;

		return worldPos;
	}
    
	public void saveTexture()
	{
		RagePixelUtil.SaveSpritesheetTextureToDisk(ragePixelSprite.spriteSheet);
	}

	public RagePixelBitmap GrabSprite(Rect spriteUV)
	{
		return new RagePixelBitmap(
            ragePixelSprite.spriteSheet.getImage(
                ragePixelSprite.GetCurrentRowIndex(),
                ragePixelSprite.GetCurrentCellIndex()
                ),
            (int)(spriteUV.width * spritesheetTexture.width),
            (int)(spriteUV.height * spritesheetTexture.height)
            );
	}

	public RagePixelBitmap GrabRectFromSpritesheet(RagePixelTexelRect rect)
	{
		return new RagePixelBitmap(
            ragePixelSprite.spriteSheet.getImage(
                ragePixelSprite.GetCurrentRowIndex(), ragePixelSprite.GetCurrentCellIndex(),
                rect.X,
                rect.Y,
                rect.Width(),
                rect.Height()
                ),
            rect.Width(),
            rect.Height()
            );
	}

	public void CutRectInSpritesheet(RagePixelTexelRect rect, Rect spriteUV)
	{
		for(int y = rect.Y; y <= rect.Y2; y++)
		{
			for(int x = rect.X; x <= rect.X2; x++)
			{
				spritesheetTexture.SetPixel(
                    (int)(spriteUV.x * spritesheetTexture.width) + x,
                    (int)(spriteUV.y * spritesheetTexture.height) + y,
                    new Color(0f, 0f, 0f, 0f)
                    );
			}
		}
	}

	public void PasteBitmapToSpritesheet(RagePixelTexel position, Rect spriteUV, RagePixelBitmap bitmap)
	{
		for(int y = Mathf.Max(position.Y, 0); y < position.Y + bitmap.Height() && y < (int)(spriteUV.height * spritesheetTexture.height); y++)
		{
			for(int x = Mathf.Max(position.X, 0); x < position.X + bitmap.Width() && x < (int)(spriteUV.width * spritesheetTexture.width); x++)
			{
				spritesheetTexture.SetPixel(
                    (int)(spriteUV.x * spritesheetTexture.width) + x,
                    (int)(spriteUV.y * spritesheetTexture.height) + y,
                    bitmap.GetPixel(x - position.X, y - position.Y)
                    );
			}
		}       
	}

	public void PasteBitmapToSpritesheetAlpha(RagePixelTexel position, Rect spriteUV, RagePixelBitmap bitmap)
	{
		for(int y = Mathf.Max(position.Y, 0); y < position.Y + bitmap.Height() && y < (int)(spriteUV.height * spritesheetTexture.height); y++)
		{
			for(int x = Mathf.Max(position.X, 0); x < position.X + bitmap.Width() && x < (int)(spriteUV.width * spritesheetTexture.width); x++)
			{
				Color src = bitmap.GetPixel(x - position.X, y - position.Y);
				Color trg = spritesheetTexture.GetPixel((int)(spriteUV.x * spritesheetTexture.width) + x, (int)(spriteUV.y * spritesheetTexture.height) + y);
                
				spritesheetTexture.SetPixel(
                    (int)(spriteUV.x * spritesheetTexture.width) + x,
                    (int)(spriteUV.y * spritesheetTexture.height) + y,
                    src + (1f - src.a) * trg
                    );
			}
		}
	}

	public void InvokeOnSelectedEvent()
	{
		if(!justSelected)
		{
			OnSelected();
			justSelected = true;
		}
	}

	public Camera GetSceneCamera()
	{
		Camera cam = SceneView.lastActiveSceneView.camera;
		return cam;
	}

	public int GetAtlasCellCount(RagePixelSpriteSheet[] spriteSheets, Material atlas)
	{
		int count = 0;

		foreach(RagePixelSpriteSheet sheet in spriteSheets)
		{
			if(sheet.atlas.Equals(atlas))
			{
				foreach(RagePixelRow row in sheet.rows)
				{
					count += row.cells.Length;            
				}
			}
		}

		return count;
	}
        
	public bool SceneCameraFacingCorrectly()
	{
		if(sceneCamera != null)
		{
			if(sceneCamera.transform.forward.z > 0.999999f && Mathf.Abs(sceneCamera.transform.forward.y) < 0.000001f && Mathf.Abs(sceneCamera.transform.forward.x) < 0.000001f)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			return false;
		}
	}

	public Color GetSceneButtonColor(bool active)
	{
		if(active)
		{
			return new Color(0.8f, 0.925f, 1f, 1f);
		}
		else
		{
			return new Color(1f, 1f, 1f, 0.5f);
		}
	}

	private int RandomKey()
	{
		int val = (int)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		if(val == 0)
		{
			val++;
		}
		return (int)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
	}
}
