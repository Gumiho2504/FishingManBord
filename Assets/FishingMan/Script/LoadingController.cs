using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadingController : MonoBehaviour
{
    public Image loadingImage;
    float fillAmount = 0f;
    public GameObject loadingPanel;
    public GameObject homeConvas;
    public Button startButton, exitButton;
    IEnumerator Start()
    {
        while(fillAmount < 100)
        {
            int speed = Random.Range(40, 100);
            fillAmount += speed * Time.deltaTime;
            loadingImage.fillAmount = fillAmount / 100;
            yield return null;
        }
        loadingPanel.SetActive(false);

        startButton.onClick.AddListener(() => {
            homeConvas.SetActive(false);
        });
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    
    
}
