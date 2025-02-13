/** 
 *	@file	EventDataPlayer.cs
 *	@brief	イベントデータの再生
 *
 *	@author miura
 */
using UnityEngine;
using System.Collections.Generic;
using EventData;

public class EventDataPlayer : MonoBehaviour
{
	/*----------------------------------------------------------------------------------------------------------*/
	private int							m_currentFrame;
	private PlayData					m_playData;
	private bool						m_isPlaying;
	private List<EventDataReflector>	m_reflectorList;
	private EventFlags					m_flags;
	/*----------------------------------------------------------------------------------------------------------*/
	public int							CurrentFrame { get => m_currentFrame; }
	public EventFlags					Flags { get => m_flags; set => m_flags = value; }
	/*----------------------------------------------------------------------------------------------------------*/
	public EventDataPlayer()
	{
		m_reflectorList = new List<EventDataReflector>();
		m_flags = EventFlags.FlagNone;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void Start()
	{
		m_isPlaying = false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void Update()
	{
		if(!m_isPlaying) return;
		
		if(!apply())
		{
			return;
		}
		
		if(++m_currentFrame >= m_playData.TotalFrames) 
		{
			m_currentFrame = m_playData.TotalFrames;
			m_isPlaying = false;
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void SetData(EventData.PlayData playData)
	{
		m_playData = playData;
		m_reflectorList.Clear();
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void AddReflector(EventDataReflector reflector)
	{
		m_reflectorList.Add(reflector);
		reflector.InitializePlay();
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void Play()
	{
		if(m_playData == null) return;
		m_currentFrame = 0;
		m_isPlaying = true;
		m_flags = EventFlags.FlagNone;
		foreach(var reflector in m_reflectorList)
		{
			reflector.InitializePlay();
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public bool IsPlaying()
	{
		return m_isPlaying;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void SetFlag(EventFlags flags)
	{
		m_flags |= flags;
	}
#if UNITY_EDITOR
	/*----------------------------------------------------------------------------------------------------------*/
	public void DeleteObject(Element element)
	{
		int count = m_reflectorList.Count;
		for(int i = 0; i < count; ++i) 
		{
			if(m_reflectorList[i].HasElement(element))
			{
				var tmp = m_reflectorList[i];
				m_reflectorList.Remove(tmp);
				DestroyImmediate(tmp.gameObject);
				break;
			}
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void SetAsset(Element element, AssetType type, UnityEngine.Object obj)
	{
		foreach(var reflector in m_reflectorList)
		{
			if(reflector.SetAsset(element, type, obj))
			{
				break;
			}
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void ChangeName(Element element)
	{
		foreach(var reflector in m_reflectorList)
		{
			if(reflector.ChangeName(element))
			{
				break;
			}
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void SetCurrentFrame(int currentFrame)
	{
		m_currentFrame = currentFrame;
		apply();
	}
	/*----------------------------------------------------------------------------------------------------------*/
	public void ForceUpdate()
	{
		apply();
	}
#endif	// UNITY_EDITOR
	/*----------------------------------------------------------------------------------------------------------*/
	private bool apply()
	{
		if(m_playData == null) return false;
		bool isComplete = true;
		foreach(var reflector in m_reflectorList)
		{
			if(!reflector.ReflectData(m_currentFrame, m_flags))
			{
				isComplete = false;
			}
		}
		return isComplete;
	}
}	// EventDataPlayer
