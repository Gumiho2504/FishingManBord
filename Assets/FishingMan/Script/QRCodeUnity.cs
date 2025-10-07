using UnityEngine;
using QRCoder;
using UnityEngine.UI;
public class QRCodeUnity : MonoBehaviour
{

    public string textToEncode = "Hello Unity!";

    void Start()
    {




        // string text = "Hello Unity!";
        // QRCodeGenerator qrGenerator = new QRCodeGenerator();
        // QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        // // Generate raw bitmap (no System.Drawing)
        // PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        // byte[] qrCodeBytes = qrCode.GetGraphic(20);

        // // Convert to Unity texture
        // Texture2D tex = new Texture2D(256, 256);
        // tex.LoadImage(qrCodeBytes);
        // qrRenderer.material.mainTexture = tex;
    }

    public static Texture2D GenerateQR(string text, int width, int height)
    {
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);

        Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
        tex.LoadImage(qrCodeBytes);
        return tex;
    }
}
