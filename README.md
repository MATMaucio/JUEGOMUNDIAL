# 🏆 Amerike World Cup Experience 2026 - Repositorio Oficial

¡Bienvenidos al proyecto juego del Mundial 2026!  Este documento contiene las reglas técnicas y de flujo de trabajo que **TODO EL EQUIPO** debe seguir para asegurar que el desarrollo.. 

## 📂 1. Estructura del Proyecto Unity
Para mantener el orden, todos los assets propios del juego deben ir dentro de una estructura limpia. 
**Patrones de Diseño:** Usaremos fuertemente **ScriptableObjects** para los datos (cartas, lugares, preguntas) y **Eventos** para comunicar sistemas.
* **No subir basura:** El proyecto ya cuenta con un `.gitignore` específico de Unity.**Por ningún motivo** intenten forzar la subida de las carpetas `Library` o `Temp`.

## 🌿 2. Flujo de Trabajo en Git (Git Flow)
Utilizaremos un flujo de tres niveles de ramas. Está estrictamente prohibido trabajar directo en `main` o `develop`.

* **`main` (Estable):** Contiene código 100% funcional y probado.Solo el Tech Lead hace merge aquí.
* **`develop` (Integración):** Es el corazón del proyecto.Aquí unimos el trabajo de todos los módulos.
* **`feature/nombre-de-tu-tarea`:** Aquí es donde tú programas. Si estás haciendo el mapa, tu rama debe llamarse `feature/mapa-3d` o similar.

## 🛑 3.Pull Requests
**Nadie hace merge a `develop` sin un Pull Request (PR) y sin revisión.**
1. Cuando termines tu tarea en tu rama `feature/`, sube los cambios (Push).
2. Entra a GitHub y abre un **Pull Request** apuntando hacia `develop`.
3. El Tech Lead o el responsable del módulo debe revisar tu código y aprobarlo. Si hay conflictos, deberás resolverlos localmente antes de la revisión.

## 💾 4. Reglas de Programación y Datos
* **Guardado Local:** Los datos importantes del jugador (colección, progreso) se guardarán usando **JSON encriptado**.
* **PlayerPrefs:** Queda restringido **sólo para cosas mínimas** (como configuraciones de volumen o idioma). No guarden cantidad de sobres o cartas aquí porque es fácilmente hackeable.

## 🎨 5. Arte y UI
* **Design System:** Todos los desarrolladores deben respetar el design system definido por el UI Owner (tipografías, paleta, botones).
* **UI/Shader Days:** Tendremos días específicos dedicados a pulir efectos visuales y la consistencia de la UI, donde todos ayudan bajo la revisión del UI Owner.

## 📋 6. Organización de Tareas
Utilizaremos **Issues y Projects (Kanban)** para organizar las tareas. Antes de empezar a programar algo, asegúrate de que esté asignado a ti en el tablero para no duplicar trabajo.

---
**¿Dudas técnicas?** Contacta al Tech Lead antes de hacer un commit.
