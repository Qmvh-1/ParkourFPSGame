using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Reset : MonoBehaviour
{
    bool resetInput;

    void Update()
    {
        if (resetInput)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            resetInput = false;
        }
    }

    public void ResetInput(InputAction.CallbackContext context)
    {
        resetInput = context.performed;
    }
}
