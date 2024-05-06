using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPB.PatientEntities.Managers.Exceptions
{
    public class PatientExceptions : Exception
    {
        public PatientExceptions() { }
        public PatientExceptions(string message) : base(message) { }

        public string GetMensajeforLogs(string method)
        {
            return $"{method} Exception: {Message}";
        }

    }
}