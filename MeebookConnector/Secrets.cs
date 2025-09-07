using CredentialManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using WindowsCredentialManager;
using Credential = CredentialManagement.Credential;


namespace MeebookConnector
{
    internal class Secrets
    {
        private static readonly string _target = "MeebookConnector";
        private static readonly string _mailTarget = "MeebookConnector-Mail";
        public static Credential? GetUnilogin()
        {
            return GetCredentials(_target, "Indtast dit brugernavn og adgangskode til Unilogin", "Unilogin");
        }

        public static bool ClearUnilogin()
        {
            return RemoveCredentials(_target);
        }

        public static Credential? GetMailLogin()
        {
            return GetCredentials(_mailTarget, "Indtast dit brugernavn (gmail) og App Password til din gmail.", "Gmail App Password");
        }

        public static bool ClearMailLogin()
        {
            return RemoveCredentials(_mailTarget);
        }

        private static Credential? GetCredentials(string target, string text, string caption)
        {
            var credentials = new Credential { Target = target };
            
            if (!credentials.Load())
            {
                int counter = 0;
                while (string.IsNullOrEmpty(credentials.Username) || string.IsNullOrEmpty(credentials.Password) && counter < 3)
                {
                    counter++;
                    
                    var promptResult = CredentialsPrompt.Show(text, caption);

                    if (!promptResult.Cancelled)
                    {
                        credentials.Username = promptResult.Username;
                        credentials.SecurePassword = promptResult.Password;
                        credentials.PersistanceType = PersistanceType.LocalComputer;
                    }
                }

                if (counter >= 3)
                {
                    return null;
                }

                credentials.Save();
            }

            return credentials;
        }

        //public static bool SetCredentials(
        //     string target, string username, string password, PersistanceType persistenceType)
        //{
        //    return new Credential
        //    {
        //        Target = target,
        //        Username = username,
        //        Password = password,
        //        PersistanceType = persistenceType
        //    }.Save();
        //}

        private static bool RemoveCredentials(string target)
        {
            return new Credential { Target = target }.Delete();
        }

        //    public static GenericCredentials GetUnilogin()
        //    {
        //        GenericCredentials credentials = new GenericCredentials("MeebookConnector");

        //        try
        //        {
        //            credentials.Load();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            // Throws Win32 not found exception if not found
        //        }

        //        if (string.IsNullOrEmpty(credentials.UserName) || credentials.Password == null)
        //        {
        //            var promptResult = CredentialsPrompt.Show(
        //"Indtast dit brugernavn og adgangskode til Unilogin",
        //"Unilogin");

        //            if (!promptResult.Cancelled)
        //            {
        //                credentials.UserName = promptResult.Username;
        //                credentials.Password = promptResult.Password;
        //                credentials.Persistence = CredentialPersistence.LocalMachine;
        //                credentials.Save();
        //            }
        //        }

        //        return credentials;
        //    }
    }
}
