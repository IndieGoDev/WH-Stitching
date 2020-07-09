using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {

    private GameObject earthPivot;
    private GameObject earth;
    private GameObject mainCamera;
    private GameObject uiCanvas;
    public static GameObject panelLogin;
    public static GameObject panelScan;
    private static GameObject panelLoad;
    private GameObject panelPass;
    private static GameObject panelHome;
    private static GameObject panelChat;
    private static GameObject panelSet;
    private GameObject panelExit;
    public static Button btnQR;
    public static Text scanValue;
    public static Button btnScanBack;
    private Button btnPassOK;
    private Button btnChat;
    private Button btnSet;
    private Toggle tglGyro;
    private Dropdown dropSky;
    private Button btnLogOut;
    private Button btnExit;
    private InputField inputPass;
    private Image loadSpin;
    private static Settings settings;
    private static string setFilePath;

    public static Text log;
    public Material[] skyBox;
    private string[] keys = Keys.names;

    void Start () {
        setFilePath = Application.persistentDataPath + "/Settings.json";
        settings = new Settings();
        LoadSettings();
        Init();
        EventListener();
    }
    void Init ()
    {
        // Find GameObject;
        mainCamera = GameObject.Find("Main Camera");
        uiCanvas = GameObject.Find("UI Canvas");
        earthPivot = GameObject.Find("EarthPivot");
        earth = earthPivot.transform.Find("Earth").gameObject;
        panelLogin = uiCanvas.transform.Find("Panel Login").gameObject;
        panelScan = uiCanvas.transform.Find("Panel Scan").gameObject;
        panelPass = uiCanvas.transform.Find("Panel Pass").gameObject;
        panelHome = uiCanvas.transform.Find("Panel Home").gameObject;
        panelChat = uiCanvas.transform.Find("Panel Chat").gameObject;
        panelSet = uiCanvas.transform.Find("Panel Setting").gameObject;
        panelLoad = uiCanvas.transform.Find("Panel Loading").gameObject;
        panelExit = uiCanvas.transform.Find("Panel Exit").gameObject;

        // Find Component;
        log = uiCanvas.transform.Find("Panel Log/Log").gameObject.GetComponent<Text>();
        btnQR = panelLogin.transform.Find("BtnQR").gameObject.GetComponent<Button>();
        scanValue = panelScan.transform.Find("Value").gameObject.GetComponent<Text>();
        btnScanBack = panelScan.transform.Find("BtnBack").gameObject.GetComponent<Button>();
        inputPass = panelPass.transform.Find("InputPass").gameObject.GetComponent<InputField>();
        btnPassOK = panelPass.transform.Find("BtnOk").gameObject.GetComponent<Button>();
        btnPassOK = panelPass.transform.Find("BtnOk").gameObject.GetComponent<Button>();
        btnChat = panelHome.transform.Find("BtnChat").gameObject.GetComponent<Button>();
        btnSet = panelHome.transform.Find("BtnSet").gameObject.GetComponent<Button>();
        tglGyro = panelSet.transform.Find("TglGyro").gameObject.GetComponent<Toggle>();
        dropSky = panelSet.transform.Find("DropSky").gameObject.GetComponent<Dropdown>();
        btnLogOut = panelSet.transform.Find("BtnLogOut").gameObject.GetComponent<Button>();
        btnExit = panelExit.transform.Find("BtnExit").gameObject.GetComponent<Button>();
        loadSpin = panelLoad.transform.Find("Image").gameObject.GetComponent<Image>();

        log.text = "Welcome to WH Stitching Support Application";

        // Load Settings
        RenderSettings.skybox = skyBox[settings.skyBox];
        dropSky.value = settings.skyBox;
        if (settings.sensorGyro)
        {
            Input.gyro.enabled = true;
            tglGyro.isOn = true;
        }

        // -- Cek Login Status
        if (settings.loginData[0] == "")
        {
            panelLogin.SetActive(true);
        }
        else
        {
            WebReq.IO(settings.loginData[3], settings.loginData[0]);
        }
            

    }
    // Save Function
    public static void SaveSettings()
    {
        string jsonData = JsonUtility.ToJson(settings, true);
        File.WriteAllText(setFilePath, jsonData);
    }
    // Load Function
    public static void LoadSettings()
    {
        if (File.Exists(setFilePath))
        {
            var data = File.ReadAllText(setFilePath);
            settings = JsonUtility.FromJson<Settings>(data);
        }
    }
    void EventListener()
    {
        tglGyro.onValueChanged.AddListener(delegate {
            Input.gyro.enabled = settings.sensorGyro = tglGyro.isOn;
            // Save Settings
            SaveSettings();
        });
        dropSky.onValueChanged.AddListener(delegate {
            settings.skyBox = dropSky.value;
            RenderSettings.skybox = skyBox[settings.skyBox];
            // Save Settings
            SaveSettings();
        });
        btnQR.onClick.AddListener(delegate {
            if (!QRScan.camTexture.isPlaying) QRScan.camTexture.Play();
        });
        btnScanBack.onClick.AddListener(delegate {
            if (QRScan.camTexture.isPlaying) QRScan.camTexture.Stop();
        });
        btnLogOut.onClick.AddListener(delegate {
            // Save Settings
            for (var i = 0; i < settings.loginData.Length; i++) settings.loginData[i] = null;
            SaveSettings();
            log.text = "Logged Out";
        });
        btnExit.onClick.AddListener(delegate { AppQuit(); });
        btnPassOK.onClick.AddListener(delegate {
            panelPass.SetActive(false);
            log.text = "Proses Validasi Password";
            WebReq.IO(inputPass.text, "LoginPass");
        });
    }

    public static void Loading(bool state)
    {
        panelLoad.SetActive(state);
    }
    void Update ()
    {
        if (panelLoad.activeSelf) loadSpin.transform.Rotate(Vector3.back * 100 * Time.deltaTime);
		if(settings.sensorGyro) CamGyro ();
        foreach (string key in keys)
        {
            if (Input.GetKeyDown(key))
            {
                switch (key)
                {
                    case "escape":
                        panelExit.SetActive(true);
                        break;
                    default:break;
                }
            }
        }
    }

    private void LateUpdate()
    {
        for (var i = 0; i < WebReq.mode.Length; i++)
        {
            switch (WebReq.mode[i])
            {
                case "LoginQR":
                case "LoginPass":
                    CekLogin(WebReq.text[i], WebReq.mode[i]);
                    WebReq.mode[i] = WebReq.text[i] = null;
                    break;
                default:break;
            }
        }
    }
    void CamGyro()
    {
		mainCamera.transform.Rotate(-Input.gyro.rotationRateUnbiased.x, -Input.gyro.rotationRateUnbiased.y, Input.gyro.rotationRateUnbiased.z);
        earth.transform.Rotate(new Vector3(0.5f, 1, 0) * 3 * Time.deltaTime);
        earthPivot.transform.Rotate(Vector3.down * 0.6f * Time.deltaTime);
    }
    void CekLogin(string data, string mode)
    {
        string[] loginData = data.Split(new string[] { "|" }, StringSplitOptions.None);
        if (loginData[0] == "Gagal")
        {
            if (loginData[1] != "")
            {
                // Save Settings
                for (var i = 0; i < settings.loginData.Length; i++) settings.loginData[i] = null;
                SaveSettings();

                log.text = "Login Gagal : " + loginData[1];
                if(loginData[1] == "QR Code Salah")
                {
                    panelScan.SetActive(true);
                    QRScan.camTexture.Play();
                } else if (loginData[1] == "Password Salah")
                {
                    panelPass.SetActive(true);
                }
            }
            else
            {
                switch (mode)
                {
                    case "LoginQR":
                        panelScan.SetActive(true);
                        break;
                    case "LoginPass":
                        panelPass.SetActive(true);
                        break;
                    default:
                        break;
                }
            }
        }
        else if (loginData[0] == "LoginQR")
        {
            log.text = "Logged with QR Code : " + loginData[1];
            // Save Settings
            for (var i = 0; i < loginData.Length; i++) settings.loginData[i] = loginData[i];
            SaveSettings();
            panelHome.SetActive(true);
        }
        else if (loginData[0] == "LoginPass")
        {
            log.text = "Logged with Password : " + loginData[1];
            // Save Settings
            for (var i = 0; i < loginData.Length; i++) settings.loginData[i] = loginData[i];
            SaveSettings();
            panelHome.SetActive(true);
        }
        else
        {
            log.text = "Error : " + loginData[0];
        }
    }
    void AppQuit()
    {
        Application.Quit();
    }
}
