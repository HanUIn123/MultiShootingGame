using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image hpFillImage;
    public int maxHP = 100;
    public int currentHP = 100;

    public void SetHP(int hp)
    {
        if (hpFillImage == null)
        {
            Debug.LogWarning("hpFillImage is not assigned!");
            return;
        }

        currentHP = Mathf.Clamp(hp, 0, maxHP);
        hpFillImage.fillAmount = (float)currentHP / maxHP;
    }
}
