using UnityEngine;

public class RagePixelBitmap {

	private int W;
	private int H;
	public Color[] pixels;
    
	public RagePixelBitmap(Color[] _pixels, int width, int height)
	{
		W = width;
		H = height;
		pixels = new Color[_pixels.Length];
		_pixels.CopyTo(pixels, 0);
	}

	public RagePixelBitmap GetSubImage(int X, int Y, int width, int height)
	{
		Color[] _pixels = new Color[width * height];
		for(int _y = Y; _y < H; _y++)
		{
			for(int _x = X; _x < W; _x++)
			{
				_pixels[(_y - Y) * width + (_x - X)] = pixels[_y * W + _x];
			}
		}
		return new RagePixelBitmap(_pixels, width, height);
	}

	public void PasteBitmap(int X, int Y, RagePixelBitmap bitmap)
	{
		for(int y = Mathf.Max(Y, 0); (y - Y) < bitmap.Height() && y < H; y++)
		{
			for(int x = Mathf.Max(X, 0); (x - X) < bitmap.Width() && x < W; x++)
			{
				pixels[y * W + x] = bitmap.pixels[(y - Y) * bitmap.Width() + (x - X)];
				//SetPixel(x, y, bitmap.GetPixel(x - X, y - Y));
			}
		}
	}

	public void PasteBitmapAlpha(int X, int Y, RagePixelBitmap bitmap)
	{
		for(int y = Mathf.Max(Y, 0); (y - Y) < bitmap.Height() && y < H; y++)
		{
			for(int x = Mathf.Max(X, 0); (x - X) < bitmap.Width() && x < W; x++)
			{
				Color src = bitmap.pixels[(y - Y) * bitmap.Width() + (x - X)];

				pixels[y * W + x] = (1f - src.a) * pixels[y * W + x] + src.a * src;
			}
		}
	}

	public void PasteToTextureWithBounds(int X, int Y, int minX, int minY, int maxX, int maxY, Texture2D tex, Color multiplyWith)
	{
		for(int y = Mathf.Max(Y, minY, 0); y < Mathf.Min(tex.height, maxY) && (y - Y) < H; y++)
		{
			for(int x = Mathf.Max(X, minX, 0); x < Mathf.Min(tex.width, maxX) && (x - X) < W; x++)
			{
				/*
                if(!Mathf.Approximately(multiplyWith.a, 0f)) 
                {
                    Color src = pixels[(y - Y) * Width() + (x - X)] * multiplyWith;
                    tex.SetPixel(x, y, (1f - src.a) * tex.GetPixel(x, y) + src.a * src);
                } else 
                {
                    Color src = pixels[(y - Y) * Width() + (x - X)];
                    tex.SetPixel(x, y, (1f - src.a) * tex.GetPixel(x, y) + src.a * multiplyWith);
                }
                */
				Color src = pixels[(y - Y) * Width() + (x - X)];

				if(!Mathf.Approximately(multiplyWith.a, 0f))
				{
					if(!Mathf.Approximately(src.a, 0f))
					{
						tex.SetPixel(x, y, src * multiplyWith);
					}
				}
				else
				{
					if(!Mathf.Approximately(src.a, 0f))
					{
						tex.SetPixel(x, y, multiplyWith);
					}
				}
			}
		}
	}

	public int Width()
	{
		return W;
	}

	public int Height()
	{
		return H;
	}

	public Color GetPixel(int x, int y)
	{
		int index = y * W + x;
		if(index >= 0 && index < pixels.Length)
		{
			return pixels[index];
		}
		else
		{
			Debug.Log("ERROR: array too small (" + pixels.Length + ") (x:" + x + ",y:" + y + ")");
			return Color.black;
		}
	}

	public void SetPixel(int x, int y, Color color)
	{
		int index = y * W + x;
		if(index < pixels.Length)
		{
			pixels[y * W + x] = color;
		}
		else
		{
			Debug.Log("ERROR: array too small (" + pixels.Length + ") (x:" + x + ",y:" + y + ")");
		}
	}

}
