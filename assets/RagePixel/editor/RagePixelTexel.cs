using System;

public class RagePixelTexel
{
	public int X;
	public int Y;

	public RagePixelTexel()
	{
		X = 0;
		Y = 0;
	}

	public RagePixelTexel(int _x, int _y)
	{
		X = _x;
		Y = _y;
	}

    public static RagePixelTexel operator +(RagePixelTexel a, RagePixelTexel b)
    {
        return new RagePixelTexel(a.X + b.X, a.Y + b.Y);
    }
}

