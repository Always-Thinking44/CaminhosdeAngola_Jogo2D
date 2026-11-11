using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Checkpoint associado a um monumento. Activa automaticamente quando o
    /// jogador entra na zona, sem precisar de tecla (o E fica reservado para
    /// a zungueira). O ponto de respawn deve ficar num filho "PontoRespawn",
    /// em frente ao monumento.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointMonumento : CheckpointBase
    {
        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (Activo || !outro.CompareTag("Player")) return;

            Activar();
        }
    }
}
