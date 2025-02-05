﻿using AutoMapper;
using Chapters;
using sReportsV2.Common.CustomAttributes;
using sReportsV2.Common.Constants;
using sReportsV2.Domain.Entities.Distribution;
using sReportsV2.Domain.Entities.Form;
using sReportsV2.Domain.Entities.FormInstance;
using sReportsV2.Common.Enums;
using sReportsV2.Domain.Services.Implementations;
using sReportsV2.Domain.Services.Interfaces;
using sReportsV2.DTOs.Form;
using sReportsV2.DTOs.Form.DataOut;
using sReportsV2.DTOs.FormDistribution.DataIn;
using sReportsV2.DTOs.FormDistribution.DataOut;
using sReportsV2.DTOs.Pagination;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using sReportsV2.Domain.Entities.FieldEntity;
using DocumentGenerator;
using sReportsV2.BusinessLayer.Interfaces;
using sReportsV2.Common.Extensions;
using sReportsV2.Common.Entities.User;
using sReportsV2.SqlDomain.Interfaces;
using sReportsV2.Domain.Sql.Entities.Patient;
using sReportsV2.DTOs.User.DTO;
using sReportsV2.DTOs.Field.DataOut;
using sReportsV2.DTOs.Autocomplete;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using sReportsV2.Cache.Resources;

namespace sReportsV2.Controllers
{
    public class FormDistributionController : FormCommonController
    {
        private readonly IFormDistributionDAL formDistributionService;
        private readonly IFormDistributionBLL formDistributionBLL;
        private readonly IMapper Mapper;

        public FormDistributionController(IFormDistributionBLL formDistributionBLL,
            IPatientDAL patientDAL, 
            IEpisodeOfCareDAL episodeOfCareDAL,
            IUserBLL userBLL, 
            IOrganizationBLL organizationBLL, 
            ICodeBLL codeBLL, 
            IFormInstanceBLL formInstanceBLL, 
            IFormBLL formBLL, 
            IEncounterDAL encounterDAL, 
            IThesaurusDAL thesaurusDAL, 
            IAsyncRunner asyncRunner, 
            IPdfBLL pdfBLL, 
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider,
            IConfiguration configuration) : 
            base(patientDAL, episodeOfCareDAL,encounterDAL,userBLL, organizationBLL, codeBLL, formInstanceBLL, formBLL, thesaurusDAL, asyncRunner, pdfBLL, mapper, httpContextAccessor, serviceProvider, configuration)
        {
            formDistributionService = new FormDistributionDAL();
            this.formDistributionBLL = formDistributionBLL;
            Mapper = mapper;
        }

        [SReportsAuditLog]
        [SReportsAuthorize(Permission = PermissionNames.View, Module = ModuleNames.Simulator)]
        public ActionResult GetAll(FormFilterDataIn dataIn)
        {
            ViewBag.FilterData = dataIn;
            return View();
        }

        [SReportsAuthorize]
        public ActionResult ReloadTable(FormDistributionFilterDataIn dataIn)
        {
            dataIn = Ensure.IsNotNull(dataIn, nameof(dataIn));

            PaginationDataOut<FormDistributionTableDataOut, FormDistributionFilterDataIn> result = new PaginationDataOut<FormDistributionTableDataOut, FormDistributionFilterDataIn>()
            {
                Count = formDistributionService.GetAllCount(),
                Data = Mapper.Map<List<FormDistributionTableDataOut>>(formDistributionService.GetAll(dataIn.Page, dataIn.PageSize)),
                DataIn = dataIn
            };
            return PartialView("ReloadTable", result);
        }

        [SReportsAuthorize]
        public ActionResult ReloadForms(FormFilterDataIn dataIn)
        {
            dataIn = Ensure.IsNotNull(dataIn, nameof(dataIn));
            dataIn.State = FormDefinitionState.ReadyForDataCapture;

            PaginationDataOut<FormDataOut, FormFilterDataIn> result = formBLL.ReloadData(dataIn, userCookieData);

            ReloadFormDataOut(result.Data);

            return PartialView(result);
        }

