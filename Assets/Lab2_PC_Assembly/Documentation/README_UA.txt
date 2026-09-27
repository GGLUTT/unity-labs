ЛАБОРАТОРНА РОБОТА №2 — VR-сцена: пересування та маніпуляція об'єктами
Тема проєкту: Інтерактивний тренажер конфігурації та складання системного блоку ПК.

ПЕРЕД ЗАПУСКОМ BUILD:
1. Переконайся, що ЛР1 вже згенерована і існує сцена:
   Assets/Lab1_Generated/Scenes/Lab1_PC_Assembly.unity
2. Window > Package Manager.
3. Встанови XR Interaction Toolkit.
4. У XR Interaction Toolkit відкрий Samples та імпортуй:
   - Starter Assets
   - XR Interaction Simulator (якщо у твоїй версії є тільки XR Device Simulator / Legacy — імпортуй його)
5. Дочекайся завершення імпорту/компіляції.
6. Tools > VR-AR Labs > Lab 2 > Build VR Interaction Scene.

БУДЕ СТВОРЕНО:
Assets/Lab2_Generated/Scenes/Lab2_PC_Assembly_VR.unity

У СЦЕНІ:
- XR Origin (XR Rig)
- XR Interaction Simulator для тестування без шолома
- RAM з XR Grab Interactable + Rigidbody
- RAM Socket Interactor / Snap Zone
- Teleportation Area на підлозі
- візуальні точки телепортації

ТЕСТ:
- Відкрий Lab2_PC_Assembly_VR.unity і натисни Play.
- XR Interaction Simulator показує доступні клавіші у своєму UI.
- У сучасному XR Interaction Simulator Tab перемикає FPS/device mode; [ та ] вмикають лівий/правий контролер.
- Підведи контролер до RAM, захопи її та перенеси в зелену Snap Zone.
- У Console при вході RAM в зону з'явиться:
  [Lab2][SnapZone] RAM entered the XR socket zone: RAM_Module_XR_Grab

РЕКОМЕНДОВАНІ СКРІНИ ДЛЯ ЗВІТУ:
1. Package Manager: XR Interaction Toolkit встановлено.
2. Samples: Starter Assets + XR Device Simulator імпортовано.
3. Загальний вигляд Lab2 сцени + Hierarchy LAB 2 - VR INTERACTION (XRI).
4. XR Origin (XR Rig) в Inspector / Hierarchy.
5. RAM_Module_XR_Grab: Rigidbody + XR Grab Interactable.
6. RAM_Socket_Interactor: Box Collider Is Trigger + XR Socket Interactor.
7. Floor: Teleportation Area.
8. Play Mode: XR Interaction Simulator UI + VR scene.
9. RAM перенесена в зелену Snap Zone + Console повідомлення [Lab2][SnapZone].
10. Git: branch lab-2, working tree clean після commit/push.
