using UnityEngine;
using System.Collections;

[System.Serializable]
public class RagePixelAnimation
{
	public string name;
	public RagePixelSprite.AnimationMode mode = 0;
	public RagePixelSprite.FrameMode frameMode = 0;

	//frame mode: range
	public int startIndex;
	public int endIndex;

	//frame mode: sequence
	public int[] frames;

	public RagePixelAnimation Clone()
	{
		RagePixelAnimation anim = new RagePixelAnimation();
		anim.name = name;
		anim.mode = mode;
		anim.frameMode = frameMode;
		anim.startIndex = startIndex;
		anim.endIndex = endIndex;
		if(frames != null)
		{
			anim.frames = (int[])frames.Clone();
		}
		return anim;
	}
}
