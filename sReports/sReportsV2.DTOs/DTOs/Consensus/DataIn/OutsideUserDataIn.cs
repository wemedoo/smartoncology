﻿using sReportsV2.DTOs.Common;
using System;

namespace sReportsV2.DTOs.Form.DataIn
{
    public class OutsideUserDataIn
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Institution { get; set; }
        public string InstitutionAddress { get; set; }
        public AddressDTO Address { get; set; }
        public string ConsensusRef { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? LastUpdate { get; set; }
    }
}