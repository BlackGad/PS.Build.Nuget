using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PS.Build.Nuget.Types
{
    public class CompositeValidationResult : ValidationResult
    {
        private readonly List<ValidationResult> _results;

        #region Constructors

        public CompositeValidationResult(string errorMessage, params ValidationResult[] results) : this(null, errorMessage, results)
        {
        }

        public CompositeValidationResult(string memberName, string errorMessage, params ValidationResult[] results)
            : base(errorMessage, memberName != null ? new[] { memberName } : null)
        {
            _results = new List<ValidationResult>();
            _results.AddRange(results);
        }

        #endregion

        #region Properties

        public IEnumerable<ValidationResult> Results
        {
            get { return _results; }
        }

        #endregion

        #region Members

        public void AddResult(ValidationResult validationResult)
        {
            _results.Add(validationResult);
        }

        #endregion
    }
}