namespace StayHealthy.Client.Models.Appointment;

public class Patient
{
    public Patient(string name, string secondName, string email, string phone)
    {
        Name = name;
        SecondName = secondName;
        Email = email;
        Phone = phone;
    }

    public string Name { get; set; }
    public string SecondName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}