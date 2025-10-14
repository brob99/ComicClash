using UnityEngine;
using TMPro;

public class CaptionTimer : MonoBehaviour
{
    public float countdownTime = 60f;
    public TMP_Text timerText;

    float t;

    void Start() { t = countdownTime; }

    void Update()
    {
        t -= Time.deltaTime;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(t));
        if (timerText) timerText.text = seconds + "s";

        if (t <= 0f)
        {
            if (timerText) timerText.text = "0s";
            // ❗ Route via GameManager so Caption1 -> MemeCreate, Caption2 -> ComicCreate
            if (GameManager.Instance) GameManager.Instance.OnCaptionPhaseEnded();
        }
    }
}
