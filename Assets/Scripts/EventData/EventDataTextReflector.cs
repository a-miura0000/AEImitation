/** 
 *	@file	EventDataTextReflector.cs
 *	@brief	イベントデータをgameObjectに反映
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using System.Collections;
using TMPro;
using EventData;

public class EventDataTextReflector : EventDataReflectorBase<TextElement>
{
	/*----------------------------------------------------------------------------------------------------------*/
	private TextMeshProUGUI				m_text;
	private LocalizeStringEvent			m_localize;
	private bool						m_isFullDisplay;
	private bool						m_isEnd;
	private bool						m_isTap;
	private Coroutine					m_coroutine;
	private const float					cm_typingSpeed = 0.01f;
	private const string				cm_textObjectName = "Panel/Text";
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void awake()
	{
	    gameObject.name = "text";
    	var button = gameObject.GetComponent<Button>();
		if(button == null)
		{
			button = gameObject.AddComponent<Button>();
		}
		button.transition = Selectable.Transition.None;
		button.onClick.AddListener(onClick);
		
	    var textGameObject = gameObject.transform.Find(cm_textObjectName).gameObject;
	    m_text = textGameObject.GetComponent<TextMeshProUGUI>();
	    if(m_text == null)
	    {
			m_text = textGameObject.AddComponent<TextMeshProUGUI>();
		}
		m_localize = textGameObject.GetComponent<LocalizeStringEvent>();
		if(m_localize == null)
		{
			m_localize = textGameObject.AddComponent<LocalizeStringEvent>();
		}
		
		m_localize.OnUpdateString.AddListener(updateText);
		
		m_isFullDisplay = false;
		m_isEnd = false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void onDestroy()
	{
		if(m_localize != null)
		{
			m_localize.OnUpdateString.RemoveListener(updateText);
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override object setAsset(Asset asset)
	{
		object data = null;
#if UNITY_EDITOR
		var table = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset.Path) as SharedTableData;
		setAsset(AssetType.SharedTableData, table);
		data = table;
#endif	// UNITY_EDITOR
		
		return data;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void setAsset(AssetType type, UnityEngine.Object obj)
	{
		var table = obj as SharedTableData;
		if(table == null) return;
		m_localize.StringReference.TableReference = table.TableCollectionName;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void initializePlay()
	{
		m_text.text = "";
		m_isFullDisplay = false;
		m_isEnd = false;
		m_isTap = false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override bool reflectData(object elementData)
	{
		if(!gameObject.activeSelf) return true;
		if(!(elementData is TextElement.Data)) return true;
		var data = (TextElement.Data)elementData;
		if(string.IsNullOrEmpty(data.label)) 
		{
			m_localize.StringReference.TableEntryReference = data.label;
			m_text.text = "";
			return true;
		}
		
		if(string.IsNullOrEmpty(m_text.text))
		{
			m_isFullDisplay = false;
			m_isTap = false;
			m_localize.StringReference.TableEntryReference = data.label;
		}
		else
		{
#if UNITY_EDITOR
			if(Input.GetKeyDown(KeyCode.Return))
			{
				m_isTap = true;
			}
#endif
			if(m_isTap)
			{
				if(m_isEnd)
				{
					m_text.text = "";
					return true;
				}
				else
				{
					m_isFullDisplay = true;
				}
			}
		}
		
		return false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	private void onClick()
	{
		m_isTap = true;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	private void updateText(string str)
	{
#if UNITY_EDITOR
		if(!UnityEditor.EditorApplication.isPlaying)
		{
			m_text.text = str;
			return;
		}
#endif
		if(m_coroutine != null)
		{
			StopCoroutine(m_coroutine);
		}
		m_coroutine = StartCoroutine(typeText(str));
	}
	/*----------------------------------------------------------------------------------------------------------*/
	private IEnumerator typeText(string str)
	{
		m_text.text = "";
		m_isEnd = false;
		foreach(char c in str)
		{
			if(m_isFullDisplay) break;
			m_text.text += c;
			yield return new WaitForSeconds(cm_typingSpeed);
		}
		m_text.text = str;
		m_isEnd = true;
		m_coroutine = null;
	}
}	// EventDataTextReflector
