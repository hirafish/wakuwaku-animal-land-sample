using System.Collections.Generic;
using System.IO;
using BugZoo.Common;
using BugZoo.Menu;
using BugZoo.Part1.Q01;
using BugZoo.Part1.Q02;
using BugZoo.Part1.Q03;
using BugZoo.Part1.Q04;
using BugZoo.Part1.Q05;
using BugZoo.Part1.Q06;
using BugZoo.Part1.Q07;
using BugZoo.Part1.Q08;
using BugZoo.Part1.Q09;
using BugZoo.Part1.Q10;
using BugZoo.Part2;
using BugZoo.Part3.Q24;
using BugZoo.Part3.Q25;
using BugZoo.Part3.Q26;
using BugZoo.Part3.Q27;
using BugZoo.Part3.Q29;
using BugZoo.Part3.Q30;
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
        [MenuItem("BugZoo/Setup/All (1〜4を一括実行)", priority = 1)]
        public static void SetupAll()
        {
            CreateMaterialsAndPrefabs();
            CreateAnimationAssets();
            CreateScenes();
            ConfigureExecutionOrder();
            Debug.Log("[BugZoo] すべてのセットアップが完了しました！ Assets/BugZoo/Menu/Menu.unity を開いて Play してください。");
        }

        [MenuItem("BugZoo/Setup/1. マテリアルとプレハブを作る", priority = 10)]
        public static void CreateMaterialsAndPrefabs()
        {
            EnsureFolder("Assets/BugZoo/Part1/Q01_ScoreNotAdded");
            EnsureFolder("Assets/BugZoo/Part1/Q04_MissingTarget");
            EnsureFolder("Assets/BugZoo/Part1/Q08_PhysicsMiss");
            EnsureFolder("Assets/BugZoo/Part3/Q26_ManyEnemies");
            EnsureFolder("Assets/BugZoo/Part3/Q29_MemoryLeak");
            EnsureFolder("Assets/BugZoo/Part3/Q30_DrawHeavy/Materials");

            // Q01 ScoreLabel Prefab
            var scoreGo = new GameObject("ScoreLabel");
            scoreGo.AddComponent<ScoreView>();
            PrefabUtility.SaveAsPrefabAsset(scoreGo, "Assets/BugZoo/Part1/Q01_ScoreNotAdded/ScoreLabel.prefab");
            Object.DestroyImmediate(scoreGo);

            // Q04 Enemy Prefab
            var enemyGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyGo.name = "Enemy";
            PrefabUtility.SaveAsPrefabAsset(enemyGo, "Assets/BugZoo/Part1/Q04_MissingTarget/Enemy.prefab");
            Object.DestroyImmediate(enemyGo);

            // Q08 Bullet Prefab
            var bulletGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulletGo.name = "Bullet";
            bulletGo.transform.localScale = Vector3.one * 0.25f;
            var rb = bulletGo.AddComponent<Rigidbody>();
            rb.useGravity = false;
            bulletGo.AddComponent<Bullet>();
            PrefabUtility.SaveAsPrefabAsset(bulletGo, "Assets/BugZoo/Part1/Q08_PhysicsMiss/Bullet.prefab");
            Object.DestroyImmediate(bulletGo);

            // Q26 SwarmEnemy & Projectile Prefabs
            var swarmEnemyGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            swarmEnemyGo.name = "SwarmEnemy";
            swarmEnemyGo.transform.localScale = Vector3.one * 0.5f;
            swarmEnemyGo.AddComponent<SwarmEnemy>();
            PrefabUtility.SaveAsPrefabAsset(swarmEnemyGo, "Assets/BugZoo/Part3/Q26_ManyEnemies/SwarmEnemy.prefab");
            Object.DestroyImmediate(swarmEnemyGo);

            var projGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projGo.name = "Projectile";
            projGo.transform.localScale = Vector3.one * 0.2f;
            PrefabUtility.SaveAsPrefabAsset(projGo, "Assets/BugZoo/Part3/Q26_ManyEnemies/Projectile.prefab");
            Object.DestroyImmediate(projGo);

            // Q29 LeakyPopup Prefab
            var popupGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            popupGo.name = "LeakyPopup";
            popupGo.AddComponent<LeakyPopup>();
            PrefabUtility.SaveAsPrefabAsset(popupGo, "Assets/BugZoo/Part3/Q29_MemoryLeak/LeakyPopup.prefab");
            Object.DestroyImmediate(popupGo);

            // Q30 Cube Prefab & 8 Shared Materials
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            for (var i = 0; i < 8; i++)
            {
                var matPath = $"Assets/BugZoo/Part3/Q30_DrawHeavy/Materials/SharedMat_{i}.mat";
                var mat = new Material(shader);
                var c = Color.HSVToRGB(i / 8f, 0.75f, 0.9f);
                mat.color = c;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            var floodCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floodCube.name = "FloodCube";
            Object.DestroyImmediate(floodCube.GetComponent<Collider>());
            PrefabUtility.SaveAsPrefabAsset(floodCube, "Assets/BugZoo/Part3/Q30_DrawHeavy/FloodCube.prefab");
            Object.DestroyImmediate(floodCube);

            AssetDatabase.SaveAssets();
            Debug.Log("[BugZoo] 1. マテリアルとプレハブを作成しました。");
        }

        [MenuItem("BugZoo/Setup/2. アニメーションのアセットを作る", priority = 11)]
        [MenuItem("BugZoo/AnimationLab/Animator を初期状態に戻す", priority = 50)]
        public static void CreateAnimationAssets()
        {
            EnsureFolder("Assets/BugZoo/Part2/Animations");
            EnsureFolder("Assets/BugZoo/Part2/Prefabs");

            // Idle.anim (1.0s loop, localScale 1 -> 1.1 -> 1)
            var idleClip = new AnimationClip { name = "Idle" };
            var scaleCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.5f, 1.1f),
                new Keyframe(1.0f, 1f));
            AnimationUtility.SetEditorCurve(idleClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"), scaleCurve);
            AnimationUtility.SetEditorCurve(idleClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"), scaleCurve);
            AnimationUtility.SetEditorCurve(idleClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.z"), scaleCurve);
            SetLoop(idleClip, true);
            AssetDatabase.CreateAsset(idleClip, "Assets/BugZoo/Part2/Animations/Idle.anim");

            // Jump.anim (0.6s, localPosition.y 0 -> 2 -> 0)
            var jumpClip = new AnimationClip { name = "Jump" };
            var jumpCurve = new AnimationCurve(
                new Keyframe(0f, 0.5f),
                new Keyframe(0.3f, 2.5f),
                new Keyframe(0.6f, 0.5f));
            AnimationUtility.SetEditorCurve(jumpClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y"), jumpCurve);
            SetLoop(jumpClip, false);
            AssetDatabase.CreateAsset(jumpClip, "Assets/BugZoo/Part2/Animations/Jump.anim");

            // Attack.anim (0.5s, localEulerAnglesRaw.y 0 -> 180, Event at 0.25s)
            var attackClip = new AnimationClip { name = "Attack" };
            var rotCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.5f, 180f));
            AnimationUtility.SetEditorCurve(attackClip, EditorCurveBinding.FloatCurve("", typeof(Transform), "localEulerAnglesRaw.y"), rotCurve);
            AnimationUtility.SetAnimationEvents(attackClip, new[]
            {
                new AnimationEvent { time = 0.25f, functionName = "OnAttackHit" }
            });
            SetLoop(attackClip, false);
            AssetDatabase.CreateAsset(attackClip, "Assets/BugZoo/Part2/Animations/Attack.anim");

            // Hit.anim (0.4s loop, _BaseColor red)
            var hitClip = new AnimationClip { name = "Hit" };
            var redOne = AnimationCurve.Constant(0f, 0.4f, 1f);
            var redZero = AnimationCurve.Constant(0f, 0.4f, 0f);
            AnimationUtility.SetEditorCurve(hitClip, EditorCurveBinding.FloatCurve("", typeof(MeshRenderer), "material._BaseColor.r"), redOne);
            AnimationUtility.SetEditorCurve(hitClip, EditorCurveBinding.FloatCurve("", typeof(MeshRenderer), "material._BaseColor.g"), redZero);
            AnimationUtility.SetEditorCurve(hitClip, EditorCurveBinding.FloatCurve("", typeof(MeshRenderer), "material._BaseColor.b"), redZero);
            SetLoop(hitClip, true);
            AssetDatabase.CreateAsset(hitClip, "Assets/BugZoo/Part2/Animations/Hit.anim");

            // Target.controller (バグ版の初期状態)
            var controllerPath = "Assets/BugZoo/Part2/Animations/Target.controller";
            if (File.Exists(controllerPath)) AssetDatabase.DeleteAsset(controllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsHit", AnimatorControllerParameterType.Bool);

            var sm = controller.layers[0].stateMachine;
            var idleState = sm.AddState("Idle");
            idleState.motion = idleClip;
            idleState.writeDefaultValues = true;
            AttachLogger(idleState, "Idle");
            sm.defaultState = idleState;

            var jumpState = sm.AddState("Jump");
            jumpState.motion = jumpClip;
            jumpState.writeDefaultValues = true;
            AttachLogger(jumpState, "Jump");

            var attackState = sm.AddState("Attack");
            attackState.motion = attackClip;
            attackState.writeDefaultValues = true;
            AttachLogger(attackState, "Attack");
            attackState.AddStateMachineBehaviour<AttackExitNotifier>();

            var hitState = sm.AddState("Hit");
            hitState.motion = hitClip;
            hitState.writeDefaultValues = true;
            AttachLogger(hitState, "Hit");

            // Q12: Idle -> Jump (Has Exit Time = true, Exit Time = 0.75)
            var tIdleJump = idleState.AddTransition(jumpState);
            tIdleJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");
            tIdleJump.hasExitTime = true;
            tIdleJump.exitTime = 0.75f;
            tIdleJump.hasFixedDuration = true;
            tIdleJump.duration = 0.1f;

            var tJumpIdle = jumpState.AddTransition(idleState);
            tJumpIdle.hasExitTime = true;
            tJumpIdle.exitTime = 1.0f;
            tJumpIdle.hasFixedDuration = true;
            tJumpIdle.duration = 0.1f;

            // Q13: Idle -> Attack (Jump -> Attack は作らないため、ジャンプ中の Trigger が残る)
            var tIdleAttack = idleState.AddTransition(attackState);
            tIdleAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            tIdleAttack.hasExitTime = false;
            tIdleAttack.hasFixedDuration = true;
            tIdleAttack.duration = 0.05f;

            // Q21: Attack -> Idle (Exit Time = 0.9, Duration = 0.06s)
            var tAttackIdle = attackState.AddTransition(idleState);
            tAttackIdle.hasExitTime = true;
            tAttackIdle.exitTime = 0.9f;
            tAttackIdle.hasFixedDuration = true;
            tAttackIdle.duration = 0.06f;

            // Q15: AnyState -> Hit (Can Transition To Self = true)
            var tAnyHit = sm.AddAnyStateTransition(hitState);
            tAnyHit.AddCondition(AnimatorConditionMode.If, 0f, "IsHit");
            tAnyHit.hasExitTime = false;
            tAnyHit.hasFixedDuration = true;
            tAnyHit.duration = 0f;
            tAnyHit.canTransitionToSelf = true;

            var tHitIdle = hitState.AddTransition(idleState);
            tHitIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsHit");
            tHitIdle.hasExitTime = false;
            tHitIdle.hasFixedDuration = true;
            tHitIdle.duration = 0.1f;

            // Timeline Asset
            var timelinePath = "Assets/BugZoo/Part2/Animations/JumpTimeline.playable";
            if (File.Exists(timelinePath)) AssetDatabase.DeleteAsset(timelinePath);
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, timelinePath);
            var track = timeline.CreateTrack<AnimationTrack>(null, "Target");
            var tlClip = track.CreateClip(jumpClip);
            tlClip.displayName = "JumpCutscene";

            // AnimTarget Prefab (for Q31 Spawn 300)
            var animTargetGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            animTargetGo.name = "AnimTarget";
            var anim = animTargetGo.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            PrefabUtility.SaveAsPrefabAsset(animTargetGo, "Assets/BugZoo/Part2/Prefabs/AnimTarget.prefab");
            Object.DestroyImmediate(animTargetGo);

            AssetDatabase.SaveAssets();
            Debug.Log("[BugZoo] 2. アニメーションのアセットを作成しました。");
        }

        [MenuItem("BugZoo/Setup/3. シーンを作る", priority = 12)]
        public static void CreateScenes()
        {
            var scenePaths = new List<string>();

            // Menu
            EnsureFolder("Assets/BugZoo/Menu");
            var menuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var menuGo = new GameObject("SceneMenu");
            menuGo.AddComponent<SceneMenu>();
            SaveScene(menuScene, "Assets/BugZoo/Menu/Menu.unity", scenePaths);

            // Q01
            var s01 = NewBugScene("Q01", "ボタンを押してもスコアが増えない",
                "1. 「コインを取る (+1)」を押しても SCORE が 0 のままなのを確認
2. Console の SetScore ログを1回クリックし、Project のプレハブが光ることを確認
3. 修正版に切り替えてもう一度押す");
            var scoreLabelInScene = new GameObject("ScoreLabel");
            var viewInScene = scoreLabelInScene.AddComponent<ScoreView>();
            var prefabView = AssetDatabase.LoadAssetAtPath<ScoreView>("Assets/BugZoo/Part1/Q01_ScoreNotAdded/ScoreLabel.prefab");
            var counterGo = new GameObject("ScoreCounter");
            var counter = counterGo.AddComponent<ScoreCounter>();
            SetPrivateField(counter, "_viewInScene", viewInScene);
            SetPrivateField(counter, "_viewPrefabAsset", prefabView);
            SaveScene(s01, "Assets/BugZoo/Part1/Q01_ScoreNotAdded/Q01_ScoreNotAdded.unity", scenePaths);

            // Q02
            EnsureFolder("Assets/BugZoo/Part1/Q02_ScoreDoubled");
            var s02 = NewBugScene("Q02", "スコアがときどき2倍になる",
                "1. Play してコイン8枚を取るとスコアが 12 になることを確認
2. Console のログで Body と Feet の2つが触れていることを確認
3. 修正版に切り替えてスコアが 8 になることを確認");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(3f, 1f, 1f);
            ground.transform.position = new Vector3(10f, 0f, 0f);

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Body";
            player.transform.position = new Vector3(0f, 1f, 0f);
            var playerRb = player.AddComponent<Rigidbody>();
            playerRb.isKinematic = true;
            playerRb.useGravity = false;
            var mover = player.AddComponent<PlayerMover>();

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
            SaveScene(s02, "Assets/BugZoo/Part1/Q02_ScoreDoubled/Q02_ScoreDoubled.unity", scenePaths);

            // Q03
            EnsureFolder("Assets/BugZoo/Part1/Q03_NullOnStart");
            var s03 = NewBugScene("Q03", "起動直後だけ NullReferenceException",
                "1. Play 直後に Console に NullReferenceException が出ることを確認
2. Awake の呼ばれた順番をログで確認
3. 修正版に切り替えて「登録 OK」が出ることを確認");
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();
            var spawnerGo = new GameObject("EnemySpawner");
            spawnerGo.AddComponent<EnemySpawner>();
            SaveScene(s03, "Assets/BugZoo/Part1/Q03_NullOnStart/Q03_NullOnStart.unity", scenePaths);

            // Q04
            var s04 = NewBugScene("Q04", "敵を倒すと MissingReferenceException",
                "1. 「敵を倒す」を押し、Destroy 直後と次フレームの == null ログを確認
2. バグ版では _target?.position が素通りして毎フレーム例外が出ることを確認
3. 修正版に切り替えて「敵を出す」→「敵を倒す」を試す");
            var enemyInScene = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemyInScene.name = "Enemy";
            enemyInScene.transform.position = new Vector3(3f, 0.5f, 0f);
            var aimGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aimGo.name = "AimMarker";
            aimGo.transform.localScale = Vector3.one * 0.4f;
            var aim = aimGo.AddComponent<AimMarker>();
            SetPrivateField(aim, "_target", enemyInScene.transform);
            SetPrivateField(aim, "_enemyPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BugZoo/Part1/Q04_MissingTarget/Enemy.prefab"));
            SaveScene(s04, "Assets/BugZoo/Part1/Q04_MissingTarget/Q04_MissingTarget.unity", scenePaths);

            // Q05
            EnsureFolder("Assets/BugZoo/Part1/Q05_InitialValueIgnored");
            var s05 = NewBugScene("Q05", "コードで速度を変えたのに速さが変わらない",
                "1. Hierarchy の Runner を選び、Inspector の Speed が 5 になっていることを確認
2. コード(RunnerMover.cs)の初期値は 10 なのに、シーンの保存値(5)が優先されていることを確認
3. コンポーネント右上の ⋮ > Reset で 10 に戻ることを確認");
            var runnerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runnerGo.name = "Runner";
            var runnerMover = runnerGo.AddComponent<RunnerMover>();
            var soRunner = new SerializedObject(runnerMover);
            soRunner.FindProperty("speed").floatValue = 5f;
            soRunner.ApplyModifiedPropertiesWithoutUndo();
            SaveScene(s05, "Assets/BugZoo/Part1/Q05_InitialValueIgnored/Q05_InitialValueIgnored.unity", scenePaths);

            // Q06
            EnsureFolder("Assets/BugZoo/Part1/Q06_SecondPlayStatic");
            var s06 = NewBugScene("Q06", "2回目の Play だけ前回のスコアが残る",
                "1. Project Settings > Editor > Enter Play Mode Settings で Reload Scene only にして Play
2. 「スコア +10」を押して Play を止め、もう一度 Play するとバグ版は値が残ることを確認
3. 確認後は Enter Play Mode Settings を元に戻す");
            var staticGo = new GameObject("StaticScore");
            staticGo.AddComponent<StaticScore>();
            SaveScene(s06, "Assets/BugZoo/Part1/Q06_SecondPlayStatic/Q06_SecondPlayStatic.unity", scenePaths);

            // Q07
            EnsureFolder("Assets/BugZoo/Part1/Q07_ButtonBlocked");
            var s07 = NewBugScene("Q07", "ボタンが押せない",
                "1. 画面中央の「買う」ボタンを押しても反応しないことを確認
2. Hierarchy の EventSystem を選び、Inspector 下部プレビューに FadeOverlay が出ることを確認
3. 「画面中央の RaycastAll 候補をログ出力」または修正版への切り替えを試す");
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
            shopRect.sizeDelta = new Vector2(400f, 200f);
            var shopGroup = shopPanel.AddComponent<CanvasGroup>();

            var buyBtnGo = new GameObject("BuyButton");
            buyBtnGo.transform.SetParent(shopPanel.transform, false);
            var btnRect = buyBtnGo.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(240f, 80f);
            buyBtnGo.AddComponent<Image>().color = new Color(0.2f, 0.6f, 1f, 1f);
            buyBtnGo.AddComponent<Button>();
            buyBtnGo.AddComponent<ShopButton>();

            var btnTextGo = new GameObject("Text");
            btnTextGo.transform.SetParent(buyBtnGo.transform, false);
            var textRect = btnTextGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            var uiText = btnTextGo.AddComponent<Text>();
            uiText.text = "買う";
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.fontSize = 32;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.color = Color.white;

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
            SaveScene(s07, "Assets/BugZoo/Part1/Q07_ButtonBlocked/Q07_ButtonBlocked.unity", scenePaths);

            // Q08
            var s08 = NewBugScene("Q08", "弾が反応しない／速い弾が壁をすり抜ける",
                "1. Window > Analysis > Physics Debugger を開く
2. (a) バグ版では ZoneWalker が TriggerZone を通ってもログが出ないことを確認
3. (b) 速さ 500 で「10発撃つ」を押し、Scene ビューの赤い軌跡とすり抜けを確認");
            var tzGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tzGo.name = "TriggerZone";
            tzGo.transform.position = new Vector3(0f, 0.5f, 3f);
            tzGo.transform.localScale = Vector3.one * 2f;
            tzGo.GetComponent<BoxCollider>().isTrigger = true;
            tzGo.AddComponent<TriggerZone>();

            var walkerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            walkerGo.name = "ZoneWalker";
            walkerGo.transform.position = new Vector3(0f, 0.5f, 3f);
            walkerGo.AddComponent<ZoneWalker>();

            var tzScenGo = new GameObject("TriggerZoneScenario");
            var tzScen = tzScenGo.AddComponent<TriggerZoneScenario>();
            SetPrivateField(tzScen, "_zoneWalker", walkerGo);

            var wallGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallGo.name = "Wall";
            wallGo.transform.position = new Vector3(10f, 1.5f, -2f);
            wallGo.transform.localScale = new Vector3(0.1f, 3f, 3f);

            var gunGo = new GameObject("Gun");
            gunGo.transform.position = new Vector3(0f, 1.5f, -2f);
            var bulletScen = gunGo.AddComponent<BulletScenario>();
            SetPrivateField(bulletScen, "_muzzle", gunGo.transform);
            SetPrivateField(bulletScen, "_bulletPrefab", AssetDatabase.LoadAssetAtPath<Bullet>("Assets/BugZoo/Part1/Q08_PhysicsMiss/Bullet.prefab"));
            SaveScene(s08, "Assets/BugZoo/Part1/Q08_PhysicsMiss/Q08_PhysicsMiss.unity", scenePaths);

            // Q09
            EnsureFolder("Assets/BugZoo/Part1/Q09_RareStateBug");
            var s09 = NewBugScene("Q09", "たまにしか起きない（攻撃がたまに消える）",
                "1. ひるみ確率を 20% にして少し待つと、同じフレーム番号で Idle->Attack と Attack->Stun が並び、攻撃が上書き消滅することを確認
2. 「2回変更で止める: ON」で問題フレームで一時停止できることを確認
3. Test Runner で EnemyBrainTests を実行して確認");
            var rareEnemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rareEnemy.name = "Enemy";
            var smComp = rareEnemy.AddComponent<EnemyStateMachine>();
            var atkTimer = rareEnemy.AddComponent<AttackTimer>();
            var hitRec = rareEnemy.AddComponent<HitReceiver>();
            var rareScen = rareEnemy.AddComponent<RareStateScenario>();
            SetPrivateField(atkTimer, "_stateMachine", smComp);
            SetPrivateField(hitRec, "_stateMachine", smComp);
            SetPrivateField(rareScen, "_stateMachine", smComp);
            SetPrivateField(rareScen, "_attackTimer", atkTimer);
            SetPrivateField(rareScen, "_hitReceiver", hitRec);
            SaveScene(s09, "Assets/BugZoo/Part1/Q09_RareStateBug/Q09_RareStateBug.unity", scenePaths);

            // Q10
            EnsureFolder("Assets/BugZoo/Part1/Q10_DeviceOnly");
            var s10 = NewBugScene("Q10", "エディタでは動くのに実機だと動かない",
                "1. エディタではバグ版・修正版ともに動くことを確認
2. Android (IL2CPP, Development Build) で実機ビルドし、バグ版の File.ReadAllText と HiddenFeature の失敗を実機ログで確認");
            var devGo = new GameObject("DeviceScenarios");
            devGo.AddComponent<ConfigLoader>();
            devGo.AddComponent<PluginLoader>();
            SaveScene(s10, "Assets/BugZoo/Part1/Q10_DeviceOnly/Q10_DeviceOnly.unity", scenePaths);

            // Part 2: AnimationLab
            var sAnim = NewBugScene("Q12-Q23", "AnimationLab (アニメーション総合実験場)",
                "Window > Animation > Animator を開いて Target を選択しながら各ボタンを試してください。
設定を変えた後はメニュー「BugZoo > AnimationLab > Animator を初期状態に戻す」でいつでもリセットできます。");
            var animGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
            animGround.name = "Ground";

            var moverGo = new GameObject("Mover");
            var moverCtrl = moverGo.AddComponent<MoverController>();
            moverGo.AddComponent<AttackEventReceiver>(); // 初期状態は親(Mover)にだけ付いている（Q20のバグ状態）

            var targetGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetGo.name = "Target";
            targetGo.transform.SetParent(moverGo.transform, false);
            targetGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var targetAnimator = targetGo.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/BugZoo/Part2/Animations/Target.controller");
            targetAnimator.runtimeAnimatorController = controller;
            targetGo.AddComponent<ScaleOverrider>();
            var endDetector = targetGo.AddComponent<AttackEndDetector>();

            var pauseIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pauseIcon.name = "PauseIcon";
            pauseIcon.transform.position = new Vector3(2.5f, 1.5f, 0f);
            pauseIcon.transform.localScale = Vector3.one * 0.5f;
            var pauseAnimator = pauseIcon.AddComponent<Animator>();
            pauseAnimator.runtimeAnimatorController = controller;
            pauseAnimator.updateMode = AnimatorUpdateMode.Normal;

            var directorGo = new GameObject("Director");
            var director = directorGo.AddComponent<PlayableDirector>();
            var timeline = AssetDatabase.LoadAssetAtPath<TimelineAsset>("Assets/BugZoo/Part2/Animations/JumpTimeline.playable");
            director.playableAsset = timeline;
            director.playOnAwake = false;
            director.extrapolationMode = DirectorWrapMode.Hold;
            if (timeline != null)
            {
                foreach (var output in timeline.outputs)
                {
                    director.SetGenericBinding(output.sourceObject, targetAnimator);
                }
            }

            var animSpawnerGo = new GameObject("Spawner");
            var animSpawner = animSpawnerGo.AddComponent<AnimSpawner>();
            SetPrivateField(animSpawner, "_prefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BugZoo/Part2/Prefabs/AnimTarget.prefab"));

            var animPanelGo = new GameObject("AnimDebugPanel");
            var animPanel = animPanelGo.AddComponent<AnimDebugPanel>();
            SetPrivateField(animPanel, "_targetAnimator", targetAnimator);
            SetPrivateField(animPanel, "_mover", moverCtrl);
            SetPrivateField(animPanel, "_pauseIconAnimator", pauseAnimator);
            SetPrivateField(animPanel, "_director", director);
            SetPrivateField(animPanel, "_spawner", animSpawner);
            SetPrivateField(animPanel, "_endDetector", endDetector);
            SaveScene(sAnim, "Assets/BugZoo/Part2/AnimationLab.unity", scenePaths);

            // Q24
            EnsureFolder("Assets/BugZoo/Part3/Q24_Fps30");
            var s24 = NewBugScene("Q24", "実機で 30fps から上がらない",
                "1. モバイル実機でバグ版（targetFrameRate = -1）が 30fps 固定になることを確認
2. Profiler で WaitForTargetFPS と BugZoo.Busy（負荷 10/30ms）の見え方の違いを確認");
            var spinCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spinCube.name = "SpinCube";
            spinCube.AddComponent<SpinCube>();
            spinCube.AddComponent<FrameRateScenario>();
            SaveScene(s24, "Assets/BugZoo/Part3/Q24_Fps30/Q24_Fps30.unity", scenePaths);

            // Q25
            EnsureFolder("Assets/BugZoo/Part3/Q25_GcHitch");
            var s25 = NewBugScene("Q25", "数秒に一度、カクッと止まる (GC Alloc)",
                "1. 右上の FpsHud の GC Alloc/Frame と Profiler の GC Alloc 列を見る
2. 修正版に切り替えると毎フレームの GC Alloc が 0 B になることを確認");
            var gcGo = new GameObject("GarbageMaker");
            gcGo.AddComponent<GarbageMaker>();
            SaveScene(s25, "Assets/BugZoo/Part3/Q25_GcHitch/Q25_GcHitch.unity", scenePaths);

            // Q26
            var s26 = NewBugScene("Q26", "敵が増えるほど重くなる (Q28 実機計測兼用)",
                "1. 1000体・3000体にして Profiler の BehaviourUpdate と Instantiate スパイクを確認
2. 修正版（UpdateManager + ObjectPool）に切り替えて比較");
            var swarmGo = new GameObject("SwarmScenario");
            swarmGo.AddComponent<UpdateManager>();
            var swarmScen = swarmGo.AddComponent<SwarmScenario>();
            SetPrivateField(swarmScen, "_enemyPrefab", AssetDatabase.LoadAssetAtPath<SwarmEnemy>("Assets/BugZoo/Part3/Q26_ManyEnemies/SwarmEnemy.prefab"));
            SetPrivateField(swarmScen, "_projectilePrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BugZoo/Part3/Q26_ManyEnemies/Projectile.prefab"));
            SaveScene(s26, "Assets/BugZoo/Part3/Q26_ManyEnemies/Q26_ManyEnemies.unity", scenePaths);

            // Q27
            EnsureFolder("Assets/BugZoo/Part3/Q27_ClickHitch");
            var s27 = NewBugScene("Q27", "ボタンを押した瞬間だけ固まる (ProfilerMarker)",
                "1. Profiler を録画しながら「持ち物一覧を開く」を押す
2. ProfilerMarker の ON/OFF を切り替え、関数内部の内訳が見える違いを確認");
            var invGo = new GameObject("InventoryScenario");
            invGo.AddComponent<InventoryScenario>();
            SaveScene(s27, "Assets/BugZoo/Part3/Q27_ClickHitch/Q27_ClickHitch.unity", scenePaths);

            // Q29
            var s29 = NewBugScene("Q29", "遊んでいるとメモリが増えて落ちる",
                "1. 「ポップアップを10回開いて閉じる」を押し、破棄されない Texture2D とイベント購読数を確認
2. バグ版で「スコア変更イベントを発火」を押すと MissingReferenceException が出ることを確認
3. Memory Profiler で開閉前後のスナップショットを比較");
            var popGo = new GameObject("PopupScenario");
            var popScen = popGo.AddComponent<PopupScenario>();
            SetPrivateField(popScen, "_popupPrefab", AssetDatabase.LoadAssetAtPath<LeakyPopup>("Assets/BugZoo/Part3/Q29_MemoryLeak/LeakyPopup.prefab"));
            SaveScene(s29, "Assets/BugZoo/Part3/Q29_MemoryLeak/Q29_MemoryLeak.unity", scenePaths);

            // Q30
            var s30 = NewBugScene("Q30", "物が多い画面だけ重い (Frame Debugger)",
                "1. 「3000個出す」を押し、Window > Analysis > Frame Debugger を Enable にする
2. バグ版（3000個個別マテリアル）と修正版（8種共有マテリアル）で描画イベント数とフレーム時間を比較");
            var floodGo = new GameObject("FloodScenario");
            var floodScen = floodGo.AddComponent<FloodScenario>();
            SetPrivateField(floodScen, "_cubePrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BugZoo/Part3/Q30_DrawHeavy/FloodCube.prefab"));
            var sharedMats = new Material[8];
            for (var i = 0; i < 8; i++)
            {
                sharedMats[i] = AssetDatabase.LoadAssetAtPath<Material>($"Assets/BugZoo/Part3/Q30_DrawHeavy/Materials/SharedMat_{i}.mat");
            }
            SetPrivateObjectArray(floodScen, "_sharedMaterials", sharedMats);
            SaveScene(s30, "Assets/BugZoo/Part3/Q30_DrawHeavy/Q30_DrawHeavy.unity", scenePaths);

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
            SetOrder("Assets/BugZoo/Part1/Q03_NullOnStart/EnemySpawner.cs", -100);
            SetOrder("Assets/BugZoo/Part1/Q09_RareStateBug/AttackTimer.cs", -20);
            SetOrder("Assets/BugZoo/Part1/Q09_RareStateBug/HitReceiver.cs", -10);
            Debug.Log("[BugZoo] 4. スクリプト実行順（Q03 EnemySpawner, Q09 AttackTimer/HitReceiver）を設定しました。");
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
            EditorSceneManager.SaveScene(scene, path);
            list.Add(path);
        }

        private static void SetLoop(AnimationClip clip, bool loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        private static void AttachLogger(AnimatorState state, string label)
        {
            var logger = state.AddStateMachineBehaviour<AnimatorStateLogger>();
            SetPrivateString(logger, "_label", label);
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
