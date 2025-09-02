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

        // ���� �߰�
        listImages.Add(originalImage);

        // �����ؼ� �ϳ� �� �����
        RectTransform cloneImages = Instantiate(originalImage, originalImage.parent);
        cloneImages.name = "BackgroundClone";
        listImages.Add(cloneImages);

        // ��ġ �ʱ�ȭ
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
