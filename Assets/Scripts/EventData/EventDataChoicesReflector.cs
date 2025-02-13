/** 
 *	@file	EventDataChoicesReflector.cs
 *	@brief	イベントデータをgameObjectに反映
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using System;
using System.Collections.Generic;
using TMPro;
using EventData;

public class EventDataChoicesReflector : EventDataReflectorBase<ChoicesElement>
{
	/*----------------------------------------------------------------------------------------------------------*/
	private enum Step
	{
		Set,
		Wait,
		Choice,
	}
	/*----------------------------------------------------------------------------------------------------------*/
	private class Choice 
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private GameObject				m_rootObject;
		private TextMeshProUGUI			m_text;
		private LocalizeStringEvent		m_localize;
		private Action<EventFlags>		m_listener;
		private EventFlags				m_flags;
		private const string			cm_textObjectName = "Text";
		/*----------------------------------------------------------------------------------------------------------*/
		public Choice(GameObject rootObject)
		{
			m_rootObject = rootObject;
			var button = m_rootObject.GetComponent<Button>();
			if(button == null)
			{
				button = m_rootObject.AddComponent<Button>();
			}
			button.onClick.AddListener(onClick);
			
			var textGameObject = m_rootObject.transform.Find(cm_textObjectName).gameObject;
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
		}
		/*----------------------------------------------------------------------------------------------------------*/
		~Choice()
		{
			m_localize.OnUpdateString.RemoveListener(updateText);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void AddListener(Action<EventFlags> listener)
		{
			m_listener += listener;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetTableName(string tableName)
		{
			m_localize.StringReference.TableReference = tableName;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void InitializePlay()
		{
			m_text.text = "";
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetActive(bool isActive)
		{
			if(isActive) return;
			m_rootObject.SetActive(isActive);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void ReflectData(ChoicesParam.Data choice)
		{
			m_rootObject.SetActive(true);
			m_localize.StringReference.TableEntryReference = choice.label;
			m_flags = choice.flags;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void onClick()
		{
			m_listener?.Invoke(m_flags);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		private void updateText(string str)
		{
			m_text.text = str;
		}
	}	// Choice
	/*----------------------------------------------------------------------------------------------------------*/
	private List<Choice>					m_choiceList;
	private Step							m_step;
	private const string					cm_objectName = "Choice";
	/*----------------------------------------------------------------------------------------------------------*/
	public void AddListener(Action<EventFlags> listener)
	{
		foreach(var choice in m_choiceList)
		{
			choice.AddListener(listener);
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void awake()
	{
		m_choiceList = new List<Choice>();
		int count = 0;
		while(true)
		{
			var trans = gameObject.transform.Find(cm_objectName + count.ToString("00"));
			if(trans == null) break;
			var choice = new Choice(trans.gameObject);
			choice.AddListener(onClick);
			m_choiceList.Add(choice);
			count++;
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
		foreach(var choice in m_choiceList)
		{
			choice.SetTableName(table.TableCollectionName);
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void initializePlay()
	{
		foreach(var choice in m_choiceList)
		{
			choice.InitializePlay();
		}
		m_step = Step.Set;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void setActive(bool isActive)
	{
		gameObject.SetActive(isActive);
		foreach(var choice in m_choiceList)
		{
			choice.SetActive(isActive);
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override bool reflectData(object elementData)
	{
		if(!gameObject.activeSelf) return true;
		if(!(elementData is ChoicesElement.Data)) return true;
		var data = (ChoicesElement.Data)elementData;
		if(data.choiceList.Count <= 0) return true;
		
		switch(m_step)
		{
			case Step.Set:
				{
					int count = 0;
					foreach(var choice in data.choiceList)
					{
						if(count >= m_choiceList.Count) break;
						m_choiceList[count].ReflectData(choice);
						count++;
					}
					for(int i = count; i < m_choiceList.Count; ++i)
					{
						m_choiceList[i].SetActive(false);
					}
#if UNITY_EDITOR
					if(!UnityEditor.EditorApplication.isPlaying) break;
#endif
					m_step = Step.Wait;
				}
				break;
			case Step.Choice:
				return true;
			default:
				break;
		}
		return false;
	}
	/*----------------------------------------------------------------------------------------------------------*/
	private void onClick(EventFlags flags)
	{
		m_step = Step.Choice;
	}
}	// EventDataChoicesReflector
