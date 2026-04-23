using System;
using System.Collections.Generic;
using System.Text;

namespace Data_Layer.Configuration
{
    public class FirestoreOptions
    {
        public const string SectionName = "Firestore";

        public string ProjectId { get; set; } = string.Empty;
        public string CredentialsPath { get; set; } = string.Empty;
    }
}
