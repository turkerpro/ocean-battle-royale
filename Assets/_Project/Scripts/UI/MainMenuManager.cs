using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace OceanBattleRoyale.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        private static bool _created = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (_created) return;
            if (SceneManager.GetActiveScene().name != "MainMenu") return;
            _created = true;

            var go = new GameObject("MainMenuManager");
            go.AddComponent<MainMenuManager>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            if (FindObjectsByType<MainMenuManager>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != "MainMenu") return;
            CreateUI();
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("MenuCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var bgGO = new GameObject("Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.02f, 0.05f, 0.12f, 1f);
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            CreateText(canvasGO.transform, "Title", "OCEAN BATTLE ROYALE",
                48, Color.white, new Vector2(0, 80));

            CreateText(canvasGO.transform, "Subtitle", "50 oyuncu | Gemi gelistirme | Battle Royale",
                18, new Color(0.5f, 0.7f, 1f), new Vector2(0, 30));

            CreateButton(canvasGO.transform, "PlayButton", "  OYNA  ",
                new Vector2(0, -50), new Color(0.15f, 0.55f, 0.25f), () =>
                {
                    SceneManager.LoadScene("Prototype");
                });

            CreateButton(canvasGO.transform, "FullscreenButton", "TAM EKRAN",
                new Vector2(0, -110), new Color(0.25f, 0.25f, 0.45f), () =>
                {
                    Screen.fullScreen = !Screen.fullScreen;
                });
        }

        private GameObject CreateText(Transform parent, string name, string text,
            float fontSize, Color color, Vector2 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(600, 80);
            return go;
        }

        private void CreateButton(Transform parent, string name, string label,
            Vector2 position, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);

            var img = btnGO.AddComponent<Image>();
            img.color = color;

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            var rect = btnGO.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200, 50);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 22;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }
    }
}
