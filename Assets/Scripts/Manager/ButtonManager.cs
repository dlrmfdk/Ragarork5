using S3MG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void onNextClicked()
    {
        SceneManager.LoadScene("MapScene");
        MapGenerator.instance.activeMap();
        MapGenerator.instance.toNextNode();
        
    }
}
