using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackgroundScrollerUI : MonoBehaviour
{
    public RectTransform originalImage;  // 백그라운드 이미지 하나 (UI용)
    public float scrollSpeed = 200f;     // UI 기준이라 px/s 속도
    private float imageHeight;

    private List<RectTransform> images = new List<RectTransform>();

    void Awake()
    {
        imageHeight = originalImage.rect.height;

        // 원본 추가
        images.Add(originalImage);

        // 복제해서 하나 더 만들기
        RectTransform clone = Instantiate(originalImage, originalImage.parent);
        clone.name = "BackgroundClone";
        images.Add(clone);

        // 위치 초기화
        images[0].anchoredPosition = Vector2.zero;
        images[1].anchoredPosition = new Vector2(0f, imageHeight);
    }

    void Update()
    {
        float moveY = scrollSpeed * Time.deltaTime;

        for (int i = 0; i < images.Count; i++)
        {
            Vector2 pos = images[i].anchoredPosition;
            pos.y -= moveY;
            images[i].anchoredPosition = pos;
        }

        for (int i = 0; i < images.Count; i++)
        {
            if (images[i].anchoredPosition.y <= -imageHeight)
            {
                int other = (i + 1) % images.Count;
                float topY = images[other].anchoredPosition.y + imageHeight;
                images[i].anchoredPosition = new Vector2(0f, topY);
            }
        }
    }
}
