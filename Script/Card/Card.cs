using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Sprite[] cards = default;
    [SerializeField] Sprite cardBack = default;
    public enum SUIT { Spade = 0, Diamond = 1, Club = 2, Heart = 3 };
    public event EventHandler<EventArgs> SelectedCardChanged;
    private static readonly Vector2 EXPANSION = new Vector2(1.2f, 1.2f);
    private Image image;
    private Color defaultColor;
    private Color darkerColor;
    private int id;
    private bool isLegal;
    private bool hasSelected;

    public int ID
    {
        get => id;

        set
        {
            id = value;
            image.sprite = cards[value];
        }
    }

    public bool IsFace
    {
        set
        {
            if (value) image.sprite = cards[id];
            else image.sprite = cardBack;
        }
    }

    public bool IsLegal
    {
        get => isLegal;

        set
        {
            if (value) image.color = defaultColor;
            else image.color = darkerColor;

            isLegal = value;
        }
    }

    public bool HasSelected
    {
        get => hasSelected;
        
        set
        {
            Vector2 scale;

            if (value) scale = EXPANSION;
            else scale = Vector2.one;

            transform.localScale = scale;
            hasSelected = value;
        }
    }

    public float PositionX
    {
        set
        {
            var p = transform.localPosition;
            p.x = value;
            transform.localPosition = new Vector2(p.x, p.y);
        }
    }

    public float RotationZ
    {
        get => transform.localRotation.z;

        set
        {
            var r = transform.localRotation;
            r.z = value;
            transform.localRotation = Quaternion.Euler(r.x, r.y, r.z);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsLegal)
        {
            HasSelected = !HasSelected;
            SelectedCardChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        defaultColor = image.color;
        darkerColor = ChangeBrightness(defaultColor, 0.6f);
    }

    private Color ChangeBrightness(Color c, float b)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);

        return Color.HSVToRGB(h, s, b);
    }
}

public struct CardData
{
    public Card.SUIT Suit { private set; get; }
    public int Number { private set; get; }

    public CardData(Card.SUIT suit, int number)
    {
        Suit = suit;
        Number = number;
    }
}
