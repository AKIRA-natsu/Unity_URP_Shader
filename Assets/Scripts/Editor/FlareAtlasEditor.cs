using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlareAtlas), false)]
public class FlareAtlasEditor : Editor
{
    private FlareAtlas m_Atlas;
    private List<FlareTexture> m_SubTextures;
    private int m_Foldout;

    public void OnEnable()
    {
        m_Atlas = target as FlareAtlas;
        m_SubTextures = m_Atlas.SubTextures;
        m_Foldout = 0;
    }

    public void OnDisable()
    {
    }

    public override void OnInspectorGUI()
    {
        GUI.changed = false;
        EditorGUILayout.LabelField("Atlas Texture");
        m_Atlas.Atlas = EditorGUILayout.ObjectField(m_Atlas.Atlas, typeof(Texture2D), false) as Texture2D;
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Sub Textures: " + m_SubTextures.Count, GUILayout.Width(250));
        EditorGUILayout.BeginVertical();
        for(int i = 0; i < m_SubTextures.Count; ++i)
        {
            FlareTexture subTex = m_SubTextures[i];
            if (subTex == null)
                return;
            EditorGUILayout.BeginHorizontal();
            bool foldout = (m_Foldout & (1 << i)) != 0;
            foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, "Tex "+ i);
            //GUILayout.Button(" - ");
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleRight;
            if (GUILayout.Button(" - ", GUILayout.Width(40)))
            {
                m_SubTextures.RemoveAt(i);
                int leftPart = m_Foldout >> 1 & ~((1 << i)-1);
                int rightPart = m_Foldout & ((1 << i) - 1);
                m_Foldout = leftPart | rightPart;
                EditorGUILayout.EndHorizontal();
                break;
            }
            EditorGUILayout.EndHorizontal();
            m_Foldout = (m_Foldout & ~((1 << i))) | (foldout ? (1 << i) : 0);
            if (!foldout)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                continue;
            }
            subTex.Scale = EditorGUILayout.Vector2Field("Scale", subTex.Scale);
            subTex.Offset = EditorGUILayout.Vector2Field("Offset", subTex.Offset);

            // edit scale offset
            Vector4 scaleOffset = subTex.ScaleOffset;
            Vector2 scale = subTex.Scale;
            Vector2 offset = subTex.Offset;
            Vector2 imageSize = new Vector2(m_Atlas.Atlas.width, m_Atlas.Atlas.height);
            Rect rect = GUILayoutUtility.GetLastRect();
            float left = offset.x * imageSize.x;
            float right = (scale.x + offset.x) * imageSize.x;
            float bottom = offset.y * imageSize.y;
            float top = (scale.y + offset.y) * imageSize.y;
            float aspect = 1;
            if (top - bottom != 0)
                aspect = (right - left) / (top - bottom);
            Rect texRect = new Rect(rect.x, rect.y + 20, 200, 200);
            Rect texRect2 = new Rect(rect.x + 220, rect.y + 20, 200 * aspect, 200);
            Rect uv0 = new Rect(0,0,1,1);
            Rect uv = new Rect(scaleOffset.z, scaleOffset.w, scaleOffset.x, scaleOffset.y);
            GUI.DrawTextureWithTexCoords(texRect, m_Atlas.Atlas, uv0);
            GUI.DrawTextureWithTexCoords(texRect2, m_Atlas.Atlas, uv);
            DrawHorizontalline(Color.white, new Vector2(texRect.x, texRect.y + 200 - (scale.y + offset.y) * 200), 200);
            DrawHorizontalline(Color.white, new Vector2(texRect.x, texRect.y + 200 - offset.y * 200), 200);
            DrawVerticleline(Color.white, new Vector2(texRect.x + offset.x * 200, texRect.y), 200);
            DrawVerticleline(Color.white, new Vector2(texRect.x + (scale.x + offset.x) * 200, texRect.y), 200);

            GUILayout.Space(200f); 
            left = Mathf.Clamp(EditorGUILayout.Slider("Left", left, 0f, 2048f), 0, right - 1);
            right = Mathf.Clamp(EditorGUILayout.Slider("Right", right, 0f, 2048f), left + 1, imageSize.x);
            bottom = Mathf.Clamp(EditorGUILayout.Slider("Bottom", bottom, 0f, 2048f), 0, top - 1);
            top = Mathf.Clamp(EditorGUILayout.Slider("Top", top, 0f, 2048f), bottom + 1, imageSize.y);
            subTex.ScaleOffset.x = (right - left)/imageSize.x;
            subTex.ScaleOffset.y = (top - bottom) /imageSize.y;
            subTex.ScaleOffset.z = left /imageSize.x;
            subTex.ScaleOffset.w = bottom / imageSize.y;

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("+ New Texture"))
        {
            if(m_SubTextures.Count >= 32)
            {
                Debug.LogError("最多支持32个子图");
            }
            else
            {
                var tex = new FlareTexture();
                tex.Atlas = m_Atlas;
                tex.ScaleOffset = new Vector4(1, 1, 0, 0);
                m_SubTextures.Add(tex);
            }
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(m_Atlas);
        }
    }

    public static void DrawHorizontalline(Color color, Vector2 left, float length, int thickness = 1, int padding = 0)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.x = left.x;
        r.y = left.y;
        r.width = length;
        EditorGUI.DrawRect(r, color);
        GUILayout.Space(-10f);
    }

    public static void DrawVerticleline(Color color, Vector2 top, float height, int thickness = 1, int padding = 0)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(padding + thickness));
        r.x = top.x;
        r.y = top.y;
        r.height = height;
        EditorGUI.DrawRect(r, color);
        GUILayout.Space(-10f);
    }

}
