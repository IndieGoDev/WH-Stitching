using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebReq : MonoBehaviour {

    public static string[] text = { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null};
    public static string[] mode = { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null};
    private static string[] modePrivate = { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null};
    private static string data = null;
    private static int count = 0;
    private bool isOnline = true;

    public static void IO(string dataReq, string modeReq)
    {
        Main.Loading(true);
        data = dataReq;
        for (int i = 0; i < mode.Length; i++)
        {
            if (i == mode.Length)
            {
                count = i + 1;
                modePrivate[count] = modeReq;
                mode[count] = null;
                text[count] = null;
                i = mode.Length;
            }

            if (mode[i] == null)
            {
                modePrivate[i] = modeReq;
                count = i;
                i = mode.Length;
            }
        }
    }

    void LateUpdate()
    {
        if (data != null)
        {
            StartCoroutine(ServerIO(modePrivate[count], data));
            data = null;
        }
    }

    private IEnumerator ServerIO(string mode2, string data2)
    {
        var form = new WWWForm();
        var url = isOnline ? "http://bahagia-computer.esy.es/io.php" : "http://192.168.137.1/wh-stitching/io.php";
        form.AddField("mode", mode2);
        form.AddField("data", data2);
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        www.timeout = 10;
        yield return www.Send();

        if (www.isDone && www.responseCode == 200)
        {
            mode[count] = modePrivate[count];
            text[count] = www.downloadHandler.text;
            modePrivate[count] = null;
            Main.Loading(false);
        }
        else
        {
            isOnline = false;
            if (modePrivate[count] == "LoginQR") QRScan.camTexture.Play();
            mode[count] = modePrivate[count];
            text[count] = "Gagal|";
            Main.log.text = "Error : [" + www.responseCode + "] " + www.error;
            modePrivate[count] = null;
            Main.Loading(false);
        }
    }
}
