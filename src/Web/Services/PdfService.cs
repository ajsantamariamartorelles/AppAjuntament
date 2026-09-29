//using iText.Forms;
//using iText.Forms.Fields;
//using iText.Kernel.Pdf;
using AppAjuntament.Models.Voluntariat;

namespace AppAjuntament.Services
{
    public class PdfService
    {
        private readonly IWebHostEnvironment _environment;

        public PdfService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public byte[] GenerateFilledVolunteerForm(Voluntari voluntari)
        {
            // TODO: Implement PDF filling with iText
            throw new NotImplementedException("PDF generation not implemented");
        }
    }
}