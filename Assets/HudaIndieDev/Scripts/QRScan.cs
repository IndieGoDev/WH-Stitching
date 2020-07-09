using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class QRScan : MonoBehaviour {

	public static WebCamTexture camTexture;
    private GameObject uiCanvas;
    private RawImage scanImage;
    public static GameObject panelScan;
    void Start () {

        uiCanvas = GameObject.Find("UI Canvas");
        panelScan = uiCanvas.transform.Find("Panel Scan").gameObject;
        scanImage = panelScan.transform.Find("QRMask/QRCam").gameObject.GetComponent<RawImage>();

        camTexture = new WebCamTexture();
        camTexture.requestedWidth = 176;
        camTexture.requestedHeight = 144;
        camTexture.requestedFPS = 20;
        scanImage.texture = camTexture;
    }

    void OnGUI()
	{
		try
		{
            if (camTexture.isPlaying)
            {
                IBarcodeReader barcodeReader = new BarcodeReader();
                var result = barcodeReader.Decode(camTexture.GetPixels32(), camTexture.requestedWidth, camTexture.requestedHeight);
                if (result != null)
                {
                    camTexture.Stop();
                    Main.scanValue.text = "Last Code : " + result.Text;
                    Main.panelScan.SetActive(false);
                    Main.log.text = "Proses Validasi QR Code";
                    WebReq.IO(result.Text, "LoginQR");
                }
            }
        }
		catch (Exception e) {
            Main.log.text = "Warning : " + e.Message;
        }
	}
}
