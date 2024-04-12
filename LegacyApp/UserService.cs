using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsValidName(firstName) || !IsValidName(lastName))
            {
                return false;
            }

            if (!IsValidEmail(email))
            {
                return false;
            }

            if (!IsAdult(dateOfBirth))
            {
                return false;
            }

            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            if (client == null)
            {
                return false;
            }

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                EmailAddress = email,
                DateOfBirth = dateOfBirth,
                Client = client
            };

            SetCreditLimit(client, user);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        private static bool IsValidEmail(string email)
        {
            return email.Contains('@') && email.Contains('.');
        }

        private bool IsAdult(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            var age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }

            return age >= 21;
        }

        private static void SetCreditLimit(Client client, User user)
        {
            switch (client.Type)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                {
                    using (var userCreditService = new UserCreditService())
                    {
                        var creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                        user.CreditLimit = creditLimit * 2;
                    }
                    user.HasCreditLimit = true;
                    break;
                }
                default:
                {
                    using (var userCreditService = new UserCreditService())
                    {
                        var creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                        user.CreditLimit = creditLimit;
                    }
                    user.HasCreditLimit = true;
                    break;
                }
            }
        }
    }
}
