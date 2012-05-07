using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class RagePixelSpriteSheetEditorWindow : EditorWindow
{
	public RagePixelSpriteSheet spriteSheet;
	public RagePixelSpriteEditor inspector;
	public RagePixelSprite selectedSprite;
	private bool showRangeAnimationsFoldout, showSequenceAnimationsFoldout;
	private bool showCopyAnimationsFoldout;
	private bool showImportFoldout;
	private bool showUpdateFoldout;
	private Texture2D newTexture;
	private int importSpriteWidth = 16, importSpriteHeight = 16;
	private bool importSpriteTopLeft = true;

	public enum SpriteSheetImportTarget
	{
		Selected = 0,
		NewFrame,
		NewSprite,
		SpriteSheet
	};
	
	public enum SpriteSheetUpdateTarget
	{
		SelectedSprite = 0,
		SelectedFrame,
		AllSprites
	};

	private RagePixelAnimStripGUI _animStripGUI;

	public RagePixelAnimStripGUI animStripGUI
	{
		get
		{
			if(_animStripGUI == null)
			{
				_animStripGUI = new RagePixelAnimStripGUI(spriteSheetGUI);
				if(inspector != null)
				{
					_animStripGUI.currentCellKey = inspector.animStripGUI.currentCellKey;
				}
				else
				{
					_animStripGUI.currentCellKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells[0].key;
				}
				_animStripGUI.positionX = 32 + 10;
				_animStripGUI.positionY = scenePixelHeight - _animStripGUI.thumbnailSize - 7;
				_animStripGUI.maxWidth = scenePixelWidth - 32 * 3 - 20;
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
				if(inspector != null)
				{
					_spriteSheetGUI.currentRowKey = inspector.spriteSheetGUI.currentRowKey;
				}
				else
				{
					_spriteSheetGUI.currentRowKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).key;
				}
				_spriteSheetGUI.spriteSheet = spriteSheet;
			}
			return _spriteSheetGUI;
		}
	}

	private RagePixelSpriteSheetGUI _copySpriteSheetGUI;

	public RagePixelSpriteSheetGUI copySpriteSheetGUI
	{
		get
		{
			if(_copySpriteSheetGUI == null)
			{
				_copySpriteSheetGUI = new RagePixelSpriteSheetGUI();
				if(inspector != null)
				{
					_copySpriteSheetGUI.currentRowKey = inspector.spriteSheetGUI.currentRowKey;
				}
				else
				{
					_copySpriteSheetGUI.currentRowKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).key;
				}
				_copySpriteSheetGUI.spriteSheet = spriteSheet;
			}
			return _copySpriteSheetGUI;
		}
	}

	private int scenePixelWidth
	{
		get
		{
			return (int)Screen.width;
		}
	}

	private int scenePixelHeight
	{
		get
		{
			return (int)Screen.height;
		}
	}

	public void OnGUI()
	{

		int y = 2;
		int x = 5;

		spriteSheet =
            (RagePixelSpriteSheet)EditorGUI.ObjectField(
                new Rect(x, y, Screen.width - x * 2, 16),
                "Sprite sheet",
                spriteSheet,
                typeof(RagePixelSpriteSheet),
                false
                );

		y += 20;

		if(spriteSheet != null)
		{
			if(spriteSheet != spriteSheetGUI.spriteSheet)
			{
				spriteSheetGUI.spriteSheet = spriteSheet;
				spriteSheetGUI.currentRowKey = spriteSheet.rows[0].key;
				copySpriteSheetGUI.spriteSheet = spriteSheet;
				copySpriteSheetGUI.currentRowKey = spriteSheet.rows[0].key;
				animStripGUI.currentCellKey = spriteSheet.rows[0].cells[0].key;
			}

			spriteSheetGUI.positionX = x;
			spriteSheetGUI.positionY = y;

			animStripGUI.positionX = x;
			animStripGUI.positionY = spriteSheetGUI.positionY + spriteSheetGUI.pixelHeight + 5;

			GUI.color = RagePixelGUIIcons.greenButtonColor;
			if(GUI.Button(new Rect(Screen.width - 38f * 2 - 5, spriteSheetGUI.positionY + spriteSheetGUI.pixelHeight - 32f, 38f, 32f), "NEW"))
			{
				int index = spriteSheet.GetIndex(spriteSheetGUI.currentRowKey);

				RagePixelRow row =
                    spriteSheet.AddRow(
                        RagePixelUtil.RandomKey(),
                        index + 1,
                        spriteSheet.GetRow(spriteSheetGUI.currentRowKey).pixelSizeX,
                        spriteSheet.GetRow(spriteSheetGUI.currentRowKey).pixelSizeY
                        );

				RagePixelCell cell =
                    row.InsertCell(0, RagePixelUtil.RandomKey());

				spriteSheetGUI.currentRowKey = row.key;
				animStripGUI.currentCellKey = cell.key;

				RagePixelUtil.RebuildAtlas(spriteSheet, true, "AddRow");
			}

			GUI.color = RagePixelGUIIcons.redButtonColor;

			if(GUI.Button(new Rect(Screen.width - 38f - 5, spriteSheetGUI.positionY + spriteSheetGUI.pixelHeight - 32f, 38f, 32f), "DEL"))
			{
				if(spriteSheet.rows.Length > 1)
				{
					if(EditorUtility.DisplayDialog("Delete selected sprite?", "Are you sure?", "Delete", "Cancel"))
					{
						int index = spriteSheet.GetIndex(spriteSheetGUI.currentRowKey);
						spriteSheet.RemoveRowByKey(spriteSheetGUI.currentRowKey);

						int newKey = spriteSheet.rows[Mathf.Clamp(index, 0, spriteSheet.rows.Length - 1)].key;

						if(selectedSprite != null)
						{
							if(selectedSprite.currentRowKey == spriteSheetGUI.currentRowKey)
							{
								selectedSprite.meshIsDirty = true;
								selectedSprite.currentRowKey = newKey;
								selectedSprite.pixelSizeX = selectedSprite.GetCurrentRow().pixelSizeX;
								selectedSprite.pixelSizeY = selectedSprite.GetCurrentRow().pixelSizeY;
							}
						}
						spriteSheetGUI.currentRowKey = newKey;

						animStripGUI.currentCellKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells[0].key;
						RagePixelUtil.RebuildAtlas(spriteSheet, false, "DeleteRow");

						if(inspector != null)
						{
							inspector.spriteSheetGUI.isDirty = true;
							inspector.animStripGUI.isDirty = true;
						}
					}
				}
				else
				{
					EditorUtility.DisplayDialog("Cannot delete", "Cannot delete the last sprite.", "OK");
				}
			}

			y += spriteSheetGUI.pixelHeight + animStripGUI.pixelHeight + 10;


			GUI.color = RagePixelGUIIcons.greenButtonColor;
			if(GUI.Button(new Rect(Screen.width - 38f * 2 - 5, animStripGUI.positionY + animStripGUI.pixelHeight - 32f, 38f, 32f), "NEW"))
			{
				int index = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey) + 1;
				RagePixelCell cell = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).InsertCell(index, RagePixelUtil.RandomKey());
				animStripGUI.currentCellKey = cell.key;
				RagePixelUtil.RebuildAtlas(spriteSheet, true, "AddCell");
			}
			GUI.color = RagePixelGUIIcons.redButtonColor;
			if(GUI.Button(new Rect(Screen.width - 38f - 5, animStripGUI.positionY + animStripGUI.pixelHeight - 32f, 38f, 32f), "DEL"))
			{
				if(spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length > 1)
				{
					if(EditorUtility.DisplayDialog("Delete selected animation frame?", "Are you sure?", "Delete", "Cancel"))
					{
						int index = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey);
						spriteSheet.GetRow(spriteSheetGUI.currentRowKey).RemoveCellByKey(animStripGUI.currentCellKey);

						int newKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells[Mathf.Clamp(index, 0, spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length - 1)].key;
						if(selectedSprite != null)
						{
							if(selectedSprite.currentCellKey == animStripGUI.currentCellKey)
							{
								selectedSprite.meshIsDirty = true;
								selectedSprite.currentCellKey = newKey;
								selectedSprite.pixelSizeX = selectedSprite.GetCurrentRow().pixelSizeX;
								selectedSprite.pixelSizeY = selectedSprite.GetCurrentRow().pixelSizeY;
							}
						}

						animStripGUI.currentCellKey = newKey;
						RagePixelUtil.RebuildAtlas(spriteSheet, true, "DeleteCell");

						if(inspector != null)
						{
							inspector.spriteSheetGUI.isDirty = true;
							inspector.animStripGUI.isDirty = true;
						}

					}
				}
				else
				{
					EditorUtility.DisplayDialog("Cannot delete", "Cannot delete the last animation frame.", "OK");
				}
			}
			GUI.color = Color.white;
			if(spriteSheet.GetRow(spriteSheetGUI.currentRowKey).name == null)
			{
				spriteSheet.GetRow(spriteSheetGUI.currentRowKey).name = "";
			}
			spriteSheet.GetRow(spriteSheetGUI.currentRowKey).name =
                EditorGUI.TextField(
                    new Rect(x, y, Mathf.Min(350, Screen.width - x * 2), 16), "Sprite Name", spriteSheet.GetRow(spriteSheetGUI.currentRowKey).name);
			y += 20;
			RagePixelCell selectedCell = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey);
			EditorGUI.LabelField(
                new Rect(x, y, Screen.width - x * 2, 16), "Frame Index", spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey).ToString() + 
				" (" + (spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length - 1).ToString() + ")"
				+ (selectedCell.importAssetPath == "" ? "" : (" - " + selectedCell.importAssetPath)));
			y += 20;
			
				
			spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).delay =
                EditorGUI.IntField(
                new Rect(x, y, Mathf.Min(200, Screen.width - x * 2), 16), "Frame Time", (int)spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).delay);
			y += 20;

			GUILayout.Space(y + 20);

			int rangeAnimationsFoldoutHeight = 0;
			showRangeAnimationsFoldout = EditorGUI.Foldout(new Rect(x, y, Screen.width - x * 2, 20), showRangeAnimationsFoldout, "Range animations");
			y += 20;
			if(showRangeAnimationsFoldout)
			{
				GUI.color = Color.gray;
				x = 5;
				GUI.Label(new Rect(x, y, 170, 16), "Name");
				x += 175;
				GUI.Label(new Rect(x, y, 40, 16), "Start");
				x += 45;
				GUI.Label(new Rect(x, y, 40, 16), "End");
				x += 45;
				GUI.Label(new Rect(x, y, 130, 16), "Type");

				GUI.color = Color.white;
				y += 20;
				RagePixelAnimation[] animations = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).animations;
				for(int animIndex = 0; animIndex < animations.Length; animIndex++)
				{
					if(animations[animIndex].frameMode != RagePixelSprite.FrameMode.Range)
					{
						continue;
					}
					x = 5;
					animations[animIndex].name = EditorGUI.TextField(new Rect(x, y, 170, 16), animations[animIndex].name);
					x += 175;
					animations[animIndex].startIndex = EditorGUI.IntField(new Rect(x, y, 40, 16), animations[animIndex].startIndex);
					x += 45;
					animations[animIndex].endIndex = EditorGUI.IntField(new Rect(x, y, 40, 16), animations[animIndex].endIndex);
					x += 45;
					animations[animIndex].mode = (RagePixelSprite.AnimationMode)EditorGUI.EnumPopup(new Rect(x, y, 130, 16), animations[animIndex].mode);
					x += 135;

					if(GUI.Button(new Rect(x, y, 60, 16), "Delete"))
					{
						spriteSheet.GetRow(spriteSheetGUI.currentRowKey).RemoveAnimation(animIndex);
					}

					y += 20;
					rangeAnimationsFoldoutHeight += 20;
				}
				x = 5;

				if(GUI.Button(new Rect(x, y, 50, 16), "Add"))
				{
					RagePixelAnimation anim = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).AddAnimation();
					anim.frameMode = RagePixelSprite.FrameMode.Range;
				}
				y += 20;
				rangeAnimationsFoldoutHeight += 20;

				GUILayout.Space(rangeAnimationsFoldoutHeight + 26);
				y += 6;
			}

			int sequenceAnimationsFoldoutHeight = 0;
			showSequenceAnimationsFoldout = EditorGUI.Foldout(new Rect(x, y, Screen.width - x * 2, 20), showSequenceAnimationsFoldout, "Sequence animations");
			y += 20;
			if(showSequenceAnimationsFoldout)
			{
				GUI.color = Color.gray;
				x = 5;
				GUI.Label(new Rect(x, y, 170, 16), "Name");
				x += 175;
				GUI.Label(new Rect(x, y, 40, 16), "Type");
				x += 135;
				GUI.Label(new Rect(x, y, 60, 16), "Frames");

				GUI.color = Color.white;
				y += 20;
				RagePixelAnimation[] animations = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).animations;
				for(int animIndex = 0; animIndex < animations.Length; animIndex++)
				{
					if(animations[animIndex].frameMode != RagePixelSprite.FrameMode.Sequence)
					{
						continue;
					}
					x = 5;
					animations[animIndex].name = EditorGUI.TextField(new Rect(x, y, 170, 16), animations[animIndex].name);
					x += 175;
					animations[animIndex].mode = (RagePixelSprite.AnimationMode)EditorGUI.EnumPopup(new Rect(x, y, 130, 16), animations[animIndex].mode);
					x += 135;
					//frames
					if(animations[animIndex].frames == null || animations[animIndex].frames.Length == 0)
					{
						animations[animIndex].frames = new int[]{0};
					}
					for(int i = 0; i < animations[animIndex].frames.Length; i++)
					{
						animations[animIndex].frames[i] = EditorGUI.IntField(new Rect(x, y, 24, 16), animations[animIndex].frames[i]);
						x += 28;
					}

					if(GUI.Button(new Rect(x, y, 20, 16), "-"))
					{
						//reduce length by 1
						Array.Resize(ref animations[animIndex].frames, animations[animIndex].frames.Length - 1);
					}
					x += 22;
					if(GUI.Button(new Rect(x, y, 20, 16), "+"))
					{
						//increase length by 1
						Array.Resize(ref animations[animIndex].frames, animations[animIndex].frames.Length + 1);
					}
					x += 28;

					if(GUI.Button(new Rect(x, y, 60, 16), "Delete"))
					{
						spriteSheet.GetRow(spriteSheetGUI.currentRowKey).RemoveAnimation(animIndex);
					}

					y += 20;
					sequenceAnimationsFoldoutHeight += 20;
				}
				x = 5;

				if(GUI.Button(new Rect(x, y, 50, 16), "Add"))
				{
					RagePixelAnimation anim = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).AddAnimation();
					anim.frameMode = RagePixelSprite.FrameMode.Sequence;
				}
				y += 20;
				sequenceAnimationsFoldoutHeight += 20;

				GUILayout.Space(sequenceAnimationsFoldoutHeight + 26);
				y += 6;
			}

			x = 5;

			showCopyAnimationsFoldout = EditorGUI.Foldout(new Rect(x, y, Screen.width - x * 2, 16), showCopyAnimationsFoldout, "Copy Animations");
			if(showCopyAnimationsFoldout)
			{
				y += 20;
				GUI.Label(new Rect(x, y, Screen.width - x * 2, 16), "Other Sprite");
				y += 20;
				copySpriteSheetGUI.positionX = x;
				copySpriteSheetGUI.positionY = y;
				y += copySpriteSheetGUI.pixelHeight + 10;
				GUI.enabled = spriteSheetGUI.currentRowKey != copySpriteSheetGUI.currentRowKey;
				if(GUI.Button(new Rect(x, y, 180f, 19f), "Copy From Other"))
				{
					RagePixelRow thisRow = spriteSheet.GetRow(spriteSheetGUI.currentRowKey);
					RagePixelRow otherRow = spriteSheet.GetRow(copySpriteSheetGUI.currentRowKey);
					thisRow.CopyAnimationsFrom(otherRow);
				}
				if(GUI.Button(new Rect(x + 190, y, 180f, 19f), "Copy To Other"))
				{
					RagePixelRow thisRow = spriteSheet.GetRow(spriteSheetGUI.currentRowKey);
					RagePixelRow otherRow = spriteSheet.GetRow(copySpriteSheetGUI.currentRowKey);
					otherRow.CopyAnimationsFrom(thisRow);
				}
				GUI.enabled = true;
				y += 21;
			}
			y += 20;

			showImportFoldout = EditorGUI.Foldout(new Rect(x, y, Screen.width - x * 2, 16), showImportFoldout, "Import");
			if(showImportFoldout)
			{
				GUILayout.Space(24);
				GUILayout.BeginHorizontal();

				newTexture = (Texture2D)EditorGUILayout.ObjectField(" ", newTexture, typeof(Texture2D), false);

				GUILayout.BeginVertical();
				y += 10;
				if(GUI.Button(new Rect(x + 240f, y, 180f, 19f), "Import to selected frame"))
				{
				
					if(newTexture != null)
					{
						ImportSprite(SpriteSheetImportTarget.Selected);
					}
				}
				y += 21;
				if(GUI.Button(new Rect(x + 240f, y, 180f, 19f), "Import as new frame"))
				{
					if(newTexture != null)
					{
						ImportSprite(SpriteSheetImportTarget.NewFrame);
					}
				}
				y += 21;
				if(GUI.Button(new Rect(x + 240f, y, 180f, 19f), "Import as new sprite"))
				{
					if(newTexture != null)
					{
						ImportSprite(SpriteSheetImportTarget.NewSprite);
					}
				}
				y += 21;
				if(GUI.Button(new Rect(x + 240f, y, 180f, 19f), "Import spritesheet"))
				{
					if(newTexture != null)
					{
						ImportSprite(SpriteSheetImportTarget.SpriteSheet);
					}
				}
				//sprite width, sprite height
				y += 21;
				importSpriteWidth = EditorGUI.IntField(new Rect(x + 240f, y, 180f, 19f), "Frame Width", importSpriteWidth);
				y += 21;
				importSpriteHeight = EditorGUI.IntField(new Rect(x + 240f, y, 180f, 19f), "Frame Height", importSpriteHeight);
				y += 21;
				importSpriteTopLeft = EditorGUI.Toggle(new Rect(x + 240f, y, 180f, 19f), "First Frame at Top Left", importSpriteTopLeft);
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
			}
			
			// Update references to sprites
			y += 20;
			
			showUpdateFoldout = EditorGUI.Foldout(new Rect(x, y, Screen.width - x * 2, 16), showUpdateFoldout, "Update");
			if (showUpdateFoldout)
			{
				y += 20;
				
				GUILayout.BeginVertical();
				if(GUI.Button(new Rect(5, y, 180f, 19f), "Update Selected Frame"))
				{
					UpdateSprite(SpriteSheetUpdateTarget.SelectedFrame);
				}
				/*
				if(GUI.Button(new Rect(190, y, 180f, 19f), "Save Frame to Source"))
				{
					if(newTexture != null)
					{
						
					}
				}
				*/
				y += 21;
				if(GUI.Button(new Rect(5, y, 180f, 19f), "Update Selected Sprite"))
				{
					UpdateSprite(SpriteSheetUpdateTarget.SelectedSprite);
				}
				
				y += 21;
				if(GUI.Button(new Rect(5, y, 180f, 19f), "Update All Sprites"))
				{
					UpdateSprite(SpriteSheetUpdateTarget.AllSprites);
				}
				GUILayout.EndVertical();
			}

			int oldRowKey = spriteSheetGUI.currentRowKey;
			animStripGUI.HandleGUIEvent(Event.current);
			spriteSheetGUI.HandleGUIEvent(Event.current);

			if(oldRowKey != spriteSheetGUI.currentRowKey)
			{
				animStripGUI.currentCellKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells[0].key;
			}

			if(animStripGUI.isDirty ||
			   spriteSheetGUI.isDirty)
			{
				Repaint();
				if(inspector != null)
				{
					inspector.spriteSheetGUI.isDirty = true;
					inspector.animStripGUI.isDirty = true;
					inspector.Repaint();
				}
				if(selectedSprite != null)
				{
					selectedSprite.meshIsDirty = true;
					selectedSprite.refreshMesh();
				}
			}

			spriteSheetGUI.maxWidth = scenePixelWidth - 38 * 2 - 10 - spriteSheetGUI.positionX;
			animStripGUI.maxWidth = scenePixelWidth - 38 * 2 - 10 - animStripGUI.positionX;
			EditorGUI.DrawPreviewTexture(spriteSheetGUI.bounds, spriteSheetGUI.spriteSheetTexture);
			EditorGUI.DrawPreviewTexture(animStripGUI.bounds, animStripGUI.animStripTexture);

			if(showCopyAnimationsFoldout)
			{
				copySpriteSheetGUI.HandleGUIEvent(Event.current);
				copySpriteSheetGUI.maxWidth = scenePixelWidth - 38 * 2 - 10 - copySpriteSheetGUI.positionX;
				EditorGUI.DrawPreviewTexture(copySpriteSheetGUI.bounds, copySpriteSheetGUI.spriteSheetTexture);
			}
		}

		if(GUI.changed)
		{
			EditorUtility.SetDirty(spriteSheet);
		}
	}


	public void Update()
	{
		if(animStripGUI.isDirty || spriteSheetGUI.isDirty)
		{
			Repaint();
		}
	}

	public void OnDestroy()
	{
		spriteSheetGUI.CleanExit();
		animStripGUI.CleanExit();
	}
	
	public void UpdateSprite(SpriteSheetUpdateTarget target)
	{
		Texture2D spritesheetTexture = spriteSheet.atlas.GetTexture("_MainTex") as Texture2D;
		
		switch(target)
		{
			case SpriteSheetUpdateTarget.SelectedSprite:
			{
				RagePixelRow row = spriteSheet.GetRow(spriteSheetGUI.currentRowKey);
			
				foreach (RagePixelCell cell in row.cells)
				{
					UpdateCell(cell, spritesheetTexture);
				}
				break;
			}
			case SpriteSheetUpdateTarget.SelectedFrame:
			{
				UpdateCell(spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey), spritesheetTexture);
				break;
			}
			case SpriteSheetUpdateTarget.AllSprites:
			{
				foreach (RagePixelRow row in spriteSheet.rows)
				{
					foreach (RagePixelCell cell in row.cells)
					{
						UpdateCell(cell, spritesheetTexture);
					}
				}
				break;
			}
		}
		
		RagePixelUtil.SaveSpritesheetTextureToDisk(spriteSheet);
		RagePixelUtil.RebuildAtlas(spriteSheet, true, "save after update");

		spriteSheetGUI.isDirty = true;
		animStripGUI.isDirty = true;

		if(inspector != null)
		{
			inspector.animStripGUI.isDirty = true;
			inspector.spriteSheetGUI.isDirty = true;
		}
	}
	
	public void UpdateCell(RagePixelCell cell, Texture2D spritesheetTexture)
	{
		if (cell.importAssetPath != "")
		{
			Texture2D textureAsset = AssetDatabase.LoadAssetAtPath(cell.importAssetPath, typeof(Texture2D)) as Texture2D;
			
			if (textureAsset)
			{	
				RagePixelUtil.CopyPixels(textureAsset, new Rect(0f, 0f, 1f, 1f), spritesheetTexture, cell.uv);
			}
			else
			{
				Debug.LogWarning("Frame has reference to '" + cell.importAssetPath + "' but it does not exist");
			}
		}
	}

	public void ImportSprite(SpriteSheetImportTarget target)
	{
		string path = AssetDatabase.GetAssetPath(newTexture);
		
		TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
		textureImporter.isReadable = true;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.npotScale = TextureImporterNPOTScale.None;
		textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

		if(textureImporter.isReadable)
		{
			int newKey = RagePixelUtil.RandomKey();
			int newKey2 = RagePixelUtil.RandomKey();
			Texture2D spritesheetTexture = spriteSheet.atlas.GetTexture("_MainTex") as Texture2D;

			switch(target)
			{
			    case SpriteSheetImportTarget.SpriteSheet: 
                {
					RagePixelRow row = spriteSheet.AddRow(newKey, importSpriteWidth, importSpriteHeight);
					row.name = newTexture.name;
					RagePixelUtil.ImportRowFromSheetTexture(
						spriteSheet,
						row,
						newTexture,
						importSpriteWidth, importSpriteHeight,
						importSpriteTopLeft
					);
					spriteSheetGUI.currentRowKey = newKey;
					animStripGUI.currentCellKey = row.cells[0].key;
					break;
				}
                case SpriteSheetImportTarget.NewSprite: 
                {
                    RagePixelRow row = spriteSheet.AddRow(newKey, newTexture.width, newTexture.height);
                    row.InsertCell(0, newKey2).importAssetPath = path;

                    spriteSheetGUI.currentRowKey = newKey;
                    animStripGUI.currentCellKey = newKey2;

                    RagePixelUtil.RebuildAtlas(spriteSheet, true, "Import texture as new sprite");
				
                    Rect uvs = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).uv;
                    RagePixelUtil.CopyPixels(newTexture, new Rect(0f, 0f, 1f, 1f), spritesheetTexture, uvs);
                    break;
                }
                case SpriteSheetImportTarget.NewFrame: 
                {
                    RagePixelRow row = spriteSheet.GetRow(spriteSheetGUI.currentRowKey);
                    int index = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey) + 1;
                    row.InsertCell(index, newKey2).importAssetPath = path;

                    animStripGUI.currentCellKey = newKey2;

                    RagePixelUtil.RebuildAtlas(spriteSheet, true, "Import texture as new frame");

                    Rect uvs = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).uv;
                    RagePixelUtil.CopyPixels(newTexture, new Rect(0f, 0f, 1f, 1f), spritesheetTexture, uvs);
                    break;
                }
                case SpriteSheetImportTarget.Selected: 
                {
					RagePixelCell cell = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey);
                    Rect uvs = cell.uv;
                    RagePixelUtil.CopyPixels(newTexture, new Rect(0f, 0f, 1f, 1f), spritesheetTexture, uvs);
					cell.importAssetPath = path;
                    break;
                }
			}

			RagePixelUtil.SaveSpritesheetTextureToDisk(spriteSheet);
			RagePixelUtil.RebuildAtlas(spriteSheet, true, "save after import");

			spriteSheetGUI.isDirty = true;
			animStripGUI.isDirty = true;

			if(inspector != null)
			{
				inspector.animStripGUI.isDirty = true;
				inspector.spriteSheetGUI.isDirty = true;
			}
		}
		else
		{
			EditorUtility.DisplayDialog("Texture is not readable", "Set texture type to advanced and read/write as enabled from the import options.", "OK");
		}
	}
}