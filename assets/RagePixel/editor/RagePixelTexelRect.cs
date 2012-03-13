using System;

public class RagePixelTexelRect
{
	public int X;
	public int Y;
	public int X2;
	public int Y2;

	public RagePixelTexelRect()
	{
		X = 0;
		Y = 0;
	}

	public RagePixelTexelRect(int _x, int _y, int _x2, int _y2)
	{
		if(_x <= _x2)
		{
			X = _x;
			X2 = _x2;
		}
		else
		{
			X2 = _x;
			X = _x2;
		}

		if(_y <= _y2)
		{
			Y = _y;
			Y2 = _y2;
		}
		else
		{
			Y2 = _y;
			Y = _y2;
		}
	}
    
	public int Width()
	{
		return X2 - X + 1;
	}

	public int Height()
	{
		return Y2 - Y + 1;
	}
}

