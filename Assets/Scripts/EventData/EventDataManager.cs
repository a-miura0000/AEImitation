/** 
 *	@file	EventDataManager.cs
 *	@brief	イベントデータの管理
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UI;

namespace EventData
{
	public class Manager
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private GameObject				m_rootGameObject;
		private GameObject				m_windowGameObject;
		private GameObject				m_choicesGameObject;
		private EventDataPlayer			m_player;
		private PlayData				m_playData;
		private const string			cm_rootName = "Root";
		/*----------------------------------------------------------------------------------------------------------*/
		public int						CurrentFrame { get => (m_player != null) ? m_player.CurrentFrame : 0; }
		public EventFlags				Flags 
										{ 
											get => (m_player != null) ? m_player.Flags : EventFlags.FlagNone;
											set { if(m_player != null) m_player.Flags = value; }
										}
		/*----------------------------------------------------------------------------------------------------------*/
		protected GameObject			rootGameObject { get => m_rootGameObject; }
		protected EventDataPlayer		player { get => m_player; }
		protected PlayData				data { get => m_playData; set { m_playData = value; if(m_player != null) m_player.SetData(value); } }
		/*----------------------------------------------------------------------------------------------------------*/
		public Manager()
		{
			m_playData = new PlayData();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void SetGameObject(GameObject targetGameObject, GameObject windowGameObject, GameObject choicesGameObject)
		{
			var rootTransform = targetGameObject.transform.Find(cm_rootName);
			if(rootTransform == null) 
			{
	            var rootGameObect = new GameObject(cm_rootName);
	            rootGameObect.transform.SetParent(targetGameObject.transform);
	            var canvas = rootGameObect.AddComponent<Canvas>();
	            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
	            rootGameObect.AddComponent<CanvasScaler>();
	            rootGameObect.AddComponent<GraphicRaycaster>();
	            
	            rootTransform = rootGameObect.transform;
			}
			m_rootGameObject = rootTransform.gameObject;
			
			m_player = m_rootGameObject.GetComponent<EventDataPlayer>();
			if(m_player == null) 
			{
				m_player = m_rootGameObject.AddComponent<EventDataPlayer>();
			}
			
			m_windowGameObject = windowGameObject;
			m_choicesGameObject = choicesGameObject;
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void AddObject(ElementType type)
		{
			addObject(m_playData.AddElement(type));
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public void AddObject(Element element)
		{
			addObject(element);
		}
		/*----------------------------------------------------------------------------------------------------------*/
		/**
	     * @brief 再生
		 */
		public void Play()
		{
			if(m_player == null) return;
			m_player.Play();
		}
		/*----------------------------------------------------------------------------------------------------------*/
		/**
	     * @brief 停止
		 */
		public void Stop()
		{
		
		}
		/*----------------------------------------------------------------------------------------------------------*/
		public bool IsPlaying()
		{
			if(m_player == null) return false;
			return m_player.IsPlaying();
		}
#if UNITY_EDITOR
		/*----------------------------------------------------------------------------------------------------------*/
		protected void moveElementPriority(int sourceIndex, int targetIndex)
		{
			var source = m_rootGameObject.transform.GetChild(sourceIndex);
			if(source == null) return;
			source.SetSiblingIndex(targetIndex);
			
			m_playData.MoveElementPriority(sourceIndex, targetIndex);
		}
#endif
		/*----------------------------------------------------------------------------------------------------------*/
		private void addObject(Element element)
		{
			GameObject obj;
			GameObject sourceObj = null;
			switch(element.Type)
			{
				case ElementType.Text:
					sourceObj = m_windowGameObject;
					break;
				case ElementType.Choices:
					sourceObj = m_choicesGameObject;
					break;
				default:
					break;
			}
			if(sourceObj != null)
			{
				obj = Object.Instantiate(sourceObj);
				var tmpPosition = obj.transform.localPosition;
				var tmpScale = obj.transform.localScale;
				obj.transform.SetParent(m_rootGameObject.transform);
				obj.transform.localPosition = tmpPosition;
				obj.transform.localScale = tmpScale;
			}
			else 
			{
				obj = new GameObject();
				obj.transform.SetParent(m_rootGameObject.transform);
				obj.transform.localPosition  = Vector3.zero;
			}
			
			EventDataReflector reflector = null;
			switch(element.Type) 
			{
				case ElementType.Image:
					reflector = obj.AddComponent<EventDataImageReflector>();
					break;	
				case ElementType.Panel:
					reflector = obj.AddComponent<EventDataPanelReflector>();
					break;	
				case ElementType.Text:
					reflector = obj.AddComponent<EventDataTextReflector>();
					break;	
				case ElementType.Choices:
					reflector = obj.AddComponent<EventDataChoicesReflector>();
					((EventDataChoicesReflector)reflector).AddListener(m_player.SetFlag);
					break;		
				case ElementType.Sound:
					reflector = obj.AddComponent<EventDataSoundReflector>();
					break;		
				case ElementType.SoundOneShot:
					reflector = obj.AddComponent<EventDataSoundOneShotReflector>();
					break;	
				default:
					break;
			}
			if(reflector != null) 
			{
				reflector.SetEement(element);
				m_player.AddReflector(reflector);
			}
		}
	}	// Manager
}	// EventData
