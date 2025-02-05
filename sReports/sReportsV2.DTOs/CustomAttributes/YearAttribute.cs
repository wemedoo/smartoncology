﻿using System;
using System.ComponentModel.DataAnnotations;

namespace sReportsV2.DTOs.CustomAttributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class YearAttribute : ValidationAttribute
    {
        public string GetErrorMessage(DateTime date) => $"{date:d} cannot be smaller than 1/1/1900.";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var date = (DateTime)value;
                if (date.Year < 1900)
                {
                    return new ValidationResult(GetErrorMessage(date));
                }
            }

            return ValidationResult.Success;
        }
    }
}
