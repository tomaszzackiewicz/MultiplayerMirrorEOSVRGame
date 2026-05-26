using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour, ISerializationCallbackReceiver where T : SingletonMono<T>
{
	protected static T m_instance;
	public static T Instance
	{
		get
		{
			if (m_instance == null)
				Debug.LogError(typeof(T).Name + " not available in the current scene.");
			return m_instance;
		}
	}

	protected virtual void Awake()
	{
		if (m_instance == null)
			m_instance = this as T;
		else if (m_instance != this)
		{
			Debug.LogError("SingletonMono<" + typeof(T).Name + "> instance mismatch: " + m_instance + " != " + this);
			Destroy(this);
		}
	}

	public virtual void OnBeforeSerialize() { }
	public virtual void OnAfterDeserialize()
	{
		if (m_instance == null)
			m_instance = this as T;
	}
}