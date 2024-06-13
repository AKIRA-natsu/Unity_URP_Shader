using System;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

/// <summary>
/// <para>可变化值，继承自BinableValue<T></para>
/// <para>带变化范围</para>
/// <para>编辑器下没有限制该范围</para>
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class BindableRangeValue<T> : BindableValue<T> where T : IComparable {
    public T minValue;
    public T maxValue;

    [SerializeField]
    private UnityEvent<T, T> onValueChanged;

    public new T Value {
        get => base.Value;
        set {
            base.Value = GetComparaValue(value);
            // 检查是否相等
            // if (temp.CompareTo(base.Value) == 0)
            //     return;
            // base.Value = temp;
            onValueChanged?.Invoke(base.Value, maxValue);
        }
    }

    public BindableRangeValue() : this(default, default, default) {}
    public BindableRangeValue(T value, T minValue) : this(value, minValue, value) {}
    public BindableRangeValue(T value, T minValue, T maxValue) : base(value) {
        onValueChanged = new();
        this.minValue = minValue;
        this.maxValue = maxValue;
        CheckRangeValues();
    }

    private T GetComparaValue(T value) {
        CheckRangeValues();

        if (value.CompareTo(minValue) < 0)
            return minValue;
        if (value.CompareTo(maxValue) > 0)
            return maxValue;
        return value;
    }

    /// <summary>
    /// 判断min和max是否是正确的（min <= max）
    /// </summary>
    private void CheckRangeValues() {
        if (minValue.CompareTo(maxValue) > 0)
            (maxValue, minValue) = (minValue, maxValue);
    }

    public void RegistBindAction(UnityAction<T, T> onValueChanged, bool calledDirectly = true) {
        this.onValueChanged.AddListener(onValueChanged);
        if (calledDirectly) onValueChanged?.Invoke(base.Value, maxValue);
    }

    public void RemoveBindAction(UnityAction<T, T> onValueChanged) => this.onValueChanged.RemoveListener(onValueChanged);
}

#region Property Drawer
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BindableRangeValue<>), false)]
public class BindableRangeValueDrawer : PropertyDrawer {
    private const float ExpendValue = 4;
    private const BindingFlags PrivateFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags PublicFlags = BindingFlags.Instance | BindingFlags.Public;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Rect valuePos = new (position.x, position.y, position.width, (position.height - GetExpendLength(property) * 50f) / ExpendValue);
        Rect lastRect = EditorGUI.PrefixLabel(valuePos, label);
        Rect minRect = new (lastRect.x, valuePos.y, lastRect.width * 0.24f, lastRect.height);
        Rect valueRect = new (minRect.x + lastRect.width * 0.25f, lastRect.y, lastRect.width * 0.5f, lastRect.height);
        Rect maxRect = new (valueRect.x + lastRect.width * 0.51f, lastRect.y, lastRect.width * 0.24f, lastRect.height);

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(minRect, property.FindPropertyRelative("minValue"), GUIContent.none);
        // 改的是value，并没有判断范围，也不会触发事件
        EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);
        EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("maxValue"), GUIContent.none);
        if (EditorGUI.EndChangeCheck()) {
            // 延迟一下，否则反射拿到的值不正确
            EditorApplication.delayCall += () => {
                var (type, obj) = GetPropertyType(property);
                var valueProperty = type.GetProperty("Value", PublicFlags);
                var value = valueProperty.GetValue(obj);
                // 重新塞值，确保范围
                valueProperty.SetValue(obj, value);
                value = valueProperty.GetValue(obj);
                $"Editor modify value: {fieldInfo.Name}[{property.serializedObject.targetObject}] -> {value}".Log();
            };
        }

        Rect eventRect = new (position.x, position.y + valuePos.height + 3f, position.width, position.height - valuePos.height);
        EditorGUI.PropertyField(eventRect, property.FindPropertyRelative("onValueChanged"), GUIContent.none);

        EditorGUI.EndProperty();
    }

    /// <summary>
    /// 获得 SerializedProperty 类型（泛型）和实例
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private (Type, object) GetPropertyType(SerializedProperty property) {
        // 对象实例
        var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
        // 泛型类型
        var generics = fieldInfo.FieldType.GetGenericArguments();
        // 获得value和onValueChanged字段
        return (typeof(BindableRangeValue<>).MakeGenericType(generics), obj);
    }

    /// <summary>
    /// 获得 UnityEvent 扩展长度
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private int GetExpendLength(SerializedProperty property) {
        var (type, obj) = GetPropertyType(property);
        UnityEventBase eventBase = (UnityEventBase)type.GetField("onValueChanged", PrivateFlags).GetValue(obj);
        return Mathf.Max(eventBase.GetPersistentEventCount(), 1);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return base.GetPropertyHeight(property, label) * ExpendValue + GetExpendLength(property) * 50f;
    }
}
#endif
#endregion
