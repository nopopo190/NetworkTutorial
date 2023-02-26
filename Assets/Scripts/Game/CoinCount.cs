using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_text;
    private Transform m_target;

    private void Update()
    {
        if(m_target == null)
        {
            Destroy(gameObject);
            return; 
        }

        //ワールド座標をスクリーン座標に変換
        transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, m_target.position + Vector3.up);
    }

    public void SetNumber(int count)
    {
        m_text.text = count.ToString();
    }

    public void SetTarget(Transform target)
    {
        m_target = target;
    }
}