        public ActionResult Edit(string formDistributionId)
        {
            var fd = formDistributionService.GetById(formDistributionId);
            var result = Mapper.Map<FormDistributionDataOut>(fd);

            return View(result);
        }

        [SReportsAuthorize(Permission = PermissionNames.View, Module = ModuleNames.Simulator)]
        public ActionResult GetFieldParameters(string formDistributionId, string formFieldDistributionId)
        {
            FormFieldDistributionDataOut result = formDistributionBLL.GetFormFieldDistribution(formDistributionId, formFieldDistributionId);
            return GetFormFieldDistributionView(formDistributionId, result);
        }

        [SReportsAuthorize(Permission = PermissionNames.View, Module = ModuleNames.Simulator)]
        public ActionResult GetByThesaurusId(int thesaurusId, string versionId)
        {
            SetViewBagPreviewTypeParameters();
            return View(EndpointConstants.Edit, formDistributionBLL.GetFormDistributionForParameterization(thesaurusId, versionId));
        }

        [HttpPost]
        [SReportsAuditLog]
        [SReportsAuthorize(Permission = PermissionNames.Update, Module = ModuleNames.Simulator)]
        public ActionResult SetParameters(FormDistributionDataIn dataIn)
        {
            this.formDistributionBLL.SetParameters(dataIn);
            return StatusCode(StatusCodes.Status201Created);
        }

        [SReportsAuthorize(Permission = PermissionNames.Update, Module = ModuleNames.Simulator)]
        [SReportsAuditLog]
        public ActionResult GenerateDocuments(string formId, int numOfDocuments)
        {
            Form form = formDAL.GetForm(formId);
            if (form == null) return NotFound(TextLanguage.FormNotExists, formId);

            FormDistribution formDistribution = formDistributionService.GetByThesaurusIdAndVersion(form.ThesaurusId, form.Version.Id);
            if (formDistribution == null) return NotFound();
            UserData userData = Mapper.Map<UserData>(userCookieData);

            List<FormInstance> generated = FormInstanceGenerator.Generate(form, formDistribution, numOfDocuments);
            foreach (FormInstance formInstance in generated)
            {
                SetFormInstanceAdditionalData(form, formInstance, userData);
            }

            InsertListOfFormInstances(generated);

            if(formDistribution.ThesaurusId == 14573)
            {
                GenerateDailyFormInstances(generated, userData);
            }
            return StatusCode(StatusCodes.Status201Created);
        }

        public ActionResult GetRelationFieldAutocompleteData(AutocompleteDataIn dataIn, string formDistributionId)
        {
            var result = formDistributionBLL.GetRelationFieldAutocomplete(dataIn, formDistributionId);
            return Json(result);
        }

        [HttpPost]
        [SReportsAuthorize(Permission = PermissionNames.Update, Module = ModuleNames.Simulator)]
        public ActionResult RenderInputsForDependentVariable(DependentVariableRelatedVariables dataIn)
        {
            Form form = formDAL.GetFormByThesaurusAndVersion(dataIn.ThesaurusId, dataIn.VersionId);
            List<Field> formFields = form.GetAllFields();
            List<FormFieldDistributionSingleParameterDataOut> variables = new List<FormFieldDistributionSingleParameterDataOut>();

            Field targetField = formFields.Find(x => x.Id.Equals(dataIn.TargetVariable));
            Field field1 = null;
            Field field2 = null;

            if (dataIn.RelatedVariables.Count > 0)
            {
                field1 = formFields.Find(x => x.Id.Equals(dataIn.RelatedVariables[0].Id));
                if (dataIn.RelatedVariables.Count > 1)
                {
                    field2 = formFields.Find(x => x.Id.Equals(dataIn.RelatedVariables[1].Id));
                }


                if (field1 is FieldNumeric)
                {
                    foreach(string option in dataIn.RelatedVariables[0].GetOptions())
                    {
                        SetField(targetField, field2, GetSingleValue(field1, option, dataIn.RelatedVariables[0].GetLabelForOption(option)), field2 != null ? dataIn.RelatedVariables[1] : null, variables);
                    }
                }
                else
                {
                    foreach (FormFieldValue value in (field1 as FieldSelectable).Values)
                    {
                        SetField(targetField, field2, GetSingleValue(field1, value.GetValueToStore(field1.Type), value.Label), field2 != null ?dataIn.RelatedVariables[1]: null, variables);
                    }
                }
            }

            FormFieldDistributionDataOut dataOut = new FormFieldDistributionDataOut()
            {
                Type = targetField.Type,
                ValuesCombination = variables,
                Label = targetField.Label,
                Id = targetField.Id,
                ThesaurusId = targetField.ThesaurusId,
                RelatedVariables = dataIn.RelatedVariables
            };
            return GetFormFieldDistributionView(dataIn.FormDistributionId, dataOut);
        }

