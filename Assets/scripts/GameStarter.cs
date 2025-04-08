using UnityEngine;

public class GameStarter : MonoBehaviour
{
    void Start()
    {
        // Oyun baþladýðýnda fareyi kilitle
        LockCursor();
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Input sistemini resetle (kritik!)
        UnityEngine.Input.ResetInputAxes();
    }
}