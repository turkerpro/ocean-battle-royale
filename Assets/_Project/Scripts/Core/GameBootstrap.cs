using UnityEngine;
using UnityEngine.UI;

namespace OceanBattleRoyale.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        private static Camera _cam;
        private static Material _waterMat;
        private static int _botCount = 30;
        private static Transform[] _ships;
        private static float _camHeight = 40f;
        private static int _localPlayerIndex = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Prototype") return;
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
        }

        private void Start()
        {
            CreateWater();
            CreateShips();
            CreateHUD();
        }

        private void CreateWater()
        {
            _waterMat = new Material(Shader.Find("Standard"));
            _waterMat.color = new Color(0.05f, 0.2f, 0.45f);
            _waterMat.SetFloat("_Glossiness", 0.9f);
            _waterMat.SetFloat("_Metallic", 0.1f);

            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Water";
            plane.transform.localScale = new Vector3(100, 1, 100);
            plane.transform.position = Vector3.zero;
            plane.GetComponent<MeshRenderer>().material = _waterMat;
        }

        private void CreateShips()
        {
            _ships = new Transform[_botCount];

            for (int i = 0; i < _botCount; i++)
            {
                var ship = CreateShip(i == _localPlayerIndex);
                Vector2 pos = Random.insideUnitCircle * 400f;
                ship.transform.position = new Vector3(pos.x, 0.5f, pos.y);
                ship.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                _ships[i] = ship.transform;
            }
        }

        private GameObject CreateShip(bool isPlayer)
        {
            var ship = new GameObject(isPlayer ? "PlayerShip" : "BotShip");

            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(ship.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(1.5f, 0.6f, 3f);
            var renderer = body.GetComponent<MeshRenderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = isPlayer ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.7f, 0.2f, 0.2f);

            var sail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sail.name = "Sail";
            sail.transform.SetParent(ship.transform);
            sail.transform.localPosition = new Vector3(0, 1.2f, 0);
            sail.transform.localScale = new Vector3(0.1f, 1.5f, 1.5f);
            sail.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            sail.GetComponent<MeshRenderer>().material.color = isPlayer ? Color.white : new Color(0.8f, 0.8f, 0.6f);

            ship.AddComponent<SphereCollider>().isTrigger = true;
            ship.GetComponent<SphereCollider>().radius = 2f;

            var rb = ship.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.drag = 2f;
            rb.angularDrag = 3f;

            ship.AddComponent<ShipHealth>();

            return ship;
        }

        private void CreateHUD()
        {
            var canvasGO = new GameObject("GameCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var infoGO = new GameObject("Info");
            infoGO.transform.SetParent(canvasGO.transform, false);
            var txt = infoGO.AddComponent<Text>();
            txt.text = "WASD - Hareket | SPACE - Ates | M - Mayin";
            txt.fontSize = 16;
            txt.color = Color.white;
            txt.alignment = TextAnchor.UpperCenter;
            txt.font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            var rect = infoGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(0, -20);
            rect.sizeDelta = new Vector2(0, 30);
        }

        private void Update()
        {
            if (_ships == null || _ships[_localPlayerIndex] == null) return;

            HandleInput();
            UpdateCamera();
        }

        private void HandleInput()
        {
            var ship = _ships[_localPlayerIndex];
            if (ship == null) return;

            float speed = 15f;
            float rotSpeed = 100f;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                ship.position += ship.forward * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                ship.position -= ship.forward * speed * 0.5f * Time.deltaTime;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                ship.Rotate(Vector3.up, -rotSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                ship.Rotate(Vector3.up, rotSpeed * Time.deltaTime);

            ship.position = new Vector3(ship.position.x, 0.5f, ship.position.z);
        }

        private void UpdateCamera()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null) return;
            }

            var target = _ships[_localPlayerIndex];
            if (target == null) return;

            _cam.transform.position = target.position + new Vector3(0, _camHeight, -20f);
            _cam.transform.LookAt(target.position);
        }

        private class ShipHealth : MonoBehaviour
        {
            public float health = 100f;
            public float maxHealth = 100f;
            public int level = 1;
            public float speed = 12f;

            private float _nextBotMove;
            private Vector3 _botTarget;

            private void Start()
            {
                _botTarget = transform.position + Random.insideUnitSphere * 100f;
                _botTarget.y = 0.5f;
                _nextBotMove = Time.time + Random.Range(2f, 8f);
            }

            private void Update()
            {
                if (gameObject.name == "PlayerShip") return;

                if (Time.time > _nextBotMove)
                {
                    _botTarget = transform.position + new Vector3(Random.Range(-200f, 200f), 0.5f, Random.Range(-200f, 200f));
                    _nextBotMove = Time.time + Random.Range(3f, 10f);
                }

                Vector3 dir = (_botTarget - transform.position);
                dir.y = 0;
                if (dir.magnitude > 5f)
                {
                    transform.position += dir.normalized * speed * 0.5f * Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), Time.deltaTime * 2f);
                }
            }
        }
    }
}
