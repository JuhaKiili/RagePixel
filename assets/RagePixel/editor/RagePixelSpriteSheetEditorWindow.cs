using UnityEngine;
using UnityEditor;
using System.IO;

public class RagePixelSpriteSheetEditorWindow : EditorWindow
{
    public RagePixelSpriteSheet spriteSheet;
    public RagePixelSpriteEditor inspector;
    public RagePixelSprite selectedSprite;

    private bool showNamedAnimationsFoldout;
    
    private bool showImportFoldout;
    private Texture2D newTexture;
        
    public enum SpriteSheetImportTarget { Selected = 0, NewFrame, NewSprite };

    private RagePixelAnimStripGUI _animStripGUI;
    public RagePixelAnimStripGUI animStripGUI
    {
        get
        {
            if (_animStripGUI == null)
            {
                _animStripGUI = new RagePixelAnimStripGUI(spriteSheetGUI);
                if (inspector != null)
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
            if (_spriteSheetGUI == null)
            {
                _spriteSheetGUI = new RagePixelSpriteSheetGUI();
                if (inspector != null)
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

        if (spriteSheet != null)
        {
            if (spriteSheet != spriteSheetGUI.spriteSheet)
            {
                spriteSheetGUI.spriteSheet = spriteSheet;
                spriteSheetGUI.currentRowKey = spriteSheet.rows[0].key;
                animStripGUI.currentCellKey = spriteSheet.rows[0].cells[0].key;
            }

            spriteSheetGUI.positionX = x;
            spriteSheetGUI.positionY = y;

            animStripGUI.positionX = x;
            animStripGUI.positionY = spriteSheetGUI.positionY + spriteSheetGUI.pixelHeight + 5;

            GUI.color = RagePixelGUIIcons.greenButtonColor;
            if (GUI.Button(new Rect(Screen.width - 38f * 2 - 5, spriteSheetGUI.positionY + spriteSheetGUI.pixelHeight - 32f, 38f, 32f), "NEW"))
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
            
            if (GUI.Button(new Rect(Screen.width - 38f - 5, spriteSheetGUI.positionY + spriteSheetGUI.pixelHeight - 32f, 38f, 32f), "DEL"))
            {
                if (spriteSheet.rows.Length > 1)
                {
                    if (EditorUtility.DisplayDialog("Delete selected sprite?", "Are you sure?", "Delete", "Cancel"))
                    {
                        int index = spriteSheet.GetIndex(spriteSheetGUI.currentRowKey);
                        spriteSheet.RemoveRowByKey(spriteSheetGUI.currentRowKey);

                        int newKey = spriteSheet.rows[Mathf.Clamp(index, 0, spriteSheet.rows.Length - 1)].key;
                        
                        if (selectedSprite != null)
                        {
                            if (selectedSprite.currentRowKey == spriteSheetGUI.currentRowKey)
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

                        if (inspector != null)
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
            if (GUI.Button(new Rect(Screen.width - 38f * 2 - 5, animStripGUI.positionY + animStripGUI.pixelHeight - 32f, 38f, 32f), "NEW"))
            {
                int index = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey) + 1;
                RagePixelCell cell = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).InsertCell(index, RagePixelUtil.RandomKey());
                animStripGUI.currentCellKey = cell.key;
                RagePixelUtil.RebuildAtlas(spriteSheet, true, "AddCell");
            }
            GUI.color = RagePixelGUIIcons.redButtonColor;
            if (GUI.Button(new Rect(Screen.width - 38f - 5, animStripGUI.positionY + animStripGUI.pixelHeight - 32f, 38f, 32f), "DEL"))
            {
                if (spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length > 1)
                {
                    if (EditorUtility.DisplayDialog("Delete selected animation frame?", "Are you sure?", "Delete", "Cancel"))
                    {
                        int index = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey);
                        spriteSheet.GetRow(spriteSheetGUI.currentRowKey).RemoveCellByKey(animStripGUI.currentCellKey);

                        int newKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells[Mathf.Clamp(index, 0, spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length - 1)].key;
                        if (selectedSprite != null)
                        {
                            if (selectedSprite.currentCellKey == animStripGUI.currentCellKey)
                            {
                                selectedSprite.meshIsDirty = true;
                                selectedSprite.currentCellKey = newKey;
                                selectedSprite.pixelSizeX = selectedSprite.GetCurrentRow().pixelSizeX;
                                selectedSprite.pixelSizeY = selectedSprite.GetCurrentRow().pixelSizeY;
                            }
                        }

                        animStripGUI.currentCellKey = newKey;
                        RagePixelUtil.RebuildAtlas(spriteSheet, true, "DeleteCell");

                        if (inspector != null)
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

            spriteSheet.GetRow(spriteSheetGUI.currentRowKey).name =
                EditorGUI.TextField(
                    new Rect(x, y, Mathf.Min(350, Screen.width - x * 2), 16), "Sprite Name", spriteSheet.GetRow(spriteSheetGUI.currentRowKey).name);
            y += 20;
            EditorGUI.LabelField(
                new Rect(x, y, Screen.width - x * 2, 16), "Frame Index", spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey).ToString() + " (" + (spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells.Length - 1).ToString()+")");
            y += 20;

            spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).delay =
                EditorGUI.IntField(
                new Rect(x, y, Mathf.Min(200, Screen.width - x * 2), 16), "Frame Time", (int)spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).delay);
            y += 20;
                        
            GUILayout.Space(y + 20);
                        
            int namedAnimationsFoldoutHeight = 0;
            showNamedAnimationsFoldout = EditorGUI.Foldout(new Rect(x, y, Screen.width - x * 2, 20), showNamedAnimationsFoldout, "Named animations");
            y += 20;
            if (showNamedAnimationsFoldout)
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
                for (int animIndex = 0; animIndex < animations.Length; animIndex++)
                {
                    x = 5;
                    animations[animIndex].name = EditorGUI.TextField(new Rect(x, y, 170, 16), animations[animIndex].name);
                    x += 175;
                    animations[animIndex].startIndex = EditorGUI.IntField(new Rect(x, y, 40, 16), animations[animIndex].startIndex);
                    x += 45;
                    animations[animIndex].endIndex = EditorGUI.IntField(new Rect(x, y, 40, 16), animations[animIndex].endIndex);
                    x += 45;
                    animations[animIndex].mode = (RagePixelSprite.AnimationMode)EditorGUI.EnumPopup(new Rect(x, y, 130, 16), animations[animIndex].mode);
                    x += 135;

                    if(GUI.Button(new Rect(x, y, 60, 16), "Delete")) {
                        spriteSheet.GetRow(spriteSheetGUI.currentRowKey).RemoveAnimation(animIndex);
                    }

                    y += 20;
                    namedAnimationsFoldoutHeight += 20;
                }
                x = 5;

                if (GUI.Button(new Rect(x, y, 50, 16), "Add"))
                {
                    spriteSheet.GetRow(spriteSheetGUI.currentRowKey).AddAnimation();
                }
                y += 20;
                namedAnimationsFoldoutHeight += 20;

                GUILayout.Space(namedAnimationsFoldoutHeight + 26);
                y += 6;
            }

            x = 5;

            showImportFoldout = EditorGUI.Foldout(new Rect(x, y, Screen.width - x * 2, 16), showImportFoldout, "Import");

            if (showImportFoldout)
            {
                GUILayout.BeginHorizontal();
                newTexture = (Texture2D)EditorGUILayout.ObjectField(" ", newTexture, typeof(Texture2D), false);
                
                GUILayout.BeginVertical();
                if (GUI.Button(new Rect(x + 240f, y, 180f, 19f), "Import to selected frame"))
                {
                    if (newTexture != null)
                    {
                        ImportSprite(SpriteSheetImportTarget.Selected);
                    }
                }
                y += 21;
                if (GUI.Button(new Rect(x + 240f, y, 180f, 19f), "Import as new frame"))
                {
                    if (newTexture != null)
                    {
                        ImportSprite(SpriteSheetImportTarget.NewFrame);
                    }
                }
                y += 21;
                if (GUI.Button(new Rect(x + 240f, y, 180f, 19f), "Import as new sprite"))
                {
                    if (newTexture != null)
                    {
                        ImportSprite(SpriteSheetImportTarget.NewSprite);
                    }
                }
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }


            int oldRowKey = spriteSheetGUI.currentRowKey;
            animStripGUI.HandleGUIEvent(Event.current);
            spriteSheetGUI.HandleGUIEvent(Event.current);
            
            if(oldRowKey != spriteSheetGUI.currentRowKey)
            {
                animStripGUI.currentCellKey = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).cells[0].key;
            }

            if (animStripGUI.isDirty || spriteSheetGUI.isDirty)
            {
                Repaint();
                if (inspector != null)
                {
                    inspector.spriteSheetGUI.isDirty = true;
                    inspector.animStripGUI.isDirty = true;
                    inspector.Repaint();
                }
                if (selectedSprite != null)
                {
                    selectedSprite.meshIsDirty = true;
                    selectedSprite.refreshMesh();
                }
            }       

            spriteSheetGUI.maxWidth = scenePixelWidth - 38 * 2 - 10 - spriteSheetGUI.positionX;
            animStripGUI.maxWidth = scenePixelWidth - 38 * 2 - 10 - animStripGUI.positionX;
            EditorGUI.DrawPreviewTexture(spriteSheetGUI.bounds, spriteSheetGUI.spriteSheetTexture);
            EditorGUI.DrawPreviewTexture(animStripGUI.bounds, animStripGUI.animStripTexture);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(spriteSheet);
        }
    }

    public void Update()
    {
        if (animStripGUI.isDirty || spriteSheetGUI.isDirty)
        {
            Repaint();
        }
    }

    public void OnDestroy()
    {
        spriteSheetGUI.CleanExit();
        animStripGUI.CleanExit();
    }

    public void ImportSprite(SpriteSheetImportTarget target)
    {
        string path = AssetDatabase.GetAssetPath(newTexture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        textureImporter.isReadable = true;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        if (textureImporter.isReadable)
        {
            int newKey = RagePixelUtil.RandomKey();
            int newKey2 = RagePixelUtil.RandomKey();
            Texture2D spritesheetTexture = spriteSheet.atlas.GetTexture("_MainTex") as Texture2D;

            switch (target)
            {
                case SpriteSheetImportTarget.NewSprite:
                    RagePixelRow row = spriteSheet.AddRow(newKey, newTexture.width, newTexture.height);
                    row.InsertCell(0, newKey2);

                    spriteSheetGUI.currentRowKey = newKey;
                    animStripGUI.currentCellKey = newKey2;

                    RagePixelUtil.RebuildAtlas(spriteSheet, true, "Import texture as new sprite");

                    Rect uvs = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).uv;
                    RagePixelUtil.CopyPixels(newTexture, new Rect(0f, 0f, 1f, 1f), spritesheetTexture, uvs);
                    break;

                case SpriteSheetImportTarget.NewFrame:
                    RagePixelRow row2 = spriteSheet.GetRow(spriteSheetGUI.currentRowKey);
                    int index = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetIndex(animStripGUI.currentCellKey) + 1;
                    row2.InsertCell(index, newKey2);

                    animStripGUI.currentCellKey = newKey2;

                    RagePixelUtil.RebuildAtlas(spriteSheet, true, "Import texture as new frame");

                    Rect uvs2 = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).uv;
                    RagePixelUtil.CopyPixels(newTexture, new Rect(0f, 0f, 1f, 1f), spritesheetTexture, uvs2);
                    break;

                case SpriteSheetImportTarget.Selected:
                    Rect uvs3 = spriteSheet.GetRow(spriteSheetGUI.currentRowKey).GetCell(animStripGUI.currentCellKey).uv;
                    RagePixelUtil.CopyPixels(newTexture, new Rect(0f, 0f, 1f, 1f), spritesheetTexture, uvs3);
                    break;
            }

            RagePixelUtil.SaveSpritesheetTextureToDisk(spriteSheet);
            RagePixelUtil.RebuildAtlas(spriteSheet, true, "save after import");

            spriteSheetGUI.isDirty = true;
            animStripGUI.isDirty = true;

            if (inspector != null)
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