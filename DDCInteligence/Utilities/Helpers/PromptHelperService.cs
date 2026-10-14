using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using static DDCInteligence.DataTypes.DocumentTypes;
using static DDCInteligence.DataTypes.LookupDatabaseTypes;

namespace DDCInteligence.Utilities.Helpers
{
    public class PromptHelperService
    {
        private readonly MySQLServices _mySQLServices;
        private readonly LogServices _logServices;
        private IProcess process => ViewModelLocator.Instance.Process;

        public PromptHelperService()
        {
            _mySQLServices = new MySQLServices();
            _logServices = new LogServices();
        }

        public PromptHelperService(MySQLServices mySQLServices, LogServices logServices)
        {
            _mySQLServices = mySQLServices ?? throw new ArgumentNullException(nameof(mySQLServices));
            _logServices = logServices ?? throw new ArgumentNullException(nameof(logServices));
        }

        /// <summary>
        /// Gets the appropriate prompt based on the context and field type
        /// </summary>
        /// <param name="dataContext">The data context of the control</param>
        /// <param name="textBoxName">Name of the textbox (optional)</param>
        /// <param name="textBoxValue">Value of the textbox (optional)</param>
        /// <returns>The formatted prompt string or null if no prompt applies</returns>
        public async Task<string> GetPromptAsync(Control dataContext, string textBoxName = null, string textBoxValue = null)
        {
            try
            {
                // Only process if we're in ENTRY state
                if (ViewModelLocator.Instance.Process.CurrentState != EState.ENTRY)
                    return null;

                var customerPrompt = await GetCustomerPromptAsync();
                if (customerPrompt == null) return null;

                // Handle AccountsModel context (shipper, consignee, billTo)
                if (dataContext.DataContext is AccountsModel accountModel)
                {
                    return GetAccountPrompt(customerPrompt, accountModel.IdentifierName);
                }

                // Handle specific textbox cases
                if (!string.IsNullOrEmpty(textBoxName))
                {
                    return GetTextBoxPrompt(customerPrompt, textBoxName, textBoxValue);
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                _logServices.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetPromptAsync));
                return null;
            }
        }

