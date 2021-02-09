using UnityEngine;
using UnityEngine.UI;

public class TextViewer : MonoBehaviour
{
    private RectTransform rect;
    private Text text;

    public void Pop(string msg, Vector2 pos, Color textColor)
    {
        Render(msg, pos, textColor);
        Destroy(gameObject, 1f);
    }

    public void Render(string msg, Vector2 pos, Color textColor)
    {
        rect.anchoredPosition = pos;
        text.text = msg;
        text.color = textColor;
    }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        text = GetComponent<Text>();
    }
}
