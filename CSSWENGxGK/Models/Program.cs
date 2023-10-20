using System;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing.Windows.Compatibility;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter the text to encode in the QR code: ");
        string text = Console.ReadLine();

        // Create a QR code writer instance
        BarcodeWriter barcodeWriter = new BarcodeWriter();
        barcodeWriter.Format = BarcodeFormat.QR_CODE;

        // Create a renderer (using a BitmapRenderer)
        barcodeWriter.Renderer = new BitmapRenderer();

        // Generate the QR code
        Bitmap barcodeBitmap = barcodeWriter.Write(text);

        // Specify the file path and save the QR code as a PNG image
        string filePath = Environment.CurrentDirectory + text + ".png";
        barcodeBitmap.Save(filePath, ImageFormat.Png);

        // Display the QR code
        Console.WriteLine("QR Code saved as: " + filePath + " in " );
        Console.WriteLine(text);
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
