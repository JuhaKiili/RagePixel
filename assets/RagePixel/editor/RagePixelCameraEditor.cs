using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

[CustomEditor(typeof(RagePixelCamera))]
public class RagePixelCameraEditor : Editor
{

	// Use this for initialization
	void Start()
	{

	}

	public override void OnInspectorGUI()
	{
		//DrawDefaultInspector();

		RagePixelCamera ragePixelCamera = target as RagePixelCamera;
		ragePixelCamera.pixelSize = EditorGUILayout.IntField("Pixel size", ragePixelCamera.pixelSize);
		ragePixelCamera.snapToIntegerPositions = EditorGUILayout.Toggle("Snap to Integral Positions", ragePixelCamera.snapToIntegerPositions);
		ragePixelCamera.resolutionPixelWidth = EditorGUILayout.IntField("Resolution width", ragePixelCamera.resolutionPixelWidth);
		ragePixelCamera.resolutionPixelHeight = EditorGUILayout.IntField("Resolution height", ragePixelCamera.resolutionPixelHeight);

		if(GUILayout.Button("Apply"))
		{
			RagePixelUtil.ResetCamera(ragePixelCamera);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
