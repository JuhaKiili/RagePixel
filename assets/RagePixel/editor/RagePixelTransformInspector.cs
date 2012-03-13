using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Transform))]
public class RagePixelTransformInspector : Editor
{
	public override void OnInspectorGUI()
	{
		Transform t = (Transform)target;
		bool useStandardTransformInspector = false;

		// Replicate the standard transform inspector gui
		if(t.gameObject.GetComponent(typeof(RagePixelSprite)))
		{
			bool clicked = false;
			RagePixelSprite ragePixelSprite = t.gameObject.GetComponent(typeof(RagePixelSprite)) as RagePixelSprite;

			if(ragePixelSprite.spriteSheet != null && !Application.isPlaying)
			{
				EditorGUIUtility.LookLikeControls();
				EditorGUI.indentLevel = 0;
				Vector3 position = EditorGUILayout.Vector2Field("Position", t.localPosition);

				//Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles);

				Vector3 scale = new Vector3();
				if(ragePixelSprite.GetCurrentRow().newPixelSizeX != ragePixelSprite.GetCurrentRow().pixelSizeX || ragePixelSprite.GetCurrentRow().newPixelSizeY != ragePixelSprite.GetCurrentRow().pixelSizeY)
				{
					GUI.color = Color.green;
					EditorGUILayout.Vector2Field("Size (bitmap)", new Vector2(ragePixelSprite.GetCurrentRow().newPixelSizeX, ragePixelSprite.GetCurrentRow().newPixelSizeY));
					GUI.color = Color.white;
					scale = new Vector2(ragePixelSprite.pixelSizeX, ragePixelSprite.pixelSizeY);
				}
				else
				{
					scale = EditorGUILayout.Vector2Field("Size", new Vector2(ragePixelSprite.pixelSizeX, ragePixelSprite.pixelSizeY));
				}
                                
				GUILayout.Label("Rotation");
				EditorGUILayout.BeginHorizontal();
				EditorGUI.indentLevel = 1;
				t.localEulerAngles = new Vector3(0f, 0f, (int)EditorGUILayout.FloatField("Degrees", -t.localEulerAngles.z)) * -1f;
				EditorGUI.indentLevel = 0;
				EditorGUILayout.EndHorizontal();
                
				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Layer ");
				ragePixelSprite.ZLayer = EditorGUILayout.IntField(ragePixelSprite.ZLayer, GUILayout.Width(30));
				if(GUILayout.Button("Forward"))
				{
					ragePixelSprite.ZLayer--;
					clicked = true;
				}
				if(GUILayout.Button("Backward"))
				{
					ragePixelSprite.ZLayer++;
					clicked = true;
				}

				GUILayout.EndHorizontal();
				position.z = ragePixelSprite.ZLayer;
				GUILayout.Space(5f);

				EditorGUIUtility.LookLikeInspector();

				if(GUI.changed || clicked)
				{
					Undo.RegisterUndo(t, "Transform Change");

					t.localPosition = FixIfNaN(position);
					ragePixelSprite.pixelSizeX = Mathf.RoundToInt(FixIfNaN(scale).x);
					ragePixelSprite.pixelSizeY = Mathf.RoundToInt(FixIfNaN(scale).y);
					ragePixelSprite.meshIsDirty = true;
					ragePixelSprite.refreshMesh();
				}
			}
			else
			{
				useStandardTransformInspector = true;
			}
		}
		else
		{
			useStandardTransformInspector = true;
		}

		if(useStandardTransformInspector)
		{
			EditorGUIUtility.LookLikeControls();
			EditorGUI.indentLevel = 0;
			Vector3 position = EditorGUILayout.Vector3Field("Position", t.localPosition);
			Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", t.localEulerAngles);
			Vector3 scale = EditorGUILayout.Vector3Field("Scale", t.localScale);
			EditorGUIUtility.LookLikeInspector();

			if(GUI.changed)
			{
				Undo.RegisterUndo(t, "Transform Change");

				t.localPosition = FixIfNaN(position);
				t.localEulerAngles = FixIfNaN(eulerAngles);
				t.localScale = FixIfNaN(scale);
			}
		}

	}

	private Vector3 FixIfNaN(Vector3 v)
	{
		if(float.IsNaN(v.x))
		{
			v.x = 0;
		}
		if(float.IsNaN(v.y))
		{
			v.y = 0;
		}
		if(float.IsNaN(v.z))
		{
			v.z = 0;
		}
		return v;
	}

}