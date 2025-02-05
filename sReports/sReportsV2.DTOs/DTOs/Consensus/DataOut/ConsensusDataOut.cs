﻿using sReportsV2.Common.Enums;
using sReportsV2.DTOs.Consensus.DataOut;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sReportsV2.DTOs.Form.DataOut
{
    public class ConsensusDataOut
    {
        public string Id { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset EntryDatetime { get; set; }
        public DateTimeOffset? LastUpdate { get; set; }
        public string FormRef { get; set; }
        public List<ConsensusQuestionDataOut> Questions { get; set; }
        public List<string> UserRefs { get; set; }
        public List<string> OutsideUserRefs { get; set; }
        public ConsensusFindingState? State { get; set; }
        public List<ConsensusIterationDataOut> Iterations { get; set; }
        public int CreatorId { get; set; }

        public ConsensusIterationDataOut GetActiveIteration()
        {
            return Iterations?.LastOrDefault();
        }

        public ConsensusIterationDataOut GetIterationById(string id)
        {
            return Iterations.Find(x => x.Id.Equals(id));
        }
    }
}