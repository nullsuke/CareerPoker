using UnityEngine;
using UnityEngine.UI;

public class PlayerPanel : MonoBehaviour
{
    [SerializeField] Color panelColor = default;
    private Image image;
    private Color defaultColor;

    public void ChangeColor()
    {
        image.color = panelColor;
    }

    public void RestoreColor()
    {
        image.color = defaultColor;
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        defaultColor = image.color;
    }
}
