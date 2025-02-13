/** 
 *	@file	EventDataReflector.cs
 *	@brief	イベントデータをgameObjectに反映
 *
 *	@author miura
 */
using UnityEngine;
using EventData;

[ExecuteInEditMode]
public abstract class EventDataReflector : MonoBehaviour
{
	/*----------------------------------------------------------------------------------------------------------*/
	public void Awake()
	{
		awake();
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void OnDestroy()
	{
		onDestroy();
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public abstract bool HasElement(Element element);
	public abstract void SetEement(Element element);
	public abstract bool SetAsset(Element element, AssetType type, UnityEngine.Object obj);
	public abstract bool ChangeName(Element element);
	public abstract void InitializePlay();
	public abstract bool ReflectData(int frame, EventFlags flags);
	/*----------------------------------------------------------------------------------------------------------*/
	protected virtual void awake() {}
	protected virtual void onDestroy() {}
	protected virtual object setAsset(Asset asset) { return null; }
	protected virtual void setAsset(AssetType type, UnityEngine.Object obj) {}
}	// EventDataReflector
/*----------------------------------------------------------------------------------------------------------*/
public abstract class EventDataReflectorBase<T> : EventDataReflector where T : Element
{
	/*----------------------------------------------------------------------------------------------------------*/
	private T				m_element;
	private bool			m_isActive;
	/*----------------------------------------------------------------------------------------------------------*/
	public EventDataReflectorBase()
	{
		m_isActive = false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public sealed override bool HasElement(Element element)
	{
		return (element == m_element) ? true : false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public sealed override void SetEement(Element element)
	{
		m_element = element as T;
		if(m_element == null) return;
		gameObject.name = m_element.Name;
		foreach(var asset in m_element.AssetList)
		{
			if(string.IsNullOrEmpty(asset.Path)) continue;
			object data = setAsset(asset);
			if(data == null) continue;
			m_element.SetAsset(asset.Type, data);
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public sealed override bool SetAsset(Element element, AssetType type, UnityEngine.Object obj)
	{
		if(element != m_element) return false;
		foreach(var asset in m_element.AssetList)
		{
			if(asset.Type == type)
			{
				setAsset(asset.Type, obj);
				break;
			}
		}
		return true;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public sealed override bool ChangeName(Element element)
	{
		if(element != m_element) return false;
		gameObject.name = element.Name;
		return true;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public sealed override void InitializePlay()
	{
		m_isActive = false;
		setActive(m_isActive);
		initializePlay();
	}
	/*----------------------------------------------------------------------------------------------------------*/
	/**
	 *	@brief データ反映
	 *	
	 *	@param frame				反映フレーム
	 *	@param flags				表示判定フラグ
	 *	@return						true:次のフレームに進んで良い
	 */	
	public sealed override bool ReflectData(int frame, EventFlags flags)
	{
		if(m_element.Flags > 0)
		{
			if((m_element.Flags & flags) == 0)
			{
#if UNITY_EDITOR
				if(!UnityEditor.EditorApplication.isPlaying)
				{
					setActive(false);
				}
#endif
				return true;
			}
		}
		var data = m_element.GetFrameData(frame);
		if(data.isActive != m_isActive)
		{
			m_isActive = data.isActive;
			setActive(m_isActive);
		}
		return reflectData(data.elementData);
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected T getElement()
	{
		return m_element;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected virtual void initializePlay() {}
	/*----------------------------------------------------------------------------------------------------------*/
	protected virtual void setActive(bool isActive)
	{
		gameObject.SetActive(isActive);
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected abstract bool reflectData(object elementData);
}	// EventDataReflectorBase<t>
