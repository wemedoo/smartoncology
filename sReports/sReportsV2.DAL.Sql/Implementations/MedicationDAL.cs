﻿using Microsoft.EntityFrameworkCore;
using sReportsV2.DAL.Sql.Sql;
using sReportsV2.Domain.Sql.Entities.ChemotherapySchema;
using sReportsV2.SqlDomain.Helpers;
using sReportsV2.SqlDomain.Interfaces;
using System.Linq;

namespace sReportsV2.SqlDomain.Implementations
{
    public class MedicationDAL : IMedicationDAL
    {
        private readonly SReportsContext context;

        public MedicationDAL(SReportsContext context)
        {
            this.context = context;
        }
        public Medication GetById(int id)
        {
            return context.Medications
                .Include("MedicationDoses")
                .Include("MedicationDoses.MedicationDoseTimes")
                .FirstOrDefault(x => x.MedicationId == id);
        }

        public void InsertOrUpdate(Medication medication)
        {
            if (medication.MedicationId == 0)
            {
                context.Medications.Add(medication);
            }
            else
            {
                context.UpdateEntryMetadata(medication, setEntityState: false);
            }
            context.SaveChanges();
        }

        public byte[] GetRowVersion(int id)
        {
            return context.Medications
                .FirstOrDefault(x => x.MedicationId == id).RowVersion;
        }
    }
}
