using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CaminhosDeAngola.Boss
{
    /// <summary>
    /// Fim do nivel final: quando O Grande Esquecimento e derrotado, aparece o
    /// painel "O Pensador acordou" (epilogo do GDD) e, com Enter, volta ao menu.
    /// </summary>
    public class FimBoss : MonoBehaviour
    {
        [SerializeField] private GrandeEsquecimento boss;
        [SerializeField] private GameObject painel;
        [SerializeField, Min(0f)] private float atraso = 1.5f;
        [SerializeField] private string cenaMenu = "MenuPrincipal";

        private bool aMostrar;

        private void Awake()
        {
            if (boss == null) boss = FindFirstObjectByType<GrandeEsquecimento>();
            if (painel != null) painel.SetActive(false);
        }

        private void OnEnable()
        {
            if (boss != null) boss.AoSerDerrotado += AoDerrotar;
        }

        private void OnDisable()
        {
            if (boss != null) boss.AoSerDerrotado -= AoDerrotar;
        }

        private void AoDerrotar()
        {
            StartCoroutine(Mostrar());
        }

        private IEnumerator Mostrar()
        {
            yield return new WaitForSeconds(atraso);
            if (painel != null) painel.SetActive(true);
            aMostrar = true;
        }

        private void Update()
        {
            if (!aMostrar || Keyboard.current == null) return;

            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(cenaMenu);
            }
        }
    }
}
