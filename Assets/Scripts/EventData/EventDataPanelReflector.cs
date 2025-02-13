/** 
 *	@file	EventDataPanelReflector.cs
 *	@brief	イベントデータをgameObjectに反映
 *
 *	@author miura
 */
using UnityEngine;
using UnityEngine.UI;
using EventData;

public class EventDataPanelReflector : EventDataReflectorBase<PanelElement>
{
	/*----------------------------------------------------------------------------------------------------------*/
	private Image					m_image;
	/*----------------------------------------------------------------------------------------------------------*/
	protected override void awake()
	{
		gameObject.name = "sprite";
		m_image = gameObject.GetComponent<Image>();
		if(m_image == null)
		{
			m_image = gameObject.AddComponent<Image>();
			m_image.color = Color.white;
			RectTransform rectTransform = m_image.GetComponent<RectTransform>();
			rectTransform.sizeDelta = Vector2.zero;
			rectTransform.anchoredPosition = Vector2.zero; 
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.localScale = Vector3.one;
		}
	}
	/*----------------------------------------------------------------------------------------------------------*/
	protected override bool reflectData(object elementData)
	{
		if(!(elementData is PanelElement.Data)) return false;
		var data = (PanelElement.Data)elementData;
		
		var transform = (RectTransform)gameObject.transform;
		transform.localPosition = data.position;
		transform.localRotation = Quaternion.Euler(0f, 0f, data.rotation);
		transform.localScale = new Vector3(data.scale.x, data.scale.y, 1f);
		
		m_image.color = data.color;
		m_image.raycastTarget = data.raycastTarget;

		return true;
	}
}	// EventDataPanelReflector
