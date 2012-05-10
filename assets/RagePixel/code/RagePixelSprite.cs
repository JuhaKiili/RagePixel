using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RagePixelSprite : MonoBehaviour, IRagePixel {

	public Vector3 accuratePosition;
	public RagePixelSpriteSheet spriteSheet;

	public enum Mode
	{
		Default = 0,
		Grid9
	};
	public Mode mode = Mode.Default;

	public enum PivotMode
	{
		BottomLeft = 0,
		Bottom,
		Middle
	};
	public PivotMode pivotMode = PivotMode.BottomLeft;

	public enum FrameMode
	{
		Range = 0,
		Sequence
	};
	public FrameMode frameMode = 0;

	public enum AnimationMode
	{
		PlayOnce = 0,
		PlayOnceReverse,
		Loop,
		LoopReverse,
		PingPong
	};
	public AnimationMode animationMode = 0;
	string currentAnimationName = null;
	int currentAnimationPriority = 0;

	//frame mode: range
	public int animationMinIndex = -1;
	public int animationMaxIndex = -1;

	//frame mode: sequence
	public int[] animationFrames;
	public int animationCurrentFrame = 0;
	private int animationPingPongDirection = 1;
	private RagePixelSpriteSheet lastCellSpriteSheetCache;
	private RagePixelCell lastCellCache;
	private int lastCellCacheKey;
	private RagePixelSpriteSheet lastRowSpriteSheetCache;
	private RagePixelRow lastRowCache;
	private int lastRowCacheKey;
	public int grid9Left;
	public int grid9Top;
	public int grid9Right;
	public int grid9Bottom;
	public int currentRowKey;
	public int currentCellKey;
	public int ZLayer;
	public int pixelSizeX = 16;
	public int pixelSizeY = 16;
	public bool meshIsDirty = false;
	public bool vertexColorsAreDirty = false;
	public Color tintColor = new Color(1f, 1f, 1f, 1f);
	public bool flipHorizontal;
	public bool flipVertical;
	public float nextAnimFrame = 0f;
	public bool playAnimation = false;
	private bool toBeRefreshed;
	private float myTime = 0f;

	void Awake()
	{
		lastRowSpriteSheetCache = null;
		lastCellSpriteSheetCache = null;
		lastRowCache = null;
		lastCellCache = null;
		lastCellCacheKey = 0;
		lastRowCacheKey = 0;

		meshIsDirty = true;
		vertexColorsAreDirty = true;

		if(!Application.isPlaying)
		{
			MeshFilter meshFilter = null;
			MeshRenderer meshRenderer = null;

			meshRenderer = gameObject.GetComponent("MeshRenderer") as MeshRenderer;
			if(meshRenderer == null)
			{
				meshRenderer = gameObject.AddComponent("MeshRenderer") as MeshRenderer;
			}

			meshFilter = gameObject.GetComponent("MeshFilter") as MeshFilter;
			if(meshFilter == null)
			{
				meshFilter = gameObject.AddComponent("MeshFilter") as MeshFilter;
			}

			if(meshFilter.sharedMesh != null)
			{
				RagePixelSprite[] ragePixelSprites = GameObject.FindObjectsOfType(typeof(RagePixelSprite)) as RagePixelSprite[];

				foreach(RagePixelSprite ragePixelSprite in ragePixelSprites)
				{
					MeshFilter otherMeshFilter = ragePixelSprite.GetComponent(typeof(MeshFilter)) as MeshFilter;
					if(otherMeshFilter != null)
					{
						if(otherMeshFilter.sharedMesh == meshFilter.sharedMesh && otherMeshFilter != meshFilter)
						{
							meshFilter.mesh = new Mesh();
							toBeRefreshed = true;
						}
					}
				}
			}

			if(meshFilter.sharedMesh == null)
			{
				meshFilter.sharedMesh = new Mesh();
				toBeRefreshed = true;
			}
		}
		else
		{
			meshIsDirty = true;
			refreshMesh();
		}
	}

	void Start()
	{
		if(Application.isPlaying && playAnimation && gameObject.active)
		{
			nextAnimFrame = myTime + GetCurrentCell().delay / 1000f;
		}
	}

	void OnEnable()
	{
		if(Application.isPlaying && playAnimation)
		{
			nextAnimFrame = myTime + GetCurrentCell().delay / 1000f;
		}
	}

	public void SnapToScale()
	{
		transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public void SnapToIntegerPosition()
	{
		if(!Application.isPlaying)
		{
			//transform.rotation = Quaternion.identity;
			transform.localEulerAngles = new Vector3(0f, 0f, transform.localEulerAngles.z);
		}
		//SnapToScale();
		transform.localPosition = new Vector3(Mathf.RoundToInt(transform.localPosition.x), Mathf.RoundToInt(transform.localPosition.y), ZLayer);
	}

	public void SnapToIntegerPosition(float divider)
	{
		transform.rotation = Quaternion.identity;
		//SnapToScale();
		transform.localPosition = new Vector3(Mathf.RoundToInt(transform.localPosition.x * divider) / divider, Mathf.RoundToInt(transform.localPosition.y * divider) / divider, ZLayer);
	}

	public void refreshMesh()
	{
		MeshRenderer meshRenderer = GetComponent(typeof(MeshRenderer)) as MeshRenderer;
		MeshFilter meshFilter = GetComponent(typeof(MeshFilter)) as MeshFilter;

		if(meshRenderer == null)
		{
			meshRenderer = gameObject.AddComponent("MeshRenderer") as MeshRenderer;
		}
		if(meshFilter == null)
		{
			meshFilter = gameObject.AddComponent("MeshFilter") as MeshFilter;
		}

		if(meshFilter.sharedMesh == null)
		{
			DestroyImmediate(meshFilter.sharedMesh);
			meshFilter.mesh = new Mesh();
		}

		if(meshFilter.sharedMesh.vertexCount == 0)
		{
			meshIsDirty = true;
		}

		if(spriteSheet != null)
		{
			GenerateMesh(meshFilter.sharedMesh);
		}

		if(!Application.isPlaying)
		{
			SnapToIntegerPosition();
			//SnapToScale();
		}
		else
		{
			//SnapToScale();
		}

		if(spriteSheet != null)
		{
			if(meshRenderer.sharedMaterial != spriteSheet.atlas)
			{
				meshRenderer.sharedMaterial = spriteSheet.atlas;
			}
		}
	}

	public void GenerateMesh(Mesh mesh)
	{
		if(meshIsDirty)
		{
			mesh.Clear();
		}

		Rect uv = new Rect();
		int[] triangles = null;
		Vector3[] verts = null;
		Vector2[] uvs = null;
		Color[] colors = null;

		int tIndex = 0;
		int uvIndex = 0;
		int vIndex = 0;
		int cIndex = 0;

		int quadCountX;
		int quadCountY;
		int quadCount;

		float pivotOffsetX;
		float pivotOffsetY;

		float xMin;
		float yMin;
		float uvWidth;
		float uvHeight;

		float left;
		float top;
		float offX;
		float offY;

		RagePixelRow currentRow = GetCurrentRow();
		RagePixelCell currentCell = GetCurrentCell();

		if(pixelSizeX > 0 && pixelSizeY > 0)
		{
			switch(mode)
			{
			case(Mode.Default):
				quadCountX = Mathf.CeilToInt((float)pixelSizeX / (float)currentRow.pixelSizeX);
				quadCountY = Mathf.CeilToInt((float)pixelSizeY / (float)currentRow.pixelSizeY);
				quadCount = quadCountX * quadCountY;

				pivotOffsetX = 0f;
				pivotOffsetY = 0f;

				switch(pivotMode)
				{
				case(PivotMode.BottomLeft):
					pivotOffsetX = 0f;
					pivotOffsetY = 0f;
					break;
				case (PivotMode.Bottom):
					pivotOffsetX = -pixelSizeX / 2f;
					pivotOffsetY = 0f;
					break;
				case (PivotMode.Middle):
					pivotOffsetX = -pixelSizeX / 2f;
					pivotOffsetY = -pixelSizeY / 2f;
					break;
				}

				triangles = new int[quadCount * 6];
				verts = new Vector3[quadCount * 4];
				uvs = new Vector2[quadCount * 4];
				colors = new Color[quadCount * 4];

				uv = currentCell.uv;
				xMin = uv.xMin;
				yMin = uv.yMin;
				uvWidth = uv.width;
				uvHeight = uv.height;

				for(int qy = 0; qy < quadCountY; qy++)
				{
					for(int qx = 0; qx < quadCountX; qx++)
					{
						left = (float)qx * (float)currentRow.pixelSizeX + pivotOffsetX;
						top = (float)qy * (float)currentRow.pixelSizeY + pivotOffsetY;
						offX = (float)currentRow.pixelSizeX;
						offY = (float)currentRow.pixelSizeY;

						if(qy == quadCountY - 1)
						{
							offY = (float)(pixelSizeY % currentRow.pixelSizeY);
							if(Mathf.Approximately(offY, 0f))
							{
								offY = (float)currentRow.pixelSizeY;
							}
						}
						if(qx == quadCountX - 1)
						{
							offX = (float)(pixelSizeX % currentRow.pixelSizeX);
							if(Mathf.Approximately(offX, 0f))
							{
								offX = (float)currentRow.pixelSizeX;
							}
						}

						if(meshIsDirty)
						{
							int triangleTmp = (qy * quadCountX + qx) * 4;
							triangles[tIndex++] = triangleTmp + 0;
							triangles[tIndex++] = triangleTmp + 1;
							triangles[tIndex++] = triangleTmp + 2;
							triangles[tIndex++] = triangleTmp + 0;
							triangles[tIndex++] = triangleTmp + 2;
							triangles[tIndex++] = triangleTmp + 3;

							verts[vIndex++] = new Vector3(left, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top, 0f);
							verts[vIndex++] = new Vector3(left, top, 0f);
						}

						uvs[uvIndex++] = new Vector2(xMin, yMin + uvHeight * (offY / (float)currentRow.pixelSizeY));
						uvs[uvIndex++] = new Vector2(xMin + uvWidth * (offX / (float)currentRow.pixelSizeX), yMin + uvHeight * (offY / (float)currentRow.pixelSizeY));
						uvs[uvIndex++] = new Vector2(xMin + uvWidth * (offX / (float)currentRow.pixelSizeX), yMin);
						uvs[uvIndex++] = new Vector2(xMin, yMin);

						if(flipHorizontal)
						{
							Vector2 tmp = uvs[uvIndex - 1];
							uvs[uvIndex - 1] = uvs[uvIndex - 2];
							uvs[uvIndex - 2] = tmp;
							Vector2 tmp2 = uvs[uvIndex - 3];
							uvs[uvIndex - 3] = uvs[uvIndex - 4];
							uvs[uvIndex - 4] = tmp2;
						}

						if(flipVertical)
						{
							Vector2 tmp = uvs[uvIndex - 1];
							uvs[uvIndex - 1] = uvs[uvIndex - 4];
							uvs[uvIndex - 4] = tmp;
							Vector2 tmp2 = uvs[uvIndex - 2];
							uvs[uvIndex - 2] = uvs[uvIndex - 3];
							uvs[uvIndex - 3] = tmp2;
						}

						if(vertexColorsAreDirty || meshIsDirty)
						{
							colors[cIndex++] = tintColor;
							colors[cIndex++] = tintColor;
							colors[cIndex++] = tintColor;
							colors[cIndex++] = tintColor;
						}
					}
				}
				break;
			case (Mode.Grid9):
				quadCountX = Mathf.CeilToInt((float)(pixelSizeX - grid9Left - grid9Right) / ((float)currentRow.pixelSizeX - grid9Left - grid9Right));
				quadCountY = Mathf.CeilToInt((float)(pixelSizeY - grid9Bottom - grid9Top) / ((float)currentRow.pixelSizeY - grid9Bottom - grid9Top));
				quadCount = quadCountX * quadCountY;

				pivotOffsetX = 0f;
				pivotOffsetY = 0f;

				switch(pivotMode)
				{
				case(PivotMode.BottomLeft):
					pivotOffsetX = 0f;
					pivotOffsetY = 0f;
					break;
				case (PivotMode.Bottom):
					pivotOffsetX = -pixelSizeX / 2f;
					pivotOffsetY = 0f;
					break;
				case (PivotMode.Middle):
					pivotOffsetX = -pixelSizeX / 2f;
					pivotOffsetY = -pixelSizeY / 2f;
					break;
				}

				int edgeQuadCount = 0;
				if(grid9Bottom > 0f && grid9Left > 0f)
				{
					edgeQuadCount++;
				}
				if(grid9Bottom > 0f && grid9Right > 0f)
				{
					edgeQuadCount++;
				}
				if(grid9Top > 0f && grid9Left > 0f)
				{
					edgeQuadCount++;
				}
				if(grid9Top > 0f && grid9Right > 0f)
				{
					edgeQuadCount++;
				}
				if(grid9Bottom > 0f)
				{
					edgeQuadCount += quadCountX;
				}
				if(grid9Top > 0f)
				{
					edgeQuadCount += quadCountX;
				}
				if(grid9Left > 0f)
				{
					edgeQuadCount += quadCountY;
				}
				if(grid9Right > 0f)
				{
					edgeQuadCount += quadCountY;
				}

				triangles = new int[quadCount * 6 + edgeQuadCount * 6];
				verts = new Vector3[quadCount * 4 + edgeQuadCount * 4];
				uvs = new Vector2[quadCount * 4 + edgeQuadCount * 4];
				colors = new Color[quadCount * 4 + edgeQuadCount * 4];

				uv = currentCell.uv;
				xMin = uv.xMin;
				yMin = uv.yMin;
				uvWidth = uv.width;
				uvHeight = uv.height;

				for(int qy = 0; qy < quadCountY; qy++)
				{
					for(int qx = 0; qx < quadCountX; qx++)
					{
						if(qy == 0 && grid9Bottom > 0f)
						{
							left = (float)qx * (float)(currentRow.pixelSizeX - grid9Left - grid9Right) + pivotOffsetX + grid9Left;
							top = pivotOffsetY;
							offX = (float)(currentRow.pixelSizeX - grid9Left - grid9Right);
							offY = grid9Bottom;

							if(qx == quadCountX - 1)
							{
								offX = (float)((pixelSizeX - grid9Left - grid9Right) % (currentRow.pixelSizeX - grid9Left - grid9Right));
								if(Mathf.Approximately(offX, 0f))
								{
									offX = (float)(currentRow.pixelSizeX - grid9Left - grid9Right);
								}
							}

							if(meshIsDirty)
							{
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 1;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 3;
							}

							verts[vIndex++] = new Vector3(left, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top, 0f);
							verts[vIndex++] = new Vector3(left, top, 0f);

							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(offX + grid9Left) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(offX + grid9Left) / (float)currentRow.pixelSizeX), yMin);
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin);

							if(vertexColorsAreDirty || meshIsDirty)
							{
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
							}
						}

						if(qy == quadCountY - 1 && grid9Top > 0f)
						{
							left = (float)qx * (float)(currentRow.pixelSizeX - grid9Left - grid9Right) + pivotOffsetX + grid9Left;
							top = pivotOffsetY + pixelSizeY - grid9Top;
							offX = (float)(currentRow.pixelSizeX - grid9Left - grid9Right);
							offY = grid9Top;

							if(qx == quadCountX - 1)
							{
								offX = (float)((pixelSizeX - grid9Left - grid9Right) % (currentRow.pixelSizeX - grid9Left - grid9Right));
								if(Mathf.Approximately(offX, 0f))
								{
									offX = (float)(currentRow.pixelSizeX - grid9Left - grid9Right);
								}
							}

							if(meshIsDirty)
							{
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 1;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 3;
							}

							verts[vIndex++] = new Vector3(left, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top, 0f);
							verts[vIndex++] = new Vector3(left, top, 0f);

							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight);
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(offX + grid9Left) / (float)currentRow.pixelSizeX), yMin + uvHeight);
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(offX + grid9Left) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(currentRow.pixelSizeY - grid9Top) / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(currentRow.pixelSizeY - grid9Top) / (float)currentRow.pixelSizeY));

							if(vertexColorsAreDirty || meshIsDirty)
							{
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
							}
						}

						if(qx == 0 && grid9Left > 0f)
						{
							left = pivotOffsetX;
							top = (float)qy * (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top) + pivotOffsetY + grid9Bottom;
							offX = grid9Left;
							offY = (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top);

							if(qy == quadCountY - 1)
							{
								offY = (float)((pixelSizeY - grid9Bottom - grid9Top) % (currentRow.pixelSizeY - grid9Bottom - grid9Top));
								if(Mathf.Approximately(offY, 0f))
								{
									offY = (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top);
								}
							}

							if(meshIsDirty)
							{
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 1;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 3;
							}

							verts[vIndex++] = new Vector3(left, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top, 0f);
							verts[vIndex++] = new Vector3(left, top, 0f);

							uvs[uvIndex++] = new Vector2(xMin, yMin + uvHeight * ((float)(offY + grid9Bottom) / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(offY + grid9Bottom) / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin, yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));

							if(vertexColorsAreDirty || meshIsDirty)
							{
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
							}
						}

						if(qx == quadCountX - 1 && grid9Right > 0f)
						{
							left = (float)(pivotOffsetX + pixelSizeX - grid9Right);
							top = (float)qy * (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top) + pivotOffsetY + grid9Bottom;
							offX = (float)grid9Right;
							offY = (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top);

							if(qy == quadCountY - 1)
							{
								offY = (float)((pixelSizeY - grid9Bottom - grid9Top) % (currentRow.pixelSizeY - grid9Bottom - grid9Top));
								if(Mathf.Approximately(offY, 0f))
								{
									offY = (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top);
								}
							}

							if(meshIsDirty)
							{
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 1;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 0;
								triangles[tIndex++] = vIndex + 2;
								triangles[tIndex++] = vIndex + 3;
							}

							verts[vIndex++] = new Vector3(left, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top, 0f);
							verts[vIndex++] = new Vector3(left, top, 0f);

							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(currentRow.pixelSizeX - grid9Right) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(offY + grid9Bottom) / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth, yMin + uvHeight * ((float)(offY + grid9Bottom) / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth, yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
							uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(currentRow.pixelSizeX - grid9Right) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));

							if(vertexColorsAreDirty || meshIsDirty)
							{
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
								colors[cIndex++] = tintColor;
							}
						}

						left = (float)qx * (float)(currentRow.pixelSizeX - grid9Left - grid9Right) + pivotOffsetX + grid9Left;
						top = (float)qy * (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top) + pivotOffsetY + grid9Bottom;
						offX = (float)(currentRow.pixelSizeX - grid9Left - grid9Right);
						offY = (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top);

						if(qy == quadCountY - 1)
						{
							offY = (float)((pixelSizeY - grid9Bottom - grid9Top) % (currentRow.pixelSizeY - grid9Bottom - grid9Top));
							if(Mathf.Approximately(offY, 0f))
							{
								offY = (float)(currentRow.pixelSizeY - grid9Bottom - grid9Top);
							}
						}
						if(qx == quadCountX - 1)
						{
							offX = (float)((pixelSizeX - grid9Left - grid9Right) % (currentRow.pixelSizeX - grid9Left - grid9Right));
							if(Mathf.Approximately(offX, 0f))
							{
								offX = (float)(currentRow.pixelSizeX - grid9Left - grid9Right);
							}
						}

						if(meshIsDirty)
						{
							//int triangleTmp = (qy * quadCountX + qx) * 4;
							triangles[tIndex++] = vIndex + 0;
							triangles[tIndex++] = vIndex + 1;
							triangles[tIndex++] = vIndex + 2;
							triangles[tIndex++] = vIndex + 0;
							triangles[tIndex++] = vIndex + 2;
							triangles[tIndex++] = vIndex + 3;

							verts[vIndex++] = new Vector3(left, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
							verts[vIndex++] = new Vector3(left + offX, top, 0f);
							verts[vIndex++] = new Vector3(left, top, 0f);
						}

						uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(offY + grid9Bottom) / (float)currentRow.pixelSizeY));
						uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(offX + grid9Left) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(offY + grid9Bottom) / (float)currentRow.pixelSizeY));
						uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(offX + grid9Left) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
						uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));

						if(flipHorizontal)
						{
							Vector2 tmp = uvs[uvIndex - 1];
							uvs[uvIndex - 1] = uvs[uvIndex - 2];
							uvs[uvIndex - 2] = tmp;
							Vector2 tmp2 = uvs[uvIndex - 3];
							uvs[uvIndex - 3] = uvs[uvIndex - 4];
							uvs[uvIndex - 4] = tmp2;
						}

						if(flipVertical)
						{
							Vector2 tmp = uvs[uvIndex - 1];
							uvs[uvIndex - 1] = uvs[uvIndex - 4];
							uvs[uvIndex - 4] = tmp;
							Vector2 tmp2 = uvs[uvIndex - 2];
							uvs[uvIndex - 2] = uvs[uvIndex - 3];
							uvs[uvIndex - 3] = tmp2;
						}

						if(vertexColorsAreDirty || meshIsDirty)
						{
							colors[cIndex++] = tintColor;
							colors[cIndex++] = tintColor;
							colors[cIndex++] = tintColor;
							colors[cIndex++] = tintColor;
						}
					}
				}

				if(grid9Bottom > 0f && grid9Left > 0f)
				{
					left = pivotOffsetX;
					top = pivotOffsetY;
					offX = grid9Left;
					offY = grid9Bottom;

					if(meshIsDirty)
					{
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 1;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 3;
					}

					verts[vIndex++] = new Vector3(left, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top, 0f);
					verts[vIndex++] = new Vector3(left, top, 0f);

					uvs[uvIndex++] = new Vector2(xMin, yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin);
					uvs[uvIndex++] = new Vector2(xMin, yMin);

					if(vertexColorsAreDirty || meshIsDirty)
					{
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
					}
				}

				if(grid9Bottom > 0f && grid9Right > 0f)
				{
					left = pivotOffsetX + pixelSizeX - grid9Right;
					top = pivotOffsetY;
					offX = grid9Right;
					offY = grid9Bottom;

					if(meshIsDirty)
					{
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 1;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 3;
					}

					verts[vIndex++] = new Vector3(left, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top, 0f);
					verts[vIndex++] = new Vector3(left, top, 0f);

					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(currentRow.pixelSizeX - grid9Right) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
					uvs[uvIndex++] = new Vector2(xMin + uvWidth, yMin + uvHeight * ((float)grid9Bottom / (float)currentRow.pixelSizeY));
					uvs[uvIndex++] = new Vector2(xMin + uvWidth, yMin);
					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(currentRow.pixelSizeX - grid9Right) / (float)currentRow.pixelSizeX), yMin);

					if(vertexColorsAreDirty || meshIsDirty)
					{
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
					}
				}

				if(grid9Top > 0f && grid9Right > 0f)
				{
					left = pivotOffsetX + pixelSizeX - grid9Right;
					top = pivotOffsetY + pixelSizeY - grid9Top;
					offX = grid9Right;
					offY = grid9Top;

					if(meshIsDirty)
					{
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 1;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 3;
					}

					verts[vIndex++] = new Vector3(left, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top, 0f);
					verts[vIndex++] = new Vector3(left, top, 0f);

					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(currentRow.pixelSizeX - grid9Right) / (float)currentRow.pixelSizeX), yMin + uvHeight);
					uvs[uvIndex++] = new Vector2(xMin + uvWidth, yMin + uvHeight);
					uvs[uvIndex++] = new Vector2(xMin + uvWidth, yMin + uvHeight * ((float)(currentRow.pixelSizeY - grid9Top) / (float)currentRow.pixelSizeY));
					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)(currentRow.pixelSizeX - grid9Right) / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(currentRow.pixelSizeY - grid9Top) / (float)currentRow.pixelSizeY));

					if(vertexColorsAreDirty || meshIsDirty)
					{
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
					}
				}

				if(grid9Top > 0f && grid9Left > 0f)
				{
					left = pivotOffsetX;
					top = pivotOffsetY + pixelSizeY - grid9Top;
					offX = grid9Left;
					offY = grid9Top;

					if(meshIsDirty)
					{
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 1;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 0;
						triangles[tIndex++] = vIndex + 2;
						triangles[tIndex++] = vIndex + 3;
					}

					verts[vIndex++] = new Vector3(left, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top + offY, 0f);
					verts[vIndex++] = new Vector3(left + offX, top, 0f);
					verts[vIndex++] = new Vector3(left, top, 0f);

					uvs[uvIndex++] = new Vector2(xMin, yMin + uvHeight);
					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight);
					uvs[uvIndex++] = new Vector2(xMin + uvWidth * ((float)grid9Left / (float)currentRow.pixelSizeX), yMin + uvHeight * ((float)(currentRow.pixelSizeY - grid9Top) / (float)currentRow.pixelSizeY));
					uvs[uvIndex++] = new Vector2(xMin, yMin + uvHeight * ((float)(currentRow.pixelSizeY - grid9Top) / (float)currentRow.pixelSizeY));

					if(vertexColorsAreDirty || meshIsDirty)
					{
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
						colors[cIndex++] = tintColor;
					}
				}
				break;
			}
		}


		if(meshIsDirty)
		{
			mesh.vertices = verts;
			mesh.triangles = triangles;
		}
		if(vertexColorsAreDirty || meshIsDirty)
		{
			mesh.colors = colors;
		}

		mesh.uv = uvs;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		meshIsDirty = false;
		vertexColorsAreDirty = false;
	}

	public void checkKeyIntegrity()
	{
		if(!GetCurrentRow().key.Equals(currentRowKey))
		{
			currentRowKey = GetCurrentRow().key;
		}
		if(!GetCurrentCell().key.Equals(currentCellKey))
		{
			currentCellKey = GetCurrentCell().key;
		}
	}

	public void OnDestroy()
	{
		MeshFilter meshFilter = GetComponent(typeof(MeshFilter)) as MeshFilter;
		if(meshFilter != null)
		{
			DestroyImmediate(meshFilter.sharedMesh);
		}
	}

	public void shiftCell(int amount, bool loop)
	{
		int currIndex = GetCurrentRow().GetIndex(currentCellKey);
		if(frameMode == FrameMode.Range)
		{
			if(currIndex + amount >= Mathf.Max(0, animationMinIndex) && (currIndex + amount < GetCurrentRow().cells.Length && animationMaxIndex < 0 || currIndex + amount <= animationMaxIndex))
			{
				currentCellKey = GetCurrentRow().cells[currIndex + amount].key;
			}
			else if(loop)
			{
				if(amount > 0)
				{
					currentCellKey = GetCurrentRow().cells[Mathf.Max(animationMinIndex, 0)].key;
				}
				else
				{
					if(animationMaxIndex >= 0)
					{
						currentCellKey = GetCurrentRow().cells[Mathf.Min(animationMaxIndex, GetCurrentRow().cells.Length - 1)].key;
					}
					else
					{
						currentCellKey = GetCurrentRow().cells[GetCurrentRow().cells.Length - 1].key;
					}
				}
			}
		}
		else if(frameMode == FrameMode.Sequence)
		{
			int nextFrame = animationCurrentFrame + amount;
			if(nextFrame < animationFrames.Length && nextFrame >= 0)
			{
				animationCurrentFrame = nextFrame;
				currentCellKey = GetCurrentRow().cells[animationFrames[animationCurrentFrame]].key;
			}
			else if(loop)
			{
				if(amount > 0)
				{
					while(nextFrame >= animationFrames.Length)
					{
						nextFrame -= animationFrames.Length;
					}
					animationCurrentFrame = nextFrame;
				}
				else
				{
					while(nextFrame < 0)
					{
						nextFrame += animationFrames.Length;
					}
					animationCurrentFrame = nextFrame;
				}
				currentCellKey = GetCurrentRow().cells[animationFrames[animationCurrentFrame]].key;
			}
		}
	}

	public void shiftRow(int amount)
	{
		int currIndex = spriteSheet.GetIndex(currentRowKey);
		if(currIndex + amount >= 0 && currIndex + amount < spriteSheet.rows.Length)
		{
			currentRowKey = spriteSheet.rows[currIndex + amount].key;
			currentCellKey = GetCurrentRow().cells[0].key;
		}
		else
		{
			if(currIndex < 0)
			{
				//noop
			}
			else
			{
				if(amount > 0)
				{
					currentRowKey = spriteSheet.rows[0].key;
				}
				else
				{
					currentRowKey = spriteSheet.rows[spriteSheet.rows.Length - 1].key;
				}
				currentCellKey = GetCurrentRow().cells[0].key;
			}
		}
	}

	public void selectRow(int index)
	{
		if(index >= 0 && index < spriteSheet.rows.Length)
		{
			currentRowKey = spriteSheet.rows[index].key;
		}
	}

	public void selectCell(int index)
	{
		currentCellKey = GetCurrentRow().cells[0].key;
	}

	public string getCurrentRowName()
	{
		return GetCurrentRow().name;
	}

	public RagePixelRow GetCurrentRow()
	{
		if(Application.isPlaying)
		{
			if(lastRowCacheKey == currentRowKey && lastRowSpriteSheetCache.Equals(spriteSheet))
			{
				return lastRowCache;
			}
			else
			{
				lastRowCache = spriteSheet.GetRow(currentRowKey);
				lastRowCacheKey = currentRowKey;
				lastRowSpriteSheetCache = spriteSheet;
				return lastRowCache;
			}
		}
		else
		{
			return spriteSheet.GetRow(currentRowKey);
		}
	}

	public RagePixelCell GetCurrentCell()
	{
		if(Application.isPlaying)
		{
			if(lastCellCacheKey == currentCellKey && lastCellSpriteSheetCache.Equals(spriteSheet))
			{
				return lastCellCache;
			}
			else
			{
				lastCellCache = GetCurrentRow().GetCell(currentCellKey);
				lastCellCacheKey = currentCellKey;
				lastCellSpriteSheetCache = spriteSheet;
				return lastCellCache;
			}
		}
		else
		{
			return GetCurrentRow().GetCell(currentCellKey);
		}
	}

	public int GetCurrentCellIndex()
	{
		return GetCurrentRow().GetIndex(GetCurrentCell().key);
	}

	public int GetCurrentRowIndex()
	{
		return spriteSheet.GetIndex(currentRowKey);
	}

	public void OnDrawGizmosSelected()
	{
		if(toBeRefreshed)
		{
			refreshMesh();
			toBeRefreshed = false;
		}
	}

	bool AnimationIsOver
	{
		get
		{
			switch(animationMode)
			{
			case AnimationMode.PlayOnce:
				if(frameMode == FrameMode.Range)
				{
					return
						(GetCurrentRow().GetIndex(currentCellKey) >= GetCurrentRow().cells.Length &&
						 animationMaxIndex < 0) ||
						GetCurrentRow().GetIndex(currentCellKey) >= animationMaxIndex;
				}
				else if(frameMode == FrameMode.Sequence)
				{
					return
						animationCurrentFrame >= animationFrames.Length - 1;
				}
				break;
			case AnimationMode.PlayOnceReverse:
				if(frameMode == FrameMode.Range)
				{
					return
						 GetCurrentRow().GetIndex(currentCellKey) <= Mathf.Max(animationMinIndex, 0);
				}
				else if(frameMode == FrameMode.Sequence)
				{
					return
						animationCurrentFrame <= 0;
				}
				break;
			case AnimationMode.PingPong:
				if(animationPingPongDirection == 1)
				{
					if(frameMode == FrameMode.Range)
					{
						return
							(GetCurrentRow().GetIndex(currentCellKey) >= GetCurrentRow().cells.Length &&
							 animationMaxIndex < 0) ||
							GetCurrentRow().GetIndex(currentCellKey) >= animationMaxIndex;
					}
					else if(frameMode == FrameMode.Sequence)
					{
						return
							animationCurrentFrame >= animationFrames.Length - 1;
					}
				}
				else
				{
					if(frameMode == FrameMode.Range)
					{
						return
							GetCurrentRow().GetIndex(currentCellKey) <= Mathf.Max(animationMinIndex, 0);
					}
					else if(frameMode == FrameMode.Sequence)
					{
						return
							animationCurrentFrame <= 0;
					}
				}
				break;
			default:
				return true;
			}
			Debug.LogError("Unrecognized frame mode(" + frameMode + ")/animation mode(" + animationMode + ") combo");
			return false;
		}
	}

	void GoToNextFrame()
	{
		switch(animationMode)
		{
		case AnimationMode.PlayOnce:
			shiftCell(1, false);
			break;
		case AnimationMode.PlayOnceReverse:
			shiftCell(-1, false);
			break;
		case AnimationMode.Loop:
			shiftCell(1, true);
			break;
		case AnimationMode.LoopReverse:
			shiftCell(-1, true);
			break;
		case AnimationMode.PingPong:
			if(AnimationIsOver)
			{
				animationPingPongDirection *= -1;
			}
			shiftCell(animationPingPongDirection, true);
			break;
		}
		nextAnimFrame = myTime + GetCurrentCell().delay / 1000f;
	}

	void Update()
	{
		if(playAnimation)
		{
			myTime += Time.deltaTime;
			if(myTime > 1000f)
			{
				nextAnimFrame -= myTime;
				myTime = 0f;
			}
			if(nextAnimFrame < myTime)
			{
				switch(animationMode)
				{
				case (AnimationMode.PlayOnce):
				case (AnimationMode.PlayOnceReverse):
					if(!AnimationIsOver)
					{
						while(nextAnimFrame < myTime)
						{
							GoToNextFrame();
						}
						refreshMesh();
					}
					else
					{
						playAnimation = false;
					}
					break;

				case (AnimationMode.Loop):
				case (AnimationMode.LoopReverse):
				case (AnimationMode.PingPong):
					while(nextAnimFrame < myTime)
					{
						GoToNextFrame();
					}
					refreshMesh();

					break;
				}
			}
		}
	}

	private void DrawRectangle(Rect rect, Color color)
	{
		Color oldColor = Gizmos.color;

		Gizmos.color = color;
		Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMin, 0f), new Vector3(rect.xMax, rect.yMin, 0f));
		Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMin, 0f), new Vector3(rect.xMax, rect.yMax, 0f));
		Gizmos.DrawLine(new Vector3(rect.xMax, rect.yMax, 0f), new Vector3(rect.xMin, rect.yMax, 0f));
		Gizmos.DrawLine(new Vector3(rect.xMin, rect.yMax, 0f), new Vector3(rect.xMin, rect.yMin, 0f));

		Gizmos.color = oldColor;
	}


	// API
	public void SetSprite(string name)
	{
		int key = spriteSheet.GetRowByName(name).key;
		if(key != 0)
		{
			currentRowKey = spriteSheet.GetRowByName(name).key;
			currentCellKey = GetCurrentRow().cells[0].key;
			meshIsDirty = true;
			refreshMesh();
		}
		else
		{
			Debug.Log("ERROR: No RagePixel sprite with name " + name + " found!");
		}
	}

	public void SetSprite(string name, int frameIndex)
	{
		int key = spriteSheet.GetRowByName(name).key;
		if(key != 0)
		{
			currentRowKey = spriteSheet.GetRowByName(name).key;
			if(GetCurrentRow().cells.Length > frameIndex)
			{
				currentCellKey = GetCurrentRow().cells[frameIndex].key;
				meshIsDirty = true;
				refreshMesh();
			}
			else
			{
				Debug.Log("ERROR: RagePixel has only " + GetCurrentRow().cells.Length + " frames!");
			}
		}
		else
		{
			Debug.Log("ERROR: No RagePixel sprite with name " + name + " found!");
		}
	}

	public int GetSizeX()
	{
		return pixelSizeX;
	}

	public int GetSizeY()
	{
		return pixelSizeY;
	}

	public void PlayAnimation()
	{
		PlayAnimation(false);
	}

	public void PlayAnimation(bool forceRestart)
	{
		if(frameMode == FrameMode.Range)
		{
			PlayAnimation(forceRestart, animationMinIndex, animationMaxIndex);
		}
		else
		{
			PlayAnimation(forceRestart, animationFrames);
		}
	}

	public void PlayAnimation(bool forceRestart, int rangeMinIndex, int rangeMaxIndex, int pri=0)
	{
		PlayAnimation(forceRestart, animationMode, rangeMinIndex, rangeMaxIndex, pri);
	}

	public void PlayAnimation(bool forceRestart, int[] frames, int pri=0)
	{
		PlayAnimation(forceRestart, animationMode, frames, pri);
	}

	public void PlayAnimation(bool forceRestart, AnimationMode animMode, int rangeMinIndex, int rangeMaxIndex, int pri=0)
	{
		if(playAnimation == false || forceRestart || pri >= currentAnimationPriority)
		{
			currentAnimationPriority = pri;
			currentAnimationName = null;
			animationMode = (AnimationMode)animMode;
			frameMode = FrameMode.Range;
			animationMinIndex = Mathf.Clamp(rangeMinIndex, 0, GetCurrentRow().cells.Length - 1);
			animationMaxIndex = Mathf.Clamp(rangeMaxIndex, animationMinIndex, GetCurrentRow().cells.Length - 1);
			CheckAnimOnPlay();
			nextAnimFrame = myTime + GetCurrentCell().delay / 1000f;
			playAnimation = true;
		}
	}

	public void PlayAnimation(bool forceRestart, AnimationMode animMode, int[] frames, int pri=0)
	{
		if(playAnimation == false || forceRestart || pri >= currentAnimationPriority)
		{
			currentAnimationPriority = pri;
			currentAnimationName = null;
			animationMode = (AnimationMode)animMode;
			frameMode = FrameMode.Sequence;
			animationFrames = frames;
			CheckAnimOnPlay();
			nextAnimFrame = myTime + GetCurrentCell().delay / 1000f;
			playAnimation = true;
		}
	}

	public void CheckAnimOnPlay()
	{
		if(frameMode == FrameMode.Range)
		{
			switch(animationMode)
			{
			case (AnimationMode.PlayOnce):
			case (AnimationMode.Loop):
			case (AnimationMode.PingPong):
				currentCellKey = GetCurrentRow().cells[Mathf.Clamp(animationMinIndex, 0, GetCurrentRow().cells.Length - 1)].key;
				refreshMesh();
				break;
			case (AnimationMode.LoopReverse):
			case (AnimationMode.PlayOnceReverse):
				if(animationMaxIndex >= 0)
				{
					currentCellKey = GetCurrentRow().cells[animationMaxIndex].key;
				}
				else
				{
					currentCellKey = GetCurrentRow().cells.Length - 1;
				}
				refreshMesh();
				break;
			}
		}
		else if(frameMode == FrameMode.Sequence)
		{
			if(animationFrames == null || animationFrames.Length == 0)
			{
				animationFrames = new int[]{0};
			}
			switch(animationMode)
			{
			case (AnimationMode.PlayOnce):
			case (AnimationMode.Loop):
			case (AnimationMode.PingPong):
				animationCurrentFrame = 0;
				currentCellKey = GetCurrentRow().cells[animationFrames[0]].key;
				break;
			case (AnimationMode.LoopReverse):
			case (AnimationMode.PlayOnceReverse):
				animationCurrentFrame = animationFrames.Length - 1;
				currentCellKey = GetCurrentRow().cells[animationFrames[animationFrames.Length - 1]].key;
				break;
			}
			refreshMesh();
		}
	}

	public bool HasNamedAnimation(string name) {
		return GetCurrentRow().GetAnimationByName(name) != null;
	}

	public void PlayNamedAnimation(string name, int pri=0)
	{
		PlayNamedAnimation(name, false, 0, pri);
	}

	public void PlayNamedAnimation(string name, bool forceRestart, int pri=0)
	{
		PlayNamedAnimation(name, forceRestart, 0, pri);
	}

	public bool AnimationIsDifferent(RagePixelAnimation a)
	{
		if(a.name != currentAnimationName ||
			 a.mode != animationMode ||
			 a.frameMode != frameMode) return true;
		if(a.frameMode == FrameMode.Range)
		{
			return
				animationMinIndex != a.startIndex ||
				animationMaxIndex != a.endIndex;
		}
		else if(a.frameMode == FrameMode.Sequence)
		{
			if(a.frames == animationFrames) return false;
			if(a.frames == null ||
				 animationFrames == null ||
				 a.frames.Length != animationFrames.Length) return true;
			for(int i = 0; i < a.frames.Length; i++)
			{
				if(a.frames[i] == animationFrames[i]) return true;
			}
		}
		return true;
	}

	public void PlayNamedAnimation(string name, bool forceRestart, float delayFirstFrame, int pri=0)
	{
		RagePixelAnimation animation = GetCurrentRow().GetAnimationByName(name);
		if(animation != null)
		{
			if((playAnimation == false ||
				forceRestart ||
				AnimationIsDifferent(animation) ||
				AnimationIsOver) &&
				(pri >= currentAnimationPriority))
			{
				currentAnimationName = name;
				currentAnimationPriority = pri;
				animationFrames = animation.frames;

				animationMinIndex = Mathf.Clamp(animation.startIndex, 0, GetCurrentRow().cells.Length - 1);
				animationMaxIndex = Mathf.Clamp(animation.endIndex, animationMinIndex, GetCurrentRow().cells.Length - 1);
				animationMode = animation.mode;
				frameMode = animation.frameMode;
				CheckAnimOnPlay();

				nextAnimFrame = myTime + GetCurrentCell().delay / 1000f + delayFirstFrame;
				playAnimation = true;
			}
		}
		else
		{
			Debug.Log("No animation: " + name + " found in the sprite: " + (GetCurrentRow().name.Length > 0 ? GetCurrentRow().name : "empty"));
		}
	}

	public void StopAnimation()
	{
		playAnimation = false;
	}

	public bool isPlaying()
	{
		return playAnimation;
	}

	public void SetSize(int height, int width)
	{
		if(height >= 1 && width >= 1 && (width != pixelSizeX || height != pixelSizeY))
		{
			pixelSizeX = width;
			pixelSizeY = height;
			meshIsDirty = true;
			refreshMesh();
		}
	}

	public Rect GetRect()
	{
		return new Rect(
			transform.position.x + GetPivotOffset().x,
			transform.position.y + GetPivotOffset().y,
			pixelSizeX,
			pixelSizeY
		);
	}

	public Vector3 GetPivotOffset()
	{
		Vector3 pivotOffset = new Vector3();

		switch(pivotMode)
		{
		case (RagePixelSprite.PivotMode.BottomLeft):
			pivotOffset.x = 0f;
			pivotOffset.y = 0f;
			break;
		case (RagePixelSprite.PivotMode.Bottom):
			pivotOffset.x = -pixelSizeX / 2f;
			pivotOffset.y = 0f;
			break;
		case (RagePixelSprite.PivotMode.Middle):
			pivotOffset.x = -pixelSizeX / 2f;
			pivotOffset.y = -pixelSizeY / 2f;
			break;
		}

		return pivotOffset;
	}

	public void SetHorizontalFlip(bool value)
	{
		if(value != flipHorizontal)
		{
			flipHorizontal = value;
			meshIsDirty = true;
			refreshMesh();
		}
	}

	public void SetVerticalFlip(bool value)
	{
		if(value != flipVertical)
		{
			flipVertical = value;
			meshIsDirty = true;
			refreshMesh();
		}
	}

	public void SetTintColor(Color color)
	{
		tintColor = color;
		vertexColorsAreDirty = true;
		refreshMesh();
	}

}
