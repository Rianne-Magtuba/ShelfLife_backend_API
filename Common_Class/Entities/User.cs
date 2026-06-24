using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Class.Entities
{
    [FirestoreData]
    public class User
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Username { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string PasswordHash { get; set; }

        [FirestoreProperty]
        public bool NotificationEnabled { get; set; } = true;
        [FirestoreProperty]
        public string NotificationFrequency { get; set; } = "daily";
        [FirestoreProperty]
        public int NotificationLeadDays { get; set; } = 3;
        [FirestoreProperty]
        public int NotificationReminderHour { get; set; } = 8;
        [FirestoreProperty]
        public int NotificationReminderMinute { get; set; } = 0;
    }
}
