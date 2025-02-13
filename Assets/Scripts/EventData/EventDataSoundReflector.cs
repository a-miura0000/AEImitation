/** 
 *	@file	EventDataSoundReflector.cs
 *	@brief	サウンド再生
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.Localization.Components;
using TMPro;
using EventData;

public class EventDataSoundReflector : EventDataReflectorBase<SoundElement>
{
	/*----------------------------------------------------------------------------------------------------------*/
	private AudioSource				m_audio;
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void awake()
	{
		gameObject.name = "sound";
		m_audio = gameObject.GetComponent<AudioSource>();
		if(m_audio == null)
		{
			m_audio = gameObject.AddComponent<AudioSource>();
		}
		m_audio.playOnAwake = false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override object setAsset(Asset asset)
	{
		UnityEngine.Object data = null;
#if UNITY_EDITOR
		data = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset.Path);
		setAsset(AssetType.Audio, data);
#else
				
#endif
		return data;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void setAsset(AssetType type, UnityEngine.Object obj)
	{
		var audio = obj as AudioClip;
		if(audio == null) return;
		m_audio.clip = audio;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void initializePlay()
	{
		m_audio.loop = getElement().IsLoop();
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void setActive(bool isActive)
	{
		if(isActive)
		{
#if UNITY_EDITOR
			if(UnityEditor.EditorApplication.isPlaying)
#endif
				m_audio.Play();
		}
		else
		{
			m_audio.Stop();
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override bool reflectData(object elementData)
	{
		if(!(elementData is SoundElement.Data)) return true;
		var data = (SoundElement.Data)elementData;
		m_audio.volume = data.volume;
		return true;
	}
}	// EventDataSoundReflector
