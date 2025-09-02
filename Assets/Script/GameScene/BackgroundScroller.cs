using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackgroundScrollerUI : MonoBehaviour
{
    public RectTransform originalImage;  
    public float scrollSpeed = 200f;     
    private float imageHeight;

    private List<RectTransform> listImages= new List<RectTransform>();

    void Awake()
    {
        imageHeight = originalImage.rect.height;

        // 원본 추가
        listImages.Add(originalImage);

        // 복제해서 하나 더 만들기
        RectTransform cloneImages = Instantiate(originalImage, originalImage.parent);
        cloneImages.name = "BackgroundClone";
        listImages.Add(cloneImages);

        // 위치 초기화
        listImages[0].anchoredPosition = Vector2.zero;
        listImages[1].anchoredPosition = new Vector2(0f, imageHeight);
    }

    void Update()
    {
        float fMoveY = scrollSpeed * Time.deltaTime;

        for (int i = 0; i < listImages.Count; i++)
        {
            Vector2 pos = listImages[i].anchoredPosition;
            pos.y -= fMoveY;
            listImages[i].anchoredPosition = pos;
        }

        for (int i = 0; i < listImages.Count; i++)
        {
            if (listImages[i].anchoredPosition.y <= -imageHeight)
            {
                int iOther= (i + 1) % listImages.Count;
                float fTopY = listImages[iOther].anchoredPosition.y + imageHeight;
                listImages[i].anchoredPosition = new Vector2(0f, fTopY);
            }
        }
    }
}
