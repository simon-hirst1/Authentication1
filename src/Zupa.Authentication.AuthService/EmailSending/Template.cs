using System;
using System.Collections.Generic;

namespace Zupa.Authentication.AuthService.EmailSending
{
    [Serializable]
    public class Template
    {
        public string Id { get; set; }
        public Dictionary<string, string> Substitutions { get; set; }
        public bool IsDynamic { get; set; }
    }
}