        /// <summary>
        /// Gets prompt for account-related fields (shipper, consignee, billTo)
        /// </summary>
        private string GetAccountPrompt(CustomerPrompt customerPrompt, string identifierName)
        {
            switch (identifierName?.ToLowerInvariant())
            {
                case "shipper":
                    return _logServices.RemoveDoubleSpace($"{customerPrompt.WHEN_SHIP_1} {customerPrompt.WHEN_SHIP_2}");

                case "consignee":
                    return _logServices.RemoveDoubleSpace($"{customerPrompt.WHEN_CONS_1} {customerPrompt.WHEN_CONS_2}");

                case "billto":
                    return _logServices.RemoveDoubleSpace($"{customerPrompt.WHEN_TP_1} {customerPrompt.WHEN_TP_2}");

                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets prompt for specific textbox controls
        /// </summary>
        private string GetTextBoxPrompt(CustomerPrompt customerPrompt, string textBoxName, string textBoxValue)
        {
            //string terms = ViewModelLocator.Instance.BillingInfo?.Terms;
            //if (terms.Equals("p", StringComparison.OrdinalIgnoreCase)) terms = "S";

            //if (textBoxName.StartsWith("textbox", StringComparison.OrdinalIgnoreCase))
            //{             
            //    switch (textBoxName?.ToLowerInvariant())
            //    {
            //        case "textboxterms":
            //            return GetTermsPrompt(customerPrompt, textBoxValue);
            //        case "textboxinstruction":
            //            return GetSIPrompt(customerPrompt, terms);
            //        // Add more textbox cases as needed
            //    }
            //} else if (textBoxName.StartsWith("reference", StringComparison.OrdinalIgnoreCase))
            //{
            //    return GetReferencePrompt(customerPrompt, textBoxName.Replace("reference", ""), terms);
            //}
            return null;
        }

        private string GetReferencePrompt(CustomerPrompt customerPrompt, string fiCode, string terms)
        {
            if (customerPrompt == null || string.IsNullOrWhiteSpace(fiCode))
                return null;

            string prompt = null;

            // Compare fiCode with FI_CODE_1..FI_CODE_6
            if (terms.Equals(customerPrompt.FI_ACCT_1, StringComparison.OrdinalIgnoreCase) && string.Equals(customerPrompt.FI_CODE_1.TrimEnd(), fiCode, StringComparison.OrdinalIgnoreCase))
                prompt = customerPrompt.FI_PROMPT_1;
            else if (terms.Equals(customerPrompt.FI_ACCT_2, StringComparison.OrdinalIgnoreCase) && string.Equals(customerPrompt.FI_CODE_2.TrimEnd(), fiCode, StringComparison.OrdinalIgnoreCase))
                prompt = customerPrompt.FI_PROMPT_2;
            else if (terms.Equals(customerPrompt.FI_ACCT_3, StringComparison.OrdinalIgnoreCase) && string.Equals(customerPrompt.FI_CODE_3.TrimEnd(), fiCode, StringComparison.OrdinalIgnoreCase))
                prompt = customerPrompt.FI_PROMPT_3;
            else if (terms.Equals(customerPrompt.FI_ACCT_4, StringComparison.OrdinalIgnoreCase) && string.Equals(customerPrompt.FI_CODE_4.TrimEnd(), fiCode, StringComparison.OrdinalIgnoreCase))
                prompt = customerPrompt.FI_PROMPT_4;
            else if (terms.Equals(customerPrompt.FI_ACCT_5, StringComparison.OrdinalIgnoreCase) && string.Equals(customerPrompt.FI_CODE_5.TrimEnd(), fiCode, StringComparison.OrdinalIgnoreCase))
                prompt = customerPrompt.FI_PROMPT_5;
            else if (terms.Equals(customerPrompt.FI_ACCT_6, StringComparison.OrdinalIgnoreCase) && string.Equals(customerPrompt.FI_CODE_6.TrimEnd(), fiCode, StringComparison.OrdinalIgnoreCase))
                prompt = customerPrompt.FI_PROMPT_6;

            // If found, clean and return
            if (!string.IsNullOrWhiteSpace(prompt))
                return _logServices?.RemoveDoubleSpace(prompt) ?? prompt;

            return null;
        }



        /// <summary>
        /// Gets prompt specific to terms textbox
        /// </summary>
        private string GetTermsPrompt(CustomerPrompt customerPrompt, string termsValue)
        {
            var trimmedValue = termsValue?.Trim();

            // Always return the prompt if PC is "x" (case-insensitive) 
            // or doesn't match the trimmed termsValue
            if (!string.Equals(customerPrompt.PC, trimmedValue, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(customerPrompt.PC, "x", StringComparison.OrdinalIgnoreCase))
            {
                return _logServices.RemoveDoubleSpace($"{customerPrompt.PC_PROMPT_1} {customerPrompt.PC_PROMPT_2}");
            }

            return null;
        }

        /// <summary>
        /// Gets prompt specific to special instruction
        /// </summary>
        /// <param name="customerPrompt"></param>
        /// <returns></returns>
        private string GetSIPrompt(CustomerPrompt customerPrompt, string terms) 
        {
            if (terms.Equals(customerPrompt.SI_PROMPT_ACCT))
            {
                string siPrompt = _logServices.RemoveDoubleSpace($"{customerPrompt.SI_PROMPT_1} {customerPrompt.SI_PROMPT_2}");
                if (!string.IsNullOrEmpty(siPrompt)) return siPrompt;
            }
            return null; 
        }

        /// <summary>
        /// Gets customer prompt from database with caching
        /// </summary>
        private async Task<CustomerPrompt> GetCustomerPromptAsync()
        {
            // Use cached version if available
            //var cachedPrompt = process.CustomerPrompt;
            //string custAccount = ViewModelLocator.Instance.Shipper.Code;
            //string termsValue = ViewModelLocator.Instance.BillingInfo.Terms;
            //if (!string.IsNullOrEmpty(termsValue) && termsValue.ToUpper() == "C")
            //{
            //    custAccount = ViewModelLocator.Instance.Consignee.Code;
            //}

            //if (cachedPrompt != null && !string.IsNullOrEmpty(custAccount))
            //    return cachedPrompt;

            //// Fetch from database
            //var customerPrompt = await _mySQLServices.GetQueryAsyncByClass<CustomerPrompt>(
            //    process.LookUpConnectionString,
            //    new Dictionary<string, object> { { "ACCT_CODE", custAccount } });

            //// Cache the result
            //process.CustomerPrompt = customerPrompt;
            //return customerPrompt;

            return null;
        }

        /// <summary>
        /// Sets the prompt in the ViewModelLocator
        /// </summary>
        public void SetPrompt(string prompt)
        {
            process.Prompt = prompt;
        }

        /// <summary>
        /// Clears the current prompt
        /// </summary>
        public void ClearPrompt()
        {
            process.Prompt = string.Empty;
        }
    }
}