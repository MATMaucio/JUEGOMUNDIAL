using UnityEngine;

/// <summary>
/// CubeItem
/// --------
/// Componente que identifica un cubo dentro del juego
/// mediante un ID único en formato string.
///
/// Este script NO contiene lógica.
/// Solo actúa como contenedor de datos (Data Holder).
///
/// Uso típico:
/// - Asociar un identificador único a cada cubo.
/// - Permitir sincronización por Bluetooth.
/// - Diferenciar objetos al enviarlos por red.
/// </summary>
public class CubeItem : MonoBehaviour
{
    /// <summary>
    /// Identificador único del cubo.
    /// 
    /// Puede usarse para:
    /// - Sincronización entre dispositivos
    /// - Búsqueda del objeto correcto al recibir datos
    /// - Lógica de red (ej: mover cubo específico)
    /// </summary>
    public string cubeID;
}