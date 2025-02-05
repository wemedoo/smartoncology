﻿using sReportsV2.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sReportsV2.DTOs.Form.DataIn
{
    public class FormFieldValueDataIn : IViewModeDataIn
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Value { get; set; }
        public int? ThesaurusId { get; set; }
        public double? NumericValue { get; set; }
        public bool IsReadOnlyViewMode { get; set; }
        public bool IsMatrix { get; set; }
    }
}