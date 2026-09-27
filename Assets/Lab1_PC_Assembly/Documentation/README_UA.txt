ЛАБОРАТОРНА РОБОТА №1 — ГОТОВИЙ СТАРТЕР
Тема проєкту: «Інтерактивний тренажер конфігурації та складання системного блоку ПК».

ЩО Є В ПАКЕТІ
1) CameraKeyboardController.cs — керування тестовою камерою через стандартний Unity Input.
2) PhysicsEventReporter.cs — демонстрація OnCollisionEnter та OnTriggerEnter.
3) TriggerZoneReporter.cs — тригер-зона з повідомленнями в Console.
4) Lab1SceneBuilder.cs — автоматично створює кімнату/майстерню, робочий стіл, ПК-обладнання,
   власний Prefab RAM_Module, Rigidbody/Collider/Trigger і тестову камеру.

ЯК ЗАПУСТИТИ
1. Створи новий Unity 3D URP-проєкт.
2. Імпортуй файл Lab1_PC_Assembly.unitypackage через Assets > Import Package > Custom Package.
3. Дочекайся завершення компіляції скриптів.
4. У верхньому меню Unity натисни:
   Tools > VR-AR Labs > Lab 1 > Build PC Assembly Scene
5. Відкриється/створиться сцена:
   Assets/Lab1_Generated/Scenes/Lab1_PC_Assembly.unity
6. Натисни Play.
7. Помаранчевий куб падає крізь Trigger Zone, після чого стикається з WorkbenchTop.
   У Console з'являються рядки [Lab1][Trigger] / [Lab1][TriggerZone] / [Lab1][Collision].
8. Керування камерою: WASD — рух, Q/E — вниз/вгору, стрілки — огляд,
   або затисни праву кнопку миші та рухай мишкою; Left Shift — швидше.

ВАЖЛИВО ПРО INPUT
Лабораторна прямо вимагає стандартну систему Input.
Якщо у твоєму проєкті ввімкнено тільки New Input System, відкрий:
Edit > Project Settings > Player > Other Settings > Active Input Handling
і вибери Both або Input Manager (Old), після чого перезапусти Unity.

ЩО СТВОРИТЬСЯ
Assets/Lab1_Generated/Scenes/Lab1_PC_Assembly.unity
Assets/Lab1_Generated/Prefabs/RAM_Module.prefab
Assets/Lab1_Generated/Materials/*.mat

Це навмисно зроблено як база для наступних лабораторних:
Lab 2 — брати RAM/SSD/GPU у VR та ставити в Snap Zone;
Lab 3 — World-Space UI з покроковою інструкцією складання;
Lab 4 — Hand Tracking для захоплення деталей;
Lab 5 — REST API/телеметрія проходження;
Lab 6 — Photon або Mixed Reality.
