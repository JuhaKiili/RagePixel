using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class RagePixelUtil
{

	private static string assetDatabaseDirSeparatorChar = "/";
	public static string defaultSpriteSheetName = "spritesheet";
	public static string defaultSpritesheetPath = "Assets" + assetDatabaseDirSeparatorChar + "RagePixelAssets";
	public static string defaultAtlasPath = "Assets" + assetDatabaseDirSeparatorChar + "RagePixelAssets";
	public static string defaultSettingsPath = "Assets" + assetDatabaseDirSeparatorChar + "RagePixelSettings";
	public static string defaultSettingsName = "ragepixelsettings";
	public static int defaultAtlasSize = 16;
	public static int defaultAtlasPadding = 1;
	public static int defaultSpriteSize = 16;
	private static RagePixelSettings _ragePixelSettings;

	[MenuItem("Assets/Create/RagePixel Spritesheet")]
	static void CreateRagePixelSpritesheet()
	{
		RagePixelUtil.CreateNewSpritesheet();
	}

	[MenuItem("GameObject/Create Other/RagePixel Sprite")]
	static void CreateRagePixelSprite()
	{
		GameObject newSpriteGO = new GameObject();
		RagePixelSprite ragePixelSprite = (RagePixelSprite)newSpriteGO.AddComponent(typeof(RagePixelSprite));
		newSpriteGO.name = "New Sprite";
		if(SceneView.lastActiveSceneView != null)
		{
			if(SceneView.lastActiveSceneView.camera != null)
			{
				newSpriteGO.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
				newSpriteGO.transform.position = new Vector3(newSpriteGO.transform.position.x, newSpriteGO.transform.position.y, 0f);
			}
		}

		bool found = false;
		foreach(Object obj in allAssets)
		{
			if(!found)
			{
				if(obj is RagePixelSpriteSheet && !obj.name.Equals("BasicFont"))
				{
					ragePixelSprite.spriteSheet = (RagePixelSpriteSheet)obj;
					ragePixelSprite.currentRowKey = ragePixelSprite.spriteSheet.rows[0].key;
					ragePixelSprite.currentCellKey = ragePixelSprite.GetCurrentRow().cells[0].key;
					found = true;
				}
			}
		}

		GameObject[] selection = new GameObject[1];
		selection[0] = newSpriteGO;
		Selection.objects = selection;
	}

	/*
    [MenuItem("GameObject/Create Other/RagePixel Text")]
    static void CreateRagePixelText()
    {
        GameObject newTextGO = new GameObject();
        RagePixelText ragePixelText = (RagePixelText)newTextGO.AddComponent(typeof(RagePixelText));
        newTextGO.name = "New Text";
        if (SceneView.lastActiveSceneView != null)
        {
            if (SceneView.lastActiveSceneView.camera != null)
            {
                newTextGO.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
                newTextGO.transform.position = new Vector3(newTextGO.transform.position.x, newTextGO.transform.position.y, 0f);
            }
        }

        foreach(Object obj in allAssets)
        {
            if (obj.name.Equals("BasicFont"))
            {
                if (obj.GetType() == typeof(RagePixelSpriteSheet))
                {
                    ragePixelText.spriteSheet = (RagePixelSpriteSheet)obj;
                }
            }
        }

        GameObject[] selection = new GameObject[1];
        selection[0] = newTextGO;
        Selection.objects = selection;
    }*/

	public static RagePixelSettings Settings
	{
		get
		{
			if(_ragePixelSettings == null)
			{
				List<UnityEngine.Object> assets = RagePixelUtil.allAssets;
				foreach(UnityEngine.Object asset in assets)
				{
					if(asset is RagePixelSettings)
					{
						_ragePixelSettings = asset as RagePixelSettings;
					}
				}

				if(_ragePixelSettings == null)
				{
					if(!Directory.Exists(defaultSettingsPath))
					{
						Directory.CreateDirectory(defaultSettingsPath);
					}
					AssetDatabase.Refresh();
					_ragePixelSettings = ScriptableObject.CreateInstance("RagePixelSettings") as RagePixelSettings;
					AssetDatabase.CreateAsset(_ragePixelSettings, RagePixelUtil.defaultSettingsPath + assetDatabaseDirSeparatorChar + defaultSettingsName + ".asset");
				}
			}
			return _ragePixelSettings;
		}
	}

	public static int RandomKey()
	{
		int val = 0;
		while(val == 0)
		{
			val = (int)UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}
		return val;
	}

	public static RagePixelSpriteSheet CreateNewSpritesheet()
	{
		RagePixelSpriteSheet spriteSheet = ScriptableObject.CreateInstance("RagePixelSpriteSheet") as RagePixelSpriteSheet;

		int cnt = 1;
		string path = defaultSpritesheetPath + assetDatabaseDirSeparatorChar + "spritesheet_" + cnt.ToString() + ".asset";

		while(File.Exists(path))
		{
			cnt++;
			path = defaultSpritesheetPath + assetDatabaseDirSeparatorChar + "spritesheet_" + cnt.ToString() + ".asset";
		}
		spriteSheet.name = "spritesheet_" + cnt.ToString();

		spriteSheet.atlas = CreateNewAtlas(defaultAtlasSize, spriteSheet.name);
		if(!Directory.Exists(defaultSpritesheetPath))
		{
			Directory.CreateDirectory(defaultSpritesheetPath);
		}

		RagePixelRow row = spriteSheet.AddRow(RandomKey(), defaultSpriteSize, defaultSpriteSize);

		int newKey = RandomKey();
		row.InsertCell(0, newKey);
		row.cells[0].uv = new Rect(0f, 0f, 1f, 1f);

		AssetDatabase.CreateAsset(spriteSheet, path);
		//spriteSheet.hideFlags = HideFlags.DontSave;
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		return spriteSheet;
	}

	public static Material CreateNewAtlas(int size, string name)
	{
		Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
		tex.filterMode = FilterMode.Point;
		Color32[] pixels = new Color32[tex.width * tex.height];

		for(int y = 0; y < tex.height; y++)
		{
			for(int x = y % 2; x < tex.width; x += 2)
			{
				pixels[y * tex.width + x] = new Color(0f, 0f, 0f, 0f);
			}
		}

		tex.SetPixels32(pixels);
		tex.Apply(false);

		Material material = null;

		if(!File.Exists(defaultAtlasPath + System.IO.Path.DirectorySeparatorChar + name + ".mat"))
		{
			saveAtlasPng(defaultAtlasPath, name, tex);

			material = new Material(Shader.Find("RagePixel/Basic"));
			material.SetTexture("_MainTex", Resources.LoadAssetAtPath(defaultAtlasPath + assetDatabaseDirSeparatorChar + name + ".png", typeof(Texture)) as Texture);
			AssetDatabase.CreateAsset(material, defaultAtlasPath + assetDatabaseDirSeparatorChar + name + ".mat");
		}
		else
		{
			material = AssetDatabase.LoadAssetAtPath(defaultAtlasPath + assetDatabaseDirSeparatorChar + name + ".mat", typeof(Material)) as Material;
		}

		Object.DestroyImmediate(tex);

		return material;
	}

	public static void saveAtlasPng(string path, string name, Texture2D tex)
	{
		if(!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		bool newAtlas = true;
		if(File.Exists(path + System.IO.Path.DirectorySeparatorChar + name + ".png"))
		{
			newAtlas = false;
		}

		FileStream fs = new FileStream(path + System.IO.Path.DirectorySeparatorChar + name + ".png", FileMode.Create);
		BinaryWriter bw = new BinaryWriter(fs);
		bw.Write(tex.EncodeToPNG());
		bw.Close();
		fs.Close();

		if(newAtlas)
		{
			AssetDatabase.Refresh();

			TextureImporter tImporter = AssetImporter.GetAtPath(path + assetDatabaseDirSeparatorChar + name + ".png") as TextureImporter;

			if(tImporter != null)
			{
				tImporter.mipmapEnabled = false;
				tImporter.isReadable = true;
				tImporter.textureFormat = TextureImporterFormat.ARGB32;
				tImporter.filterMode = FilterMode.Point;
				tImporter.maxTextureSize = 4096;

				tImporter.wrapMode = TextureWrapMode.Repeat;
				AssetDatabase.ImportAsset(path + assetDatabaseDirSeparatorChar + name + ".png", ImportAssetOptions.Default);
			}
		}
	}

	public static void CopyPixels(Texture2D sourceTexture, Rect sourceUV, Texture2D targetTexture, Rect targetUV)
	{
		//Debug.Log(targetTexture.name + ":" + targetTexture.maxWidth+"/"+targetTexture.height + ", " + targetUV);

		int srcPixelX = Mathf.RoundToInt(sourceUV.xMin * sourceTexture.width);
		int srcPixelY = Mathf.RoundToInt(sourceUV.yMin * sourceTexture.height);
		int srcPixelWidth = Mathf.RoundToInt(sourceUV.width * sourceTexture.width);
		int srcPixelHeight = Mathf.RoundToInt(sourceUV.height * sourceTexture.height);

		int trgPixelX = Mathf.RoundToInt(targetUV.xMin * targetTexture.width);
		int trgPixelY = Mathf.RoundToInt(targetUV.yMin * targetTexture.height);
		int trgPixelWidth = Mathf.RoundToInt(targetUV.width * targetTexture.width);
		int trgPixelHeight = Mathf.RoundToInt(targetUV.height * targetTexture.height);

		int sizeX = Mathf.Min(trgPixelWidth, targetTexture.width - trgPixelX, srcPixelWidth, sourceTexture.width - srcPixelX);
		int sizeY = Mathf.Min(trgPixelHeight, targetTexture.height - trgPixelY, srcPixelHeight, sourceTexture.height - srcPixelY);

		/*
        Debug.Log(
            "trgPixelWidth:" + trgPixelWidth +
            " targetTexture.pixelWidth - trgPixelX:" + (targetTexture.pixelWidth - trgPixelX) +
            " srcPixelWidth:" + trgPixelWidth +
            " sourceTexture.pixelWidth - srcPixelX:" + (sourceTexture.pixelWidth - srcPixelX) +
            " trgPixelHeight:" + trgPixelHeight +
            " targetTexture.height - trgPixelY:" + (targetTexture.height - trgPixelY) +
            " srcPixelHeight:" + srcPixelHeight +
            " sourceTexture.height - srcPixelY:" + (sourceTexture.height - srcPixelY)
            );
        */

		targetTexture.SetPixels(
            trgPixelX,
            trgPixelY,
            sizeX,
            sizeY,
            sourceTexture.GetPixels(
                srcPixelX,
                srcPixelY,
                sizeX,
                sizeY
            ));
	}

	public static void RebuildAtlas(RagePixelSpriteSheet spriteSheet, bool atlasIsGrowing, string caller = "nobody")
	{
		int frameCount = spriteSheet.GetTotalCellCount();

		Texture2D[] textures = new Texture2D[frameCount];
		RagePixelCell[] cells = new RagePixelCell[frameCount];

		int index = 0;

		EditorUtility.SetDirty(spriteSheet);
		Texture2D sourceTexture = spriteSheet.atlas.GetTexture("_MainTex") as Texture2D;

		if(sourceTexture != null)
		{
			foreach(RagePixelRow row in spriteSheet.rows)
			{
				foreach(RagePixelCell cell in row.cells)
				{
					textures[index] = new Texture2D(row.newPixelSizeX, row.newPixelSizeY);

					if(cell.uv.width > 0f && cell.uv.height > 0f)
					{
						clearPixels(textures[index]);
						CopyPixels(sourceTexture, cell.uv, textures[index], new Rect(0f, 0f, 1f, 1f));
					}
					else
					{
						clearPixels(textures[index]);
					}
					cells[index] = cell;
					index++;
				}
			}
		}

		Texture2D newAtlasTexture = new Texture2D(defaultAtlasSize, defaultAtlasSize);
		int atlasPadding = defaultAtlasPadding;
		if(textures.Length == 1)
		{
			atlasPadding = 0;
		}
		Rect[] newUvs = newAtlasTexture.PackTextures(textures, atlasPadding, 4096);

		if(newUvs != null)
		{

			float totalAreaOld = 0f;
			float totalAreaNew = 0f;
			for(int i = 0; i < cells.Length; i++)
			{
				totalAreaOld += cells[i].uv.height * cells[i].uv.width;
			}
			for(int i = 0; i < newUvs.Length; i++)
			{
				totalAreaNew += newUvs[i].height * newUvs[i].width;
			}

			// Checking if the PackTextures() is going to crap all over spritesheet, when going over max texture size
			if(!(atlasIsGrowing && totalAreaNew < totalAreaOld && sourceTexture.width * sourceTexture.height >= newAtlasTexture.width * newAtlasTexture.height))
			{
				for(int i = 0; i < newUvs.Length && i < cells.Length; i++)
				{
					cells[i].uv = newUvs[i];
				}

				saveAtlasPng(defaultAtlasPath, spriteSheet.atlas.name, newAtlasTexture);

				for(int i = 0; i < textures.Length; i++)
				{
					Object.DestroyImmediate(textures[i]);
				}
				Object.DestroyImmediate(newAtlasTexture);

				RagePixelSprite[] sprites = Resources.FindObjectsOfTypeAll(typeof(RagePixelSprite)) as RagePixelSprite[];

				EditorUtility.SetDirty(spriteSheet);
				EditorUtility.SetDirty(spriteSheet.atlas);
				EditorUtility.SetDirty(spriteSheet.atlas.GetTexture("_MainTex"));

				foreach(RagePixelRow row in spriteSheet.rows)
				{
					row.pixelSizeX = row.newPixelSizeX;
					row.pixelSizeY = row.newPixelSizeY;
				}

				foreach(RagePixelSprite sprite in sprites)
				{
					EditorUtility.SetDirty(sprite);
					sprite.checkKeyIntegrity();
					sprite.SnapToIntegerPosition();
					sprite.refreshMesh();
				}

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.LogError("ERROR: Too much data for max texture size (Spritesheet: " + spriteSheet.name + ")");
			}
		}
		else
		{
			Debug.LogError("ERROR: Atlas PackTextures() failed (Spritesheet: " + spriteSheet.name + ")");
		}
	}

	public static void SaveSpritesheetTextureToDisk(RagePixelSpriteSheet spriteSheet)
	{
		RagePixelUtil.saveAtlasPng(
            Path.GetDirectoryName(AssetDatabase.GetAssetPath(spriteSheet)),
            spriteSheet.name,
            spriteSheet.atlas.GetTexture("_MainTex") as Texture2D
            );
	}

	public static List<Object> allAssets
	{
		get
		{
			List<FileInfo> files = DirSearch(new DirectoryInfo(Application.dataPath), "*.*");

			List<Object> assetRefs = new List<Object>();

			foreach(FileInfo fi in files)
			{
				if(fi.Name.StartsWith("."))
					continue; // Unity ignores dotfiles.
				assetRefs.Add(AssetDatabase.LoadMainAssetAtPath(getRelativeAssetPath(fi.FullName)));
			}
			return assetRefs;
		}
	}

	public static string fixSlashes(string s)
	{
		const string forwardSlash = "/";
		const string backSlash = "\\";
		return s.Replace(backSlash, forwardSlash);
	}

	public static string getRelativeAssetPath(string pathName)
	{
		return fixSlashes(pathName).Replace(Application.dataPath, "Assets");
	}

	public static List<FileInfo> DirSearch(DirectoryInfo d, string searchFor)
	{
		List<FileInfo> founditems = d.GetFiles(searchFor).ToList();
		// Add (by recursing) subdirectory items.
		DirectoryInfo[] dis = d.GetDirectories();
		foreach(DirectoryInfo di in dis)
			founditems.AddRange(DirSearch(di, searchFor));

		return (founditems);
	}

	public static void drawPixelBorder(Texture2D tex, Color color)
	{
		for(int pY = 0; pY < tex.height; pY++)
		{
			for(int pX = 0; pX < tex.width; pX++)
			{
				if(pX == 0 || pX == tex.width - 1 || pY == 0 || pY == tex.height - 1)
				{
					tex.SetPixel(pX, pY, color);
				}
			}
		}
	}

	public static void drawPixelBorder(int x, int y, int width, int height, Texture2D tex, Color color)
	{
		for(int pX = x; pX <= Mathf.Min(x+width,tex.width-1); pX++)
		{
			tex.SetPixel(pX, y, color);
			tex.SetPixel(pX, Mathf.Min(y + height, tex.height - 1), color);
		}
		for(int pY = y; pY <= Mathf.Min(y + height, tex.height - 1); pY++)
		{
			tex.SetPixel(x, pY, color);
			tex.SetPixel(Mathf.Min(x + width, tex.width - 1), pY, color);
		}

	}

	public static void clearPixels(Texture2D targetTexture, Rect targetUV)
	{
		int trgPixelX = Mathf.RoundToInt(targetUV.xMin * targetTexture.width);
		int trgPixelY = Mathf.RoundToInt(targetUV.yMin * targetTexture.height);
		int trgPixelWidth = Mathf.RoundToInt(targetUV.width * targetTexture.width);
		int trgPixelHeight = Mathf.RoundToInt(targetUV.height * targetTexture.height);


		Color[] colors = new Color[trgPixelWidth * trgPixelHeight];
		targetTexture.SetPixels(trgPixelX, trgPixelY, trgPixelWidth, trgPixelHeight, colors);
	}

	public static void clearPixels(Texture2D tex)
	{
		Color[] colors = new Color[tex.width * tex.height];
		tex.SetPixels(colors);
	}

	public static void ResetCamera(RagePixelCamera ragePixelCamera)
	{
		Camera camera = ragePixelCamera.GetComponent(typeof(Camera)) as Camera;

		camera.orthographic = true;

		float screenW = ragePixelCamera.resolutionPixelWidth;
		float screenH = ragePixelCamera.resolutionPixelHeight;

		Vector3 position = SceneView.lastActiveSceneView.pivot;

		position.z = -10.0f;
		position.x = screenW / 2 / ragePixelCamera.pixelSize;
		position.y = screenH / 2 / ragePixelCamera.pixelSize;

		if(SceneView.lastActiveSceneView != null)
		{
			SceneView.lastActiveSceneView.pivot = position;
		}
		camera.transform.position = position;
		camera.orthographicSize = screenH / 2 / ragePixelCamera.pixelSize;
	}

	public static void ImportRowFromSheetTexture(
			RagePixelSpriteSheet spritesheet,
			RagePixelRow destRow,
			Texture2D tex,
			int importSpriteWidth, int importSpriteHeight,
			bool importSpriteTopLeft
		)
	{
		int framesWide = tex.width / importSpriteWidth;
		int framesHigh = tex.height / importSpriteHeight;
		int cellCount = framesWide * framesHigh;
		destRow.Clear();
		//loop to allocate cell space
		for(int i = 0; i < cellCount; i++)
		{
			destRow.InsertCell(i, RagePixelUtil.RandomKey());
		}
		RebuildAtlas(spritesheet, true, "Import row from spritesheet");
		float importUVPerFrameW = 1.0f / framesWide;
		float importUVPerFrameH = 1.0f / framesHigh;

		Texture2D spritesheetTexture = spritesheet.atlas.GetTexture("_MainTex") as Texture2D;
		//loop to copy texture to UVs
		for(int i = 0; i < cellCount; i++)
		{
			int y = i / framesWide;
			int x = i - (y * framesWide);
			Rect importUVs = new Rect(x * importUVPerFrameW, (importSpriteTopLeft ? (framesHigh - 1 - y) : y) * importUVPerFrameH, importUVPerFrameW, importUVPerFrameH);
			Rect uvs = destRow.cells[i].uv;
			RagePixelUtil.CopyPixels(tex, importUVs, spritesheetTexture, uvs);
		}
	}


}
