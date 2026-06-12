using BloodBankManagementSystem.Forms.Shared;
using System;
using System.Windows.Forms;

namespace BloodBankManagementSystem
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Blood Bank Management System
        /// Actors: Admin, Manager, Doctor, Donor, Patient, Technician
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}
