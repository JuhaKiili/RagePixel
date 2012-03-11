using UnityEngine;
using System.Collections;

public interface IRagePixel {
    void SetSprite(string name);
    void SetSprite(string name, int frameIndex);
    void PlayAnimation();
    void PlayAnimation(bool forceRestart);
    void PlayNamedAnimation(string name);
    void PlayNamedAnimation(string name, bool forceRestart);
    void PlayNamedAnimation(string name, bool forceRestart, float delayFirstFrame);
    bool isPlaying();
    void StopAnimation();
    void SetSize(int width, int height);
    Rect GetRect();
    void SetHorizontalFlip(bool value);
    void SetVerticalFlip(bool value);
    void SetTintColor(Color color);
}
