using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;

namespace EventTicketSystem.Controllers
{
    public class QRCodeController : Controller
    {
        public ActionResult GenerateQRCode(string data)
        {
            if (string.IsNullOrEmpty(data))
                return null;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            using (var stream = new MemoryStream())
            {
                qrCodeImage.Save(stream, ImageFormat.Png);
                byte[] imageBytes = stream.ToArray();
                return File(imageBytes, "image/png");
            }
        }

        public ActionResult GenerateTicketQR(int bookingDetailId)
        {
            var data = $"TICKET:{bookingDetailId}:{System.Guid.NewGuid()}";
            return GenerateQRCode(data);
        }
    }
}