using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer; 
    public string nombreEscenaMenu = "Menu"; 

    void Start()
    {
        // Suscribirse al evento que avisa cuando el video termina
        videoPlayer.loopPointReached += EndReached;
    }

    void Update()
    {
        // Opción para saltar la intro con cualquier tecla (Space, Enter, Click)
        if (Input.anyKeyDown)
        {
            CargarMenu();
        }
    }

    // Se llama automáticamente cuando el video termina
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        CargarMenu();
    }

    void CargarMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}