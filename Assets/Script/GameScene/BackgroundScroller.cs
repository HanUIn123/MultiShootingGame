using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BackgroundScrollerUI : MonoBehaviour
{
    public RectTransform originalImage;  // ��׶��� �̹��� �ϳ� (UI��)
    public float scrollSpeed = 200f;     // UI �����̶� px/s �ӵ�
    private float imageHeight;

    private List<RectTransform> images = new List<RectTransform>();

    void Awake()
    {
        imageHeight = originalImage.rect.height;

        // ���� �߰�
        images.Add(originalImage);

        // �����ؼ� �ϳ� �� �����
        RectTransform clone = Instantiate(originalImage, originalImage.parent);
        clone.name = "BackgroundClone";
        images.Add(clone);

        // ��ġ �ʱ�ȭ
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
