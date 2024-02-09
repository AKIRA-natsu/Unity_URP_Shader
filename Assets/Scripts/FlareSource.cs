using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Flare
{
    public string Comment = "";
	public bool IsActive = true;
    public FlareAtlas Atlas; // flare atlas
    public int Index; // flare texture
    [MinMaxSlider(0.0f, 15f)]
    public Vector2 ScaleRange;
    public Color Color; // 颜色
    [Range(0, 360)]
    public float Rotation; // 初始的旋转值，即正对着阳光时的姿势
    public bool RotateWith; // 随着视角变化，是否跟随转动 
    public bool OverridePixelPerUnit; // 
    public float PixelPerUnit;

    [Range(-5, 10)]
    public float Distance = 0; // 一个相对于光源的偏移距离
    [HideInInspector]
    public const float DistanceAmount = 10;

    public float DistanceAspect
    {
        get
        {
            return Distance / DistanceAmount;
        }
    }

    public FlareTexture Texture
    {
        get
        {
            if (Index < Atlas.SubTextures.Count)
                return Atlas.SubTextures[Index];
            return null;
        }
    }
}

// 光源
public class FlareSource : MonoBehaviour
{
    [SerializeField] private Camera m_GameCamera;
    [SerializeField] private Camera m_FlareCamera;

    public float PixelPerUnit = 100; // 用来控制缩放，在flare space生成网格的大小依据
    public AnimationCurve FadeCurve; // 淡入淡出的曲线
    public float FadeDuration;// 淡入淡出的时间间隔
    public AnimationCurve ScaleCurve;
    public AnimationCurve AlphaCurve;
    [Range(0.1f, 5)]
    public float SpreadAmount = 5; // 总长度/光源到光晕中心的长度，用来求得总长度
    public float SpreadMaximum = 5; // 最大扩散长度，单位Unit

    public Vector2 Center;

    [TextArea(1, 10)]
    public string Comment;
    public List<Flare> Flares;

    private Vector3 m_ViewportPosition;
    public Vector3 ViewportPosition
    {
        get
        {
            return m_ViewportPosition;
        }
    }

    private float m_AlphaBase = 0f;
    public float AlphaBase
    {
        get
        {
            return m_AlphaBase;
        }
    }
    private float m_FadeTime = 0;

    public bool IsVisible
    {
        get;private set;
    }

    private bool IsHitLast = true;

    // Start is called before the first frame update
    void Start()
    {
        IsHitLast = true;
        IsVisible = false;
        if (m_GameCamera == null)
            m_GameCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        m_ViewportPosition = m_GameCamera.WorldToViewportPoint(position);

        Ray ray = new Ray(m_GameCamera.transform.position, position - m_GameCamera.transform.position);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            if (IsVisible)
            {
                if (!IsHitLast)
                    m_FadeTime = (1 - m_AlphaBase) * FadeDuration;
                m_FadeTime += Time.deltaTime;
                float fac = GetFadeCurveValue(m_FadeTime / FadeDuration);
                m_AlphaBase = Mathf.Lerp(1, 0, fac);
                if (m_AlphaBase <= 0)
                {
                    IsVisible = false;
                    m_FadeTime = 0;
                    m_AlphaBase = 0;
                }
            }
            IsHitLast = true;
        }
        else
        {
            if(IsHitLast)
                m_FadeTime = m_AlphaBase * FadeDuration;
            if(m_AlphaBase < 1)
            {
                m_FadeTime += Time.deltaTime;
                float fac = GetFadeCurveValue(m_FadeTime / FadeDuration);
                m_AlphaBase = Mathf.Lerp(0, 1, fac);
                if (m_AlphaBase >= 1)
                {
                    m_FadeTime = 0;
                    m_AlphaBase = 1;
                }
            }
            IsVisible = true;
            IsHitLast = false;
        }
        Debug.DrawLine(transform.position, m_GameCamera.transform.position, IsHitLast ? Color.red : Color.white);
    }

    // 获取flare space下的初始大小，用来构建mesh
    public Vector2 GetFlareSize(Flare flare)
    {
        Vector2 size = flare.Texture.PixelSize;
        if(flare.OverridePixelPerUnit && flare.PixelPerUnit > 0)
        {
            size /= flare.PixelPerUnit;
        }
        else
        {
            size /= PixelPerUnit;
        }
        return size;
    }

    public float GetScaleCurveValue(float fac)
    {
        return ScaleCurve.Evaluate(fac);
    }

    public float GetAlphaCurveValue(float fac)
    {
        return AlphaCurve.Evaluate(fac);
    }

    public float GetFadeCurveValue(float fac)
    {
        return FadeCurve.Evaluate(fac);
    }

}
