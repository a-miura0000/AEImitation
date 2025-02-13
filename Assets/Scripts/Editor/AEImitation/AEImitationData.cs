/*! @file	AEImitationData.cs
	@brief	エディタ用データ

	@author miura
 */
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using EventData;

namespace AEImitation
{
	/*----------------------------------------------------------------------------------------------------------*/
	/*! @class	Window
	 *  @brief  ウィンドウ表示クラス
	 */
	public partial class Window : EditorWindow
	{
		/*----------------------------------------------------------------------------------------------------------*/
		/*! @class	DataManager
		 *  @brief  エディタ用データ管理クラス
		 */
		private partial class DataManager : Manager, IDataManager
		{
			/*----------------------------------------------------------------------------------------------------------*/
			public class Comment
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private int					m_frame;
				private string				m_text;
				/*----------------------------------------------------------------------------------------------------------*/
				public int					Frame { get => m_frame; set => m_frame = value; }
				public string				Text { get => m_text; set => m_text = value; }
				/*----------------------------------------------------------------------------------------------------------*/
				public Comment() : this(0)
				{
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public Comment(int frame)
				{
					m_frame = frame;
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/
			private class EditorData : PlayData
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private List<Comment>			m_commentList;
				/*----------------------------------------------------------------------------------------------------------*/
				public IReadOnlyList<Comment>	CommentList { get => m_commentList.AsReadOnly(); }
				/*----------------------------------------------------------------------------------------------------------*/
				public EditorData()
				{
					m_commentList = new List<Comment>();
				}
				/*----------------------------------------------------------------------------------------------------------*/	
				public void AddComment(Comment comment)
				{
					addComment(comment);
				}
				/*----------------------------------------------------------------------------------------------------------*/	
				public void SetComment(int frame, string text)
				{
					var comment = m_commentList.Find((p) => p.Frame == frame);
					if(comment == null)
					{
						comment = new Comment(frame);
						addComment(comment);
					}
					comment.Text = text;
				}
				/*----------------------------------------------------------------------------------------------------------*/	
				public void DeleteComment(int frame)
				{
					var comment = m_commentList.Find((p) => p.Frame == frame);
					if(comment == null) return;
					m_commentList.Remove(comment);
				}
				/*----------------------------------------------------------------------------------------------------------*/	
				public new void InsertFrame(int baseFrame, int frames)
				{
					foreach(var comment in m_commentList)
					{
						if(comment.Frame < baseFrame) continue;
						comment.Frame += frames;
					}
					
					base.InsertFrame(baseFrame, frames);
				}
				/*----------------------------------------------------------------------------------------------------------*/	
				public new void DeleteFrame(int baseFrame, int frames)
				{
					if(frames <= 0) return;
					
					int count = m_commentList.Count;
					for(int i = count - 1; i >= 0; --i)
					{
						if(m_commentList[i].Frame < baseFrame) break;
						if(m_commentList[i].Frame <= (baseFrame + frames - 1))
						{
							m_commentList.Remove(m_commentList[i]);
							continue;
						}
						m_commentList[i].Frame -= frames;
					}
					
					base.DeleteFrame(baseFrame, frames);
				}
				/*----------------------------------------------------------------------------------------------------------*/
				/**
				 *	@brief フレーム昇順でリストに登録
				 *	
				 *	@param comment				対象コメント
				 */	
				private void addComment(Comment comment)
				{
					int insertIndex = ~m_commentList.BinarySearch(comment, 
						Comparer<Comment>.Create((a, b) => a.Frame.CompareTo(b.Frame)));
					
					if(insertIndex >= m_commentList.Count)
					{
						m_commentList.Add(comment);
					}
					else 
					{
						m_commentList.Insert(insertIndex, comment);
					}
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public int 							TotalFrames { get => this.data.TotalFrames; }
			public IReadOnlyList<Element>		ElementList { get => this.data.ElementList; }
			public IReadOnlyList<Comment>		CommentList { get => this.data.CommentList; }
			/*----------------------------------------------------------------------------------------------------------*/
			private new EditorData				data { get => (EditorData)base.data; set => base.data = value; }
			/*----------------------------------------------------------------------------------------------------------*/
			public DataManager()
			{
				this.data = new EditorData();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void Initialize()
			{
				var transform = this.rootGameObject.transform;
				for(int i = transform.childCount - 1; i >= 0; --i)
				{
					DestroyImmediate(transform.GetChild(i).gameObject);
				}
				this.data = new EditorData();
				SetCurrentFrame(0);
				Flags = EventFlags.FlagNone;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void DeleteObject(Element element)
			{
				this.data.DeleteElement(element);
				if(this.player == null) return;
				this.player.DeleteObject(element);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void SetCurrentFrame(int currentFrame)
			{
				if(this.player == null) return;
				this.player.SetCurrentFrame(currentFrame);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void SetTotalFrames(int totalFrames)
			{
				this.data.TotalFrames = totalFrames;
				foreach(var element in this.data.ElementList)
				{
					if(element.StartFrame >= totalFrames)
					{
						element.StartFrame = System.Math.Max(0, totalFrames - 1);
					}
					if(element.EndFrame > totalFrames)
					{
						element.EndFrame = totalFrames;
					}
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void ForceUpdate()
			{
				if(this.player == null) return;
				this.player.ForceUpdate();	
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void SwitchKeyFrame(Param param, int frame)
			{
				SetCurrentFrame(frame);
				param.SwitchKeyFrame(frame);
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void MoveKeyFrame(Param param, int frame, int moveFrame)
			{
				param.MoveKeyFrame(frame, moveFrame);
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public void AddKeyFrame(Param param, int frame)
			{
				param.AddKeyFrame(frame);
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void DeleteKeyFrame(Param param, KeyFrameBase keyFrame)
			{
				param.DeleteKeyFrame(keyFrame);
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void MoveElement(Element element, int moveFrame)
			{
				element.Move(moveFrame);
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void SetStartFrame(Element element, int startFrame)
			{
				element.StartFrame = startFrame;
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void SetEndFrame(Element element, int endFrame)
			{
				element.EndFrame = endFrame;
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void SetAsset(Element element, AssetType type, UnityEngine.Object obj)
			{
				element.SetAsset(type, obj);
				element.SetAssetPath(type, AssetDatabase.GetAssetPath(obj));
				player.SetAsset(element, type, obj);
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public object GetParamValue(Param param)
			{
				return param.GetValue(CurrentFrame);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			/**
			 *	@brief キーフレーム値設定
			 *	
			 *	@param param				対象パラメータ
			 *	@param value				値
			 *	@return						true: 新規キーフレーム追加
			 */			
			public bool SetParamValue(Param param, object value)
			{
				var isAdd = param.SetValue(CurrentFrame, value);
				ForceUpdate();
				
				return isAdd;
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void SelectCondition(AnimatorAsset asset, string selectCondition)
			{
				asset.SelectCondition = selectCondition;
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void ExpandElement(Element element, bool isExpand)
			{
				element.IsExpand = isExpand;
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public void ChangeElementName(Element element, string name)
			{
				if(element.Name == name) return;
				element.Name = name;
				this.player.ChangeName(element);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			public void MoveElementPriority(int sourceIndex, int targetIndex)
			{
				moveElementPriority(sourceIndex, targetIndex);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			public void InsertFrame(int baseFrame, int frames)
			{
				if(frames <= 0) return;
				this.data.InsertFrame(baseFrame, frames);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			public void DeleteFrame(int baseFrame, int frames)
			{
				if(frames <= 0) return;
				if(TotalFrames <= frames)
				{
					frames = TotalFrames - 1;
				}
				this.data.DeleteFrame(baseFrame, frames);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			public void SetComment(int frame, string text)
			{
				this.data.SetComment(frame, text);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			public void DeleteComment(int frame)
			{
				this.data.DeleteComment(frame);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			public void ApplySetting(Setting setting, object value)
			{
				setting.Apply(value);
				ForceUpdate();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			public new Element AddObject(ElementType type)
			{
				base.AddObject(type);
				var list = ElementList;
				var element = list[list.Count - 1];
				element.StartFrame = CurrentFrame;
				element.EndFrame = TotalFrames;
				ForceUpdate();
				return element;
			}
		}	// DataManager
		/*----------------------------------------------------------------------------------------------------------*/
		private struct DataInfo
		{
			/*----------------------------------------------------------------------------------------------------------*/
			public ElementType					type;
			public string						text;
			/*----------------------------------------------------------------------------------------------------------*/
			public DataInfo(ElementType _type, string _text = null)
			{
				type = _type;
				if(_text == null)
				{
					text = _type.ToString();
				}
				else 
				{
					text = _text;
				}
			}
		}	// DataInfo
		/*----------------------------------------------------------------------------------------------------------*/
		private const float						cm_itemHeight = 20f;
		private static readonly DataInfo[]		sm_dataInfo = 
		{
			new DataInfo(ElementType.Image),
			new DataInfo(ElementType.Panel),
			new DataInfo(ElementType.Text),
			new DataInfo(ElementType.Choices),
			new DataInfo(ElementType.Sound),
			new DataInfo(ElementType.SoundOneShot, "Sound(OneShot)"),
		};
	}	// Window
}	// AEImitation
