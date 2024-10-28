using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSettingManager : MonoBehaviour
{
    //UI Text
    public Text resolutionText;
    public Text graphicsQualityText;
    public Text textfullScreanText;

    public CanvasScaler canvasScaler;

    private int resolutionIndex = 0;
    private int graphicsQualityIndex = 0;
    private bool isFullScrean = true;

    private string[] resolutions = { "800x600", "1280x720", "1920x1080" };
    private string[] graphicsQualityOptions = { "Low", "Normal", "high" };

    public GameObject option;

    private void Start()
    {
        LoadSettings();
        UpdateFullScreanText();
        UpdateGraphicsQualityText();
        UpdateResolutionText();
    }
    public void OnResolutionLeftClick()
    {
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpdateResolutionText();
    }
    public void OnResolutionRightClick()
    {
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpdateResolutionText();
    }
    private void UpdateResolutionText()
    {
        resolutionText.text = resolutions[resolutionIndex];
    }
    public void OnGraphicsLeftClick()
    {
        graphicsQualityIndex = Mathf.Max(0, graphicsQualityIndex - 1);
        UpdateGraphicsQualityText();
    }
    public void OnGraphicsRightClick()
    {
        graphicsQualityIndex = Mathf.Min(graphicsQualityOptions.Length - 1, graphicsQualityIndex + 1);
        UpdateGraphicsQualityText();
    }
    private void UpdateGraphicsQualityText()
    {
        graphicsQualityText.text = graphicsQualityOptions[graphicsQualityIndex];
    }
    public void OnFullScreanToggleClick()
    {
        isFullScrean = !isFullScrean;
        UpdateFullScreanText();
    }
    public void UpdateFullScreanText()
    {
        textfullScreanText.text = "전체 화면 : " + (isFullScrean ? "켜짐" : "꺼짐");
    }
    public void ApplySettings()
    {
        string[] res = resolutions[resolutionIndex].Split('x');
        int width = int.Parse(res[0]);
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScrean);

        QualitySettings.SetQualityLevel(graphicsQualityIndex);

        //정말 변경하시겠습니까? 문구 출력 필요
        SaveSettings();
        OptionExitButton();
    }
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex", graphicsQualityIndex);
        PlayerPrefs.SetInt("FullScrean", isFullScrean ? 1 : 0);
    }
    private void LoadSettings()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        graphicsQualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);
        isFullScrean = PlayerPrefs.GetInt("FullScrean", 1) == 1;
    }
    public void OptionExitButton()
    {
        option.SetActive(false);
    }
    public void OnOptionSetting()
    {
        option.SetActive(true);
    }
    public void OnGameStart()
    {
        SceneManager.LoadScene("Level2");
    }
    public void OnExitGame()
    {
        
    }
    public void OnMainMenu()
    {

    }
    public void OnPesue()
    {

    }
}
