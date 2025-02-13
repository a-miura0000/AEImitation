/*! @file	AEImitationKeyFramePain.cs
	@brief	

	@author miura
 */
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
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
		private class KeyFramePain : DataPainBase
		{
			/*----------------------------------------------------------------------------------------------------------*/
			private class DragController
			{
				/*----------------------------------------------------------------------------------------------------------*/				
				private VisualElement				m_element;
				private VisualElement				m_parentElement;
				private Action<float>				m_downCallback;
				private Action<float>				m_moveCallback;
				private Action<int, int>			m_upCallback;
				private Func<float, float, float>	m_autoScrollFunc;
				private float						m_downPosition;
				private float						m_oldPosition;
				private int							m_initFrame;
				private bool						m_isDrag;
				/*----------------------------------------------------------------------------------------------------------*/								
				public DragController(VisualElement element, VisualElement parentElement, 
										Action<float> downCallback, Action<float> moveCallback, 
										Action<int, int> upCallback, Func<float, float, float> autoScrollFunc)
				{
					m_element = element;
					m_element.RegisterCallback<PointerDownEvent>(onPointerDown);
					m_element.RegisterCallback<PointerMoveEvent>(onPointerMove);
					m_element.RegisterCallback<PointerUpEvent>(onPointerUp);
					m_parentElement = parentElement;
					
					m_downCallback = downCallback;
					m_moveCallback = moveCallback;
					m_upCallback = upCallback;
					m_autoScrollFunc = autoScrollFunc;
					
					m_isDrag = false;
				}
				/*----------------------------------------------------------------------------------------------------------*/				
				private void onPointerDown(PointerDownEvent evt)
				{
					if(evt.target != m_element)
					{
						if(!((evt.localPosition.x >= 0f && evt.localPosition.x <= m_element.localBound.width) &&
							(evt.localPosition.y >= 0f && evt.localPosition.y <= m_element.localBound.height)))
						{
							return;
						}
					}
					if(evt.clickCount >= 2) return;
					// 右クリック
					if(evt.button == 1) return;
					
					m_isDrag = true;
					m_downPosition = evt.localPosition.x;
					m_oldPosition = m_element.resolvedStyle.left;
					m_initFrame = localPositionToFrame(evt.localPosition);
					
					m_element.CapturePointer(evt.pointerId);
					evt.StopPropagation();
					
					m_downCallback(getLocalToParentPosition(evt.localPosition));
				}
				/*----------------------------------------------------------------------------------------------------------*/				
				private void onPointerMove(PointerMoveEvent evt)
				{
					if(!m_isDrag) return;
					
					float pos = getLocalToParentPosition(evt.localPosition);
					
					// todo
					var parentPosition = m_parentElement.WorldToLocal(m_element.LocalToWorld(evt.localPosition)).x;
					var move = m_autoScrollFunc(parentPosition, pos - m_oldPosition);
					
					m_element.style.left = pos;
					m_oldPosition = pos;
					m_moveCallback(pos);
				}
				/*----------------------------------------------------------------------------------------------------------*/				
				private void onPointerUp(PointerUpEvent evt)
				{
					if(!m_isDrag) return;
					m_isDrag = false;
					
					m_element.ReleasePointer(evt.pointerId);
					evt.StopPropagation();
					
					m_upCallback(m_initFrame, localPositionToFrame(evt.localPosition));
				}
				/*----------------------------------------------------------------------------------------------------------*/	
				private float getLocalToParentPosition(Vector2 localPosition)
				{
					return localPosition.x - m_downPosition + m_element.resolvedStyle.left;
				}
				/*----------------------------------------------------------------------------------------------------------*/				
				private int localPositionToFrame(Vector2 localPosition)
				{
					var position = getLocalToParentPosition(localPosition);
					position += (m_element.resolvedStyle.width * 0.5f + m_element.resolvedStyle.marginLeft);
					return calcFrame(position);
				}
			}	// DragController
			/*----------------------------------------------------------------------------------------------------------*/
			private class ActiveFrameProperty : PopupWindowContent
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private Element						m_element;
				private Action<int, int>			m_sendCallback;
				private IntegerField				m_startFrameField;
				private IntegerField				m_endFrameField;
				/*----------------------------------------------------------------------------------------------------------*/
				public ActiveFrameProperty(Element element, Action<int, int> sendCallback)
				{
					m_element = element;
					m_sendCallback = sendCallback;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement CreateGUI()
				{
					var rootElement = new VisualElement();
					
					m_startFrameField = createField("StartFrame", m_element.StartFrame);
					rootElement.Add(m_startFrameField);
					m_endFrameField = createField("EndFrame", m_element.EndFrame);
					rootElement.Add(m_endFrameField);
					
					return rootElement;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void OnClose()
				{
					if(m_endFrameField.value <= m_startFrameField.value)
					{
						m_endFrameField.value = m_startFrameField.value + 1;
					}
					m_sendCallback(m_startFrameField.value, m_endFrameField.value);
				}
				/*----------------------------------------------------------------------------------------------------------*/
				private IntegerField createField(string title, int frame)
				{
					var field = new IntegerField(title);
					field.style.maxWidth = 300;
					field.value = frame;
					field.RegisterValueChangedCallback((evt) =>
						{
							if(evt.newValue < 0)
							{
								field.value = 0;
							}
						});
					
					return field;
				}
			}	// ActiveFrameProperty
			/*----------------------------------------------------------------------------------------------------------*/
			private class VariationFrameProperty : PopupWindowContent
			{
				/*----------------------------------------------------------------------------------------------------------*/
				private int							m_baseFrame;
				private Message						m_message;
				private Action<Message, int, int>	m_sendCallback;
				private IntegerField				m_baseFrameField;
				private IntegerField				m_framesField;
				/*----------------------------------------------------------------------------------------------------------*/
				public VariationFrameProperty(int baseFrame, Message message, Action<Message, int, int> sendCallback)
				{
					m_baseFrame = baseFrame;
					m_message = message;
					m_sendCallback = sendCallback;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override VisualElement CreateGUI()
				{
					var rootElement = new VisualElement();
					
					m_baseFrameField = createField(m_message.ToString(), m_baseFrame);
					rootElement.Add(m_baseFrameField);
					m_framesField = createField("Frames", 0);
					rootElement.Add(m_framesField);
					
					return rootElement;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public override void OnClose()
				{
					m_sendCallback(m_message, m_baseFrameField.value, m_framesField.value);
				}
				/*----------------------------------------------------------------------------------------------------------*/
				private IntegerField createField(string title, int frame)
				{
					var field = new IntegerField(title);
					field.style.maxWidth = 300;
					field.value = frame;
					field.RegisterValueChangedCallback((evt) =>
						{
							if(evt.newValue < 0)
							{
								field.value = 0;
							}
						});
					
					return field;
				}
			}	// VariationFrameProperty
			/*----------------------------------------------------------------------------------------------------------*/
			private class ParamData {
				/*----------------------------------------------------------------------------------------------------------*/
				private float						m_top;
				private float						m_height;
				private Param						m_param;
				/*----------------------------------------------------------------------------------------------------------*/
				public ParamData(float top, float height, Param param)
				{
					m_top = top;
					m_height = height;
					m_param = param;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public bool IsContains(float y)
				{
					return ((y >= m_top) && (y < (m_top + m_height))) ? true : false;
				}
				/*----------------------------------------------------------------------------------------------------------*/
				public Param GetParam() => m_param;
			}	// ParamData
			/*----------------------------------------------------------------------------------------------------------*/
			private ScrollView						m_headerBody;
			private float							m_lineWidth;
			private float							m_currentFrameAreaOffset;
			private VisualElement					m_currentFrameArea;
			private List<DragController>			m_dragControllerList;
			private float							m_top;
			private bool							m_sync;
			private List<ParamData>					m_paramDataList;
			private static float					sm_marginLeft;
			private static float					sm_span;
			/*----------------------------------------------------------------------------------------------------------*/
			public KeyFramePain()
			{
				m_lineWidth = 0f;
				sm_marginLeft = 0f;
				sm_span = 0f;
				m_currentFrameAreaOffset = 0f;
				m_dragControllerList = new List<DragController>();
				m_sync = false;
				m_paramDataList = new List<ParamData>();
			}
			public void ScrollReset()
			{
				setScrollOffset(Vector2.zero);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			/**
			 *	@brief Unity再生時の処理
			 *
			 *	@param dataManager	データ
			 */	
			public override void Play(IDataManager dataManager)
			{
				setCurrentFrame(dataManager.CurrentFrame);
				var position = new Vector2(m_currentFrameArea.resolvedStyle.left, m_currentFrameArea.resolvedStyle.top) -
					((ScrollView)bodyElement).scrollOffset;
				var viewPosition = ((ScrollView)bodyElement).contentViewport.WorldToLocal(
					m_currentFrameArea.LocalToWorld(position));
				autoScroll(viewPosition.x, sm_span);
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			/**
			 *	@brief EventData.Elementの追加
			 *
			 *	@param element		追加要素
			 */
			public override void AddElement(Element element)
			{
				// 有効範囲
				var activeFrame = element.EndFrame - element.StartFrame;
				var startPosition = element.StartFrame * sm_span + sm_marginLeft;
				EventCallback<MouseDownEvent> mouseDownCallback = 
					(evt) =>
					{
						if(evt.button == (int)MouseButton.RightMouse)
						{
							var menu = new GenericMenu();
							var rect = new Rect(evt.mousePosition.x, evt.mousePosition.y, 0, 0);
							menu.AddItem(new GUIContent("Property"), false, () => 
								{ 
									UnityEditor.PopupWindow.Show(rect, new ActiveFrameProperty(element, 
													(startFrame, endFrame) => 
													{
														sendMessage(Message.ChangeStartFrame, element, startFrame);
														sendMessage(Message.ChangeEndFrame, element, endFrame);
														sendMessage(Message.RePaint, this);
													})); 
								} );
							menu.DropDown(rect);
							
							evt.StopPropagation();
						}
					};
				var bar = new VisualElement();
				bar.AddToClassList("active-bar");
				bar.style.width = activeFrame * sm_span;
				bar.style.height = cm_itemHeight;
				bar.style.left = startPosition;
				bar.style.top = m_top;
				bar.RegisterCallback<MouseDownEvent>(mouseDownCallback);
				bodyElement.Add(bar);
				m_dragControllerList.Add(new DragController(bar, bodyElement,
					(position) =>
					{
					},
					(position) => 
					{ 
					}, 
					(oldFrame, newFrame) => 
					{
						sendMessage(Message.MoveData, element, newFrame - oldFrame);
					},
					autoScroll));
					
				var leftArea = new VisualElement();
				leftArea.AddToClassList("active-bar-resize-area");
				leftArea.style.height = cm_itemHeight;
				leftArea.style.left = startPosition;
				leftArea.style.top = m_top;
				leftArea.tooltip = element.StartFrame.ToString();
				leftArea.RegisterCallback<MouseDownEvent>(mouseDownCallback);
				bodyElement.Add(leftArea);
				float oldPosition = 0f;
				m_dragControllerList.Add(new DragController(leftArea, bodyElement,
					(position) =>
					{
						oldPosition = position;
					},
					(position) => 
					{ 
						bar.style.left = position;
						bar.style.width = bar.resolvedStyle.width - (position - oldPosition);
						oldPosition = position;
					}, 
					(oldFrame, newFrame) => 
					{
						leftArea.tooltip = newFrame.ToString();
						bar.style.left = newFrame * sm_span + sm_marginLeft;
						bar.style.width = (element.EndFrame - newFrame) * sm_span;

						sendMessage(Message.ChangeStartFrame, element, newFrame);
					},
					autoScroll));
				var rightArea = new VisualElement();
				rightArea.AddToClassList("active-bar-resize-area");
				rightArea.style.height = cm_itemHeight;
				rightArea.style.left = startPosition + activeFrame * sm_span;
				rightArea.style.top = m_top;
				rightArea.tooltip = element.EndFrame.ToString();
				rightArea.RegisterCallback<MouseDownEvent>(mouseDownCallback);
				bodyElement.Add(rightArea);
				m_dragControllerList.Add(new DragController(rightArea, bodyElement,
					(position) => 
					{ 
					},
					(position) => 
					{ 
						bar.style.width = position - bar.resolvedStyle.left;
					}, 
					(oldFrame, newFrame) => 
					{
						rightArea.tooltip = newFrame.ToString();
						bar.style.width = (newFrame - element.StartFrame) * sm_span;
						
						sendMessage(Message.ChangeEndFrame, element, newFrame);
					},
					autoScroll));
				
				m_top += cm_itemHeight;
				
				// キーフレーム
				if(!element.IsExpand) 
				{
					HashSet<int> frames = new HashSet<int>();
					foreach(var param in element.ParamList)
					{
						foreach(var keyFrame in param.KeyFrameList)
						{
							frames.Add(keyFrame.Frame - element.StartFrame);
						}
					}
					foreach(var frame in frames)
					{
						var key = new Label("●");
						key.AddToClassList("collapse-key-frame");
						key.tooltip = frame.ToString();
						bool isFirst = true;
						key.RegisterCallback<GeometryChangedEvent>((evt) => 
						{
							if(!isFirst) return;
							key.style.left = frame * sm_span + m_lineWidth * 0.5f
												- key.resolvedStyle.width * 0.5f 
												- bar.resolvedStyle.borderLeftWidth;
							isFirst = false;
						});
						bar.Add(key);
					}
					updateContainerHeight();
					return;
				}
				foreach(var param in element.ParamList)
				{
					m_paramDataList.Add(new ParamData(m_top, cm_itemHeight, param));
			/*		var hitBox = new VisualElement();
					hitBox.style.width = new StyleLength(Length.Percent(100));
					hitBox.style.height = cm_itemHeight;
					hitBox.style.top = m_top;
					hitBox.style.position = Position.Absolute; 
					hitBox.style.backgroundColor = new Color(0.1f, 0, 0, 0.1f);
					hitBox.RegisterCallback<PointerDownEvent>((evt) => 
					{
						if(evt.clickCount != 2) 
						{
							if(evt.target == hitBox)
							{
								evt.target.ReleasePointer(evt.pointerId);
							}
							return;
						}
						sendMessage(Message.SwitchKeyFrame, param, calcFrame(evt.localPosition.x));
					});
					bodyElement.Add(hitBox);*/
				
					foreach(var keyFrame in param.KeyFrameList)
					{
						var label = new Label("◆");
						label.AddToClassList("key-frame");
						label.style.height = cm_itemHeight;
						label.style.top = m_top;
						label.tooltip = keyFrame.Frame.ToString();
						float width = label.resolvedStyle.width;
						label.RegisterCallback<GeometryChangedEvent>((evt) => 
							{
								if(width > 0f) return;
								width = label.resolvedStyle.width;
								label.style.left = keyFrame.Frame * sm_span + m_lineWidth * 0.5f + sm_marginLeft 
									- width * 0.5f;
							});
						label.RegisterCallback<MouseDownEvent>((evt) => 
							{
								if(evt.button == (int)MouseButton.RightMouse)
								{
									var menu = new GenericMenu();
									menu.AddItem(new GUIContent("DeleteKey"), false, () => 
										{ 
											sendMessage(Message.DeleteKeyFrame, param, keyFrame); 
											bodyElement.Remove(label);
										});
									menu.DropDown(new Rect(evt.mousePosition.x, evt.mousePosition.y, 0, 0));
									
									evt.StopPropagation();
								}
							});
						bodyElement.Add(label);
						
						m_dragControllerList.Add(new DragController(label, bodyElement,
							(position) => 
							{ 
							}, 
							(position) => 
							{ 
							}, 
							(oldFrame, newFrame) => 
							{
								sendMessage(Message.MoveKeyFrame, param, oldFrame, newFrame - oldFrame);
							},
							autoScroll));
					}
					
					m_top += cm_itemHeight;	
				}
				// 空白
				foreach(var asset in element.AssetList)
				{					
					m_top += cm_itemHeight;
				}
				foreach(var setting in element.SettingList)
				{					
					m_top += cm_itemHeight;
				}
				updateContainerHeight();
			}
			/*----------------------------------------------------------------------------------------------------------*/		
			public override void SyncScroll(float scrollOffset)
			{
				m_sync = true;
				var element = (ScrollView)bodyElement;
				element.scrollOffset = new Vector2(element.scrollOffset.x, scrollOffset);
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override VisualElement createHeaderElement() 
			{
				var element = new VisualElement(); 
		
				m_headerBody = new ScrollView(ScrollViewMode.Horizontal);
				m_headerBody.verticalScrollerVisibility  = ScrollerVisibility.Hidden;
				m_headerBody.horizontalScrollerVisibility  = ScrollerVisibility.Hidden;
				m_headerBody.style.minHeight = new Length(100, LengthUnit.Percent);
				m_headerBody.RegisterCallback<MouseDownEvent>((evt) => 
					{
						if(evt.button == (int)MouseButton.RightMouse)
						{
							var menu = new GenericMenu();
							var posX = evt.localMousePosition.x + m_headerBody.scrollOffset.x;
							menu.AddItem(new GUIContent("Comment"), false, () => 
								{ 
									string text = "Comment";
									var frame = calcFrame(posX);
									createComment(frame, text);
									sendMessage(Message.SetComment, frame, text);
								});
							menu.DropDown(new Rect(evt.mousePosition.x, evt.mousePosition.y, 0, 0));
							
							evt.StopPropagation();
						}
					});
				m_headerBody.contentContainer.style.alignItems = Align.FlexEnd;
				element.Add(m_headerBody);
				
				return element;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override void createHeaderVisualElement(IDataManager dataManager)
			{			
				m_headerBody.Clear();
										
				List<VisualElement> lineList = new List<VisualElement>();
				for(int i = 0; i <= dataManager.TotalFrames + 1; ++i)
				{
					bool isBorder = ((i % 10) == 0);
					var line = new VisualElement();
					line.AddToClassList("frame-line-base");
					line.AddToClassList(isBorder ? "frame-line-10" : "frame-line");
					line.style.height = new StyleLength(Length.Percent(isBorder ? 50 : 40));
					m_headerBody.Add(line);
					
					if(isBorder)
					{
						lineList.Add(line);
					}
				}
				int frame = 0;
				foreach(var line in lineList)
				{
					var label = new Label(frame.ToString());
					label.AddToClassList("key-pain-header-frame-text");
					bool isComplete = false;
					label.RegisterCallback<GeometryChangedEvent>((evt) => 
					{
						if(isComplete) return;
						label.style.left = line.resolvedStyle.left;
						isComplete = true;
					});
					
					m_headerBody.Add(label);
					
					frame += 10;
				}
				
				foreach(var comment in dataManager.CommentList)
				{
					createComment(comment.Frame, comment.Text);
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override VisualElement createBodyElement()
			{
				var element = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
				element.RemoveFromClassList(ScrollView.verticalHorizontalVariantUssClassName);
				element.AddToClassList(ScrollView.horizontalVariantUssClassName);
				element.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
				element.horizontalScrollerVisibility  = ScrollerVisibility.AlwaysVisible;
				element.style.minHeight = new Length(100, LengthUnit.Percent);
				element.verticalScroller.valueChanged += (value) =>
					{
						if(!m_sync)
						{
							sendMessage(Message.SyncScroll, this, value);	
						}
						m_sync = false;
					};
				element.horizontalScroller.valueChanged += (value) =>
					{
						m_headerBody.scrollOffset = new Vector2(value, m_headerBody.scrollOffset.y);
					};
				element.RegisterCallback<MouseDownEvent>((evt) => 
					{
						if(evt.button == (int)MouseButton.RightMouse)
						{
							Action<Rect, int, Message> createProperty = (rect, frame, message) =>
								{
									UnityEditor.PopupWindow.Show(rect, 
										new VariationFrameProperty(frame, message,
													(message, baseFrame, frames) => 
													{
														sendMessage(message, baseFrame, frames);
													})); 
								};
							
							var rect = new Rect(evt.mousePosition.x, evt.mousePosition.y, 0, 0);
							var frame = calcFrame(evt.localMousePosition.x + element.scrollOffset.x);
							var menu = new GenericMenu();
							
							Param param = null;
							foreach(var data in m_paramDataList)
							{
								if(data.IsContains(evt.localMousePosition.y))
								{
									param = data.GetParam();
									break;
								}
							}
							if(param != null)
							{
								menu.AddItem(new GUIContent("AddKey"), false, () => 
									{ 
										sendMessage(Message.AddKeyFrame, param, frame); 
									});
							}
							else
							{
								menu.AddDisabledItem(new GUIContent("AddKey"));
							}
							menu.AddSeparator("");
							
							menu.AddItem(new GUIContent("InsertFrame"), false, () => { createProperty(rect, frame, Message.InsertFrame); } );
							menu.AddItem(new GUIContent("DeleteFrame"), false, () => { createProperty(rect, frame, Message.DeleteFrame); } );
							menu.DropDown(rect);
							
							evt.StopPropagation();
						}
					});
				element.contentContainer.RegisterCallback<GeometryChangedEvent>((evt) => 
					{
						updateContainerHeight();
					});
				element.contentContainer.RegisterCallback<WheelEvent>((evt) =>
					{
						var scroller = element.verticalScroller;
						var offset = element.scrollOffset;
						offset.y += evt.delta.y * ((scroller.lowValue < scroller.highValue) ? 1f : (-1f)) * element.mouseWheelScrollSize;
						element.scrollOffset = offset;
						
						evt.StopPropagation();
					});
					
			//	element.contentContainer.style.transformOrigin = 
			//		new TransformOrigin(new Length(0, LengthUnit.Percent), new Length(50, LengthUnit.Percent));
			//	element.contentContainer.style.scale = new Vector2(0.5f, 1f);
				return element;
			}
			/*----------------------------------------------------------------------------------------------------------*/			
			protected override void createBodyVisualElement(IDataManager dataManager)
			{		
				bodyElement.Clear();
				
				((ScrollView)bodyElement).scrollOffset = Vector2.zero;
				m_dragControllerList.Clear();
				m_paramDataList.Clear();
				
				Action createObject = () =>
				{
					// 現在のフレーム
					m_currentFrameArea = new VisualElement();
					m_currentFrameArea.AddToClassList("current-frame-area");
					bool isFirst = true;
					m_currentFrameArea.RegisterCallback<GeometryChangedEvent>((evt) => 
					{
						if(!isFirst) return;
						m_currentFrameAreaOffset = m_currentFrameArea.resolvedStyle.width * 0.5f;
						setCurrentFrame(dataManager.CurrentFrame);

						isFirst = false;
					});
					int oldFrame = dataManager.CurrentFrame;
					m_dragControllerList.Add(new DragController(m_currentFrameArea, bodyElement,
						(position) => 
						{ 
						}, 
						(position) => 
						{ 
							int frame = calcFrame(position);
							if(frame != oldFrame)
							{
								oldFrame = frame;
								sendMessage(Message.MoveCurrentFrame, frame); 
							}							
						}, 
						(oldFrame, newFrame) => 
						{
							sendMessage(Message.ChangeCurrentFrame, newFrame); 
						},
						autoScroll));
								
					bodyElement.Add(m_currentFrameArea);
					
					var currentLine = new VisualElement();
					currentLine.AddToClassList("frame-line-base");
					currentLine.AddToClassList("current-frame-line");
					currentLine.style.marginLeft = 0;
					m_currentFrameArea.Add(currentLine);
				
					// キーフレーム
					m_top = 0f;
					var elementList = dataManager.ElementList;
					for(int i = 0; i < elementList.Count; ++i) 
					{
						AddElement(elementList[i]);
					}
				};
				// ライン描画		
				for(int i = 0; i <= dataManager.TotalFrames + 1; ++i)
				{
					var line = new VisualElement();
					line.AddToClassList("frame-line-base");
					line.AddToClassList(((i % 10) == 0) ? "frame-line-10" : "frame-line");
					if(i == 0) 
					{
						line.RegisterCallback<GeometryChangedEvent>((evt) => 
						{
							bool isFirst = (m_lineWidth <= 0f);
							m_lineWidth = line.resolvedStyle.width;
							sm_marginLeft = line.resolvedStyle.marginLeft;
							sm_span = sm_marginLeft + m_lineWidth;
							
							if(isFirst) createObject();
						});
					}
					bodyElement.Add(line);
				}
				
				// 現在フレーム移動用あたり
				var hitBox = new VisualElement();
				hitBox.style.width = new StyleLength(Length.Percent(100));
				hitBox.style.height = new StyleLength(Length.Percent(100));
				hitBox.style.position = Position.Absolute; 
				hitBox.RegisterCallback<PointerDownEvent>((evt) => 
				{
					if(evt.clickCount != 2) return;
					sendMessage(Message.ChangeCurrentFrame, calcFrame(evt.localPosition.x));
				});
				bodyElement.Add(hitBox);

				if(m_lineWidth > 0f) createObject();
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override Vector2 getScrollOffset()
			{
				return ((ScrollView)bodyElement).scrollOffset;
			}
			/*----------------------------------------------------------------------------------------------------------*/
			protected override void setScrollOffset(Vector2 scrollOffset)
			{
				m_headerBody.scrollOffset = new Vector2(scrollOffset.x, 0f);
				((ScrollView)bodyElement).scrollOffset = scrollOffset;
			}
			/*----------------------------------------------------------------------------------------------------------*/			
			/**
			 *	@brief scrollView contentContainerサイズ調整
			 */		
			private void updateContainerHeight()
			{
				var element = (ScrollView)bodyElement;
				if(float.IsNaN(element.contentContainer.resolvedStyle.height)) return;
				if(element.contentContainer.resolvedStyle.height < m_top)
				{
					element.contentContainer.style.height = m_top;
				}
				else if((element.contentContainer.resolvedStyle.height > element.contentViewport.resolvedStyle.height) && 
					(element.contentContainer.resolvedStyle.height > m_top))
				{
					element.contentContainer.style.height = element.contentViewport.resolvedStyle.height;
				}
			}
			/*----------------------------------------------------------------------------------------------------------*/				
			private float autoScroll(float position, float move)
			{
				var element = (ScrollView)bodyElement;
				float range = element.contentViewport.resolvedStyle.width * 0.05f;
				if((position >= (element.contentViewport.resolvedStyle.width - range)) && (move > 0f))
				{
					float scrollMax = Mathf.Max(element.contentContainer.resolvedStyle.width - 
						element.contentViewport.resolvedStyle.width, 0f);
					if(element.scrollOffset.x < scrollMax)
					{
						move = Mathf.Min(move, scrollMax - element.scrollOffset.x);
					}
					else
					{
						move = 0f;
					}
				}
				else if((position <= range) && (move < 0f))
				{
					if(element.scrollOffset.x > 0f)
					{
						move = Mathf.Min(Math.Abs(move), element.scrollOffset.x) * -1f;
					}
					else
					{
						move = 0f;
					}
				}
				else
				{
					move = 0f;
				}
				
				element.scrollOffset = new Vector2(element.scrollOffset.x + move, element.scrollOffset.y);
				
				return move;
			}
			/*----------------------------------------------------------------------------------------------------------*/				
			private void setCurrentFrame(int frame)
			{
				m_currentFrameArea.style.left = frame * sm_span
													+ (sm_marginLeft - m_currentFrameAreaOffset + m_lineWidth * 0.5f);
			}
			/*----------------------------------------------------------------------------------------------------------*/	
			private void createComment(int frame, string comment)
			{
				var text = new TextField();
				text.AddToClassList("comment");
				text.value = comment;
				bool isFirst = true;
				text.RegisterCallback<GeometryChangedEvent>((evt) => 
					{
						if(!isFirst) return;
						text.style.left = frame * sm_span + sm_marginLeft - text.resolvedStyle.width * 0.5f;
						isFirst = false;
					});
				text.RegisterCallback<MouseDownEvent>((evt) => 
					{
						if(evt.button == (int)MouseButton.RightMouse)
						{
							var menu = new GenericMenu();
							menu.AddItem(new GUIContent("Delete"), false, () => 
								{ 
									sendMessage(Message.DeleteComment, frame); 
									m_headerBody.Remove(text);
								});
							menu.DropDown(new Rect(evt.mousePosition.x, evt.mousePosition.y, 0, 0));
							
							evt.StopPropagation();
						}
					});
				text.RegisterCallback<FocusOutEvent>((evt) =>
				{
					sendMessage(Message.SetComment, frame, text.value);
				});
				text.hierarchy[0].AddToClassList("comment-color");
				m_headerBody.Add(text);
			}
			/*----------------------------------------------------------------------------------------------------------*/				
			private static int calcFrame(float position)
			{
				float pos = position - sm_marginLeft;
				if(pos < 0f) pos = 0f;
				return (int)(pos / sm_span + 0.5f);
			}
		}	// KeyFramePain
	}	// Window
}	// AEImitation
