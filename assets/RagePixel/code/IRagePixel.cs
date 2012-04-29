using UnityEngine;
using System.Collections;

public interface IRagePixel
{
	void SetSprite(string name);
	void SetSprite(string name, int frameIndex);
	void PlayAnimation();
	void PlayAnimation(bool forceRestart);
	void PlayAnimation(bool forceRestart, int rangeMinIndex, int rangeMaxIndex, int priority=0);
	void PlayAnimation(bool forceRestart, int[] frames, int pri=0);
	void PlayAnimation(bool forceRestart, RagePixelSprite.AnimationMode animMode, int rangeMinIndex, int rangeMaxIndex, int priority=0);
	void PlayAnimation(bool forceRestart, RagePixelSprite.AnimationMode animMode, int[] frames, int priority=0);
	void PlayNamedAnimation(string name, int priority=0);
	void PlayNamedAnimation(string name, bool forceRestart, int priority=0);
	void PlayNamedAnimation(string name, bool forceRestart, float delayFirstFrame, int priority=0);
	bool HasNamedAnimation(string name);
	bool isPlaying();
	void StopAnimation();
	int GetSizeX();
	int GetSizeY();
	void SetSize(int width, int height);
	Rect GetRect();
	void SetHorizontalFlip(bool value);
	void SetVerticalFlip(bool value);
	void SetTintColor(Color color);
}
