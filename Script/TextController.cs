using UnityEngine;

public class TextController : MonoBehaviour
{
    [SerializeField] GameObject canvas = default;
    [SerializeField] TextViewer textPrefab = default;
    [SerializeField] Color passColor = default;
    [SerializeField] Color greatRichColor = default;
    [SerializeField] Color richColor = default;
    [SerializeField] Color poorColor = default;
    [SerializeField] Color greatPoorColor = default;
    [SerializeField] Vector2 passTextPosition = default;
    [SerializeField] Vector2 resultTextPosition = default;
    private TextViewer text;

    public void Render(string msg)
    {
        text = Instantiate(textPrefab, canvas.transform);

        switch (msg)
        {
            case "pass":
                text.Pop("パス", passTextPosition, passColor);
                break;

            case "grich":
                text.Render("大富豪", resultTextPosition, greatRichColor);
                break;

            case "rich":
                text.Render("富豪", resultTextPosition, richColor);
                break;

            case "poor":
                text.Render("貧民", resultTextPosition, poorColor);
                break;

            case "gpoor":
                text.Render("大貧民", resultTextPosition, greatPoorColor);
                break;

            default:
                break;
        }
    }

    public void Clear()
    {
        Destroy(text.gameObject);
    }
}
