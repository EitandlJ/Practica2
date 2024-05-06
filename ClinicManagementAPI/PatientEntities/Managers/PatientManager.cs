using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UPB.PatientEntities.Managers.Exceptions;
using UPB.PatientEntities.Models;
using Serilog;
using Microsoft.Extensions.Configuration;


namespace UPB.PatientEntities.Managers
{
    public class PatientManager
    {
        private readonly string _filePath;
        private readonly string _fileLog;
        private readonly IConfiguration _configuration;
        Dictionary<int,Patient> _patients = new Dictionary<int,Patient>();
        public PatientManager(IConfiguration configuration)
        {
            _configuration = configuration;

            try
            {
                _filePath = _configuration.GetSection("Paths").GetSection("txt").Value;
            }
            catch (Exception ex)
            {

                PatientExceptions bsEx = new PatientExceptions(ex.Message);
                Log.Error(bsEx.GetMensajeforLogs("Configuring file Path"));

                throw bsEx;

            }

            LoadPatientsFromFile();
        }
       



        public Patient CreatePatient(Patient patientToCreate)
        {
            Patient createdPatient = new Patient()
            {
                CI = patientToCreate.CI,
                Name = patientToCreate.Name,
                LastName = patientToCreate.LastName,
                BloodGroup = patientToCreate.GetRandomBloodGroup()
            };
            _patients.Add(createdPatient.CI ,createdPatient);
            EscribirPacientesEnArchivo();
            return createdPatient;
            
        }
        public string UpdatePatient(int ci, string name, string lastName)
        {
            try
            {
                _patients[ci].Name = name;
                _patients[ci].LastName = lastName;
                EscribirPacientesEnArchivo();
                return "Datos actualizados";
            }
            catch (Exception ex)
            {
                PatientExceptions bsEx = new PatientExceptions(ex.Message);
                Log.Error(bsEx.GetMensajeforLogs("updatebyCiApellido"));

                throw bsEx;
            }
        }

        public void DeletePatient(int ci)
        {
            
            if (_patients.TryGetValue(ci,out Patient patient) )
            {
                _patients.Remove(ci);
                EscribirPacientesEnArchivo();
            }
            else
            {
                PatientExceptions bsEx = new PatientExceptions();
                Log.Error(bsEx.GetMensajeforLogs("Remove by Ci"));

                throw new Exception("Error while removing CI");
            }
        }

        public Patient GetPatientByCI(int ci)
        {
            try
            {
                return _patients[ci];
            }

            catch (Exception ex)
            {
                PatientExceptions bsEx = new PatientExceptions(ex.Message);
                Log.Error(bsEx.GetMensajeforLogs("obtenerPacienteCI"));

                throw bsEx;

            }
        }

        public Dictionary<int, Patient> GetAll()
        {
            return _patients;
        }
        public void LoadPatientsFromFile()
        {

            try
            {
                var lines = File.ReadAllLines(_filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    var patient = new Patient()
                    {
                        Name = parts[0],
                        LastName = parts[1],
                        CI = int.Parse(parts[2]),
                        BloodGroup = parts[3],
                    };
                    _patients.Add(int.Parse(parts[2]), patient);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public void EscribirPacientesEnArchivo()
        {
            using (StreamWriter writer = new StreamWriter(_filePath, false)) // El segundo parámetro 'false' significa que sobreescribirá el archivo si existe
            {
                foreach (Patient paciente in _patients.Values)
                {
                    string linea = $"{paciente.Name},{paciente.LastName},{paciente.CI},{paciente.BloodGroup}";
                    writer.WriteLine(linea);
                }
            }
        }

    }
}
