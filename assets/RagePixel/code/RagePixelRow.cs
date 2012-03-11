using UnityEngine;
using System.Collections;

[System.Serializable]
public class RagePixelRow  {
    public int pixelSizeX;
    public int pixelSizeY;
    public int newPixelSizeX;
    public int newPixelSizeY;
    public int key;
    public string name;
    [HideInInspector]
    public string fontCharacter = "";
    [HideInInspector]
    public int fontYOffset = 0;

    [SerializeField]
    private RagePixelAnimation[] _animations;
    public RagePixelAnimation[] animations
    {
        get
        {
            if (_animations == null)
            {
                _animations = new RagePixelAnimation[0];
            }
            return _animations;
        }
        set
        {
            _animations = value;
        }
    }

    [SerializeField]
    private RagePixelCell[] _cells;
    public RagePixelCell[] cells
    {
        get
        {
            if (_cells == null)
            {
                _cells = new RagePixelCell[0];
            }
            return _cells;
        }
        set
        {
            _cells = value;
        }
    }

    public RagePixelCell InsertCell(int index, int key)
    {
        RagePixelCell newCell = new RagePixelCell();
        newCell.delay = 200;
        newCell.uv = new Rect();
        newCell.key = key;

        RagePixelCell[] tmpArr = new RagePixelCell[cells.Length + 1];

        for (int i = 0; i < tmpArr.Length; i++)
        {
            if (i < index)
            {
                tmpArr[i] = cells[i];
                
                if (i == index - 1)
                {
                    newCell.uv = cells[i].uv;
                }
                
            }
            else if (i > index)
            {
                tmpArr[i] = cells[i - 1];
            }
            else
            {
                tmpArr[i] = newCell;
            }
        }
        
        cells = tmpArr;
        return newCell;
    }

    public void RemoveCellByKey(int key)
    {
        int toBeRemovedIndex = -1;

        if (cells.Length > 1)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].key.Equals(key))
                {
                    toBeRemovedIndex = i;
                }
            }

            if (toBeRemovedIndex >= 0)
            {
                RemoveCellByIndex(toBeRemovedIndex);
            }
        }
        else
        {
            Debug.Log("Error: Can't remove the last cell. Remove row instead");
        }
    }

    public void MoveCell(int fromIndex, int toIndex)
    {
        if (fromIndex >= 0 && fromIndex < cells.Length && toIndex >= 0 && toIndex <= cells.Length)
        {
            RagePixelCell[] tmpArr = new RagePixelCell[cells.Length];

            for (int i = 0; i < tmpArr.Length; i++)
            {
                if (i == toIndex)
                {
                    tmpArr[toIndex] = cells[fromIndex];
                }
                else if (i < fromIndex && i < toIndex || i > fromIndex && i > toIndex)
                {
                    tmpArr[i] = cells[i];
                }
                else if (i >= fromIndex && i < toIndex)
                {
                    tmpArr[i] = cells[i + 1];
                }
                else if (i <= fromIndex && i > toIndex)
                {
                    tmpArr[i] = cells[i - 1];
                }
            }
            cells = tmpArr;
        }
    } 

    public int GetKey(int index)
    {
        if (cells.Length > index)
        {
            return cells[index].key;
        }

        Debug.Log("Error: Indvalid array size");

        return cells[0].key;
    }

    public int GetIndex(int key)
    {
        if (cells.Length > 0)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].key.Equals(key))
                {
                    return i;
                }
            }
        }

        return 0;
    }

    public RagePixelCell GetCell(int key)
    {
        if (cells.Length > 0)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i].key.Equals(key))
                {
                    return cells[i];
                }
            }
        }

        return cells[0];
    }

    public void RemoveCellByIndex(int index)
    {
        if (cells.Length > 0)
        {
            RagePixelCell[] tmpArr = new RagePixelCell[cells.Length - 1];

            for (int i = 0; i < cells.Length; i++)
            {
                if (i < index)
                {
                    tmpArr[i] = cells[i];
                }
                else if (i > index)
                {
                    tmpArr[i-1] = cells[i];
                }
            }

            cells = tmpArr;
        }
    }

    public void AddAnimation()
    {
        RagePixelAnimation newAnim = new RagePixelAnimation();
        newAnim.mode = RagePixelSprite.AnimationMode.PlayOnce;
        newAnim.startIndex = 0;
        newAnim.endIndex = cells.Length - 1;
        newAnim.name = "New animation";

        RagePixelAnimation[] tmpArr = new RagePixelAnimation[animations.Length + 1];

        for (int i = 0; i < tmpArr.Length-1; i++)
        {
            tmpArr[i] = animations[i];
        }

        tmpArr[tmpArr.Length - 1] = newAnim;

        animations = tmpArr;
    }

    public void RemoveAnimation(int index)
    {
        if (animations.Length > 0)
        {
            RagePixelAnimation[] tmpArr = new RagePixelAnimation[animations.Length - 1];

            for (int i = 0; i < animations.Length; i++)
            {
                if (i < index)
                {
                    tmpArr[i] = animations[i];
                }
                else if (i > index)
                {
                    tmpArr[i - 1] = animations[i];
                }
            }

            animations = tmpArr;
        }
    }

    public RagePixelAnimation GetAnimationByName(string name)
    {
        if (animations.Length > 0)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                if (animations[i].name.Equals(name))
                {
                    return animations[i];
                }
            }
        }

        return null;
    }

    public void ClearUndoHistory()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].undoHistory == null)
            {
                cells[i].undoHistory = new ArrayList();
            }
            else
            {
                cells[i].undoHistory.Clear();
            }
        }
    }
}
