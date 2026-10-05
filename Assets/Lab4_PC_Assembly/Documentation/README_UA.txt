ЛАБОРАТОРНА РОБОТА №4 — Природні взаємодії руками (Hand Tracking)
Тема проєкту: Інтерактивний тренажер конфігурації та складання системного блоку ПК.
Виконують: Волянський Максим, Лютий Євгеній, група ІК-33.

ВАЖЛИВО:
Цей проєкт продовжує стек Unity XRI, обраний у ЛР2. Для тестування без шолома використовується XR Interaction Simulator 3.6.x у режимі Hand. Він підтримує simulated hands і типові пози idle / poke / pinch / grab.

ПЕРЕД ЗАПУСКОМ:
1. Заверши ЛР3 і переконайся, що є:
   Assets/Lab3_Generated/Scenes/Lab3_PC_Assembly_SpatialUI.unity
2. Window > Package Manager > XR Interaction Toolkit > Samples:
   імпортуй Hands Interaction Demo.
3. Window > Package Manager > Unity Registry:
   встанови XR Hands.
4. У пакеті XR Hands > Samples імпортуй HandVisualizer.
5. Якщо Project Validation показує проблеми — натисни Fix / Fix All.
6. Імпортуй Lab4_PC_Assembly_HandTracking.unitypackage.
7. Запусти:
   Tools > VR-AR Labs > Lab 4 > Build Hand Tracking Scene

БУДЕ СТВОРЕНО:
Assets/Lab4_Generated/Scenes/Lab4_PC_Assembly_HandTracking.unity

У СЦЕНІ:
- XR Origin Hands (XR Rig) із Hands Interaction Demo;
- синтетичні руки / перемикання Input Modality;
- RAM із XR Grab Interactable для pinch/grab;
- монтажний паз RAM з автоматичною фіксацією та візуальним feedback;
- World-Space UI з Toggle, Slider і Poke Test;
- окрема 3D-кнопка для poke-взаємодії;
- статусні повідомлення та Console logs.

ТЕСТ БЕЗ ШОЛОМА:
1. Відкрий Lab4_PC_Assembly_HandTracking.unity.
2. Play.
3. В XR Interaction Simulator перемкни Input Method у Hand.
4. Користуйся підказками hotkeys, які показує сам Simulator для поточної версії.
5. Перевір Poke pose на UI / 3D button.
6. Перевір Pinch/Grab pose на RAM.
7. Перемісти RAM до червоного монтажного пазу. При правильному встановленні він стає зеленим і RAM фіксується.

РЕКОМЕНДОВАНІ СКРІНИ ДЛЯ ЗВІТУ:
1. Lab4 scene + Hierarchy "LAB 4 - HAND TRACKING".
2. XR Origin Hands (XR Rig) в Inspector, де видно Left Hand / Right Hand або Input Modality Manager.
3. XR Interaction Simulator у режимі Hand із синтетичними руками.
4. RAM_Hand_PinchGrab в Inspector: Rigidbody + XR Grab Interactable.
5. Poke UI / TrackedDeviceGraphicRaycaster та рука біля Toggle/Slider/Button.
6. Poke_3D_Button з XRSimpleInteractable / XRPokeFilter (якщо компонент доступний у версії XRI).
7. Процес pinch/grab: рука утримує RAM.
8. Успішне складання: RAM у правильному пазі, зелений feedback.
9. Console з [Lab4][HandAssembly] або [Lab4][Poke].
10. Git: гілка lab-4 + working tree clean.
