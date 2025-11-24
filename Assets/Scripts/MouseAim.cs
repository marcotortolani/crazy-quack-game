using UnityEngine;

public class MouseAim : MonoBehaviour
{
    private Camera _mainCamera;
    public float targetZ = 0f; 
    void Start()
    {
        _mainCamera = Camera.main;
        Cursor.visible = false;
        UpdatePosition();
    }
    void Update()
    {
        // Actualizar la posición a la ubicación actual del mouse.
        UpdatePosition();
    }
    void UpdatePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = _mainCamera.transform.position.z - transform.position.z;
        
        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);

        // Forzar la posición Z de la mira al plano de juego.
        worldPosition.z = targetZ; 
        
        transform.position = worldPosition;
    }
}
