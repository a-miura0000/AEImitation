/** 
 *	@file	EventDataSoundOneShotReflector.cs
 *	@brief	サウンドOneShot再生
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.Localization.Components;
using TMPro;
using EventData;

public class EventDataSoundOneShotReflector : EventDataReflectorBase<SoundOneShotElement>
{
	/*----------------------------------------------------------------------------------------------------------*/
	private AudioSource				m_audio;
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void awake()
	{
		gameObject.name = "sound(oneshot)";
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
	protected override void setActive(bool isActive)
	{
		if(isActive)
		{
#if UNITY_EDITOR
			if(UnityEditor.EditorApplication.isPlaying)
#endif
				m_audio.PlayOneShot(m_audio.clip);
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override bool reflectData(object elementData)
	{
		return true;
	}
}	// EventDataSoundReflector
