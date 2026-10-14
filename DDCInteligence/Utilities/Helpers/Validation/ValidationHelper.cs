using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.Utilities.Helpers.Validation
{
    public class ValidationHelper
    {
        private static readonly IEnumerable<string> usStates = new string[] { "AL", "AK", "AS", "AZ", "AR", "CA",
            "CO", "CT", "DE", "DC", "FM", "FL", "GA", "GU", "HI", "ID", "IL", "IN",
            "IA", "KS", "KY", "LA", "ME", "MH", "MD", "MA", "MI", "MN", "MS", "MO",
            "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "MP", "OH", "OK",
            "OR", "PW", "PA", "PR", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VI",
            "VA", "WA", "WV", "WI", "WY" };
        public static bool IsEmailValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static Task<IEnumerable<string>> ValidateEmail(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string email)
                {
                    int limit = 100;

                    if (email.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", email.Length, limit));
                    }
                    if (email.Length > 0 && !IsEmailValid(email))
                    {
                        errors.Add("Invalid email.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateState(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string state)
                {
                    if (string.IsNullOrEmpty(state))
                        errors.Add("Account state can't be empty.");
                    if (!string.IsNullOrEmpty(state) && state.Length != 2)
                        errors.Add("Invalid state length.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateAccountName(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string accountName)
                {
                    if (string.IsNullOrEmpty(accountName))
                        errors.Add("Name is required.");

                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateAddress(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string address)
                {
                    if (string.IsNullOrEmpty(address))
                        errors.Add("Address 1 is required.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateCity(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string city)
                {
                    if (string.IsNullOrEmpty(city))
                        errors.Add("City is required.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateContact(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();

                if (sender.GetType().GetProperty(propertyName)?.GetValue(sender) is string contact)
                {
                    // Check if contact is null or empty
                    if (string.IsNullOrWhiteSpace(contact))
                    {
                        errors.Add("Contact is required");
                    }
                    else if (!string.IsNullOrWhiteSpace(contact))
                    {
                        if (!contact.All(char.IsDigit))
                        {
                            errors.Add("Phone number must contain only numbers");
                        }
                        else if (!Regex.IsMatch(contact, @"^\d{10}$"))
                        {
                            errors.Add("Phone number must be exactly 10 digits (area code + phone number)");
                        }
                    }
                }
                else
                {
                    errors.Add("Contact is required");
                }

                return (IEnumerable<string>)errors;
            });
        }

        public static Task<IEnumerable<string>> ValidatePhone(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string phone)
                {
                    int limit = 20;
                    if (phone.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", phone.Length, limit));
                    }
                    if (phone.Length > 0 && !Regex.IsMatch(phone, @"^[0-9]+$"))
                    {
                        errors.Add("Invalid phone number.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateName(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string name)
                {
                    int limit = 100;

                    if (name.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", name.Length, limit));
                    }
                    if (name.Length > 0 && !Regex.IsMatch(name, @"^[/.a-zA-Z\s-]+$"))
                    {
                        errors.Add("Invalid name.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateZip(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string zip)
                {
                    int limit = 9;

                    if (string.IsNullOrEmpty(zip))
                        errors.Add("Account zip can't be empty.");
                    if (!string.IsNullOrEmpty(zip) && zip.Length > limit)
                        errors.Add($"{zip.Length}/{limit} character limit.");

                    if (!string.IsNullOrEmpty(zip) && zip.Length > 0)
                    {
                        // Regex patterns:
                        // USA 5-digit or 5+4: ^\d{5}(-\d{4})?$
                        // CAN 6-char: ^[A-Za-z]\d[A-Za-z]\d[A-Za-z]\d$
                        // CAN 5-char: ^[A-Za-z]\d[A-Za-z]\d[A-Za-z]$
                        // Mexico 5-digit: same as USA 5-digit

                        bool isValid = Regex.IsMatch(zip, @"^\d{5}(-\d{4})?$") // USA/MEX
                            || Regex.IsMatch(zip, @"^[A-Za-z]\d[A-Za-z]\d[A-Za-z]\d$") // CAN 6
                            || Regex.IsMatch(zip, @"^[A-Za-z]\d[A-Za-z]\d[A-Za-z]$"); // CAN 5

                        if (!isValid)
                            errors.Add("Invalid zip format.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }

        public static Task<IEnumerable<string>> ValidateReferenceNumber(object sender, string propertyName, params object[] args)
        {
            //return Task.Run(() =>
            //{
            //    List<string> errors = new List<string>();
            //    if (sender is BillingInfo dataReference)
            //    {
            //        string referenceNumber = dataReference.reference_number;

            //        if (string.IsNullOrWhiteSpace(referenceNumber))
            //        {
            //            errors.Add("Reference number can't be empty.");
            //        }
            //        else
            //        {
            //            int limit = 30;
            //            if (referenceNumber.Length > limit)
            //            {
            //                errors.Add(string.Format("{0}/{1} character limit.", referenceNumber.Length, limit));
            //            }
            //            if (referenceNumber.Length > 0 && !Regex.IsMatch(referenceNumber, @"^[a-zA-Z0-9]+$"))
            //            {
            //                errors.Add("Invalid reference number.");
            //            }
            //        }
            //    }
            //    return (IEnumerable<string>)errors;
            //});

            return null;
        }
        public static Task<IEnumerable<string>> ValidatePieces(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName)?.GetValue(sender) is string pieces)
                {
                    int limit = 6;
                    int ret = 0;
                    if (pieces.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", pieces.Length, limit));
                    }

                    bool isNumber = int.TryParse(pieces,out ret);
                    if (pieces.Length > 0 && !isNumber) //!Regex.IsMatch(pieces, @"^[0-9][0-9]{0,}$")
                    {
                        errors.Add("Must be numeric.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateWeight(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                var _blank = "Weight cannot be blank.";
                var _zero = "Weight must be greater than zero.";
                var _decimal = "Must be decimal.";

                var weight_value = sender.GetType().GetProperty(propertyName)?.GetValue(sender);
                if (weight_value != null)
                {
                    if (weight_value is string weight)
                    {
                        int limit = 6;
                        if (!string.IsNullOrEmpty(weight))
                        {
                            if (weight != "0")
                            {
                                if (weight.Length > limit)
                                {
                                    var _limit = string.Format("{0}/{1} character limit.", weight.Length, limit);
                                    errors.Add(_limit);
                                }
                                if (weight.Length > 0 && !decimal.TryParse(weight, out decimal result))
                                {
                                    errors.Add(_decimal);
                                }
                            } else
                            {
                                errors.Add(_zero);
                            }
                        } else
                        {
                            errors.Add(_blank);
                        }
                    }
                } else
                {
                    errors.Add(_blank);
                }
                
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateAmount(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string fee)
                {
                    //if (fee.Length > 6)
                    //{
                    //    errors.Add(string.Format("{0}/6 character limit.", fee.Length));
                    //}
                    if (fee.Length > 0 && !decimal.TryParse(fee, out decimal result))
                    {
                        errors.Add("Must be decimal.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateClass(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName)?.GetValue(sender) is string _class && !string.IsNullOrWhiteSpace(_class))
                {
                    int limit = 10;
                    if (_class.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", _class.Length, limit));
                    }
                    if (_class.Length > 0 && !Regex.IsMatch(_class, @"^[0-9a-zA-Z.]+$"))
                    {
                        errors.Add("Invalid class.");
                    }
                }
                else
                {
                    errors.Add("Class can't be empty.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateDescription(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                string description = sender.GetType().GetProperty(propertyName)?.GetValue(sender).ToString();
                if (!string.IsNullOrEmpty(description))
                {
                    int limit = 250;
                    if (description.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", description.Length, limit));
                    }
                }
                else
                {
                    errors.Add("Description can't be empty.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateHazmat(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string hazmat)
                {
                    //if (hazmat.Length > 0 && (hazmat != "X" || hazmat != "1" || hazmat != "0"))
                    //{
                    //    errors.Add("'X' or '1' only if hazmat.");
                    //}
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateLogRunNumber(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string input && !string.IsNullOrWhiteSpace(input))
                {
                    int limit = 5;
                    if (input.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", input.Length, limit));
                    }
                    var isnumeric = int.TryParse(input, out _);
                    if (!isnumeric)
                    //if (!Regex.IsMatch(input, @"([0-9]+$)"))
                    {
                        if (input.Length != 0)
                        {
                            errors.Add("Numeric only.");
                        }
                    }
                }

                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateCube(object sender, string propertyName, params object[] args)

        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string input && !string.IsNullOrWhiteSpace(input))
                {
                    int limit = 5;
                    if (input.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", input.Length, limit));
                    }
                }
                return (IEnumerable<string>)errors;

            });
        }
        public static Task<IEnumerable<string>> ValidateEmpty(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName)?.GetValue(sender) is string input && !string.IsNullOrWhiteSpace(input))
                {
                    int limit = 20;
                    if (input.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", input.Length, limit));
                    }
                }
                else
                {
                    errors.Add("Can't be empty.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateDataItems(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is ICollection<ItemsInfo> dataItems)
                {
                    if (dataItems.Count < 1)
                    {
                        errors.Add("Items can't be empty.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateTermsIndex(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is int termsIndex)
                {
                    if (termsIndex == -1)
                    {
                        errors.Add("Terms can't be blank.");
                    }
                }
                else
                {
                    errors.Add("Can't be empty.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateTerms(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                var valid_terms = new List<string>() { "C", "P"};
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string terms)
                {
                    if (string.IsNullOrEmpty(terms))
                    {
                        errors.Add("Terms can't be blank.");
                    } else
                    {
                        if (!valid_terms.Any(t => t == terms))
                        {
                            errors.Add($"Invalid terms must be C/P");
                        }
                    }
                }
                else
                {
                    errors.Add("Can't be empty.");
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateRequesterName(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string name)
                {
                    int limit = 50;

                    if (name.Length > limit)
                    {
                        errors.Add(string.Format("{0}/{1} character limit.", name.Length, limit));
                    }
                    if (name.Length > 0 && !Regex.IsMatch(name, @"^[A-Z0-9\#\%\&\s]+$"))
                    {
                        errors.Add("Invalid name.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }

        #region "References Validation"
        public static Task<IEnumerable<string>> ValidateReferenceDescription(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() =>
            {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string value)
                {
                    if (sender is ReferenceInfo @ref && !string.IsNullOrEmpty(@ref.code))
                    {
                        if (@ref.code.StartsWith("quote", StringComparison.OrdinalIgnoreCase))
                        {
                            bool isMatch = Regex.IsMatch(value, @"^E\d*$", RegexOptions.IgnoreCase);
                            if (!isMatch)
                                errors.Add("Invalid quote number format.");
                        }
                        else if (@ref.code.StartsWith("sid", StringComparison.OrdinalIgnoreCase) || 
                        @ref.code.StartsWith("shipment id", StringComparison.OrdinalIgnoreCase))
                        {
                            bool isMatch = Regex.IsMatch(value, @"^SID\d*$", RegexOptions.IgnoreCase);
                            if (!isMatch)
                                errors.Add("Invalid shipment id format.");
                        }
                        else if (@ref.code.StartsWith("mbl", StringComparison.OrdinalIgnoreCase) || 
                        @ref.code.StartsWith("master bol", StringComparison.OrdinalIgnoreCase))
                        {
                            bool isMatch = Regex.IsMatch(value, @"^MBL\d*$", RegexOptions.IgnoreCase);
                            if (!isMatch)
                                errors.Add("Invalid master bol format.");
                        }
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }

        public static Task<IEnumerable<string>> ValidateReferencePieces(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() => {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string value && !string.IsNullOrEmpty(value))
                {
                    if (sender is ReferenceInfo info && !string.IsNullOrEmpty(info.code) && info.code.StartsWith("po", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!int.TryParse(value, out int ret))
                            errors.Add("Must be numeric.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateReferenceWeight(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() => {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string value && !string.IsNullOrEmpty(value))
                {
                    if (sender is ReferenceInfo info && !string.IsNullOrEmpty(info.code) && info.code.StartsWith("po", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!int.TryParse(value, out int ret))
                            errors.Add("Must be numeric.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        public static Task<IEnumerable<string>> ValidateReferenceAdditionalShipperInfo(object sender, string propertyName, params object[] args)
        {
            return Task.Run(() => {
                List<string> errors = new List<string>();
                if (sender.GetType().GetProperty(propertyName).GetValue(sender) is string sid)
                {
                    if (sender is ReferenceInfo info && !string.IsNullOrEmpty(info.code) &&
                        (info.code.StartsWith("mbl", StringComparison.OrdinalIgnoreCase) ||
                        info.code.StartsWith("master bol", StringComparison.OrdinalIgnoreCase)))
                    {
                        bool isMatch = Regex.IsMatch(sid, @"^MBL\d*$", RegexOptions.IgnoreCase);
                        if (!isMatch)
                            errors.Add("Invalid master bol format.");
                    }
                }
                return (IEnumerable<string>)errors;
            });
        }
        #endregion

    }
}
