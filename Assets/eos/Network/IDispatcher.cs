using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SDispatcher<T>
#if UNITY_EDITOR
	: ISerializationCallbackReceiver
#endif
{
	[SerializeField] protected T m_value;
	[SerializeField] public UnityEvent<T> OnChange;

	public T Value
	{
		get { return m_value; }
		set
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				m_value = value;
				return;
			}
#endif
			if (EqualityComparer<T>.Default.Equals(m_value, value))
				return;

			m_value = value;
			OnChange?.Invoke(value);
		}
	}

	public void Attach(UnityAction<T> action)
	{
		OnChange.AddListener(action);
		action(m_value);
	}

	public void Detach(UnityAction<T> action)
	{
		OnChange.RemoveListener(action);
	}

	public void ClearValue() => Value = default;
	public void SetValue(T value) => Value = value;

	public Action PipeTo(SDispatcher<T> other)
	{
		Attach(other.SetValue);
		return () => {
			if (this != null)
				Detach(other.SetValue);
		};
	}

	public Action PipeTo<U>(SDispatcher<U> other, Func<T, U> transform)
	{
		void action(T value) => other.Value = transform(value);
		Attach(action);
		return () => {
			if (this != null)
				Detach(action);
		};
	}

#if UNITY_EDITOR
	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		UnityEditor.EditorApplication.delayCall += () => {
			if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
				OnChange?.Invoke(m_value);
		};
	}
#endif

	public static implicit operator T(SDispatcher<T> dispatcher) => dispatcher.Value;
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(SDispatcher<>), true)]
public class SDispatcherDrawer : UnityEditor.PropertyDrawer
{
	public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
	{

		// Foldout:
		property.isExpanded = UnityEditor.EditorGUI.Foldout(new Rect(position.x, position.y, position.width, UnityEditor.EditorGUIUtility.singleLineHeight), property.isExpanded, label);

		var value = property.FindPropertyRelative("m_value");
		float height = UnityEditor.EditorGUI.GetPropertyHeight(value);

		if (height != UnityEditor.EditorGUIUtility.singleLineHeight)
		{
			if (property.isExpanded)
			{
				UnityEditor.EditorGUI.PropertyField(new Rect(position.x, position.y + UnityEditor.EditorGUIUtility.singleLineHeight, position.width, height), value, GUIContent.none, true);
				position.y += height;
			}
		}
		else {
			UnityEditor.EditorGUI.PropertyField(new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width, UnityEditor.EditorGUIUtility.singleLineHeight), value, GUIContent.none, false);
		}
		position.y += UnityEditor.EditorGUIUtility.singleLineHeight;

		if (property.isExpanded)
		{
			var onChange = property.FindPropertyRelative("OnChange");
			float height2 = UnityEditor.EditorGUI.GetPropertyHeight(onChange);
			UnityEditor.EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, height2), onChange);
			position.y += height2;
		}
	}

	public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
	{
		float height = UnityEditor.EditorGUIUtility.singleLineHeight;

		var value = property.FindPropertyRelative("m_value");
		var h = UnityEditor.EditorGUI.GetPropertyHeight(value);
		if (h != UnityEditor.EditorGUIUtility.singleLineHeight)
			height += h;

		if (property.isExpanded)
		{
			var onChange = property.FindPropertyRelative("OnChange");
			height += UnityEditor.EditorGUI.GetPropertyHeight(onChange);
		}

		return height;
	}
}
#endif