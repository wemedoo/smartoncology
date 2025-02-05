﻿using MongoDB.Bson.Serialization.Attributes;
using sReportsV2.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sReportsV2.Domain.Entities.FieldEntity
{
    [BsonIgnoreExtraElements]
    [BsonDiscriminator(FieldTypes.Text)]
    public class FieldText : FieldString
    {
        public override string Type { get; set; } = FieldTypes.Text;
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
    }
}
