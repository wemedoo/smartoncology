﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sReportsV2.DTOs.Form.DataIn
{
    public class FormEpisodeOfCareDataDataIn
    {
        public string Status { get; set; }
        public int? Type { get; set; }
        public string DiagnosisCondition { get; set; }
        public int? DiagnosisRole { get; set; }
        public string DiagnosisRank { get; set; }
    }
}