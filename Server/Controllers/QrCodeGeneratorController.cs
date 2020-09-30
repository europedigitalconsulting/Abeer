using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Threading.Tasks;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCodeGeneratorController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<byte[]>> Generate(string link)
        {
            return await Task.Run(() =>
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);
                return File(qrCodeAsBitmapByteArr, "image/png");
            });
        }
    }
}
