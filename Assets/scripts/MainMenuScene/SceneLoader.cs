using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void Load(SceneList scene)
    {
        SceneManager.LoadScene((int)scene);
    }
}
