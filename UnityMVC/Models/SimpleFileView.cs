using System.Web;

namespace UnityMVC.Models
{
    public class SimpleFileView
    {
        public HttpPostedFileBase UploadedFile { get; set; }
    }
}