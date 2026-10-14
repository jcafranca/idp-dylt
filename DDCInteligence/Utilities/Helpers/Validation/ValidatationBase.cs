using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Helpers.Validation
{
    public abstract class ValidatableBase : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private bool isValidating;

        [NotMapped]
        public bool IsValidating
        {
            get => isValidating;
            protected set => SetProperty(ref isValidating, value);
        }

        private bool isValid = true;
        [NotMapped]
        public bool IsValid
        {
            get => isValid;
            protected set => SetProperty(ref isValid, value);
        }
        [NotMapped]
        protected virtual string Identifier { get; }
        private readonly ConcurrentDictionary<string, IEnumerable<string>> validationErrors = new ConcurrentDictionary<string, IEnumerable<string>>();
        private readonly ConcurrentDictionary<string, Guid> lastValidationProcesses = new ConcurrentDictionary<string, Guid>();
        private readonly ConcurrentDictionary<string, ValidatorDelegate> validators = new ConcurrentDictionary<string, ValidatorDelegate>();
        private readonly ConcurrentDictionary<string, object[]> validatorArgs = new ConcurrentDictionary<string, object[]>();

        protected ValidatableBase()
        {
            PropertyChanged += (sender, e) => Task.Run(() => Validate(e.PropertyName));
        }
        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }
            storage = value;
            RaisePropertyChanged(propertyName);
        }
        public void ClearErrors()
        {
            validationErrors.Clear();
        }
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        public void CustomNotifyError(IEnumerable<string> errors, [CallerMemberName] string propertyName = null)
        {
            if (errors != null && errors.Any())
            {
                validationErrors[propertyName] = errors;
            }
            else if (validationErrors.ContainsKey(propertyName))
            {
                validationErrors.TryRemove(propertyName, out var _);
            }
            RaiseErrorsChanged(propertyName);
        }
        public void RegisterValidator(string propertyName, ValidatorDelegate validator, params object[] args)
        {
            if (validators.ContainsKey(propertyName))
            {
                validators.TryRemove(propertyName, out var _);
            }
            if (validatorArgs.ContainsKey(propertyName))
            {
                validatorArgs.TryRemove(propertyName, out var _);
            }
            validators[propertyName] = validator;
            validatorArgs[propertyName] = args;
        }
        public void UnregisterValidator(string propertyName)
        {
            if (validators.ContainsKey(propertyName))
            {
                validators.TryRemove(propertyName, out var _);
            }
            if (validatorArgs.ContainsKey(propertyName))
            {
                validatorArgs.TryRemove(propertyName, out var _);
            }
        }
        protected virtual Task<string> Validate([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return Task.FromResult("");
            }
            if (!validators.TryGetValue(propertyName, out ValidatorDelegate validator))
            {
                return Task.FromResult("");
            }
            if (!validatorArgs.TryGetValue(propertyName, out object[] args))
            {
                return Task.FromResult("");
            }
            Guid validationProcessKey = Guid.NewGuid();
            lastValidationProcesses[propertyName] = validationProcessKey;
            try
            {
                IEnumerable<string> errors = validator(this, propertyName, args).GetAwaiter().GetResult();
                if (lastValidationProcesses.ContainsKey(propertyName) && lastValidationProcesses[propertyName] == validationProcessKey)
                {
                    if (errors.Any())
                    {
                        validationErrors[propertyName] = errors;
                    }
                    else if (validationErrors.ContainsKey(propertyName))
                    {
                        validationErrors.TryRemove(propertyName, out var _);
                    }
                }
            }
            catch (Exception ex)
            {
                validationErrors[propertyName] = new string[] { ex.GetBaseException().Message };
            }
            finally
            {
                if (lastValidationProcesses.ContainsKey(propertyName) &&
                    lastValidationProcesses[propertyName] == validationProcessKey)
                {
                    lastValidationProcesses.TryRemove(propertyName, out var _);
                }
                RaiseErrorsChanged(propertyName);
            }
            if (validationErrors.ContainsKey(propertyName))
            {
                return Task.FromResult(string.Format("{0}: '{1}'", propertyName, string.Join(" ", validationErrors[propertyName])));
            }
            return Task.FromResult("");
        }
        public virtual async Task<IEnumerable<string>> ValidateAllAsync()
        {
            IEnumerable<string> errors = await Task.WhenAll(validators.Keys.Select(t => Task.Run(() => Validate(t))));
            return errors.Where(t => !string.IsNullOrEmpty(t)).Select(t => string.Format("{0}: '{1}'", Identifier, t));
        }
        protected bool CheckProperty(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException();
            }
            if (validationErrors.ContainsKey(propertyName) && validationErrors[propertyName].Any())
            {
                return false;
            }
            return true;
        }

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        [NotMappedAttribute]
        public bool HasErrors => validationErrors.Count > 0;
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || !validationErrors.ContainsKey(propertyName))
            {
                return null;
            }
            return validationErrors[propertyName];
        }
        #endregion
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
