using System;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing.Windows.Compatibility;
using System.IO;

class QR
{
    static void generateQRCode(string text, string directoryPath)
    {
        if (text is null)
        {
            Console.WriteLine("Input text is null. Unable to generate QR code.");
            return;
        }

        // Combine the directory path and the file name to create the full file path
        string filePath = Path.Combine(directoryPath, text + ".png");

        // Create the QR code and save it to the specified file path
        BarcodeWriter barcodeWriter = new BarcodeWriter();
        barcodeWriter.Format = BarcodeFormat.QR_CODE;
        barcodeWriter.Renderer = new BitmapRenderer();
        Bitmap barcodeBitmap = barcodeWriter.Write(text);
        barcodeBitmap.Save(filePath, ImageFormat.Png);

        // Display the QR code
        Console.WriteLine("QR Code saved as: " + filePath);
        Console.WriteLine(text);
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
