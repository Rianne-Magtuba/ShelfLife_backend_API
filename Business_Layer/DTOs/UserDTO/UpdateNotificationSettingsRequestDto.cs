using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business_Layer.DTOs.UserDTO
{
    public class UpdateNotificationSettingsRequestDto
    {
        public bool Enabled { get; set; }
        public string Frequency { get; set; }
        public int AlertLeadDays { get; set; }
        public int ReminderHour { get; set; }
        public int ReminderMinute { get; set; }
    }
}