        [SReportsAuthorize(Permission = PermissionNames.Delete, Module = ModuleNames.Simulator)]
        public ActionResult ResetAllRelationsForField(string formDistributionId, string formFieldDistributionId)
        {
            FormFieldDistributionDataOut result = formDistributionBLL.ResetAllRelationsForField(formDistributionId, formFieldDistributionId, userCookieData);
            return GetFormFieldDistributionView(formDistributionId, result);
        }

        public string ShowField(FormFieldDistributionDataOut f)
        {
            f = Ensure.IsNotNull(f, nameof(f));

            switch (f.Type)
            {
                case FieldTypes.Number:
                    return RenderPartialViewToString("~/Views/FormDistribution/NormalDistributionParameters.cshtml", f);
                case FieldTypes.Checkbox:
                    return RenderPartialViewToString("~/Views/FormDistribution/BinominalDistributionParameters.cshtml", f);
                case FieldTypes.Radio:
                case FieldTypes.Select:
                    return RenderPartialViewToString("~/Views/FormDistribution/MultinominalDistributionParameters.cshtml", f);
                default:
                    return string.Empty;
            }
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());

                viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();

                return sw.ToString();
            }
        }

        private void ReloadFormDataOut(List<FormDataOut> forms)
        {
            var formDistributions = formDistributionService.GetAllVersionAndThesaurus();

            foreach (FormDataOut form in forms)
            {
                form.IsParameterize = formDistributions.Exists(x => x.ThesaurusId == form.ThesaurusId && x.VersionId != null && x.VersionId == form.Version.Id);
            }
        }

        private ActionResult GetFormFieldDistributionView(string formDistributionId, FormFieldDistributionDataOut formFieldDistribution)
        {
            SetFieldDefinitionForView(formDistributionId, formFieldDistribution, userCookieData);
            SetViewBagPreviewTypeParameters();
            return PartialView("RenderInputs", formFieldDistribution);
        }

        private void SetField(Field targetField, Field field2, SingleDependOnValueDataOut singleValue1, DTOs.FormDistribution.DataIn.RelatedVariable relatedVariable, List<FormFieldDistributionSingleParameterDataOut> variables)
        {
            if (field2 != null)
            {
                if (field2.Type == FieldTypes.Number)
                {
                    foreach (string option in relatedVariable.GetOptions())
                    {
                        List<SingleDependOnValueDataOut> dependOn = new List<SingleDependOnValueDataOut>
                        {
                            singleValue1,
                            GetSingleValue(field2, option, relatedVariable.GetLabelForOption(option))
                        };
                        variables.Add(GetFormFieldDistributionSingleParameterDataOut(dependOn, targetField));
                    }
                }
                else
                {
                    foreach (FormFieldValue value2 in (field2 as FieldSelectable).Values)
                    {
                        List<SingleDependOnValueDataOut> dependOn = new List<SingleDependOnValueDataOut>
                        {
                            singleValue1,
                            GetSingleValue(field2,  value2.GetValueToStore(field2.Type), value2.Label)
                        };
                        variables.Add(GetFormFieldDistributionSingleParameterDataOut(dependOn, targetField));
                    }
                }
            }
            else
            {
                List<SingleDependOnValueDataOut> dependOn = new List<SingleDependOnValueDataOut>
                                {
                                    singleValue1
                                };
                variables.Add(GetFormFieldDistributionSingleParameterDataOut(dependOn, targetField));

            }
        }

        private SingleDependOnValueDataOut GetSingleValue(Field field, string value, string label)
        {
            return new SingleDependOnValueDataOut()
            {
                Id = field.Id,
                FieldLabel = field.Label,
                Type = field.Type,
                Value = value,
                ValueLabel = label
            };
        }

        private FormFieldDistributionSingleParameterDataOut GetFormFieldDistributionSingleParameterDataOut(List<SingleDependOnValueDataOut> dependOn, Field targetField)
        {
            FormFieldDistributionSingleParameterDataOut result = new FormFieldDistributionSingleParameterDataOut()
            {
                DependOn = dependOn,
                NormalDistributionParameters = new FormFieldNormalDistributionParametersDataOut()
            };

            if(targetField is FieldSelectable fieldSelectable) 
            {
                result.Values = fieldSelectable.Values.Select(x => new FormFieldValueDistributionDataOut()
                {
                    Label = x.Label,
                    ThesaurusId = x.ThesaurusId,
                    Value = x.Value
                }).ToList();
            }

            return result;
        }

        private int ParseAndInsertPatient(Form form)
        {
            PatientParser patientParser = new PatientParser();
            Patient patient = patientParser.ParsePatientChapter(form.Chapters.Find(x => x.ThesaurusId.ToString().Equals(ResourceTypes.PatientThesaurus)));
            patient.OrganizationId = GetOrganizationId(form);

            return InsertPatient(patient);
        }

        /*HACKATON*/

        private void GenerateDailyFormInstances(List<FormInstance> generated, UserData user)
        {
            FormDistribution dailyFormDistribution = formDistributionService.GetByThesaurusId(14911);
            Form formDaily = formDAL.GetFormByThesaurus(dailyFormDistribution.ThesaurusId);
            List<FormInstance> dailyGenerated = FormInstanceGenerator.GenerateDailyForms(generated, formDaily, dailyFormDistribution);

            foreach (FormInstance formInstance in dailyGenerated)
            {
                SetFormInstanceAdditionalData(formDaily, formInstance, user);
            }

            InsertListOfFormInstances(dailyGenerated);

        }

        private void SetFormInstanceAdditionalData(Form form, FormInstance formInstance, UserData user)
        {
            if (!form.DisablePatientData)
            {
                int patientId = ParseAndInsertPatient(form);
                int eocId = InsertEpisodeOfCare(patientId, form.EpisodeOfCare, "Simulator", DateTime.Now, user);
                formInstance.PatientId = patientId;
                formInstance.EpisodeOfCareRef = eocId;
                formInstance.EncounterRef = InsertEncounter(eocId);
            }

            formInstance.UserId = userCookieData.Id;
            formInstance.OrganizationId = userCookieData.ActiveOrganization;
            formInstance.Copy(null, new FormInstanceStatus(FormState.OnGoing, formInstance.UserId, isSigned: false));
            formInstance.InitOrUpdateChapterPageWorkflowHistory(form, formInstance.UserId);
        }

        private void InsertListOfFormInstances(List<FormInstance> formInstances)
        {
            int skip = 0;
            int take = 50;
            while (skip < formInstances.Count)
            {
                formInstanceDAL.InsertMany(formInstances.Skip(skip).Take(take).ToList());
                skip += take;
            }
        }

        private void SetFieldDefinitionForView(string formDistributionId, FormFieldDistributionDataOut formFieldDistribution, UserCookieData userCookieData)
        {
            FormDistribution formDistribution = formDistributionService.GetById(formDistributionId);
            Field fieldDefinition = formDistributionBLL.GetFormField(formDistribution.ThesaurusId, formDistribution.VersionId, userCookieData, formFieldDistribution.Id);
            FieldDataOut fieldDefinitionDataOut = Mapper.Map<FieldDataOut>(fieldDefinition);
            ViewBag.FieldDefinition = fieldDefinitionDataOut;
        }

        private void SetViewBagPreviewTypeParameters()
        {
            ViewBag.CanDelete = ViewBag.UserCookieData.UserHasPermission(PermissionNames.Delete, ModuleNames.Simulator);
            SetReadOnlyAndDisabledViewBag(!ViewBag.UserCookieData.UserHasPermission(PermissionNames.Update, ModuleNames.Simulator));
        }
    }
}