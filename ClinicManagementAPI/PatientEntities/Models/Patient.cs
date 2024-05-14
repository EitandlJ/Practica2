namespace UPB.PatientEntities.Models
{
    public class Patient
    {
        public int CI { get; set; } // Cédula de identidad
        public string Name { get; set; }
        public string LastName { get; set; }
        public string BloodGroup { get; set; }
        public string Code { get; set; }

        public Patient() 
        {
            BloodGroup = GetRandomBloodGroup();
        }
        public string GetRandomBloodGroup()
        {
            string[] bloodGroups = { "A+", "B+", "AB+", "O+", "A-", "B-", "AB-", "O-" };
            Random rand = new Random();
            int index = rand.Next(bloodGroups.Length);
            return bloodGroups[index];
        }
    }
}
