using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// <para>可变化值，可以注册时间监听，C# 11可以继承 INumber 接口重载运算符</para>
/// <para>编辑器下修改会调用绑定事件</para>
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public partial class BindableValue<T> where T : IComparable {
    [SerializeField]
    private T value;
    public T Value { 
        get => value;
        set {
            if (this.value.Equals(value))
                return;
            this.value = value;
            onValueChanged?.Invoke(value);
        }
    }
    
    private Action<T> onValueChanged;

    public BindableValue() {
        value = default;
    }

    public BindableValue(T defaultValue) {
        value = defaultValue;
    }

    public void RegistBindAction(Action<T> onValueChanged, bool calledDirectly = true) {
        this.onValueChanged += onValueChanged;
        if (calledDirectly)
            onValueChanged.Invoke(value);
    }

    public void RemoveBindAction(Action<T> onValueChanged) {
        this.onValueChanged -= onValueChanged;
    }

    #region operator
    public static bool operator >(BindableValue<T> a, T b) {
        return a.value.CompareTo(b) > 0;
    }

    public static bool operator <(BindableValue<T> a, T b) {
        return a.value.CompareTo(b) < 0;
    }

    public static bool operator <=(BindableValue<T> a, T b) {
        return a.value.CompareTo(b) <= 0;
    }

    public static bool operator >=(BindableValue<T> a, T b) {
        return a.value.CompareTo(b) >= 0;
    }

    public static bool operator >(T b, BindableValue<T> a) {
        return a.value.CompareTo(b) < 0;
    }

    public static bool operator <(T b, BindableValue<T> a) {
        return a.value.CompareTo(b) > 0;
    }

    public static bool operator <=(T b, BindableValue<T> a) {
        return a.value.CompareTo(b) >= 0;
    }

    public static bool operator >=(T b, BindableValue<T> a) {
        return a.value.CompareTo(b) <= 0;
    }

    public static bool operator >(BindableValue<T> a, BindableValue<T> b) {
        return a.value.CompareTo(b.value) > 0;
    }

    public static bool operator <(BindableValue<T> a, BindableValue<T> b) {
        return a.value.CompareTo(b.value) < 0;
    }

    public static bool operator <=(BindableValue<T> a, BindableValue<T> b) {
        return a.value.CompareTo(b.value) <= 0;
    }

    public static bool operator >=(BindableValue<T> a, BindableValue<T> b) {
        return a.value.CompareTo(b.value) >= 0;
    }
    #endregion
}

#region Property Drawer
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BindableValue<>), false)]
public class BindableValueDrawer: PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), label);
        if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
            // 延迟一下，否则反射拿到的值不正确
            EditorApplication.delayCall += () => {
                // 对象实例
                var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
                // 泛型类型
                var generics = fieldInfo.FieldType.GetGenericArguments();
                // 获得value和onValueChanged字段
                var type = typeof(BindableValue<>).MakeGenericType(generics);
                System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                var action = type.GetField("onValueChanged", flags).GetValue(obj);
                var value = type.GetField("value", flags).GetValue(obj);
                $"Editor modify value: {fieldInfo.Name}[{property.serializedObject.targetObject}] -> {value}".Log();
                // 判断是否注册了事件
                if (action == null)
                    return;
                // 获得委托的Invoke方法
                var method = action.GetType().GetMethod("Invoke");
                method.Invoke(action, new object[] { value });
            };
        }
    }
}
#endif
#endregion
