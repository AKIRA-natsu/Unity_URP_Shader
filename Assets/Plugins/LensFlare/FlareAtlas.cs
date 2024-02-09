using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FlareTexture
{
    public FlareAtlas Atlas;
    public Vector4 ScaleOffset;
    public Vector2 Scale
    {
        get
        {
            return new Vector2(ScaleOffset.x, ScaleOffset.y);
        }
        set
        {
            ScaleOffset.x = value.x;
            ScaleOffset.y = value.y;
        }
    }
    public Vector2 Offset
    {
        get
        {
            return new Vector2(ScaleOffset.z, ScaleOffset.w);
        }
        set
        {
            ScaleOffset.z = value.x;
            ScaleOffset.w = value.y;
        }
    }
    public Vector2 PixelSize
    {
        get
        {
            if(Atlas != null)
                return Scale * Atlas.PixelSize;
            return Vector2.zero;
        }
    }
}

[CreateAssetMenu(fileName = "Flare Atlas", menuName = "Rendering/New Flare Atlas", order = 5)]
public class FlareAtlas : ScriptableObject
{
    public Texture2D Atlas;
    public List<FlareTexture> SubTextures;
    public Vector2 PixelSize
    {
        get
        {
            if (Atlas != null)
                return new Vector2(Atlas.width, Atlas.height);
            return Vector2.zero;
        }
    }

    public Vector4 GetScaleOffset(int index)
    {
        if (SubTextures.Count <= index)
            return Vector4.zero;
        return SubTextures[index].ScaleOffset;
    }

}
