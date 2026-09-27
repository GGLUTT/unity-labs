ЛАБОРАТОРНА РОБОТА №3 — Просторові інтерфейси (Spatial UI) та покрокові форми
Тема проєкту: Інтерактивний тренажер конфігурації та складання системного блоку ПК.

ПЕРЕД ЗАПУСКОМ:
1. ЛР2 повинна бути завершена.
2. Повинна існувати сцена:
   Assets/Lab2_Generated/Scenes/Lab2_PC_Assembly_VR.unity
3. XR Interaction Toolkit має бути встановлений (у ЛР2 він уже встановлений).
4. Імпортуй цей unitypackage.
5. Дочекайся завершення компіляції.
6. Запусти:
   Tools > VR-AR Labs > Lab 3 > Build Spatial UI & Wizard Scene

БУДЕ СТВОРЕНО:
Assets/Lab3_Generated/Scenes/Lab3_PC_Assembly_SpatialUI.unity

У СЦЕНІ:
- World-Space Canvas у вигляді віртуального планшета;
- XR UI EventSystem + TrackedDeviceGraphicRaycaster;
- Ray interaction через XR Origin з ЛР2;
- Direct Poke helper interactors для лівого/правого контролера;
- інформаційні картки для RAM, материнської плати та монітора;
- Toggle техніки безпеки;
- Slider;
- InputField ідентифікатора оператора;
- кнопки Save / Reset;
- Wizard / State Machine із 4 кроків;
- перевірка відповідей, підрахунок помилок, часу та фінальний результат.

ТЕСТ БЕЗ ШОЛОМА:
- Відкрий Lab3_PC_Assembly_SpatialUI.unity.
- Натисни Play.
- XR Interaction Simulator з ЛР2 залишається у сцені.
- Клавіша M відкриває/закриває планшет.
- Через XR Ray Interactor наведи промінь на кнопки UI та натискай Select.
- Для Direct Poke підведи контролер безпосередньо до UI.
- Заповни Wizard:
  1) підтвердь правила безпеки;
  2) введи ID, зміни slider і натисни «Зберегти дані»;
  3) обери правильну відповідь «DIMM»;
  4) заверши — буде показано час, помилки, ID і значення slider.

РЕКОМЕНДОВАНІ СКРІНИ ДЛЯ ЗВІТУ:
1. Загальний вигляд Lab3 сцени + Hierarchy LAB 3 - SPATIAL UI & WIZARD.
2. Spatial_Tablet_Canvas: Canvas = World Space + TrackedDeviceGraphicRaycaster.
3. EventSystem_XR_UI: XRUIInputModule.
4. XR Origin / Ray Interactor у Hierarchy.
5. Left/Right Poke Interactor helper з XRPokeInteractor в Inspector.
6. Wizard крок 1 з Toggle.
7. Wizard крок 2: Slider + InputField + Save/Reset.
8. Wizard крок 3: питання та кнопки відповідей.
9. Wizard крок 4: фінальний результат.
10. Інформаційна картка для RAM або Motherboard.
11. Git: branch lab-3 + working tree clean.
