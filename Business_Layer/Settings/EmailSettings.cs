using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.Settings
{
    public class EmailSettings
    {
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string AppPassword { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
    }
}
