/*! @file	AEImitationHeaderPain.cs
	@brief	

	@author miura
 */
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
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
		/**
		 *	@class	HeaderPain
		 *  @brief  ウィンドウ上部表示クラス
		 */
		private class HeaderPain : PainBase
		{
			/*----------------------------------------------------------------------------------------------------------*/
			private IntegerField			m_currentFrame;
			private IntegerField			m_totalFrames;
			/*----------------------------------------------------------------------------------------------------------*/		
			/**
			 *	@brief Unity再生時の処理
			 *
			 *	@param dataManager	データ
			 */
			public override void Play(IDataManager dataManager)
			{
				m_currentFrame.value = dataManager.CurrentFrame;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override VisualElement createRootElement()
			{
				return new VisualElement();
			}
			/*----------------------------------------------------------------------------------------------------------*/			
			protected override void createVisualElement(IDataManager dataManager)
			{
				var toolbar = new Toolbar();
				RootElement.Add(toolbar);
				
				var menu = new ToolbarMenu();
				menu.text = "ファイル";
				menu.menu.AppendAction("新規作成", (action) =>
					{
						sendMessage(Message.InitializeData); 
						EditorPrefs.DeleteKey(cm_filePathSaveKey);
					});
				menu.menu.AppendAction("開く", (action) =>
					{ 
						string path = EditorUtility.OpenFilePanel("ファイルを開く", EditorPrefs.GetString(cm_filePathSaveKey, ""), "xml"); 
						sendMessage(Message.OpenFile, path); 
						if(!string.IsNullOrEmpty(path))
						{
							EditorPrefs.SetString(cm_filePathSaveKey, path);
						}
					});
				menu.menu.AppendAction("上書き保存", (action) =>
					{
						string path = EditorPrefs.GetString(cm_filePathSaveKey, "");
						if(string.IsNullOrEmpty(path))
						{
							path = EditorUtility.SaveFilePanel("名前を付けて保存", "", "", "xml");
						}
						sendMessage(Message.SaveFile, path); 
						if(!string.IsNullOrEmpty(path))
						{
							EditorPrefs.SetString(cm_filePathSaveKey, path);
						}
					});
				menu.menu.AppendAction("名前を付けて保存", (action) =>
					{
						string path = EditorUtility.SaveFilePanel("名前を付けて保存", "", "", "xml");
						sendMessage(Message.SaveFile, path); 
						if(!string.IsNullOrEmpty(path))
						{
							EditorPrefs.SetString(cm_filePathSaveKey, path);
						}
					});
				toolbar.Add(menu);
				
				var statusArea = new VisualElement();
				statusArea.AddToClassList("header-area");
				RootElement.Add(statusArea);
				
				var frameInfoArea = new VisualElement();
				frameInfoArea.AddToClassList("frame-info-area");
				statusArea.Add(frameInfoArea);
				
				var frameText = new Label("Frame");
				frameText.AddToClassList("frame-info-text");
				frameInfoArea.Add(frameText);
				
				var currentFrame = new IntegerField("", dataManager.TotalFrames);
				currentFrame.AddToClassList("frame-info-text");
				currentFrame.value = dataManager.CurrentFrame;
				currentFrame.RegisterCallback<FocusOutEvent>((evt) => 
				{
					sendMessage(Message.ChangeCurrentFrame, currentFrame.value); 
				});
		    	frameInfoArea.Add(currentFrame);	
		    	m_currentFrame = currentFrame;
				
				var slashText = new Label("/");
				slashText.AddToClassList("frame-info-text");
				frameInfoArea.Add(slashText);		
				
				var totalFrames = new IntegerField("");
				totalFrames.AddToClassList("frame-info-text");
				totalFrames.value = dataManager.TotalFrames;
				totalFrames.RegisterCallback<FocusOutEvent>(evt => 
				{
					sendMessage(Message.ChangeTotalFrames, totalFrames.value); 
				});
				frameInfoArea.Add(totalFrames);		
				m_totalFrames = totalFrames;	

				var debugArea = new VisualElement();
				debugArea.AddToClassList("header-area");
				RootElement.Add(debugArea);
				
				var flagField = new EnumFlagsField("Flag", dataManager.Flags);
				flagField.AddToClassList("header-flag-field");
				flagField.labelElement.AddToClassList("header-flag-field__label");
				flagField.RegisterValueChangedCallback((evt) =>
				{
					dataManager.Flags = (EventFlags)evt.newValue;
				});
				debugArea.Add(flagField);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override void repaint(IDataManager dataManager)
			{
				m_currentFrame.value = dataManager.CurrentFrame;
				m_totalFrames.value = dataManager.TotalFrames;
			}
		}	// HeaderPain
	}	// Window
}	// AEImitation
