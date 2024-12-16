﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sReportsV2.DTOs.CodeEntry.DataIn
{
    public class CodeFilterDataIn : Common.DataIn
    {
        public int? Id { get; set; }
        public int CodeId { get; set; }
        public string CodeDisplay { get; set; }
        public int CodeSetId { get; set; }
        public string CodeSetDisplay { get; set; }
        public string ActiveLanguage { get; set; }
        public DateTimeOffset? ValidFrom { get; set; }
        public DateTimeOffset? ValidTo { get; set; }
        public bool ShowActive { get; set; }
        public bool ShowInactive { get; set; }
        public bool IsReadOnly { get; set; }
    }
}
