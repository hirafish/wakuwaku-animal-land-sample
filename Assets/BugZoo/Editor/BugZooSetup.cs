using System.Collections.Generic;
using System.IO;
using BugZoo.Common;
using BugZoo.Menu;
using BugZoo.Steps.Step01;
using BugZoo.Steps.Step02;
using BugZoo.Steps.Step01b;
using BugZoo.Steps.Step03;
using BugZoo.Steps.Step04;
using BugZoo.Steps.Step05;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace BugZoo.Editor
{
    public static class BugZooSetup
    {
        [MenuItem("Tools/WakuwakuAnimalLand/Generate All Scenes & Assets", priority = 1)]
        [MenuItem("BugZoo/Setup/All (1〜4を一括実行)", priority = 2)]
        public static void SetupAll()
        {
            CreateMaterialsAndPrefabs();
            CreateScenes();
            ConfigureExecutionOrder();
            Debug.Log("[BugZoo] すべてのセットアップが完了しました！ Assets/BugZoo/Menu/Menu.unity を開いて Play してください。");
        }

        [MenuItem("BugZoo/Setup/1. マテリアルとプレハブを作る", priority = 10)]
        public static void CreateMaterialsAndPrefabs()
        {
            EnsureFolder("Assets/BugZoo/Steps/Step01_ScoreNotAdded");
            EnsureFolder("Assets/BugZoo/Steps/Step03_MissingTarget");

            // Step 01 ScoreLabel Prefab
            var scoreGo = new GameObject("ScoreLabel");
            scoreGo.AddComponent<ScoreView>();
            PrefabUtility.SaveAsPrefabAsset(scoreGo, "Assets/BugZoo/Steps/Step01_ScoreNotAdded/ScoreLabel.prefab");
            Object.DestroyImmediate(scoreGo);

            // Step 03 Enemy Prefab
            var enemyGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyGo.name = "Enemy";
            AttachAnimal(enemyGo, "Taipan", new Vector3(0f, -0.5f, 0f), 180f, 2f);
            PrefabUtility.SaveAsPrefabAsset(enemyGo, "Assets/BugZoo/Steps/Step03_MissingTarget/Enemy.prefab");
            Object.DestroyImmediate(enemyGo);

            AssetDatabase.SaveAssets();
            Debug.Log("[BugZoo] 1. マテリアルとプレハブを作成しました。");
        }

        [MenuItem("BugZoo/Setup/2. シーンを作る", priority = 12)]
        public static void CreateScenes()
        {
            var scenePaths = new List<string>();

            // Menu
            EnsureFolder("Assets/BugZoo/Menu");
            var menuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var menuGo = new GameObject("SceneMenu");
            menuGo.AddComponent<SceneMenu>();
            SaveScene(menuScene, "Assets/BugZoo/Menu/Menu.unity", scenePaths);

            // Step 01
            var s01 = NewBugScene("Step 1a", "おやつをあげてもスコアが増えない（Prefab誤参照）",
                "1. 「おやつをあげる (+1)」を押しても SCORE が 0 のままなのを確認\n"
                + "2. Console の SetScore ログを1回クリックし、Project のプレハブが光ることを確認\n"
                + "3. 修正版に切り替えてもう一度押す");
            var scoreLabelInScene = new GameObject("ScoreLabel");
            var viewInScene = scoreLabelInScene.AddComponent<ScoreView>();
            var prefabView = AssetDatabase.LoadAssetAtPath<ScoreView>("Assets/BugZoo/Steps/Step01_ScoreNotAdded/ScoreLabel.prefab");
            var counterGo = new GameObject("ScoreCounter");
            var counter = counterGo.AddComponent<ScoreCounter>();
            SetPrivateField(counter, "_viewInScene", viewInScene);
            SetPrivateField(counter, "_viewPrefabAsset", prefabView);

            var puduPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Quirky Series Ultimate/FREE/Prefabs/Pudu.prefab");
            if (puduPrefab != null)
            {
                var pudu = (GameObject)PrefabUtility.InstantiatePrefab(puduPrefab);
                pudu.name = "Pudu";
                pudu.transform.position = new Vector3(0f, -0.5f, 0f);
                pudu.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                pudu.transform.localScale = new Vector3(2f, 2f, 2f);
            }

            SaveScene(s01, "Assets/BugZoo/Steps/Step01_ScoreNotAdded/Step01_ScoreNotAdded.unity", scenePaths);

            // Step 01b
            EnsureFolder("Assets/BugZoo/Steps/Step01b_InitialValueIgnored");
            var s01b = NewBugScene("Step 1b", "コードで初期値を変えたのに速度が変わらない（Inspector保存値優先）",
                "1. Hierarchy の Runner を選び、Inspector の Speed が 5 になっていることを確認\n"
                + "2. コード(RunnerMover.cs)の初期値は 10 なのに、シーンの保存値(5)が優先されていることを確認\n"
                + "3. コンポーネント右上のメニュー > Reset で 10 に戻ることを確認");
            var runnerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runnerGo.name = "Runner";
            AttachAnimal(runnerGo, "Gecko", new Vector3(0f, -0.5f, 0f), 90f, 2f);
            var runnerMover = runnerGo.AddComponent<RunnerMover>();
            var soRunner = new SerializedObject(runnerMover);
            soRunner.FindProperty("speed").floatValue = 5f;
            soRunner.ApplyModifiedPropertiesWithoutUndo();
            SaveScene(s01b, "Assets/BugZoo/Steps/Step01b_InitialValueIgnored/Step01b_InitialValueIgnored.unity", scenePaths);


            // Step 02
            EnsureFolder("Assets/BugZoo/Steps/Step02_ScoreDoubled");
            var s02 = NewBugScene("Step 2", "処理が2回呼ばれる・スコアが2倍になる（BodyとFeetの2重コライダー）",
                "1. Play してコイン8枚を取るとスコアが 12 になることを確認\n"
                + "2. Console のログで Body と Feet の2つが触れていることを確認\n"
                + "3. 修正版に切り替えてスコアが 8 になることを確認");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3f, 1f, 1f);
            ground.transform.position = new Vector3(10f, 0f, 0f);

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Body";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            var playerRb = player.AddComponent<Rigidbody>();
            playerRb.isKinematic = true;
            playerRb.useGravity = false;
            var mover = player.AddComponent<PlayerMover>();

            if (puduPrefab != null)
            {
                var pudu = (GameObject)PrefabUtility.InstantiatePrefab(puduPrefab, player.transform);
                pudu.name = "Pudu";
                pudu.transform.localPosition = new Vector3(0f, -1f, 0f);
                pudu.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                pudu.transform.localScale = new Vector3(2f, 2f, 2f);
                var mr = player.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }

            var feet = new GameObject("Feet");
            feet.transform.SetParent(player.transform, false);
            feet.transform.localPosition = new Vector3(0f, -0.8f, 0f);
            var feetCol = feet.AddComponent<SphereCollider>();
            feetCol.radius = 0.3f;

            var scoreHolder = new GameObject("CoinScore");
            var coinScore = scoreHolder.AddComponent<CoinScore>();
            var coins = new Coin[8];
            for (var i = 0; i < 8; i++)
            {
                var coinGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                coinGo.name = $"Coin_{i + 1}";
                coinGo.transform.localScale = Vector3.one * 0.5f;
                var y = (i % 2 == 0) ? 0.2f : 1.5f;
                coinGo.transform.position = new Vector3((i + 1) * 2f, y, 0f);
                coinGo.GetComponent<SphereCollider>().isTrigger = true;
                var c = coinGo.AddComponent<Coin>();
                SetPrivateField(c, "_score", coinScore);
                coins[i] = c;
            }
            SetPrivateField(coinScore, "_player", mover);
            SetPrivateObjectArray(coinScore, "_coins", coins);
            SaveScene(s02, "Assets/BugZoo/Steps/Step02_ScoreDoubled/Step02_ScoreDoubled.unity", scenePaths);

            // Step 03
            var s03 = NewBugScene("Step 3", "倒したはずの対象で MissingReferenceException（?. vs == null）",
                "1. 「敵を倒す」を押し、Destroy 直後と次フレームの == null ログを確認\n"
                + "2. バグ版では _target?.position が素通りして毎フレーム例外が出ることを確認\n"
                + "3. 修正版に切り替えて「敵を出す」→「敵を倒す」を試す");
            var enemyInScene = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyInScene.name = "Enemy";
            enemyInScene.transform.position = new Vector3(3f, 0.5f, 0f);
            AttachAnimal(enemyInScene, "Taipan", new Vector3(0f, -0.5f, 0f), 180f, 2f);
            var aimGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aimGo.name = "AimMarker";
            aimGo.transform.localScale = Vector3.one * 0.4f;
            var aim = aimGo.AddComponent<AimMarker>();
            SetPrivateField(aim, "_target", enemyInScene.transform);
            SetPrivateField(aim, "_enemyPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BugZoo/Steps/Step03_MissingTarget/Enemy.prefab"));
            SaveScene(s03, "Assets/BugZoo/Steps/Step03_MissingTarget/Step03_MissingTarget.unity", scenePaths);

            // Step 04
            EnsureFolder("Assets/BugZoo/Steps/Step04_ButtonBlocked");
            var s04 = NewBugScene("Step 4", "UIのボタンが押せない・反応しない（透明FadeOverlayのRaycast Target）",
                "1. 画面中央の「買う」ボタンを押しても反応しないことを確認\n"
                + "2. Hierarchy の EventSystem を選び、Inspector 下部プレビューに FadeOverlay が出ることを確認\n"
                + "3. 「画面中央の RaycastAll 候補をログ出力」または修正版への切り替えを試す");
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var shopPanel = new GameObject("ShopPanel");
            shopPanel.transform.SetParent(canvasGo.transform, false);
            var shopRect = shopPanel.AddComponent<RectTransform>();
            shopRect.sizeDelta = new Vector2(460f, 320f);
            var shopGroup = shopPanel.AddComponent<CanvasGroup>();
            var shopBg = shopPanel.AddComponent<Image>();
            shopBg.color = new Color(0.96f, 0.94f, 0.90f, 0.96f);
            var shopOutline = shopPanel.AddComponent<Outline>();
            shopOutline.effectColor = new Color(0.80f, 0.75f, 0.68f, 1f);
            shopOutline.effectDistance = new Vector2(2f, -2f);
            var shopShadow = shopPanel.AddComponent<Shadow>();
            shopShadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
            shopShadow.effectDistance = new Vector2(4f, -6f);

            var defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Header Title
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(shopPanel.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0f, 115f);
            titleRect.sizeDelta = new Vector2(420f, 40f);
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = "どうぶつ特製フードショップ";
            titleText.font = defaultFont;
            titleText.fontSize = 22;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(0.35f, 0.25f, 0.15f, 1f);

            // Item Name & Description
            var descGo = new GameObject("Description");
            descGo.transform.SetParent(shopPanel.transform, false);
            var descRect = descGo.AddComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0f, 35f);
            descRect.sizeDelta = new Vector2(420f, 80f);
            var descText = descGo.AddComponent<Text>();
            descText.text = "プレミアム特製おやつ\n動物たちがよろこぶ栄養満点フード\n価格: 500 GOLD";
            descText.font = defaultFont;
            descText.fontSize = 16;
            descText.lineSpacing = 1.3f;
            descText.alignment = TextAnchor.MiddleCenter;
            descText.color = new Color(0.45f, 0.35f, 0.25f, 1f);

            // Buy Button
            var buyBtnGo = new GameObject("BuyButton");
            buyBtnGo.transform.SetParent(shopPanel.transform, false);
            var btnRect = buyBtnGo.AddComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0f, -65f);
            btnRect.sizeDelta = new Vector2(320f, 70f);
            var btnImg = buyBtnGo.AddComponent<Image>();
            btnImg.color = new Color(1.0f, 0.58f, 0.12f, 1f);
            var btnOutline = buyBtnGo.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0.85f, 0.40f, 0.05f, 1f);
            btnOutline.effectDistance = new Vector2(2f, -2f);
            var btnShadow = buyBtnGo.AddComponent<Shadow>();
            btnShadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
            btnShadow.effectDistance = new Vector2(2f, -4f);

            var button = buyBtnGo.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1.0f, 0.72f, 0.25f, 1f);
            colors.pressedColor = new Color(0.85f, 0.45f, 0.05f, 1f);
            button.colors = colors;
            var shopBtn = buyBtnGo.AddComponent<ShopButton>();

            var btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(buyBtnGo.transform, false);
            var textRect = btnTextGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var uiText = btnTextGo.AddComponent<Text>();
            uiText.text = "購入する";
            uiText.font = defaultFont;
            uiText.fontSize = 28;
            uiText.fontStyle = FontStyle.Bold;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.color = Color.white;
            var textShadow = btnTextGo.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0.5f, 0.2f, 0f, 0.6f);
            textShadow.effectDistance = new Vector2(1f, -2f);

            // Purchase Feedback Toast
            var feedbackGo = new GameObject("FeedbackText");
            feedbackGo.transform.SetParent(canvasGo.transform, false);
            var fbRect = feedbackGo.AddComponent<RectTransform>();
            fbRect.anchoredPosition = new Vector2(0f, -220f);
            fbRect.sizeDelta = new Vector2(600f, 50f);
            var fbText = feedbackGo.AddComponent<Text>();
            fbText.font = defaultFont;
            fbText.fontSize = 24;
            fbText.fontStyle = FontStyle.Bold;
            fbText.alignment = TextAnchor.MiddleCenter;
            fbText.color = new Color(0.15f, 0.75f, 0.30f, 1f);
            var fbShadow = feedbackGo.AddComponent<Shadow>();
            fbShadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            fbShadow.effectDistance = new Vector2(2f, -2f);
            feedbackGo.SetActive(false);
            shopBtn.SetFeedbackLabel(fbText);

            var overlayGo = new GameObject("FadeOverlay");
            overlayGo.transform.SetParent(canvasGo.transform, false);
            var ovRect = overlayGo.AddComponent<RectTransform>();
            ovRect.anchorMin = Vector2.zero;
            ovRect.anchorMax = Vector2.one;
            ovRect.sizeDelta = Vector2.zero;
            var ovImage = overlayGo.AddComponent<Image>();
            ovImage.color = new Color(0f, 0f, 0f, 0f);
            ovImage.raycastTarget = true;

            var blockerGo = new GameObject("OverlayBlocker");
            var blocker = blockerGo.AddComponent<OverlayBlocker>();
            SetPrivateField(blocker, "_overlay", ovImage);
            SetPrivateField(blocker, "_shopPanelGroup", shopGroup);
            SaveScene(s04, "Assets/BugZoo/Steps/Step04_ButtonBlocked/Step04_ButtonBlocked.unity", scenePaths);

            // Step 05
            EnsureFolder("Assets/BugZoo/Steps/Step05_RareStateBug");
            var s05 = NewBugScene("Step 5", "止めると再現しないバグ・同一フレーム上書き（Time.frameCount と Debug.Break）",
                "1. 敵は普段2秒に1回前方に突進（Attack）を出します\n"
                + "2. 「バグを再現する（攻撃＋被弾を同一フレームで発生）」を押すと、突進が出ずに棒立ちになるバグが発生します\n"
                + "3. 「2回変更で止める: ON」にして再現すると、Unityが自動で一時停止し、Consoleに同一フレーム番号のログが2行並ぶのを確認できます");
            var rareEnemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rareEnemy.name = "Enemy";

            var sparrowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Quirky Series Ultimate/FREE/Prefabs/Sparrow.prefab");
            if (sparrowPrefab != null)
            {
                var sparrow = (GameObject)PrefabUtility.InstantiatePrefab(sparrowPrefab, rareEnemy.transform);
                sparrow.name = "Sparrow";
                sparrow.transform.localPosition = Vector3.zero;
                sparrow.transform.localRotation = Quaternion.Euler(0f, 160f, 0f);
                sparrow.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
                var mr = rareEnemy.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = false;
            }

            var cam05 = Camera.main;
            if (cam05 != null)
            {
                cam05.transform.position = new Vector3(1.0f, 1.8f, -4.8f);
                cam05.transform.rotation = Quaternion.Euler(14f, -12f, 0f);
            }

            var smComp = rareEnemy.AddComponent<EnemyStateMachine>();
            var atkTimer = rareEnemy.AddComponent<AttackTimer>();
            var hitRec = rareEnemy.AddComponent<HitReceiver>();
            var rareScen = rareEnemy.AddComponent<RareStateScenario>();
            SetPrivateField(atkTimer, "_stateMachine", smComp);
            SetPrivateField(hitRec, "_stateMachine", smComp);
            SetPrivateField(rareScen, "_stateMachine", smComp);
            SetPrivateField(rareScen, "_attackTimer", atkTimer);
            SetPrivateField(rareScen, "_hitReceiver", hitRec);
            SaveScene(s05, "Assets/BugZoo/Steps/Step05_RareStateBug/Step05_RareStateBug.unity", scenePaths);

            // Register scenes in Build Settings
            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (var p in scenePaths)
            {
                buildScenes.Add(new EditorBuildSettingsScene(p, true));
            }
            EditorBuildSettings.scenes = buildScenes.ToArray();

            Debug.Log($"[BugZoo] 3. 全 {scenePaths.Count} シーンを作成し、Build Settings に登録しました。");
        }

        [MenuItem("BugZoo/Setup/4. 実行順を設定する", priority = 13)]
        public static void ConfigureExecutionOrder()
        {
            SetOrder("Assets/BugZoo/Steps/Step05_RareStateBug/AttackTimer.cs", -20);
            SetOrder("Assets/BugZoo/Steps/Step05_RareStateBug/HitReceiver.cs", -10);
            Debug.Log("[BugZoo] 4. スクリプト実行順（Step05 AttackTimer/HitReceiver）を設定しました。");
        }

        private static UnityEngine.SceneManagement.Scene NewBugScene(string id, string title, string steps)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var uiGo = new GameObject("BugZooUI");
            var panel = uiGo.AddComponent<QuestionPanel>();
            panel.Configure(id, title, steps);
            uiGo.AddComponent<FpsHud>();
            return scene;
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string path, List<string> list)
        {
            DecorateScene();
            EditorSceneManager.SaveScene(scene, path);
            list.Add(path);
        }

        // ===== 動物の見た目を差し込む（Quirky Series FREE がない環境ではキューブのまま） =====
        private const string AnimalFolder = "Assets/Quirky Series Ultimate/FREE/Prefabs";

        private static GameObject AttachAnimal(GameObject host, string animal, Vector3 localPos,
            float yRot, float scale)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{AnimalFolder}/{animal}.prefab");
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, host.transform);
            go.name = animal;
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(0f, yRot, 0f);
            go.transform.localScale = Vector3.one * scale;
            // 元のキューブ等の見た目だけ消す（Collider やスクリプトはそのまま残す）
            var mr = host.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
            return go;
        }

        // ===== 園内の背景（素材なし・Built-in RP 用） =====
        private const string EnvFolder = "Assets/BugZoo/Common/Environment";

        private static void DecorateScene()
        {
            // 空：プロシージャルスカイ＋淡いフォグ
            var sky = GetOrCreateMaterial("Sky", "Skybox/Procedural", Color.white);
            if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", new Color(0.45f, 0.7f, 1f));
            if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", new Color(0.62f, 0.75f, 0.55f));
            if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 0.7f);
            if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.25f);
            if (sky.HasProperty("_SunSize")) sky.SetFloat("_SunSize", 0.03f);
            RenderSettings.skybox = sky;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.78f, 0.86f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.82f, 0.85f, 0.72f);
            RenderSettings.ambientGroundColor = new Color(0.5f, 0.45f, 0.36f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.8f, 0.9f, 1f);
            RenderSettings.fogStartDistance = 25f;
            RenderSettings.fogEndDistance = 70f;

            // ライト：暖色寄り＋ソフトシャドウ
            var light = Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(1f, 0.9f, 0.76f);
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.6f;
                light.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            }

            var cam = Camera.main;
            if (cam != null) cam.clearFlags = CameraClearFlags.Skybox;

            var root = new GameObject("Environment");
            var grass = GetOrCreateMaterial("Grass", "Standard", new Color(0.55f, 0.78f, 0.42f));
            var path = GetOrCreateMaterial("Path", "Standard", new Color(0.86f, 0.76f, 0.58f));

            // 既存の Ground（Step 2）は「園路」の色にして、その少し下に芝生を敷く
            var groundY = -0.5f;
            var existingGround = GameObject.Find("Ground");
            if (existingGround != null)
            {
                existingGround.GetComponent<Renderer>().sharedMaterial = path;
                groundY = existingGround.transform.position.y - 0.01f;
            }
            else if (GameObject.Find("Sparrow") != null)
            {
                groundY = -0.01f; // スズメは足元が原点
            }

            var lawn = CreateDecor(PrimitiveType.Plane, "Lawn", root.transform, grass,
                new Vector3(0f, groundY, 10f), new Vector3(8f, 1f, 5f));
            lawn.GetComponent<Renderer>().receiveShadows = true;

            var wood = GetOrCreateMaterial("Wood", "Standard", new Color(0.62f, 0.42f, 0.26f));
            var leaf = GetOrCreateMaterial("Leaf", "Standard", new Color(0.3f, 0.62f, 0.32f));
            var board = GetOrCreateMaterial("SignBoard", "Standard", new Color(1f, 0.85f, 0.45f));

            // 柵：杭＋横木2本
            const float fenceZ = 6f;
            for (var x = -24f; x <= 24f; x += 2f)
            {
                CreateDecor(PrimitiveType.Cube, "FencePost", root.transform, wood,
                    new Vector3(x, groundY + 0.45f, fenceZ), new Vector3(0.18f, 0.9f, 0.18f));
            }
            foreach (var h in new[] { 0.35f, 0.75f })
            {
                CreateDecor(PrimitiveType.Cube, "FenceRail", root.transform, wood,
                    new Vector3(0f, groundY + h, fenceZ), new Vector3(48f, 0.1f, 0.08f));
            }

            // 木：円柱の幹＋球の葉
            var treeXs = new[] { -18f, -12f, -7f, 7f, 12f, 18f };
            for (var i = 0; i < treeXs.Length; i++)
            {
                var z = 9f + (i % 2) * 2.5f;
                var s = 1f + (i % 3) * 0.25f;
                CreateDecor(PrimitiveType.Cylinder, "TreeTrunk", root.transform, wood,
                    new Vector3(treeXs[i], groundY + 1f * s, z), new Vector3(0.35f, 1f * s, 0.35f));
                CreateDecor(PrimitiveType.Sphere, "TreeLeaf", root.transform, leaf,
                    new Vector3(treeXs[i], groundY + 2.6f * s, z), Vector3.one * 2.4f * s);
            }

            // 看板：「わくわくアニマルランド」
            var signY = groundY + 4.2f;
            const float signZ = 8f;
            foreach (var px in new[] { -2.6f, 2.6f })
            {
                var postHalf = (signY - groundY) * 0.5f; // Cylinder は高さ2なので scale.y = 高さの半分
                CreateDecor(PrimitiveType.Cylinder, "SignPost", root.transform, wood,
                    new Vector3(px, groundY + postHalf, signZ + 0.1f),
                    new Vector3(0.2f, postHalf, 0.2f));
            }
            CreateDecor(PrimitiveType.Cube, "SignBoard", root.transform, board,
                new Vector3(0f, signY, signZ), new Vector3(6.4f, 1.3f, 0.2f));
            var textGo = new GameObject("SignText");
            textGo.transform.SetParent(root.transform, false);
            textGo.transform.position = new Vector3(0f, signY, signZ - 0.12f);
            var tm = textGo.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            tm.font = font;
            tm.text = "わくわくアニマルランド";
            tm.fontSize = 96;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.45f, 0.22f, 0.1f);
            textGo.GetComponent<MeshRenderer>().sharedMaterial = font.material;
        }

        private static GameObject CreateDecor(PrimitiveType type, string name, Transform parent,
            Material mat, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            // 物理判定に影響させない（Step 2 のトリガー検証などを壊さないため）
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.isStatic = true;
            return go;
        }

        private static Material GetOrCreateMaterial(string name, string shaderName, Color color)
        {
            EnsureFolder(EnvFolder);
            var matPath = $"{EnvFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find(shaderName));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            if (shaderName == "Standard")
            {
                mat.color = color;
                mat.SetFloat("_Glossiness", 0.1f);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void SetPrivateField(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetPrivateString(Object target, string fieldName, string value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.stringValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetPrivateObjectArray(Object target, string fieldName, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null && prop.isArray)
            {
                prop.arraySize = values.Length;
                for (var i = 0; i < values.Length; i++)
                {
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetOrder(string scriptPath, int order)
        {
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
            if (script != null && MonoImporter.GetExecutionOrder(script) != order)
            {
                MonoImporter.SetExecutionOrder(script, order);
            }
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
