/** 
 *	@file	AEImitationMessage.cs
 *	@brief	メッセージ処理
 *
 *	@author miura
 */
using UnityEngine;
using UnityEditor;
using EventData;

namespace AEImitation
{
	/*----------------------------------------------------------------------------------------------------------*/
	/**
	 *	@class	Window
	 *  @brief  ウィンドウ表示クラス
	 */
	public partial class Window : EditorWindow
	{
		/*----------------------------------------------------------------------------------------------------------*/
		private enum Message
		{
			AddItem,
			DeleteItem,
			ChangeCurrentFrame,
			ChangeTotalFrames,
			MoveCurrentFrame,
			SwitchKeyFrame,
			MoveKeyFrame,
			AddKeyFrame,
			DeleteKeyFrame,
			MoveData,
			ChangeStartFrame,
			ChangeEndFrame,
			ChangeValue,
			GetValue,
			OpenFile,
			SaveFile,
			InitializeData,
			SetAsset,
			SelectCondition,
			ExpandElement,
			ChangeElementName,
			MoveElementPriority,
			SyncScroll,
			InsertFrame,
			DeleteFrame,
			RePaint,
			SetComment,
			DeleteComment,
			ApplySetting,
		}	// Message
		/*----------------------------------------------------------------------------------------------------------*/
		/**
		 *	@brief メッセージ処理
		 *
		 *	@param message		処理メッセージ
		 *	@param objs			処理データ
		 *	@return				メッセージ毎の戻り値
		 */
		private object onMessageReceived(Message message, params object[] objs)
		{
			if(EditorApplication.isPlaying) 
			{
				if(message != Message.GetValue)
				{
					return null;
				}
			}
			
