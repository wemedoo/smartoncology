﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sReportsV2.DTOs.Field.DataIn
{
    public class FieldDatetimeDataIn : FieldStringDataIn
    {
        public bool PreventFutureDates { get; set; } = true;
    }
}