			switch(message)
			{
				case Message.AddItem:
					{
						if(objs.Length == 0) break;
						if(!(objs[0] is EventData.ElementType)) break;
						var type = (EventData.ElementType)objs[0];
						var element = m_dataManager.AddObject(type);
						m_hierarchyPain.AddElement(element);
						m_keyFramePain.AddElement(element);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.DeleteItem:
					{
						if(objs.Length == 0) break;
						if(!(objs[0] is Element)) break;
						m_dataManager.DeleteObject((Element)objs[0]);
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.ChangeCurrentFrame:
				case Message.ChangeTotalFrames:
					{
						if(objs.Length == 0) break;
						var obj = objs[0];
						if(!(obj is int)) break;
						var frame = (int)obj;
						if(frame < 0) break;
						if(message == Message.ChangeCurrentFrame)
						{
							m_dataManager.SetCurrentFrame(frame);
						}
						else
						{
							m_dataManager.SetTotalFrames(frame);
						}
						m_headerPain.RePaint(m_dataManager);
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.MoveCurrentFrame:
					{
						if(objs.Length == 0) break;
						var obj = objs[0];
						if(!(obj is int)) break;
						var frame = (int)obj;
						if(frame < 0) break;
						m_dataManager.SetCurrentFrame(frame);
						m_headerPain.Play(m_dataManager);
						m_hierarchyPain.Play(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.SwitchKeyFrame:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Param)) break;
						if(!(objs[1] is int)) break;
						m_dataManager.SwitchKeyFrame(((Param)objs[0]), (int)objs[1]);
						m_headerPain.RePaint(m_dataManager);
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.MoveKeyFrame:
					{
						if(objs.Length < 3) break;
						if(!(objs[0] is Param)) break;
						if(!(objs[1] is int)) break;
						if(!(objs[2] is int)) break;
						m_dataManager.MoveKeyFrame(((Param)objs[0]), (int)objs[1], (int)objs[2]);
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.AddKeyFrame:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Param)) break;
						if(!(objs[1] is int)) break;
						m_dataManager.AddKeyFrame((Param)objs[0], ((int)objs[1]));
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.DeleteKeyFrame:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Param)) break;
						if(!(objs[1] is KeyFrameBase)) break;
						m_dataManager.DeleteKeyFrame((Param)objs[0], ((KeyFrameBase)objs[1]));
						m_hierarchyPain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.MoveData:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Element)) break;
						if(!(objs[1] is int)) break;
						m_dataManager.MoveElement(((Element)objs[0]), (int)objs[1]);
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.ChangeStartFrame:
				case Message.ChangeEndFrame:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Element)) break;
						if(!(objs[1] is int)) break;
						if(message == Message.ChangeStartFrame)
						{
							m_dataManager.SetStartFrame((Element)objs[0], (int)objs[1]);
						}
						else 
						{
							m_dataManager.SetEndFrame((Element)objs[0], (int)objs[1]);
						}
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.ChangeValue:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Param)) break;
						if(m_dataManager.SetParamValue((Param)objs[0], objs[1]))
						{
							m_keyFramePain.RePaint(m_dataManager);
						}
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.GetValue:
					{
						if(objs.Length == 0) break;
						if(!(objs[0] is Param)) break;	
						return m_dataManager.GetParamValue((Param)objs[0]);
					}
				case Message.OpenFile:
				case Message.SaveFile:
					{
						if(objs.Length == 0) break;
						if(!(objs[0] is string)) break;	
						var path = (string)objs[0];
						if(string.IsNullOrEmpty(path)) break;
						if(message == Message.OpenFile)
						{
							readFile(path);
							m_dataManager.ForceUpdate();
							Canvas.ForceUpdateCanvases();
						}
						else
						{
							m_dataManager.WriteXml(path);
						}
					}
					break;
				case Message.InitializeData:
					{
						m_dataManager.Initialize();
						m_headerPain.RePaint(m_dataManager);
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.SetAsset:
					{
						if(objs.Length < 3) break;
						if(!(objs[0] is Element)) break;
						if(!(objs[1] is AssetType)) break;
						if(!(objs[2] is UnityEngine.Object)) break;
						m_dataManager.SetAsset((Element)objs[0], (AssetType)objs[1], (UnityEngine.Object)objs[2]);
						
						m_hierarchyPain.RePaint(m_dataManager);
					}
					break;
				case Message.SelectCondition:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is AnimatorAsset)) break;
						if(!(objs[1] is string)) break;
						m_dataManager.SelectCondition((AnimatorAsset)objs[0], (string)objs[1]);
					}
					break;
				case Message.ExpandElement:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Element)) break;
						if(!(objs[1] is bool)) break;
						m_dataManager.ExpandElement((Element)objs[0], (bool)objs[1]);
						
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
					}
					break;
				case Message.ChangeElementName:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is Element)) break;
						if(!(objs[1] is string)) break;
						m_dataManager.ChangeElementName((Element)objs[0], (string)objs[1]);
					}
					break;
				case Message.MoveElementPriority:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is int)) break;
						if(!(objs[1] is int)) break;
						m_dataManager.MoveElementPriority((int)objs[0], (int)objs[1]);
						
						m_hierarchyPain.RePaint(m_dataManager);
						m_keyFramePain.RePaint(m_dataManager);
						Canvas.ForceUpdateCanvases();
					}
					break;
				case Message.SyncScroll:
					{
						if(objs.Length < 2) break;
						if(!(objs[0] is DataPainBase)) break;
						if(!(objs[1] is float)) break;
						if(objs[0] == m_hierarchyPain)
						{
							m_keyFramePain.SyncScroll((float)objs[1]);
						}
						else
						{
							m_hierarchyPain.SyncScroll((float)objs[1]);
						}
					}
					break;
				case Message.InsertFrame:
				case Message.DeleteFrame:
					if(objs.Length < 2) break;
					if(!(objs[0] is int)) break;
					if(!(objs[1] is int)) break;
					if(message == Message.InsertFrame)
					{
						m_dataManager.InsertFrame((int)objs[0],(int)objs[1]);
					}
					else
					{
						m_dataManager.DeleteFrame((int)objs[0],(int)objs[1]);
					}
					m_headerPain.RePaint(m_dataManager);
					m_hierarchyPain.RePaint(m_dataManager);
					m_keyFramePain.RePaint(m_dataManager);
					Canvas.ForceUpdateCanvases();
					break;
				case Message.RePaint:
					if(objs.Length < 1) break;
					if(!(objs[0] is PainBase)) break;
					((PainBase)objs[0]).RePaint(m_dataManager);
					break;
				case Message.SetComment:
					if(objs.Length < 2) break;
					if(!(objs[0] is int)) break;
					if(!(objs[1] is string)) break;
					m_dataManager.SetComment((int)objs[0], (string)objs[1]);
					break;
				case Message.DeleteComment:
					if(objs.Length < 1) break;
					if(!(objs[0] is int)) break;
					m_dataManager.DeleteComment((int)objs[0]);
					break;
				case Message.ApplySetting:
					if(objs.Length < 2) break;
					if(!(objs[0] is Setting)) break;
					m_dataManager.ApplySetting((Setting)objs[0], objs[1]);
					break;
				default:
					break;
			}
			
			return null;
		}
	}	// Window
}	// AEImitation
